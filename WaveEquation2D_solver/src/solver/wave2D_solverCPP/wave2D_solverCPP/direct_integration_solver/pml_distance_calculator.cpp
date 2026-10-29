#include "pml_distance_calculator.h"

meshEdgeStore pml_distance_calculator::makeEdge(int i, int j)
{
    // 1. canonical edge (order the two node ids)

	return (i < j) ? meshEdgeStore{ i, j } : meshEdgeStore{ j, i };
}



double pml_distance_calculator::point_to_segment_distance(const Eigen::Vector2d& p, const Eigen::Vector2d& a, const Eigen::Vector2d& b)
{
    // 2. distance from a point to a line segment (exact geometry)

    Eigen::Vector2d ab = b - a;
    Eigen::Vector2d ap = p - a;

    const double ab2 = ab.squaredNorm();
    if (ab2 == 0.0)                // degenerate segment → treat as a point
        return (p - a).norm();

    const double t = std::clamp(ap.dot(ab) / ab2, 0.0, 1.0);
    Eigen::Vector2d projection = a + t * ab;
    return (p - projection).norm();

}



Eigen::Vector2d pml_distance_calculator::centroid(const std::vector<int>& node_ids, const std::unordered_map<int, Eigen::Vector2d>& node_pos)
{
    // 3. centroid of an element (tri or quad)

    Eigen::Vector2d c{ 0.0, 0.0 };
    for (int nid : node_ids)
        c += node_pos.at(nid);

    return c / static_cast<double>(node_ids.size());

}


EdgeDir pml_distance_calculator::classify(const Eigen::Vector2d& a, const Eigen::Vector2d& b, double tol)
{
    const double dx = std::abs(a.x() - b.x());
    const double dy = std::abs(a.y() - b.y());
    if (dx < tol * std::max(1.0, dy)) return EdgeDir::Vertical;   // normal is x
    if (dy < tol * std::max(1.0, dx)) return EdgeDir::Horizontal; // normal is y
    return EdgeDir::Oblique;
}


std::unordered_map<int, double>	pml_distance_calculator::run_dijkstra(const std::unordered_map<int, std::vector<Neighbour>>& adj,
    const std::vector<int>& seeds)
{
    constexpr double INF = std::numeric_limits<double>::infinity();
    std::unordered_map<int, double> dist;

    // Priority queue element (distance, element id)
    using PQItem = std::pair<double, int>;
    std::priority_queue<PQItem, std::vector<PQItem>, std::greater<>> pq;

    for (int s : seeds) 
    {
        auto [it, ins] = dist.emplace(s, 0.0);
        if (ins) pq.emplace(0.0, s);
    }


    while (!pq.empty()) 
    {
        auto [d, u] = pq.top(); pq.pop();
        auto it = dist.find(u);

        if (it == dist.end() || d > it->second) continue;

        for (const Neighbour& nb : adj.at(u)) 
        {
            const double alt = d + nb.weight;
            auto [nit, ins] = dist.emplace(nb.element_id, INF);
            if (alt < nit->second) { nit->second = alt; pq.emplace(alt, nb.element_id); }
        }
    }

    return dist;
}


void pml_distance_calculator::calculate_pml_distances(const spectral_mesh2d& spec_mesh2d, std::unordered_map<int, Eigen::Vector2d>& pml_dist)
{
    
    // -------------------------------------------------------------------
    // 0️ - Gather the geometry we need only once (node → coordinates)
    // -------------------------------------------------------------------
    std::unordered_map<int, Eigen::Vector2d> node_pos;
    node_pos.reserve(spec_mesh2d.spectral_node_list.size());

    for (const auto& [nid, node] : spec_mesh2d.spectral_node_list) 
    {
        node_pos.emplace(nid, Eigen::Vector2d{ node.x_coord, node.y_coord });

    }


    // -------------------------------------------------------------------
    // 1️ - Build the *edge => incident elements* map
    // -------------------------------------------------------------------
    using EdgeMap = std::unordered_map<meshEdgeStore, std::vector<int>, meshEdgeHash>;

    EdgeMap edge_to_elements;
    edge_to_elements.reserve(spec_mesh2d.spectral_trielement_list.size() * 3 + spec_mesh2d.spectral_quadelement_list.size() * 4);

    // Helper that inserts the edges of a generic element
    auto insert_edges = [&](int elem_id, const std::vector<int>& nodes)
        {
            const int n = static_cast<int>(nodes.size());

            for (int i = 0; i < n; ++i) 
            {
                meshEdgeStore e = makeEdge(nodes[i], nodes[(i + 1) % n]);
                edge_to_elements[e].push_back(elem_id);
            }
        };

    // Record which element ids belong to the PML
    std::unordered_set<int> pml_elements;

    // ----- triangles ----------------------------------------------------
    for (const auto& [eid, tri] : spec_mesh2d.spectral_trielement_list) 
    {
        const std::vector<int> nodes = tri.lexi_ordered_node_ids;
        insert_edges(eid, nodes);
        if (tri.materialid == PML_MATERIAL_ID)  
            pml_elements.insert(eid);
    }

    // ----- quadrilaterals ------------------------------------------------
    for (const auto& [eid, quad] : spec_mesh2d.spectral_quadelement_list) 
    {
        const std::vector<int> nodes = quad.row_ordered_node_ids;
        insert_edges(eid, nodes);
        if (quad.materialid == PML_MATERIAL_ID)
            pml_elements.insert(eid);
    }


    // -------------------------------------------------------------------
    // 2️ - Identify *interface edges* (one PML + one non‑PML incident element)
    // -------------------------------------------------------------------
    std::vector<InterfaceEdge> iface_x, iface_y, iface_oblique;
    constexpr double orient_tol = 1e-10;

    for (const auto& [e, elems] : edge_to_elements)
    {
        if (elems.size() != 2) continue;
        const int e0 = elems[0], e1 = elems[1];
        const bool e0_pml = pml_elements.contains(e0);
        const bool e1_pml = pml_elements.contains(e1);

        if (e0_pml == e1_pml) continue;

        const int nonpml_id = e0_pml ? e1 : e0;
        const Eigen::Vector2d a = node_pos.at(e.a);
        const Eigen::Vector2d b = node_pos.at(e.b);
        InterfaceEdge ie{ e, nonpml_id, a, b };

        switch (classify(a, b, orient_tol)) {
        case EdgeDir::Vertical:   iface_x.push_back(ie);       break;
        case EdgeDir::Horizontal: iface_y.push_back(ie);       break;
        case EdgeDir::Oblique:    iface_oblique.push_back(ie); break;
        }
    }



    // -------------------------------------------------------------------
    // 3️ - Centroids (keyed by element id — sparse-safe)
    // -------------------------------------------------------------------
    // Pre‑compute centroids for **all** elements – we need them for the edge
    // weights later.
    std::unordered_map<int, Eigen::Vector2d> centroids;
    centroids.reserve(spec_mesh2d.spectral_trielement_list.size() +
        spec_mesh2d.spectral_quadelement_list.size());

    // First fill centroids (triangles)
    for (const auto& [eid, tri] : spec_mesh2d.spectral_trielement_list)
    {
        centroids[eid] = centroid(tri.lexi_ordered_node_ids, node_pos);
    }

    // then quadrilaterals
    for (const auto& [eid, quad] : spec_mesh2d.spectral_quadelement_list)
    {
        centroids[eid] = centroid(quad.row_ordered_node_ids, node_pos);
    }


    // -------------------------------------------------------------------
    // 4 - Build the *dual graph* (elements <=> neighbours)
    // -------------------------------------------------------------------
    std::unordered_map<int, std::vector<Neighbour>> adjacency;
    adjacency.reserve(spec_mesh2d.spectral_trielement_list.size() +
        spec_mesh2d.spectral_quadelement_list.size());

    for (const auto& [e, elems] : edge_to_elements) 
    {
        if (elems.size() != 2) continue;

        const int u = elems[0], v = elems[1];
        const double w = (centroids.at(u) - centroids.at(v)).norm();
        adjacency[u].push_back({ v, w });
        adjacency[v].push_back({ u, w });
    }


    // -------------------------------------------------------------------
    // 5 - Seeds + two Dijkstras
    // -------------------------------------------------------------------
    std::vector<int> seeds_x, seeds_y;
    seeds_x.reserve(iface_x.size());
    seeds_y.reserve(iface_y.size());

    for (const auto& ie : iface_x) seeds_x.push_back(ie.nonpml_elem_id);
    for (const auto& ie : iface_y) seeds_y.push_back(ie.nonpml_elem_id);

    const auto dist_x = run_dijkstra(adjacency, seeds_x);
    const auto dist_y = run_dijkstra(adjacency, seeds_y);


    // -------------------------------------------------------------------
    // 6 - Store {dx, dy} for PML elements
    // -------------------------------------------------------------------
    constexpr double INF = std::numeric_limits<double>::infinity();
    constexpr double NO_PML = 0.0;   // d = 0  =>  s = 1

    for (const int pid : pml_elements)
    {
        const Eigen::Vector2d c = centroids.at(pid);

        double dx = dist_x.count(pid) ? dist_x.at(pid) : INF;
        for (const auto& ie : iface_x)
            dx = std::min(dx, point_to_segment_distance(c, ie.a, ie.b));
        if (!std::isfinite(dx)) dx = NO_PML;

        double dy = dist_y.count(pid) ? dist_y.at(pid) : INF;
        for (const auto& ie : iface_y)
            dy = std::min(dy, point_to_segment_distance(c, ie.a, ie.b));
        if (!std::isfinite(dy)) dy = NO_PML;

        pml_dist[pid] = Eigen::Vector2d{ dx, dy };
    }

	// -------------------------------------------------------------------
}


