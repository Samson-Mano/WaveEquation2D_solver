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
	const double& yieldpoint, const double& thickness)
{
	// Material additio
	material_store temp_material(materialid, youngsmodulus, matdensity, poissonsratio, yieldpoint, thickness);

	// Insert to the material list
	material_list.insert({ materialid, temp_material });

}



void h_refinement_store::add_nodeconstraint(const int& node_constraint_set_id,
	std::vector<int>& node_ids,
	const bool& isFieldBC,
	const double& fieldvalue,
	const double& sourcevalue,
	const double& sourcefrequency,
	const int& sourcetype,
	const double& sourcestarttime)
{
	// Constraint addition
	node_constraint_store temp_node_constraint;
	temp_node_constraint.node_constraint_set_id = node_constraint_set_id;
	temp_node_constraint.isFieldBC = isFieldBC;
	temp_node_constraint.fieldvalue = fieldvalue;
	temp_node_constraint.sourcevalue = sourcevalue;
	temp_node_constraint.sourcefrequency = sourcefrequency;
	temp_node_constraint.sourcetype = sourcetype;
	temp_node_constraint.sourcestarttime = sourcestarttime;

	temp_node_constraint.constraint_node_ids = std::move(node_ids);

	// Insert to the constraint list
	node_constraint_list.insert({ node_constraint_set_id, temp_node_constraint });

}



void h_refinement_store::add_edgeconstraint(const int& edge_constraint_set_id,
	std::vector<int>& constraint_edge_startpt_ids,
	std::vector<int>& constraint_edge_endpt_ids,
	std::vector<int>& constraint_edge_ids,
	const bool& isSommerfieldBC,
	const bool& isFieldBC,
	const bool& isDerivFieldBC,
	const bool& isSource,
	const double& fieldvalue,
	const double& normalderivfieldvalue,
	const double& sourcevalue,
	const double& sourcefrequency,
	const int& sourcetype,
	const double& sourcestarttime)
{
	// Edge constraint addition
	edge_constraint_store temp_edge_constraint;
	temp_edge_constraint.edge_constraint_set_id = edge_constraint_set_id;

	temp_edge_constraint.isSommerfieldBC = isSommerfieldBC;
	temp_edge_constraint.isFieldBC = isFieldBC;
	temp_edge_constraint.isDerivFieldBC = isDerivFieldBC;
	temp_edge_constraint.isSource = isSource;

	temp_edge_constraint.fieldvalue = fieldvalue;
	temp_edge_constraint.normalderivfieldvalue = normalderivfieldvalue;

	temp_edge_constraint.sourcevalue = sourcevalue;
	temp_edge_constraint.sourcefrequency = sourcefrequency;
	temp_edge_constraint.sourcetype = sourcetype;
	temp_edge_constraint.sourcestarttime = sourcestarttime;

	temp_edge_constraint.constraint_edge_startpt_ids = std::move(constraint_edge_startpt_ids);
	temp_edge_constraint.constraint_edge_endpt_ids = std::move(constraint_edge_endpt_ids);
	temp_edge_constraint.constraint_edge_ids = std::move(constraint_edge_ids);

	// Insert to the edge constraint list
	edge_constraint_list.insert({ edge_constraint_set_id, temp_edge_constraint });

}




void h_refinement_store::renumber_model()
{

	//_________________________________________________________________
	// Use reserve to avoid rehashing (performance optimization)
	std::unordered_map<int, node_store> temp_node_list;
	std::unordered_map<int, edge_store> temp_edge_list;
	std::unordered_map<int, trielement_store> temp_trielement_list;
	std::unordered_map<int, quadelement_store> temp_quadelement_list;

	std::unordered_map<int, node_constraint_store> temp_node_constraint_list;
	std::unordered_map<int, edge_constraint_store> temp_edge_constraint_list;

	// Reserve space to prevent rehashing
	temp_node_list.reserve(node_list.size());
	temp_edge_list.reserve(edge_list.size());
	temp_trielement_list.reserve(trielement_list.size());
	temp_quadelement_list.reserve(quadelement_list.size());

	temp_node_constraint_list.reserve(node_constraint_list.size());
	temp_edge_constraint_list.reserve(edge_constraint_list.size());


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


	// Node Constraint list
	int node_constraint_set_id = 0;

	for (const auto& node_cnstr : node_constraint_list)
	{
		node_constraint_store temp_node_constraint;
		temp_node_constraint.node_constraint_set_id = node_constraint_set_id;
		temp_node_constraint.isFieldBC = node_cnstr.second.isFieldBC;
		temp_node_constraint.fieldvalue = node_cnstr.second.fieldvalue;
		temp_node_constraint.sourcevalue = node_cnstr.second.sourcevalue;
		temp_node_constraint.sourcefrequency = node_cnstr.second.sourcefrequency;	
		temp_node_constraint.sourcetype = node_cnstr.second.sourcetype;
		temp_node_constraint.sourcestarttime = node_cnstr.second.sourcestarttime;

		std::vector<int> new_node_ids;

		for (const int& nd_id : node_cnstr.second.constraint_node_ids)
		{
			new_node_ids.push_back(nodeid_map[nd_id]);
		}

		temp_node_constraint.constraint_node_ids = std::move(new_node_ids);

		// Add to the list
		temp_node_constraint_list.emplace(node_constraint_set_id, std::move(temp_node_constraint));

		node_constraint_set_id++;

	}


	// Edge Constraint list
	int edge_constraint_set_id = 0;

	for (const auto& edge_cnstr : edge_constraint_list)
	{
		edge_constraint_store temp_edge_constraint;
		temp_edge_constraint.edge_constraint_set_id = edge_constraint_set_id;
		temp_edge_constraint.isSommerfieldBC = edge_cnstr.second.isSommerfieldBC;
		temp_edge_constraint.isFieldBC = edge_cnstr.second.isFieldBC;
		temp_edge_constraint.isDerivFieldBC = edge_cnstr.second.isDerivFieldBC;
		temp_edge_constraint.isSource = edge_cnstr.second.isSource;

		temp_edge_constraint.fieldvalue = edge_cnstr.second.fieldvalue;
		temp_edge_constraint.normalderivfieldvalue = edge_cnstr.second.normalderivfieldvalue;

		temp_edge_constraint.sourcevalue = edge_cnstr.second.sourcevalue; // Source value in the node
		temp_edge_constraint.sourcefrequency = edge_cnstr.second.sourcefrequency; // Source frequency in the node
		temp_edge_constraint.sourcetype = edge_cnstr.second.sourcetype; // Source type in the node
		temp_edge_constraint.sourcestarttime = edge_cnstr.second.sourcestarttime; // Source start time in the node	


		std::vector<int> new_edge_startpt_ids;
		std::vector<int> new_edge_endpt_ids;

		for (const int& startnd_id : edge_cnstr.second.constraint_edge_startpt_ids)
		{
			new_edge_startpt_ids.push_back(nodeid_map[startnd_id]);
		}

		for (const int& endnd_id : edge_cnstr.second.constraint_edge_endpt_ids)
		{
			new_edge_endpt_ids.push_back(nodeid_map[endnd_id]);
		}


		std::vector<int> new_edge_ids;

		for (const int& edge_id : edge_cnstr.second.constraint_edge_ids)
		{
			new_edge_ids.push_back(edge_id); // Edge IDs remain the same
		}	

		temp_edge_constraint.constraint_edge_startpt_ids = std::move(new_edge_startpt_ids);
		temp_edge_constraint.constraint_edge_endpt_ids = std::move(new_edge_endpt_ids);
		temp_edge_constraint.constraint_edge_ids = std::move(new_edge_ids);

		// Add to the list
		temp_edge_constraint_list.emplace(edge_constraint_set_id, std::move(temp_edge_constraint));


		edge_constraint_set_id++;

	}


	// Move to original (more efficient than clear + insert)
	node_list = std::move(temp_node_list);
	edge_list = std::move(temp_edge_list);
	trielement_list = std::move(temp_trielement_list);
	quadelement_list = std::move(temp_quadelement_list);

	node_constraint_list = std::move(temp_node_constraint_list);
	edge_constraint_list = std::move(temp_edge_constraint_list);
	
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
		extend_nodeconstraints_to_midnodes(edge_to_node_ids);
	}

	// Recreate edges
	recreate_edges();
}



void h_refinement_store::extend_nodeconstraints_to_midnodes(const std::unordered_map<int, int>& edge_to_node_ids)
{
	// Pre-allocate for performance
	std::unordered_map<int, std::unordered_set<int>> nodeconstraint_node_sets;
	nodeconstraint_node_sets.reserve(node_constraint_list.size());

	// Build a set for each constraint for faster lookup
	for (const auto& node_constraint_pair : node_constraint_list)
	{
		const node_constraint_store& node_constraint = node_constraint_pair.second;
		std::unordered_set<int> node_set;
		node_set.reserve(node_constraint.constraint_node_ids.size());
		node_set.insert(node_constraint.constraint_node_ids.begin(), node_constraint.constraint_node_ids.end());
		nodeconstraint_node_sets.emplace(node_constraint_pair.first, std::move(node_set));
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
		for (auto& node_constraint_pair : node_constraint_list)
		{
			int node_constraint_id = node_constraint_pair.first;
			node_constraint_store& node_constraint = node_constraint_pair.second;

			// pre-built set for fast lookup
			const auto& node_set = nodeconstraint_node_sets[node_constraint_id];

			if (node_set.find(startnodeid) != node_set.end() &&
				node_set.find(endnodeid) != node_set.end())
			{
				// Both nodes are in this constraint
				// Check if new node is already in constraint
				auto it = std::find(node_constraint.constraint_node_ids.begin(),
					node_constraint.constraint_node_ids.end(),
					new_node_id);

				if (it == node_constraint.constraint_node_ids.end())
				{
					node_constraint.constraint_node_ids.push_back(new_node_id);
					// Update the set as well
					nodeconstraint_node_sets[node_constraint_id].insert(new_node_id);
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
	stopwatch_events* stopwatch,
	void(*callback)(const char*))
{

	// Set the stopwatch
	this->m_stopwatch = stopwatch;

	// Store callback locally
	this->m_callback = callback;

	this->isConstraintExtend = isConstraintExtend;


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
	for (const auto& nd_m : node_list)
	{
		const node_store& nd = nd_m.second;

		int32_t nodeid = static_cast<int32_t>(nd.node_id);

		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&nd.x_coord), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&nd.y_coord), sizeof(double));
	}

	report("H Refined: Nodes written");

	// Write the tri elements
	int32_t tri_elements_count = static_cast<int32_t>(trielement_list.size());
	bin_file.write(reinterpret_cast<const char*>(&tri_elements_count), sizeof(int32_t));

	for (const auto& tri_m : trielement_list)
	{
		const trielement_store& tri = tri_m.second;


		int32_t triid = static_cast<int32_t>(tri.tri_id);
		int32_t n1 = static_cast<int32_t>(tri.nodeid1);
		int32_t n2 = static_cast<int32_t>(tri.nodeid2);
		int32_t n3 = static_cast<int32_t>(tri.nodeid3);
		int32_t matid = static_cast<int32_t>(tri.materialid);


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

	for (const auto& quad_m : quadelement_list)
	{
		const quadelement_store& quad = quad_m.second;

		int32_t quadid = static_cast<int32_t>(quad.quad_id);
		int32_t n1 = static_cast<int32_t>(quad.nodeid1);
		int32_t n2 = static_cast<int32_t>(quad.nodeid2);
		int32_t n3 = static_cast<int32_t>(quad.nodeid3);
		int32_t n4 = static_cast<int32_t>(quad.nodeid4);
		int32_t matid = static_cast<int32_t>(quad.materialid);


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

	for (const auto& mat_m : material_list)
	{

		const material_store& mat = mat_m.second;

		int32_t matid = static_cast<int32_t>(mat.materialid);
		bin_file.write(reinterpret_cast<const char*>(&matid), sizeof(int32_t));


		// Write the string length as a 4 - byte integer
		std::string mat_name = "MAT " + std::to_string(matid);

		int32_t length = static_cast<int32_t>(mat_name.length());
		bin_file.write(reinterpret_cast<const char*>(&length), sizeof(int32_t));

		// Write the raw bytes
		bin_file.write(mat_name.c_str(), length);

		bin_file.write(reinterpret_cast<const char*>(&mat.matdensity), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.youngsmodulus), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.poissonsratio), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.yieldpoint), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&mat.thickness), sizeof(double));

	}

	report("H Refined: Materials written");


	// Write the node constraints
	int32_t node_constraints_count = static_cast<int32_t>(node_constraint_list.size());
	bin_file.write(reinterpret_cast<const char*>(&node_constraints_count), sizeof(int32_t));

	for (const auto& node_cnstr_m : node_constraint_list)
	{
		const node_constraint_store& node_cnstr = node_cnstr_m.second;

		int32_t node_cnstr_setid = static_cast<int32_t>(node_cnstr.node_constraint_set_id);
		bin_file.write(reinterpret_cast<const char*>(&node_cnstr_setid), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&node_cnstr.fieldvalue), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node_cnstr.sourcevalue), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node_cnstr.sourcefrequency), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node_cnstr.sourcestarttime), sizeof(double));

		int32_t node_cnstr_sourcetype = static_cast<int32_t>(node_cnstr.sourcetype);
		bin_file.write(reinterpret_cast<const char*>(&node_cnstr_sourcetype), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&node_cnstr.isFieldBC), sizeof(bool));


		int32_t node_ids_count = static_cast<int32_t>(node_cnstr.constraint_node_ids.size());
		bin_file.write(reinterpret_cast<const char*>(&node_ids_count), sizeof(int32_t));

		for (const auto& nd_idint : node_cnstr.constraint_node_ids)
		{
			int32_t nd_id = static_cast<int32_t>(nd_idint);
			bin_file.write(reinterpret_cast<const char*>(&nd_id), sizeof(int32_t));
		}
	}

	report("H Refined: Nodal constraints written");



	// Write the edge constraints
	int32_t edge_constraints_count = static_cast<int32_t>(edge_constraint_list.size());
	bin_file.write(reinterpret_cast<const char*>(&edge_constraints_count), sizeof(int32_t));

	for (const auto& edge_cnstr_m : edge_constraint_list)
	{

		const edge_constraint_store& edge_cnstr = edge_cnstr_m.second;

		int32_t edge_cnstr_setid = static_cast<int32_t>(edge_cnstr.edge_constraint_set_id);
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr_setid), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.fieldvalue), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.normalderivfieldvalue), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.sourcevalue), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.sourcefrequency), sizeof(double));

		int32_t edge_cnstr_sourcetype = static_cast<int32_t>(edge_cnstr.sourcetype);
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr_sourcetype), sizeof(int32_t));

		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.sourcestarttime), sizeof(double));

		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.isFieldBC), sizeof(bool));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.isDerivFieldBC), sizeof(bool));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.isSommerfieldBC), sizeof(bool));
		bin_file.write(reinterpret_cast<const char*>(&edge_cnstr.isSource), sizeof(bool));


		int32_t edge_ids_count = static_cast<int32_t>(edge_cnstr.constraint_edge_ids.size());
		bin_file.write(reinterpret_cast<const char*>(&edge_ids_count), sizeof(int32_t));

		for (int j = 0; j < edge_ids_count; j++)
		{
			int32_t edge_id = static_cast<int32_t>(edge_cnstr.constraint_edge_ids[j]);
			bin_file.write(reinterpret_cast<const char*>(&edge_id), sizeof(int32_t));

			int32_t startnodeid = static_cast<int32_t>(edge_cnstr.constraint_edge_startpt_ids[j]);
			bin_file.write(reinterpret_cast<const char*>(&startnodeid), sizeof(int32_t));

			int32_t endnodeid = static_cast<int32_t>(edge_cnstr.constraint_edge_endpt_ids[j]);
			bin_file.write(reinterpret_cast<const char*>(&endnodeid), sizeof(int32_t));

		}
	}

	report("H Refined: Edge constraints written");

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




