#include "wave2d_system_store.h"


wave2d_system_store::wave2d_system_store()
{
	// Empty constructor
}

void wave2d_system_store::add_node(const int& node_id,
	const double& x_coord, const double& y_coord)
{
	// Node addition
	node_store temp_node;
	temp_node.node_id = node_id;
	temp_node.x_coord = x_coord;
	temp_node.y_coord = y_coord;
	temp_node.isboundarynode = false;
	temp_node.fieldvalue = 0.0; // Field value in the node
	temp_node.sourcevalue = 0.0; // Source value in the node

	// Insert to the node list
	node_list.insert({ node_id, temp_node });

}

void wave2d_system_store::add_edge(const int& edge_id,
	const int& startnodeid, const int& endnodeid)
{
	// Edge addition
	edge_store temp_edge;
	temp_edge.edge_id = edge_id;
	temp_edge.startnodeid = startnodeid;
	temp_edge.endnodeid = endnodeid;
	temp_edge.isboundaryedge = false;
	temp_edge.isSommerfieldBC = false;
	temp_edge.fieldvalue = 0.0;
	temp_edge.normalderivfieldvalue = 0.0;

	// Insert to the edge list
	edge_list.insert({ edge_id, temp_edge });

	// Add edge to node-to-edge map for both start and end nodes
	node_edge_map[startnodeid].push_back(edge_id);
	node_edge_map[endnodeid].push_back(edge_id);

}


void wave2d_system_store::add_trielement(const int& tri_id,
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


	// Set the edge face IDs for the three edges of the triangle element
	set_edge_faceid(nd1_id, nd2_id, tri_id); // Edge 1
	set_edge_faceid(nd2_id, nd3_id, tri_id); // Edge 2
	set_edge_faceid(nd3_id, nd1_id, tri_id); // Edge 3
	//
}





void wave2d_system_store::add_quadelement(const int& quad_id,
	const int& nodeid1, const int& nodeid2,
	const int& nodeid3, const int& nodeid4, const int& materialid)
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


	// Set the edge face IDs for the four edges of the quadrilateral element
	set_edge_faceid(nd1_id, nd2_id, quad_id); // Edge 1
	set_edge_faceid(nd2_id, nd3_id, quad_id); // Edge 2
	set_edge_faceid(nd3_id, nd4_id, quad_id); // Edge 3
	set_edge_faceid(nd4_id, nd1_id, quad_id); // Edge 4
	//
}



void wave2d_system_store::add_material(const int& materialid,
	const double& permittivity, const double& permeability,
	const double& wave_speed)
{
	// Material addition
	material_store temp_material;
	temp_material.materialid = materialid;
	temp_material.permittivity = permittivity;
	temp_material.permeability = permeability;
	temp_material.wave_speed = wave_speed;

	// Insert to the material list
	material_list.insert({ materialid, temp_material });

}



void wave2d_system_store::add_nodeconstraint(const int& node_id,
	const bool& isFieldBC,
	const double& fieldvalue, const double& sourcevalue)
{
	// Node constraint addition
	node_list[node_id].isboundarynode = true;
	node_list[node_id].isFieldBC = isFieldBC;
	node_list[node_id].fieldvalue = fieldvalue;
	node_list[node_id].sourcevalue = sourcevalue;

}



void wave2d_system_store::add_edgeconstraint(const int& edge_id,
	const bool& isSommerfieldBC, const bool& isFieldBC,
	const bool& isDerivFieldBC, const double& fieldvalue,
	const double& normalderivfieldvalue)
{
	// Edge constraint addition
	edge_list[edge_id].isboundaryedge = true;
	edge_list[edge_id].isSommerfieldBC = isSommerfieldBC;
	edge_list[edge_id].isFieldBC = isFieldBC;
	edge_list[edge_id].isDerivFieldBC = isDerivFieldBC;
	edge_list[edge_id].fieldvalue = fieldvalue;
	edge_list[edge_id].normalderivfieldvalue = normalderivfieldvalue;

}



void wave2d_system_store::set_edge_faceid(const int& startnodeid,
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



int wave2d_system_store::get_edge_id(const int& startnodeid, const int& endnodeid)
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







