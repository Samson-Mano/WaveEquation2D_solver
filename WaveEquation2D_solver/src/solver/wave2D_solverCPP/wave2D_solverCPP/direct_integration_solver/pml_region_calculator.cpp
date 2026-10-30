#include "pml_region_calculator.h"

void pml_region_calculator::set_pml_region(const spectral_mesh2d& spec_mesh2d)
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
    std::unordered_set<int> pml_elements;

    // ----- triangles ----------------------------------------------------
    for (const auto& [eid, tri] : spec_mesh2d.spectral_trielement_list)
    {
        const std::vector<int> nodes = tri.lexi_ordered_node_ids;
        
        if (tri.materialid == PML_MATERIAL_ID)
        {
            pml_elements.insert(eid);
            insert_edges(eid, nodes);
        }
    }

    // ----- quadrilaterals ------------------------------------------------
    for (const auto& [eid, quad] : spec_mesh2d.spectral_quadelement_list)
    {
        const std::vector<int> nodes = quad.row_ordered_node_ids;
        
        if (quad.materialid == PML_MATERIAL_ID)
        {
            pml_elements.insert(eid);
            insert_edges(eid, nodes);
        }
    }




}
