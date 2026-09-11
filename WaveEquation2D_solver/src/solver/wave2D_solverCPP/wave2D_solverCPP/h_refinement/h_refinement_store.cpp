#include "h_refinement_store.h"

h_refinement_store::h_refinement_store()
{
	// Empty constructor
}



void h_refinement_store::add_node(const int& node_id, const double& x_coord, const double& y_coord)
{
	// Node addition
	node_store temp_node;
	temp_node.node_id = node_id;
	temp_node.x_coord = x_coord;
	temp_node.y_coord = y_coord;

	// Insert to the node list
	node_list.insert({ node_id, temp_node });

}



void h_refinement_store::create_edge_wireframe()
{
	// Create the edges wire frame
	recreate_edges();

}



void h_refinement_store::add_trielement(const int& tri_id,
	const int& nodeid1, const int& nodeid2, const int& nodeid3,
	const int& materialid)
{
	// Triangle element addition
	trielement_store temp_trielement;
	temp_trielement.tri_id = tri_id;

	// Test the orientation of the triangle element and 
	// reorder the node IDs if necessary to ensure counter-clockwise ordering
	const node_store& n1 = node_list.at(nodeid1);
	const node_store& n2 = node_list.at(nodeid2);
	const node_store& n3 = node_list.at(nodeid3);

	// Calculate the orientation using the determinant of the matrix formed by the node coordinates
	double orientation = (n2.x_coord - n1.x_coord) * (n3.y_coord - n1.y_coord) -
		(n3.x_coord - n1.x_coord) * (n2.y_coord - n1.y_coord);

	int nd1_id = nodeid1; // Node id 1
	int nd2_id = nodeid2; // Node id 2
	int nd3_id = nodeid3; // Node id 3

	if (orientation < 0)
	{
		// If the orientation is negative, the nodes are in clockwise order, so we need to reorder them
		nd2_id = nodeid3; // Node id 2 becomes node id 3
		nd3_id = nodeid2; // Node id 3 becomes node id 2
	}


	temp_trielement.nodeid1 = nd1_id;
	temp_trielement.nodeid2 = nd2_id;
	temp_trielement.nodeid3 = nd3_id;
	temp_trielement.materialid = materialid;

	// Insert to the tri element list
	trielement_list.insert({ tri_id, temp_trielement });

}



void h_refinement_store::add_quadelement(const int& quad_id,
	const int& nodeid1, const int& nodeid2, const int& nodeid3, const int& nodeid4, const int& materialid)
{
	// Quadrilateral element addition
	quadelement_store temp_quadelement;
	temp_quadelement.quad_id = quad_id;

	// Test the orientation of the triangle element and 
	// reorder the node IDs if necessary to ensure counter-clockwise ordering
	const node_store& n1 = node_list.at(nodeid1);
	const node_store& n2 = node_list.at(nodeid2);
	const node_store& n3 = node_list.at(nodeid3);
	const node_store& n4 = node_list.at(nodeid4);

	// Compute signed area (shoelace formula)
	double area =
		n1.x_coord * n2.y_coord - n2.x_coord * n1.y_coord +
		n2.x_coord * n3.y_coord - n3.x_coord * n2.y_coord +
		n3.x_coord * n4.y_coord - n4.x_coord * n3.y_coord +
		n4.x_coord * n1.y_coord - n1.x_coord * n4.y_coord;

	int nd1_id = nodeid1; // Node id 1
	int nd2_id = nodeid2; // Node id 2
	int nd3_id = nodeid3; // Node id 3
	int nd4_id = nodeid4; // Node id 4

	if (area < 0)
	{
		// If the orientation is negative, the nodes are in clockwise order, so we need to reorder them
		nd2_id = nodeid4; // Node id 2 becomes node id 4
		nd4_id = nodeid2; // Node id 4 becomes node id 2
	}



	temp_quadelement.nodeid1 = nd1_id;
	temp_quadelement.nodeid2 = nd2_id;
	temp_quadelement.nodeid3 = nd3_id;
	temp_quadelement.nodeid4 = nd4_id;
	temp_quadelement.materialid = materialid;

	// Insert to the quad element list
	quadelement_list.insert({ quad_id, temp_quadelement });

}



void h_refinement_store::add_material(const int& materialid,
	const double& youngsmodulus, const double& matdensity, const double& poissonsratio,
	const double& yieldpoint, const double& thickness,
	int formulation)
{
	// Material addition
	material_store temp_material(materialid, youngsmodulus, matdensity, poissonsratio, yieldpoint, thickness, formulation);

	// Insert to the material list
	material_list.insert({ materialid, temp_material });

}



void h_refinement_store::add_nodeconstraint(const int& constraint_set_id,
	const int& constrainttype,  // 0 = Pinned, 1 = Roller
	const double& constraintangle, std::vector<int>& node_ids)
{
	// Constraint addition
	constraint_store temp_constraint;
	temp_constraint.constraint_set_id = constraint_set_id;
	temp_constraint.constrainttype = constrainttype;
	temp_constraint.constraintangle = constraintangle;

	temp_constraint.node_ids = std::move(node_ids);

	// Insert to the constraint list
	constraint_list.insert({ constraint_set_id, temp_constraint });

}



void h_refinement_store::add_nodeload(const int& load_set_id,
	const double& loadamplitude,
	const double& loadangle, std::vector<int>& node_ids)
{
	// Load addition
	load_store temp_load;
	temp_load.load_set_id = load_set_id;
	temp_load.loadamplitude = loadamplitude;
	temp_load.loadangle = loadangle;

	temp_load.node_ids = std::move(node_ids);

	// Insert to the load list
	load_list.insert({ load_set_id, temp_load });

}




void h_refinement_store::renumber_model()
{

	//_________________________________________________________________
	// Use reserve to avoid rehashing (performance optimization)
	std::unordered_map<int, node_store> temp_node_list;
	std::unordered_map<int, edge_store> temp_edge_list;
	std::unordered_map<int, trielement_store> temp_trielement_list;
	std::unordered_map<int, quadelement_store> temp_quadelement_list;

	std::unordered_map<int, constraint_store> temp_constraint_list;
	std::unordered_map<int, load_store> temp_load_list;

	// Reserve space to prevent rehashing
	temp_node_list.reserve(node_list.size());
	temp_edge_list.reserve(edge_list.size());
	temp_trielement_list.reserve(trielement_list.size());
	temp_quadelement_list.reserve(quadelement_list.size());

	temp_constraint_list.reserve(constraint_list.size());
	temp_load_list.reserve(load_list.size());


	// Create the node map
	std::unordered_map<int, int> nodeid_map;
	nodeid_map.reserve(node_list.size());

	int nd_id_t = 0;
	for (const auto& nd : node_list)
	{
		// Node addition with move semantics
		node_store temp_node;
		temp_node.node_id = nd_id_t;
		temp_node.x_coord = nd.second.x_coord;
		temp_node.y_coord = nd.second.y_coord;

		temp_node_list.emplace(nd_id_t, std::move(temp_node));
		nodeid_map.emplace(nd.second.node_id, nd_id_t);
		nd_id_t++;
	}

	// Create the element id map
	std::unordered_map<int, int> elemid_map;
	elemid_map.reserve(trielement_list.size() + quadelement_list.size());

	int elem_id_t = 0;

	// Process triangles
	for (const auto& tri : trielement_list)
	{
		trielement_store temp_trielement;
		temp_trielement.tri_id = elem_id_t;
		temp_trielement.nodeid1 = nodeid_map[tri.second.nodeid1];
		temp_trielement.nodeid2 = nodeid_map[tri.second.nodeid2];
		temp_trielement.nodeid3 = nodeid_map[tri.second.nodeid3];
		temp_trielement.materialid = tri.second.materialid;

		temp_trielement_list.emplace(elem_id_t, std::move(temp_trielement));
		elemid_map.emplace(tri.second.tri_id, elem_id_t);
		elem_id_t++;
	}

	// Process quads
	for (const auto& quad : quadelement_list)
	{
		quadelement_store temp_quadelement;
		temp_quadelement.quad_id = elem_id_t;
		temp_quadelement.nodeid1 = nodeid_map[quad.second.nodeid1];
		temp_quadelement.nodeid2 = nodeid_map[quad.second.nodeid2];
		temp_quadelement.nodeid3 = nodeid_map[quad.second.nodeid3];
		temp_quadelement.nodeid4 = nodeid_map[quad.second.nodeid4];
		temp_quadelement.materialid = quad.second.materialid;

		temp_quadelement_list.emplace(elem_id_t, std::move(temp_quadelement));
		elemid_map.emplace(quad.second.quad_id, elem_id_t);
		elem_id_t++;
	}

	// Create the edge id map
	// std::unordered_map<int, int> edgeid_map;
	// edgeid_map.reserve(edge_list.size());

	std::unordered_map<int, std::vector<int>> temp_node_edge_map;

	int edge_id_t = 0;
	for (const auto& edge : edge_list)
	{
		edge_store temp_edge;
		temp_edge.edge_id = edge_id_t;
		temp_edge.startnodeid = nodeid_map[edge.second.startnodeid];
		temp_edge.endnodeid = nodeid_map[edge.second.endnodeid];

		// Handle face IDs 
		temp_edge.leftfaceid = -1;
		temp_edge.rightfaceid = -1;

		//auto left_it = elemid_map.find(edge.second.leftfaceid);
		//if (left_it != elemid_map.end())
		//{
		//	temp_edge.leftfaceid = left_it->second;
		//}

		//auto right_it = elemid_map.find(edge.second.rightfaceid);
		//if (right_it != elemid_map.end())
		//{
		//	temp_edge.rightfaceid = right_it->second;
		//}

		if (edge.second.leftfaceid != -1)
		{
			temp_edge.leftfaceid = elemid_map[edge.second.leftfaceid];
		}

		if (edge.second.rightfaceid != -1)
		{
			temp_edge.rightfaceid = elemid_map[edge.second.rightfaceid];
		}


		// Add edge to node-to-edge map for both start and end nodes
		temp_node_edge_map[temp_edge.startnodeid].push_back(edge_id_t);
		temp_node_edge_map[temp_edge.endnodeid].push_back(edge_id_t);

		temp_edge_list.emplace(edge_id_t, std::move(temp_edge));

		// edgeid_map.emplace(edge.second.edge_id, edge_id_t);
		edge_id_t++;
	}


	// Constraint list
	int constraint_set_id = 0;

	for (const auto& cnst : constraint_list)
	{
		constraint_store temp_constraint;
		temp_constraint.constraint_set_id = constraint_set_id;
		temp_constraint.constrainttype = cnst.second.constrainttype;
		temp_constraint.constraintangle = cnst.second.constraintangle;

		std::vector<int> new_node_ids;

		for (const int& nd_id : cnst.second.node_ids)
		{
			new_node_ids.push_back(nodeid_map[nd_id]);
		}

		temp_constraint.node_ids = std::move(new_node_ids);

		// Add to the list
		temp_constraint_list.emplace(constraint_set_id, std::move(temp_constraint));

		constraint_set_id++;

	}

	// Load list
	int load_set_id = 0;

	for (const auto& load : load_list)
	{
		load_store temp_load;
		temp_load.load_set_id = load_set_id;
		temp_load.loadamplitude = load.second.loadamplitude;
		temp_load.loadangle = load.second.loadangle;

		std::vector<int> new_node_ids;

		for (const int& nd_id : load.second.node_ids)
		{
			new_node_ids.push_back(nodeid_map[nd_id]);
		}

		temp_load.node_ids = std::move(new_node_ids);

		// Add to the list
		temp_load_list.emplace(load_set_id, std::move(temp_load));

		load_set_id++;

	}


	// Move to original (more efficient than clear + insert)
	node_list = std::move(temp_node_list);
	edge_list = std::move(temp_edge_list);
	trielement_list = std::move(temp_trielement_list);
	quadelement_list = std::move(temp_quadelement_list);

	constraint_list = std::move(temp_constraint_list);
	load_list = std::move(temp_load_list);

	node_edge_map = std::move(temp_node_edge_map);


	report("Mesh renumbered for solver");

}


void h_refinement_store::refine_elements()
{
	std::unordered_map<int, int> edge_to_node_ids;
	edge_to_node_ids.reserve(edge_list.size());  // Reserve space for performance

	// Get the current node count (will be updated as we add nodes)
	int node_id = static_cast<int>(node_list.size());

	// Lambda to create mid-node (captures by reference)
	auto create_midnode = [&](int startnodeid, int endnodeid) -> int
		{
			//// Get the nodes
			//auto start_it = node_list.find(startnodeid);
			//auto end_it = node_list.find(endnodeid);

			//if (start_it == node_list.end() || end_it == node_list.end())
			//{
			//	// Handle error: node not found
			//	return -1;
			//}

			//const node_store& start_node = start_it->second;
			//const node_store& end_node = end_it->second;

			const node_store& start_node = node_list[startnodeid];
			const node_store& end_node = node_list[endnodeid];

			// Create the mid point
			double midpt_xcoord = (start_node.x_coord + end_node.x_coord) * 0.5;
			double midpt_ycoord = (start_node.y_coord + end_node.y_coord) * 0.5;

			node_store temp_node;
			temp_node.node_id = node_id;
			temp_node.x_coord = midpt_xcoord;
			temp_node.y_coord = midpt_ycoord;

			// Insert the new node to the original list
			node_list.emplace(node_id, std::move(temp_node));

			return node_id++;
		};

	// Create refined element lists
	std::unordered_map<int, trielement_store> refined_trielement_list;
	std::unordered_map<int, quadelement_store> refined_quadelement_list;

	refined_trielement_list.reserve(trielement_list.size() * 4);
	refined_quadelement_list.reserve(quadelement_list.size() * 4);

	int elem_id = 0;

	// Lambda to create triangle element (captures by reference)
	auto create_trielement = [&](int nd1, int nd2, int nd3, int matid) -> void
		{
			trielement_store tri;
			tri.tri_id = elem_id;
			tri.nodeid1 = nd1;
			tri.nodeid2 = nd2;
			tri.nodeid3 = nd3;
			tri.materialid = matid;

			refined_trielement_list.emplace(elem_id, std::move(tri));
			elem_id++;
		};

	// Process triangles
	for (const auto& tri : trielement_list)
	{
		const trielement_store& trielement = tri.second;

		// Get the three node ids
		int nd1 = trielement.nodeid1;
		int nd2 = trielement.nodeid2;
		int nd3 = trielement.nodeid3;

		// Get the edge ids
		int edge1_id = get_edge_id(nd1, nd2);
		int edge2_id = get_edge_id(nd2, nd3);
		int edge3_id = get_edge_id(nd3, nd1);

		int mid_node1 = -1;
		int mid_node2 = -1;
		int mid_node3 = -1;

		// Create three mid nodes (with edge sharing)
		auto edge1_it = edge_to_node_ids.find(edge1_id);
		if (edge1_it != edge_to_node_ids.end())
		{
			mid_node1 = edge1_it->second;
		}
		else
		{
			mid_node1 = create_midnode(nd1, nd2);
			edge_to_node_ids.emplace(edge1_id, mid_node1);
		}

		auto edge2_it = edge_to_node_ids.find(edge2_id);
		if (edge2_it != edge_to_node_ids.end())
		{
			mid_node2 = edge2_it->second;
		}
		else
		{
			mid_node2 = create_midnode(nd2, nd3);
			edge_to_node_ids.emplace(edge2_id, mid_node2);
		}

		auto edge3_it = edge_to_node_ids.find(edge3_id);
		if (edge3_it != edge_to_node_ids.end())
		{
			mid_node3 = edge3_it->second;
		}
		else
		{
			mid_node3 = create_midnode(nd3, nd1);
			edge_to_node_ids.emplace(edge3_id, mid_node3);
		}

		// Create 4 triangle elements
		// Corner triangles
		create_trielement(nd1, mid_node1, mid_node3, trielement.materialid);
		create_trielement(nd2, mid_node2, mid_node1, trielement.materialid);
		create_trielement(nd3, mid_node3, mid_node2, trielement.materialid);
		// Center triangle
		create_trielement(mid_node1, mid_node2, mid_node3, trielement.materialid);
	}

	// Process quads (if you have them)
	auto create_quadelement = [&](int nd1, int nd2, int nd3, int nd4, int matid) -> void
		{
			quadelement_store quad;
			quad.quad_id = elem_id;
			quad.nodeid1 = nd1;
			quad.nodeid2 = nd2;
			quad.nodeid3 = nd3;
			quad.nodeid4 = nd4;
			quad.materialid = matid;

			refined_quadelement_list.emplace(elem_id, std::move(quad));
			elem_id++;
		};

	for (const auto& quad : quadelement_list)
	{
		const quadelement_store& quadelement = quad.second;

		int nd1 = quadelement.nodeid1;
		int nd2 = quadelement.nodeid2;
		int nd3 = quadelement.nodeid3;
		int nd4 = quadelement.nodeid4;

		// Get edge ids
		int edge1_id = get_edge_id(nd1, nd2);
		int edge2_id = get_edge_id(nd2, nd3);
		int edge3_id = get_edge_id(nd3, nd4);
		int edge4_id = get_edge_id(nd4, nd1);

		// Create mid nodes (4 edges)
		int mid_node1 = -1, mid_node2 = -1, mid_node3 = -1, mid_node4 = -1;

		// Edge 1 (nd1-nd2)
		auto it = edge_to_node_ids.find(edge1_id);
		if (it != edge_to_node_ids.end())
			mid_node1 = it->second;
		else
		{
			mid_node1 = create_midnode(nd1, nd2);
			edge_to_node_ids.emplace(edge1_id, mid_node1);
		}

		// Edge 2 (nd2-nd3)
		it = edge_to_node_ids.find(edge2_id);
		if (it != edge_to_node_ids.end())
			mid_node2 = it->second;
		else
		{
			mid_node2 = create_midnode(nd2, nd3);
			edge_to_node_ids.emplace(edge2_id, mid_node2);
		}

		// Edge 3 (nd3-nd4)
		it = edge_to_node_ids.find(edge3_id);
		if (it != edge_to_node_ids.end())
			mid_node3 = it->second;
		else
		{
			mid_node3 = create_midnode(nd3, nd4);
			edge_to_node_ids.emplace(edge3_id, mid_node3);
		}

		// Edge 4 (nd4-nd1)
		it = edge_to_node_ids.find(edge4_id);
		if (it != edge_to_node_ids.end())
			mid_node4 = it->second;
		else
		{
			mid_node4 = create_midnode(nd4, nd1);
			edge_to_node_ids.emplace(edge4_id, mid_node4);
		}

		// Create center node
		// Get the coordinates of the four corners
		auto n1_it = node_list.find(nd1);
		auto n2_it = node_list.find(nd2);
		auto n3_it = node_list.find(nd3);
		auto n4_it = node_list.find(nd4);

		if (n1_it != node_list.end() && n2_it != node_list.end() &&
			n3_it != node_list.end() && n4_it != node_list.end())
		{
			double center_x = (n1_it->second.x_coord + n2_it->second.x_coord +
				n3_it->second.x_coord + n4_it->second.x_coord) * 0.25;
			double center_y = (n1_it->second.y_coord + n2_it->second.y_coord +
				n3_it->second.y_coord + n4_it->second.y_coord) * 0.25;

			node_store center_node;
			center_node.node_id = node_id;
			center_node.x_coord = center_x;
			center_node.y_coord = center_y;

			node_list.emplace(node_id, std::move(center_node));
			int center_node_id = node_id++;

			// Create 4 quad elements
			create_quadelement(nd1, mid_node1, center_node_id, mid_node4, quadelement.materialid);
			create_quadelement(mid_node1, nd2, mid_node2, center_node_id, quadelement.materialid);
			create_quadelement(center_node_id, mid_node2, nd3, mid_node3, quadelement.materialid);
			create_quadelement(mid_node4, center_node_id, mid_node3, nd4, quadelement.materialid);
		}
	}

	// Replace old element lists with refined ones
	trielement_list = std::move(refined_trielement_list);
	quadelement_list = std::move(refined_quadelement_list);

	// Extend the loads and constraints to the newly created midnodes
	if (this->isConstraintExtend == true)
	{
		extend_constraints_to_midnodes(edge_to_node_ids);
	}
	
	if (this->isLoadExtend == true)
	{
		extend_loads_to_midnodes(edge_to_node_ids);
	}
	


	// Recreate edges
	recreate_edges();
}



void h_refinement_store::extend_constraints_to_midnodes(const std::unordered_map<int, int>& edge_to_node_ids)
{
	// Pre-allocate for performance
	std::unordered_map<int, std::unordered_set<int>> constraint_node_sets;
	constraint_node_sets.reserve(constraint_list.size());

	// Build a set for each constraint for faster lookup
	for (const auto& constraint_pair : constraint_list)
	{
		const constraint_store& constraint = constraint_pair.second;
		std::unordered_set<int> node_set;
		node_set.reserve(constraint.node_ids.size());
		node_set.insert(constraint.node_ids.begin(), constraint.node_ids.end());
		constraint_node_sets.emplace(constraint_pair.first, std::move(node_set));
	}

	// Loop through all new nodes
	for (const auto& edge_node_pair : edge_to_node_ids)
	{
		int edge_id = edge_node_pair.first;
		int new_node_id = edge_node_pair.second;

		// Get the edge
		auto edge_it = edge_list.find(edge_id);
		if (edge_it == edge_list.end())
			continue;

		const edge_store& edge = edge_it->second;
		int startnodeid = edge.startnodeid;
		int endnodeid = edge.endnodeid;

		// Find which constraints contain both nodes
		for (auto& constraint_pair : constraint_list)
		{
			int constraint_id = constraint_pair.first;
			constraint_store& constraint = constraint_pair.second;

			// pre-built set for fast lookup
			const auto& node_set = constraint_node_sets[constraint_id];

			if (node_set.find(startnodeid) != node_set.end() &&
				node_set.find(endnodeid) != node_set.end())
			{
				// Both nodes are in this constraint
				// Check if new node is already in constraint
				auto it = std::find(constraint.node_ids.begin(),
					constraint.node_ids.end(),
					new_node_id);

				if (it == constraint.node_ids.end())
				{
					constraint.node_ids.push_back(new_node_id);
					// Update the set as well
					constraint_node_sets[constraint_id].insert(new_node_id);
				}
			}
		}
	}
	//
}


void h_refinement_store::extend_loads_to_midnodes(const std::unordered_map<int, int>& edge_to_node_ids)
{
	// Pre-allocate for performance
	std::unordered_map<int, std::unordered_set<int>> load_node_sets;
	load_node_sets.reserve(load_list.size());

	// Build a set for each load for faster lookup
	for (const auto& load_pair : load_list)
	{
		const load_store& load = load_pair.second;
		std::unordered_set<int> node_set;
		node_set.reserve(load.node_ids.size());
		node_set.insert(load.node_ids.begin(), load.node_ids.end());
		load_node_sets.emplace(load_pair.first, std::move(node_set));
	}

	// Loop through all new nodes
	for (const auto& edge_node_pair : edge_to_node_ids)
	{
		int edge_id = edge_node_pair.first;
		int new_node_id = edge_node_pair.second;

		// Get the edge
		auto edge_it = edge_list.find(edge_id);
		if (edge_it == edge_list.end())
			continue;

		const edge_store& edge = edge_it->second;
		int startnodeid = edge.startnodeid;
		int endnodeid = edge.endnodeid;

		// Find which load contain both nodes
		for (auto& load_pair : load_list)
		{
			int load_id = load_pair.first;
			load_store& load = load_pair.second;

			// pre-built set for fast lookup
			const auto& node_set = load_node_sets[load_id];

			if (node_set.find(startnodeid) != node_set.end() &&
				node_set.find(endnodeid) != node_set.end())
			{
				// Both nodes are in this load
				// Check if new node is already in load
				auto it = std::find(load.node_ids.begin(),
					load.node_ids.end(),
					new_node_id);

				if (it == load.node_ids.end())
				{
					load.node_ids.push_back(new_node_id);
					// Update the set as well
					load_node_sets[load_id].insert(new_node_id);
				}
			}
		}
	}
	//
}


void h_refinement_store::recreate_edges()
{
	// Use a set of encoded edge IDs for faster lookup
	std::unordered_set<uint64_t> edge_set;
	edge_set.reserve(node_list.size() * 2);

	auto encode_edge = [](int node1, int node2) -> uint64_t
		{
			uint64_t n1 = static_cast<uint64_t>(std::min(node1, node2));
			uint64_t n2 = static_cast<uint64_t>(std::max(node1, node2));
			return (n1 << 32) | n2;  // Shift by 32 bits for 64-bit (! Limit of 4.29 Billions)
		};

	auto check_edge_already_exist = [&](int startnodeid, int endnodeid) -> bool
		{
			uint64_t encoded = encode_edge(startnodeid, endnodeid);
			auto it = edge_set.find(encoded);
			if (it != edge_set.end())
			{
				return true;  // Edge exists
			}
			else
			{
				edge_set.insert(encoded);  // Add to set
				return false;  // New edge
			}
		};

	std::unordered_map<int, edge_store> temp_edge_list;
	temp_edge_list.reserve(node_list.size() * 2);
	int edge_id = 0;

	auto create_edges = [&](int startnodeid, int endnodeid) -> void
		{
			if (check_edge_already_exist(startnodeid, endnodeid))
				return;

			edge_store edge;
			edge.edge_id = edge_id;
			edge.startnodeid = startnodeid; // std::min(startnodeid, endnodeid);
			edge.endnodeid = endnodeid; // std::max(startnodeid, endnodeid);
			edge.leftfaceid = -1;
			edge.rightfaceid = -1;

			temp_edge_list.emplace(edge_id, std::move(edge));

			// Add edge to node-to-edge map for both start and end nodes
			node_edge_map[startnodeid].push_back(edge_id);
			node_edge_map[endnodeid].push_back(edge_id);

			edge_id++;
		};


	node_edge_map.clear();

	// Process triangle edges
	for (const auto& tri : trielement_list)
	{
		const trielement_store& trielement = tri.second;
		create_edges(trielement.nodeid1, trielement.nodeid2);
		create_edges(trielement.nodeid2, trielement.nodeid3);
		create_edges(trielement.nodeid3, trielement.nodeid1);
	}

	// Process quadrilateral edges
	for (const auto& quad : quadelement_list)
	{
		const quadelement_store& quadelement = quad.second;
		create_edges(quadelement.nodeid1, quadelement.nodeid2);
		create_edges(quadelement.nodeid2, quadelement.nodeid3);
		create_edges(quadelement.nodeid3, quadelement.nodeid4);
		create_edges(quadelement.nodeid4, quadelement.nodeid1);
	}

	edge_list = std::move(temp_edge_list);


	//___________________________________________________________________________________
	// Set the edges face id
	for (const auto& tri : trielement_list)
	{
		const trielement_store& trielement = tri.second;

		set_edge_faceid(trielement.nodeid1, trielement.nodeid2, tri.first);
		set_edge_faceid(trielement.nodeid2, trielement.nodeid3, tri.first);
		set_edge_faceid(trielement.nodeid3, trielement.nodeid1, tri.first);

	}

	for (const auto& quad : quadelement_list)
	{
		const quadelement_store& quadelement = quad.second;

		set_edge_faceid(quadelement.nodeid1, quadelement.nodeid2, quad.first);
		set_edge_faceid(quadelement.nodeid2, quadelement.nodeid3, quad.first);
		set_edge_faceid(quadelement.nodeid3, quadelement.nodeid4, quad.first);
		set_edge_faceid(quadelement.nodeid4, quadelement.nodeid1, quad.first);

	}


	//
}



void h_refinement_store::perform_refinement(int h_refinement, bool isConstraintExtend,
	bool isLoadExtend, stopwatch_events* stopwatch,
	void(*callback)(const char*))
{

	// Set the stopwatch
	this->m_stopwatch = stopwatch;

	// Store callback locally
	this->m_callback = callback;

	this->isConstraintExtend = isConstraintExtend;
	this->isLoadExtend = isLoadExtend;

	// Renumber the nodes and elements
	renumber_model();

	if (h_refinement == 1)
	{
		// 1 element to 4 elements
		refine_elements();
	}
	else if (h_refinement == 2)
	{
		// 1 element to 16 elements
		refine_elements();

		refine_elements();
	}

}



void h_refinement_store::set_edge_faceid(const int& startnodeid,
	const int& endnodeid, const int& face_id)
{
	// Fix the direction of the edges of the element based on the node ordering
	int edge_id = get_edge_id(startnodeid, endnodeid); // Edge

	//if (edge_list.find(edge_id) != edge_list.end())
	//{
		// If the edge already exists, check if the direction matches the node ordering
	edge_store& edge = edge_list.at(edge_id);
	if (edge.startnodeid == startnodeid && edge.endnodeid == endnodeid)
	{
		// The edge direction matches the node ordering
		edge.leftfaceid = face_id; // Set the left face ID for this edge
	}
	else if (edge.startnodeid == endnodeid && edge.endnodeid == startnodeid)
	{
		edge.rightfaceid = face_id; // Set the right face ID for this edge
	}
	//else
	//{
	//	// std::cerr << "Error: Edge " << edge1_id << " does not connect the correct nodes for triangle element " << tri_id << std::endl;
	//	exit(1);
	//}
	//}

}



int h_refinement_store::get_edge_id(const int& startnodeid, const int& endnodeid)
{

	// Get the connected edges to start node
	const std::vector<int>& connected_edges = this->node_edge_map[startnodeid];

	for (const int& edge_id : connected_edges)
	{
		const auto& edge = this->edge_list[edge_id];
		if ((edge.startnodeid == startnodeid && edge.endnodeid == endnodeid) ||
			(edge.startnodeid == endnodeid && edge.endnodeid == startnodeid))
		{
			// Line with the same start and end nodes
			return edge_id;
		}

	}

	return -1;
}




void h_refinement_store::save_hrefined_model()
{
	// Print the binary file for debugging

	std::string output_file = "h_refined_model.bin";

	std::ofstream bin_file(output_file.c_str(), std::ios::binary);

	if (!bin_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + output_file;
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}



	int32_t node_points_count = static_cast<int32_t>(node_list.size());
	bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));

	// Write the nodes
	for (const auto& node : node_list)
	{
		int32_t nodeid = static_cast<int32_t>(node.second.node_id);

		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&node.second.x_coord), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node.second.y_coord), sizeof(double));
	}

	report("H Refined: Nodes written");

	// Write the tri elements
	int32_t tri_elements_count = static_cast<int32_t>(trielement_list.size());
	bin_file.write(reinterpret_cast<const char*>(&tri_elements_count), sizeof(int32_t));

	for (const auto& tri : trielement_list)
	{
		int32_t triid = static_cast<int32_t>(tri.second.tri_id);
		int32_t n1 = static_cast<int32_t>(tri.second.nodeid1);
		int32_t n2 = static_cast<int32_t>(tri.second.nodeid2);
		int32_t n3 = static_cast<int32_t>(tri.second.nodeid3);
		int32_t matid = static_cast<int32_t>(tri.second.materialid);


		bin_file.write(reinterpret_cast<const char*>(&triid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&matid), sizeof(int32_t));

	}

	report("H Refined: Tri elements written");

	// Write the quad elements
	int32_t quad_elements_count = static_cast<int32_t>(quadelement_list.size());
	bin_file.write(reinterpret_cast<const char*>(&quad_elements_count), sizeof(int32_t));

	for (const auto& quad : quadelement_list)
	{
		int32_t quadid = static_cast<int32_t>(quad.second.quad_id);
		int32_t n1 = static_cast<int32_t>(quad.second.nodeid1);
		int32_t n2 = static_cast<int32_t>(quad.second.nodeid2);
		int32_t n3 = static_cast<int32_t>(quad.second.nodeid3);
		int32_t n4 = static_cast<int32_t>(quad.second.nodeid4);
		int32_t matid = static_cast<int32_t>(quad.second.materialid);


		bin_file.write(reinterpret_cast<const char*>(&quadid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n4), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&matid), sizeof(int32_t));

	}

	report("H Refined: Quad elements written");


	// Write the materials
	int32_t materials_count = static_cast<int32_t>(material_list.size());
	bin_file.write(reinterpret_cast<const char*>(&materials_count), sizeof(int32_t));

	for (const auto& mat : material_list)
	{
		int32_t matid = static_cast<int32_t>(mat.second.materialid);
		bin_file.write(reinterpret_cast<const char*>(&matid), sizeof(int32_t));


		// Write the string length as a 4 - byte integer
		std::string mat_name = "MAT " + std::to_string(matid);

		int32_t length = static_cast<int32_t>(mat_name.length());
		bin_file.write(reinterpret_cast<const char*>(&length), sizeof(int32_t));

		// Write the raw bytes
		bin_file.write(mat_name.c_str(), length);

		bin_file.write(reinterpret_cast<const char*>(&mat.second.matdensity), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.second.youngsmodulus), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.second.poissonsratio), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.second.yieldpoint), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.second.thickness), sizeof(double));

	}

	report("H Refined: Materials written");


	// Write the node constraints
	int32_t constraints_count = static_cast<int32_t>(constraint_list.size());
	bin_file.write(reinterpret_cast<const char*>(&constraints_count), sizeof(int32_t));

	for (const auto& cnstr : constraint_list)
	{
		int32_t cnstrsetid = static_cast<int32_t>(cnstr.second.constraint_set_id);
		bin_file.write(reinterpret_cast<const char*>(&cnstrsetid), sizeof(int32_t));

		int32_t cnstrtype = static_cast<int32_t>(cnstr.second.constrainttype);
		bin_file.write(reinterpret_cast<const char*>(&cnstrtype), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&cnstr.second.constraintangle), sizeof(double));

		int32_t node_ids_count = static_cast<int32_t>(cnstr.second.node_ids.size());
		bin_file.write(reinterpret_cast<const char*>(&node_ids_count), sizeof(int32_t));

		for (const auto& nd_id1 : cnstr.second.node_ids)
		{
			int32_t nd_id = static_cast<int32_t>(nd_id1);
			bin_file.write(reinterpret_cast<const char*>(&nd_id), sizeof(int32_t));
		}
	}

	report("H Refined: Nodal constraints written");



	// Write the node loads
	int32_t loads_count = static_cast<int32_t>(load_list.size());
	bin_file.write(reinterpret_cast<const char*>(&loads_count), sizeof(int32_t));

	for (const auto& load : load_list)
	{
		int32_t loadsetid = static_cast<int32_t>(load.second.load_set_id);
		bin_file.write(reinterpret_cast<const char*>(&loadsetid), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&load.second.loadamplitude), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&load.second.loadangle), sizeof(double));

		int32_t node_ids_count = static_cast<int32_t>(load.second.node_ids.size());
		bin_file.write(reinterpret_cast<const char*>(&node_ids_count), sizeof(int32_t));

		for (const auto& nd_id1 : load.second.node_ids)
		{
			int32_t nd_id = static_cast<int32_t>(nd_id1);
			bin_file.write(reinterpret_cast<const char*>(&nd_id), sizeof(int32_t));
		}
	}

	report("H Refined: Nodal loads written");

	bin_file.flush();

	auto file_size = bin_file.tellp();  // tellp() for output file (tellg() is for input)

	bin_file.close();

	// Report Success and file size
	std::string success_msg = "H Refined Model Stored Successfully: " +
		output_file +
		" (" + std::to_string(node_points_count) + " nodes, " +
		std::to_string(tri_elements_count) + " triangles, " +
		std::to_string(quad_elements_count) + " Quadrilaterals)";

	report(success_msg.c_str());


}




void h_refinement_store::report(const char* msg)
{
	std::stringstream stopwatch_elapsed_str;

	stopwatch_elapsed_str << std::fixed << std::setprecision(6)
		<< this->m_stopwatch->elapsed();

	std::string final_msg = std::string(msg) + " " +
		stopwatch_elapsed_str.str() +
		" secs";

	if (m_callback)
		m_callback(final_msg.c_str());
	//
}




