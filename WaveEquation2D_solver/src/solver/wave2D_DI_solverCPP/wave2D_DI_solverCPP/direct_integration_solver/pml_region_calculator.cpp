#include "pml_region_calculator.h"

void pml_region_calculator::set_pml_region(const spectral_mesh2d& spec_mesh2d, std::unordered_map<int, pmlDistanceStore>& pml_distances)
{

	auto makeEdge = [](int i, int j)
		{
			// Canonical edge (order the two node ids)

			return (i < j) ? regionEdgeStore{ i, j } : regionEdgeStore{ j, i };
		};


	// -------------------------------------------------------------------
   // 0️ - Gather the geometry we need only once (node => coordinates)
   // -------------------------------------------------------------------
	std::unordered_map<int, Eigen::Vector2d> node_pos;
	node_pos.reserve(spec_mesh2d.spectral_node_list.size());

	for (const auto& [nid, node] : spec_mesh2d.spectral_node_list)
	{
		node_pos.emplace(nid, Eigen::Vector2d{ node.x_coord, node.y_coord });

	}


	// -------------------------------------------------------------------
	// 1️ - Build the pml edge => incident pml elements map
	// -------------------------------------------------------------------
	using EdgeMap = std::unordered_map<regionEdgeStore, std::vector<int>, regionEdgeHash>;

	EdgeMap edge_to_elements;
	edge_to_elements.reserve(spec_mesh2d.spectral_trielement_list.size() * 3 + spec_mesh2d.spectral_quadelement_list.size() * 4);

	// Helper that inserts the edges of a generic element
	auto insert_edges = [&](int elem_id, const std::vector<int>& nodes)
		{
			const int n = static_cast<int>(nodes.size());

			for (int i = 0; i < n; ++i)
			{
				regionEdgeStore e = makeEdge(nodes[i], nodes[(i + 1) % n]);
				edge_to_elements[e].push_back(elem_id);
			}
		};

	// Record which element ids belong to the PML
	std::unordered_set<int> pml_trielements;
	std::unordered_set<int> pml_quadelements;

	// ----- triangles ----------------------------------------------------
	for (const auto& [eid, tri] : spec_mesh2d.spectral_trielement_list)
	{
		const std::vector<int> nodes = tri.corner_nodes;

		if (tri.materialid == PML_MATERIAL_ID)
		{
			pml_trielements.insert(eid);
			insert_edges(eid, nodes);
		}
	}

	// ----- quadrilaterals ------------------------------------------------
	for (const auto& [eid, quad] : spec_mesh2d.spectral_quadelement_list)
	{
		const std::vector<int> nodes = quad.corner_nodes;

		if (quad.materialid == PML_MATERIAL_ID)
		{
			pml_quadelements.insert(eid);
			insert_edges(eid, nodes);
		}
	}


	// -------------------------------------------------------------------
	// 2 - Identify the boundary edges of the PML region
	// -------------------------------------------------------------------

	std::vector<regionEdgeStore> boundary_edges;


	for (const auto& [edge, incident_elements] : edge_to_elements)
	{
		// If the edge is incident to only one element, it is a boundary edge
		if (incident_elements.size() == 1)
		{
			boundary_edges.push_back(edge);
		}
	}


	//-------------------------------------------------------------------------------------
	// 3 - Trace the boundary edges to form a closed loop and collect the boundary nodes
	//-------------------------------------------------------------------------------------

	std::vector<std::vector<regionEdgeStore>> region_boundaries = getRegionBoundaries(boundary_edges, node_pos);


	//-------------------------------------------------------------------------------------
	// 4 - Store the boundary edges and nodes in the regionBoundary structure
	//-------------------------------------------------------------------------------------

	pml_regions.clear();

	setPMLRegionBoundaries(spec_mesh2d, region_boundaries, node_pos, pml_trielements, pml_quadelements);



	//-----------------------------------------------------------------------------------------------------------------
	// 5. Set the PML distance and thickness for each PML element based on the distance to the nearest boundary edge
	//-----------------------------------------------------------------------------------------------------------------
	pml_distances.clear();

	setPMLDistances(spec_mesh2d, node_pos, pml_distances);


}




std::vector<std::vector<regionEdgeStore>> pml_region_calculator::getRegionBoundaries(const std::vector<regionEdgeStore>& edges)
{
	int total_edges = static_cast<int>(edges.size());

	// Create unordered_map for quick edge lookup
	std::unordered_map<int, regionEdgeStore> edge_lookup;

	int edge_id = 0;

	for (const auto& edge : edges)
	{
		edge_lookup[edge_id++] = edge;
	}

	int visited_edge_count = 0;
	std::unordered_map<int, int> visited_edges; // 0 = unvisited, 1 = visited
	for (int i = 0; i < total_edges; ++i)
	{
		visited_edges[i] = 0;
	}


	std::vector<std::vector<regionEdgeStore>> region_boundaries;

	// Create the first boundary loop
	while (visited_edge_count < total_edges)
	{
		for (const auto& [edge_id, edge] : edge_lookup)
		{
			if (visited_edges[edge_id] == 0)
			{
				std::vector<regionEdgeStore> boundary_loop;
				boundary_loop.push_back(edge);
				visited_edges[edge_id] = 1;
				visited_edge_count++;
				int current_node = edge.b; // Start from the second node of the first edge
				while (true)
				{
					bool found_next_edge = false;
					for (const auto& [next_edge_id, next_edge] : edge_lookup)
					{
						if (visited_edges[next_edge_id] == 0 &&
							(next_edge.a == current_node || next_edge.b == current_node))
						{
							boundary_loop.push_back(next_edge);
							visited_edges[next_edge_id] = 1;
							visited_edge_count++;
							current_node = (next_edge.a == current_node) ? next_edge.b : next_edge.a;
							found_next_edge = true;
							break;
						}
					}
					if (!found_next_edge)
					{
						break; // No more edges to follow
					}
				}
				region_boundaries.push_back(boundary_loop); // Add the first closed loop found
			}
		}
	}


	return region_boundaries;
}




std::vector<std::vector<regionEdgeStore>>
pml_region_calculator::getRegionBoundaries(const std::vector<regionEdgeStore>& edges,
	const std::unordered_map<int, Eigen::Vector2d>& node_pos)
{
	const int total_edges = static_cast<int>(edges.size());
	if (total_edges == 0) return {};

	// -------------------------------------------------------------------
	// 1. Node -> incident edge indices.  Also record every node's endpoints.
	// -------------------------------------------------------------------
	std::unordered_map<int, std::vector<int>> node_to_edges;
	node_to_edges.reserve(total_edges * 2);

	for (int e = 0; e < total_edges; ++e) {
		node_to_edges[edges[e].a].push_back(e);
		node_to_edges[edges[e].b].push_back(e);
	}

	// -------------------------------------------------------------------
	// 2. Helper: the "other" endpoint of an edge given one node.
	// -------------------------------------------------------------------
	auto otherNode = [&](int e, int n) {
		return (edges[e].a == n) ? edges[e].b : edges[e].a;
		};

	// -------------------------------------------------------------------
	// 3. Helper: choose the next edge at a node, angle-sorted for degree >= 3.
	//
	//    We come in along (cur_n - prev_n).  We leave along (far - cur_n).
	//    At a pinch node we want to turn as far *counter-clockwise* as
	//    possible (equivalently, hug the interior on our left).  Any
	//    consistent rule keeps loops simple; CCW matches the usual
	//    outer-boundary convention (outer loop has positive signed area).
	// -------------------------------------------------------------------
	auto pickNextEdge = [&](int cur_n, int prev_n, int cur_e,
		const std::vector<bool>& used) -> int
		{
			const auto& inc = node_to_edges.at(cur_n);

			if (inc.size() <= 2) {
				// Degree 1 or 2: the only sensible continuation (or none).
				for (int cand : inc)
					if (cand != cur_e && !used[cand]) return cand;
				return -1;
			}

			// Pinch node.  Score every unused incident edge by signed turn angle
			// relative to the incoming direction.
			const Eigen::Vector2d p_cur = node_pos.at(cur_n);
			const Eigen::Vector2d p_prev = node_pos.at(prev_n);
			const Eigen::Vector2d dir_in = (p_cur - p_prev).normalized();

			int    best_e = -1;
			double best_ang = -std::numeric_limits<double>::infinity();

			for (int cand : inc) {
				if (cand == cur_e || used[cand]) continue;

				const int far = otherNode(cand, cur_n);
				// Skip degenerate (shouldn't happen in a conforming mesh)
				if (far == cur_n) continue;

				const Eigen::Vector2d dir_out = (node_pos.at(far) - p_cur).normalized();

				// Signed turn from dir_in to dir_out (right-hand rule, z-component)
				double cross = dir_in.x() * dir_out.y() - dir_in.y() * dir_out.x();
				double dot = dir_in.dot(dir_out);
				double ang = std::atan2(cross, dot);   // (-pi, pi]

				// Prefer the largest CCW turn.  Use a small epsilon so that
				// a "straight back" edge (ang ~= -pi) loses to a genuine turn.
				if (ang > best_ang) { best_ang = ang; best_e = cand; }
			}
			return best_e;
		};

	// -------------------------------------------------------------------
	// 4. Trace loops.
	// -------------------------------------------------------------------
	std::vector<bool> used(total_edges, false);
	std::vector<std::vector<regionEdgeStore>> region_boundaries;
	region_boundaries.reserve(4); // typical: outer + a few inner holes

	int visited_edge_count = 0;

	// Deterministic starting order: iterate edge indices, not unordered_map
	for (int start_e = 0; start_e < total_edges; ++start_e) {
		if (used[start_e]) continue;

		std::vector<regionEdgeStore> loop;
		loop.reserve(64);

		const int start_node = edges[start_e].a;
		int cur_e = start_e;
		int prev_n = start_node;                 // "came from" node
		int cur_n = otherNode(start_e, start_node);

		used[cur_e] = true;
		++visited_edge_count;
		loop.push_back(edges[cur_e]);

		bool closed = false;

		while (true) {
			// Try to step forward
			int next_e = pickNextEdge(cur_n, prev_n, cur_e, used);

			if (next_e < 0) {
				// Dead end.  For a proper PML region this should not happen.
				// If it did, we still have a partially traced chain; emit it.
				break;
			}

			used[next_e] = true;
			++visited_edge_count;
			loop.push_back(edges[next_e]);

			prev_n = cur_n;
			cur_n = otherNode(next_e, prev_n);
			cur_e = next_e;

			if (cur_n == start_node) { closed = true; break; }
		}

		// Optional: reject open chains (report or discard).
		// We keep them so you can diagnose non-closed PML sub-meshes.
		(void)closed;
		region_boundaries.push_back(std::move(loop));
	}

	return region_boundaries;
}




void pml_region_calculator::setPMLRegionBoundaries(spectral_mesh2d spec_mesh2d,
	const std::vector<std::vector<regionEdgeStore>>& region_boundaries,
	const std::unordered_map<int, Eigen::Vector2d>& node_pos,
	const std::unordered_set<int>& pml_trielements,
	const std::unordered_set<int>& pml_quadelements)
{

	auto makeEdge = [](int i, int j)
		{
			// Canonical edge (order the two node ids)

			return (i < j) ? regionEdgeStore{ i, j } : regionEdgeStore{ j, i };
		};

	// -------------------------------------------------------------------
	// 1️ - Build the pml edge => incident pml elements map
	// -------------------------------------------------------------------
	using EdgeMap = std::unordered_map<regionEdgeStore, std::vector<int>, regionEdgeHash>;

	EdgeMap nonpml_edge_to_elements;
	nonpml_edge_to_elements.reserve(spec_mesh2d.spectral_trielement_list.size() * 3 + spec_mesh2d.spectral_quadelement_list.size() * 4);

	// Helper that inserts the edges of a generic element
	auto insert_edges = [&](int elem_id, const std::vector<int>& nodes)
		{
			const int n = static_cast<int>(nodes.size());

			for (int i = 0; i < n; ++i)
			{
				regionEdgeStore e = makeEdge(nodes[i], nodes[(i + 1) % n]);
				nonpml_edge_to_elements[e].push_back(elem_id);
			}
		};


	// ----- triangles ----------------------------------------------------
	for (const auto& [eid, tri] : spec_mesh2d.spectral_trielement_list)
	{
		const std::vector<int> nodes = tri.corner_nodes;

		if (tri.materialid != PML_MATERIAL_ID)
		{
			insert_edges(eid, nodes);
		}
	}

	// ----- quadrilaterals ------------------------------------------------
	for (const auto& [eid, quad] : spec_mesh2d.spectral_quadelement_list)
	{
		const std::vector<int> nodes = quad.corner_nodes;

		if (quad.materialid != PML_MATERIAL_ID)
		{
			insert_edges(eid, nodes);
		}
	}




	//--------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------
	// Create a PML element visited map to track which PML elements have been processed
	std::unordered_map<int, int> pml_element_visited;

	for (const auto& id : pml_trielements)
	{
		pml_element_visited[id] = 0; // 0 = unvisited, 1 = visited
	}

	for (const auto& id : pml_quadelements)
	{
		pml_element_visited[id] = 0; // 0 = unvisited, 1 = visited
	}


	auto centroid = [](const std::vector<int>& node_ids, const std::unordered_map<int, Eigen::Vector2d>& node_pos)
		{
			// centroid of an element (tri or quad)

			Eigen::Vector2d c{ 0.0, 0.0 };
			for (int nid : node_ids)
				c += node_pos.at(nid);

			return c / static_cast<double>(node_ids.size());

		};


	for (const auto& boundary_edges : region_boundaries)
	{
		regionBoundary region_boundary;

		for (const auto& e : boundary_edges)
		{
			// Check if the edge is incident to a non-PML element
			if (nonpml_edge_to_elements.find(e) != nonpml_edge_to_elements.end())
			{
				region_boundary.inner_edges.push_back(e);
			}
			else
			{
				region_boundary.outer_edges.push_back(e);
			}
		}


		//-------------------------------------------------------------------------------------------------
		for (const int id : pml_trielements)
		{
			// Check if the PML element has been visited
			if (pml_element_visited[id] == 0)
			{
				// Check if the element centroid is inside the boundary loop
				const std::vector<int> nodes = spec_mesh2d.spectral_trielement_list[id].corner_nodes;

				// Get the centroid of the triangle
				Eigen::Vector2d c = centroid(nodes, node_pos);

				// Check if the centroid is inside the boundary loop using a point-in-polygon test
				if (pointInPolygon(c, boundary_edges, node_pos) == true)
				{
					pml_element_visited[id] = 1; // Mark as visited
					region_boundary.pml_trielements.push_back(id);
				}
			}
		}

		for (const int id : pml_quadelements)
		{
			// Check if the PML element has been visited
			if (pml_element_visited[id] == 0)
			{
				// Check if the element centroid is inside the boundary loop
				const std::vector<int> nodes = spec_mesh2d.spectral_quadelement_list[id].corner_nodes;

				// Get the centroid of the quadrilateral
				Eigen::Vector2d c = centroid(nodes, node_pos);

				// Check if the centroid is inside the boundary loop using a point-in-polygon test
				if (pointInPolygon(c, boundary_edges, node_pos) == true)
				{
					pml_element_visited[id] = 1; // Mark as visited
					region_boundary.pml_quadelements.push_back(id);
				}
			}
		}

		// Add the region boundary to the list of PML regions
		pml_regions.push_back(region_boundary);
	}

}



bool pml_region_calculator::pointInPolygon(const Eigen::Vector2d& p,
	const std::vector<regionEdgeStore>& loop,
	const std::unordered_map<int, Eigen::Vector2d>& node_pos)
{
	bool inside = false;

	for (const auto& e : loop)
	{
		const Eigen::Vector2d& a = node_pos.at(e.a);
		const Eigen::Vector2d& b = node_pos.at(e.b);

		// Half-open rule on y: [ymin, ymax) to avoid double-counting vertices
		const bool cross_y = ((a.y() > p.y()) != (b.y() > p.y()));

		if (!cross_y) continue;

		// x-coordinate where edge crosses the horizontal line y = p.y()
		const double x_cross = a.x() + (p.y() - a.y()) * (b.x() - a.x()) / (b.y() - a.y());

		if (p.x() < x_cross) inside = !inside;
	}

	return inside;
}


std::pair<Eigen::Vector2d, double> pml_region_calculator::closestOnSegment(const Eigen::Vector2d& p,
	const Eigen::Vector2d& a, const Eigen::Vector2d& b)
{
	// Closest point on segment, returns(q, t) with t in[0, 1]

	const Eigen::Vector2d ab = b - a;
	const double ab2 = ab.squaredNorm();

	const double t = (ab2 < 1e-24) ? 0.0 : std::clamp((p - a).dot(ab) / ab2, 0.0, 1.0);

	return { a + t * ab, t };

}


double pml_region_calculator::raySegment(const Eigen::Vector2d& p, const Eigen::Vector2d& dhat,
	const Eigen::Vector2d& a, const Eigen::Vector2d& b)
{
	// Intersect ray (p + t·d̂, t ≥ 0) with segment (a, b); return t if hit, else +inf.

	const Eigen::Vector2d ab = b - a;
	const Eigen::Vector2d ap = p - a;
	const double denom = dhat.x() * ab.y() - dhat.y() * ab.x();

	if (std::abs(denom) < 1e-12) return std::numeric_limits<double>::infinity(); // parallel

	const double t = (ap.x() * ab.y() - ap.y() * ab.x()) / denom;   // parameter along ray
	const double s = (ap.x() * dhat.y() - ap.y() * dhat.x()) / denom; // parameter along segment

	if (t < 0.0 || s < 0.0 || s > 1.0) return std::numeric_limits<double>::infinity();

	return t;

}




void pml_region_calculator::setPMLDistances(const spectral_mesh2d& spec_mesh2d,
	const std::unordered_map<int, Eigen::Vector2d>& node_pos,
	std::unordered_map<int, pmlDistanceStore>& pml_distances)
{

	auto centroid = [&](const std::vector<int>& nids)
		{

			Eigen::Vector2d c = Eigen::Vector2d::Zero();
			for (int n : nids) c += node_pos.at(n);
			return c / static_cast<double>(nids.size());

		};


	for (const auto& region : pml_regions)
	{

		if (region.inner_edges.empty()) continue;

		auto process = [&](int id, const std::vector<int>& corner_nodes)
			{
				const Eigen::Vector2d c = centroid(corner_nodes);

				// ---- 1. Closest point on the inner interface ----
				double best_d2 = std::numeric_limits<double>::infinity();
				regionEdgeStore best_e{ -1, -1 };
				Eigen::Vector2d best_q{ 0, 0 };

				for (const auto& e : region.inner_edges)
				{
					const auto [q, _t] = closestOnSegment(c, node_pos.at(e.a), node_pos.at(e.b));
					const double d2 = (c - q).squaredNorm();

					if (d2 < best_d2) 
					{ 
						best_d2 = d2; best_q = q; best_e = e; 
					}
				}
				const double d = std::sqrt(best_d2);

				// ---- 2. Outward normal at q (perpendicular to the chosen interface edge) ----
				const Eigen::Vector2d A = node_pos.at(best_e.a);
				const Eigen::Vector2d B = node_pos.at(best_e.b);

				Eigen::Vector2d n = Eigen::Vector2d{ -(B.y() - A.y()), (B.x() - A.x()) }.normalized();

				// Orient n to point away from the physical (non-PML) side → toward the PML.
				if ((c - best_q).dot(n) < 0.0) 
					n = -n;

				// ---- 3. March from q along n to the outer boundary ----
				double L = 0.0;
				for (const auto& e : region.outer_edges)
				{
					const double t = raySegment(best_q, n, node_pos.at(e.a), node_pos.at(e.b));

					if (std::isfinite(t) && t > L) 
						L = t;   // farthest exit = local thickness
				}

				if (L <= 0.0)
				{
					// Fallback: no outer boundary hit (open PML, or normal missed).
					// Use max d in the region as an estimate; or leave L = d so d/L = 1.
					L = d > 0.0 ? d : 1.0;
				}

				// ---- 4. Optional cartesian split ----
				pmlDistanceStore out;
				out.d = d;
				out.L = L;
				out.n = n;
				out.dx = d * std::abs(n.x());
				out.dy = d * std::abs(n.y());
				out.Lx = L * std::abs(n.x());
				out.Ly = L * std::abs(n.y());
				pml_distances[id] = out;
			};

		for (int id : region.pml_trielements)
		{
			process(id, spec_mesh2d.spectral_trielement_list.at(id).corner_nodes);
		}


		for (int id : region.pml_quadelements)
		{
			process(id, spec_mesh2d.spectral_quadelement_list.at(id).corner_nodes);
		}

	}

}


