#include "spectral_mesh2d.h"

spectral_mesh2d::spectral_mesh2d()
{
	// Empty constructor
}



void spectral_mesh2d::generate_spectral_mesh(const helmholtz_system_store& linear_mesh)
{
	// Copy to local variable
	this->linear_mesh = linear_mesh;

	// Clear any existing spectral mesh data
	spectral_node_list.clear();
	spectral_edge_list.clear();
	spectral_trielement_list.clear();
	spectral_quadelement_list.clear();
	material_list.clear();

	this->material_list = linear_mesh.material_list;


	// p-Refinement
	// Generate spectral nodes, edges, and elements based on the linear mesh
	this->spectral_order = linear_mesh.spectral_order; // Set the spectral order from the linear mesh

	// Create the Local ID structure from spectral order
	create_local_id_structure(this->spectral_order);


	// Get the gll locations and gll weights for the given spectral order 
	std::vector<double> gll_locations = gll_utility::get_gll_locations(this->spectral_order);
	std::vector<double> gll_weights = gll_utility::get_gll_weights(this->spectral_order, gll_locations);


	unique_id_control node_id_control(0); // Unique ID control for nodes

	// Get the existing node IDs from the linear mesh to avoid duplicates
	std::vector<int> existing_node_ids;
	for (const auto& node : linear_mesh.node_list)
	{
		existing_node_ids.push_back(node.first);
	}

	node_id_control.create_free_ids(existing_node_ids); // Initialize the unique ID control with existing node IDs


	// Initialize the renderer elements
	renderer_edge_lines.clear();
	renderer_node_points.clear();
	renderer_element_triangles.clear();

	// Keep track of renderer nodes already added
	std::unordered_set<int> added_nodes;

	// Keep track of renderer edges already added
	std::unordered_set<int> added_edges;


	// QUADRILATERAL ELEMENTS

	for (const auto& quad : linear_mesh.quadelement_list)
	{
		const quadelement_store& quad_elm = quad.second;
		// Get the corner node IDs of the quadrilateral element
		int nd1_id = quad_elm.nodeid1; // Node id 1
		int nd2_id = quad_elm.nodeid2; // Node id 2
		int nd3_id = quad_elm.nodeid3; // Node id 3
		int nd4_id = quad_elm.nodeid4; // Node id 4
		// Get the material ID of the quadrilateral element
		int material_id = quad_elm.materialid;


		// Generate spectral nodes, edges, and elements for the quadrilateral element
		// Nodes are stored in the counter-clockwise order (nd1, nd2, nd3, nd4)
		const node_store& n1 = linear_mesh.node_list.at(nd1_id);
		const node_store& n2 = linear_mesh.node_list.at(nd2_id);
		const node_store& n3 = linear_mesh.node_list.at(nd3_id);
		const node_store& n4 = linear_mesh.node_list.at(nd4_id);

		// get the Four edge ids of the quadrilateral element
		int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edge3_id = get_edge_id(nd3_id, nd4_id); // Edge 3
		int edge4_id = get_edge_id(nd4_id, nd1_id); // Edge 4


		// Get the edges of the quadrilateral element
		const edge_store& e1 = linear_mesh.edge_list.at(edge1_id); // Edge 1
		const edge_store& e2 = linear_mesh.edge_list.at(edge2_id); // Edge 2
		const edge_store& e3 = linear_mesh.edge_list.at(edge3_id); // Edge 3
		const edge_store& e4 = linear_mesh.edge_list.at(edge4_id); // Edge 4


		std::vector<int> corner_node_ids;
		std::vector<std::vector<int>> edge_node_ids{ 4 }; // To be filled with edge node IDs
		std::vector<int> internal_node_ids; // To be filled with internal node IDs


		// Create the edge nodes
		for (int i = 0; i < 4; i++)
		{
			// edge_store edge;
			const edge_store& edge = (i == 0) ? e1 : (i == 1) ? e2 : (i == 2) ? e3 : e4;

			// Reuse existing edge
			if (spectral_edge_list.find(edge.edge_id) != spectral_edge_list.end())
			{
				// If the spectral edge already exists, skip to the next edge
				const auto& existing_edge = spectral_edge_list.at(edge.edge_id);
				if (existing_edge.leftfaceid == quad_elm.quad_id)
				{
					// Same direction
					edge_node_ids[i] = existing_edge.edge_internal_node_ids;
				}
				else
				{
					// reverse the direction before adding
					edge_node_ids[i] = std::vector<int>(existing_edge.edge_internal_node_ids.rbegin(),
						existing_edge.edge_internal_node_ids.rend());

				}

				continue;
			}

			// Get the start and end nodes of the edge to flip based on the direction
			int startnodeid = -1;
			int endnodeid = -1;
			int leftfaceid = -1;
			int rightfaceid = -1;

			if (edge.leftfaceid == quad_elm.quad_id)
			{
				// Same direction
				startnodeid = edge.startnodeid;
				endnodeid = edge.endnodeid;
				leftfaceid = edge.leftfaceid;
				rightfaceid = edge.rightfaceid;
			}
			else
			{
				// Flip direction
				startnodeid = edge.endnodeid;
				endnodeid = edge.startnodeid;
				leftfaceid = edge.rightfaceid;
				rightfaceid = edge.leftfaceid;
			}

			// Get the start and end nodes of the edge
			const node_store& start_node = linear_mesh.node_list.at(startnodeid);
			const node_store& end_node = linear_mesh.node_list.at(endnodeid);

			std::vector<int> edge_internal_node_ids; // To store internal node IDs for this edge

			bool edge_isboundarynode = false;
			bool edge_isFieldBC = false;
			double edge_startnode_fieldvalue = 0.0;
			double edge_endnode_fieldvalue = 0.0;

			double edge_startnode_sourcevalue = 0.0;
			double edge_endnode_sourcevalue = 0.0;

			if (start_node.isboundarynode == true && end_node.isboundarynode == true)
			{
				edge_isboundarynode = true;

				// If both start and end nodes are boundary nodes 
				// then distribute the boundary condition to the edge nodes
				if (start_node.isFieldBC == true && end_node.isFieldBC == true)
				{
					edge_isFieldBC = true;

					// Get the field value
					edge_startnode_fieldvalue = start_node.fieldvalue;
					edge_endnode_fieldvalue = end_node.fieldvalue;
				}

				// Get the source value
				edge_startnode_sourcevalue = start_node.sourcevalue;
				edge_endnode_sourcevalue = end_node.sourcevalue;
			}


			// Create edge nodes based on the spectral order
			for (int j = 1; j < spectral_order; j++)
			{

				double xi = gll_locations[j];
				int node_id = node_id_control.get_unique_id(); // Get a unique node ID

				double x = 0.5 * ((1 - xi) * start_node.x_coord +
					(1 + xi) * end_node.x_coord);

				double y = 0.5 * ((1 - xi) * start_node.y_coord +
					(1 + xi) * end_node.y_coord);

				// Linear interpolation for field value (if edge has field BC)
				double edge_node_fieldvalue = 0.0;
				double edge_node_sourcevalue = 0.0;

				if (edge_isboundarynode == true)
				{
					if (edge_isFieldBC == true)
					{
						// Linear interpolation between start and end field values
						edge_node_fieldvalue = 0.5 * ((1 - xi) * edge_startnode_fieldvalue +
							(1 + xi) * edge_endnode_fieldvalue);
					}

					// Linear interpolation for source value
					edge_node_sourcevalue = 0.5 * ((1 - xi) * edge_startnode_sourcevalue +
						(1 + xi) * edge_endnode_sourcevalue);
				}


				create_spectral_nodes(node_id,
					x, y, edge_isboundarynode, edge_isFieldBC, edge_node_fieldvalue, edge_node_sourcevalue); // Create edge node and store it

				edge_internal_node_ids.push_back(node_id); // Add to this edge's internal node IDs
				edge_node_ids[i].push_back(node_id); // Add to edge node IDs

			}

			// Add spectral edge to the spectral edge list
			create_spectral_edges(edge, startnodeid, endnodeid, leftfaceid, rightfaceid, edge_internal_node_ids);
			//
		}



		// Corner nodes

		// Node 1 (corner) // [-1,-1]
		corner_node_ids.push_back(n1.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n1.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n1.node_id,
				n1.x_coord, n1.y_coord, n1.isboundarynode, n1.isFieldBC,
				n1.fieldvalue, n1.sourcevalue); // Create corner node and store it
		}
		//


		// Node 2 (corner)// [-1,1]
		corner_node_ids.push_back(n2.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n2.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n2.node_id,
				n2.x_coord, n2.y_coord, n2.isboundarynode, n2.isFieldBC,
				n2.fieldvalue, n2.sourcevalue); // Create corner node and store it

		}
		//


		// Node 3 (corner)// [1,1]
		corner_node_ids.push_back(n3.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n3.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n3.node_id,
				n3.x_coord, n3.y_coord, n3.isboundarynode, n3.isFieldBC,
				n3.fieldvalue, n3.sourcevalue); // Create corner node and store it
		}
		//

		// Node 4 (corner)// [1,-1]
		corner_node_ids.push_back(n4.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n4.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n4.node_id,
				n4.x_coord, n4.y_coord, n4.isboundarynode, n4.isFieldBC,
				n4.fieldvalue, n4.sourcevalue); // Create corner node and store it
		}
		//


		// Create the internal nodes for the quadrilateral element using bilinear mapping
		for (int i = 1; i < spectral_order; i++)
		{
			for (int j = 1; j < spectral_order; j++)
			{

				double xi = gll_locations[j];
				double eta = gll_locations[i];

				// Bilinear mapping
				double x = 0.25 * (
					(1 - xi) * (1 - eta) * n1.x_coord +
					(1 + xi) * (1 - eta) * n2.x_coord +
					(1 + xi) * (1 + eta) * n3.x_coord +
					(1 - xi) * (1 + eta) * n4.x_coord
					);

				double y = 0.25 * (
					(1 - xi) * (1 - eta) * n1.y_coord +
					(1 + xi) * (1 - eta) * n2.y_coord +
					(1 + xi) * (1 + eta) * n3.y_coord +
					(1 - xi) * (1 + eta) * n4.y_coord
					);

				// Internal nodes
				// Create spectral internal node and store it
				int node_id = node_id_control.get_unique_id(); // Get a unique node ID
				create_spectral_nodes(node_id,
					x, y, false, false, 0.0, 0.0); // Create internal node and store it

				internal_node_ids.push_back(node_id); // Add to internal node ID

			}
			//
		}

		// Store spectral quad element
		spectral_quadelement_store spec_quad;

		spec_quad.quad_id = quad_elm.quad_id;
		spec_quad.materialid = quad_elm.materialid;

		// Assigne the edge ids
		spec_quad.edge_ids[0] = edge1_id;
		spec_quad.edge_ids[1] = edge2_id;
		spec_quad.edge_ids[2] = edge3_id;
		spec_quad.edge_ids[3] = edge4_id;

		spec_quad.corner_nodes = corner_node_ids;
		spec_quad.edge_node_ids = edge_node_ids;
		spec_quad.internal_nodes = internal_node_ids;

		// Create the renderer nodes
		create_renderer_nodes(added_nodes, corner_node_ids, edge_node_ids, internal_node_ids);

		// Create the renderer triangle
		create_spectralquad_renderer_triangles(spec_quad);

		// Create the rendere edges
		std::vector<int> edge_ids = { edge1_id, edge2_id, edge3_id, edge4_id };
		create_renderer_edges(added_edges, corner_node_ids, edge_node_ids,
			internal_node_ids, edge_ids, spec_quad.renderer_tri_elements);


		spectral_quadelement_list[spec_quad.quad_id] = spec_quad;
		//
	}

	// End of quadrilateral element loop




	// TRIANGLE ELEMENTS

	for (const auto& tri : linear_mesh.trielement_list)
	{
		const trielement_store& tri_elm = tri.second;
		// Get the corner node IDs of the triangle element
		int nd1_id = tri_elm.nodeid1; // Node id 1
		int nd2_id = tri_elm.nodeid2; // Node id 2
		int nd3_id = tri_elm.nodeid3; // Node id 3
		// Get the material ID of the triangle element
		int material_id = tri_elm.materialid;

		// Generate spectral nodes, edges, and elements for the triangle element
		// Nodes are stored in the counter-clockwise order (nd1, nd2, nd3)
		const node_store& n1 = linear_mesh.node_list.at(nd1_id);
		const node_store& n2 = linear_mesh.node_list.at(nd2_id);
		const node_store& n3 = linear_mesh.node_list.at(nd3_id);

		// get the Four edge ids of the triangle element
		int edge1_id = get_edge_id(nd1_id, nd2_id); // Edge 1
		int edge2_id = get_edge_id(nd2_id, nd3_id); // Edge 2
		int edge3_id = get_edge_id(nd3_id, nd1_id); // Edge 3


		// Get the edges of the triangle element
		const edge_store& e1 = linear_mesh.edge_list.at(edge1_id); // Edge 1
		const edge_store& e2 = linear_mesh.edge_list.at(edge2_id); // Edge 2
		const edge_store& e3 = linear_mesh.edge_list.at(edge3_id); // Edge 3


		std::vector<int> corner_node_ids;
		std::vector<std::vector<int>> edge_node_ids{ 3 }; // To be filled with edge node IDs
		std::vector<int> internal_node_ids; // To be filled with internal node IDs


		// Create the edge nodes
		for (int i = 0; i < 3; i++)
		{
			// edge_store edge;
			const edge_store& edge = (i == 0) ? e1 : (i == 1) ? e2 : e3;

			// Reuse existing edge
			if (spectral_edge_list.find(edge.edge_id) != spectral_edge_list.end())
			{
				// If the spectral edge already exists, skip to the next edge
				const auto& existing_edge = spectral_edge_list.at(edge.edge_id);
				if (existing_edge.leftfaceid == tri_elm.tri_id)
				{
					// Same direction
					edge_node_ids[i] = existing_edge.edge_internal_node_ids;
				}
				else
				{
					// reverse the direction before adding
					edge_node_ids[i] = std::vector<int>(existing_edge.edge_internal_node_ids.rbegin(),
						existing_edge.edge_internal_node_ids.rend());

				}
				continue;
			}

			// Get the start and end nodes of the edge to flip based on the direction
			int startnodeid = -1;
			int endnodeid = -1;
			int leftfaceid = -1;
			int rightfaceid = -1;

			if (edge.leftfaceid == tri_elm.tri_id)
			{
				// Same direction
				startnodeid = edge.startnodeid;
				endnodeid = edge.endnodeid;
				leftfaceid = edge.leftfaceid;
				rightfaceid = edge.rightfaceid;
			}
			else
			{
				// Flip direction
				startnodeid = edge.endnodeid;
				endnodeid = edge.startnodeid;
				leftfaceid = edge.rightfaceid;
				rightfaceid = edge.leftfaceid;
			}

			const node_store& start_node = linear_mesh.node_list.at(startnodeid);
			const node_store& end_node = linear_mesh.node_list.at(endnodeid);

			std::vector<int> edge_internal_node_ids; // To store internal node IDs for this edge

			bool edge_isboundarynode = false;
			bool edge_isFieldBC = false;
			double edge_startnode_fieldvalue = 0.0;
			double edge_endnode_fieldvalue = 0.0;

			double edge_startnode_sourcevalue = 0.0;
			double edge_endnode_sourcevalue = 0.0;

			if (start_node.isboundarynode == true && end_node.isboundarynode == true)
			{
				edge_isboundarynode = true;

				// If both start and end nodes are boundary nodes 
				// then distribute the boundary condition to the edge nodes
				if (start_node.isFieldBC == true && end_node.isFieldBC == true)
				{
					edge_isFieldBC = true;

					// Get the field value
					edge_startnode_fieldvalue = start_node.fieldvalue;
					edge_endnode_fieldvalue = end_node.fieldvalue;
				}

				// Get the source value
				edge_startnode_sourcevalue = start_node.sourcevalue;
				edge_endnode_sourcevalue = end_node.sourcevalue;
			}


			// Create edge nodes based on the spectral order
			for (int j = 1; j < spectral_order; j++)
			{

				double xi = gll_locations[j];
				int node_id = node_id_control.get_unique_id(); // Get a unique node ID

				double x = 0.5 * ((1 - xi) * start_node.x_coord +
					(1 + xi) * end_node.x_coord);

				double y = 0.5 * ((1 - xi) * start_node.y_coord +
					(1 + xi) * end_node.y_coord);

				// Linear interpolation for field value (if edge has field BC)
				double edge_node_fieldvalue = 0.0;
				double edge_node_sourcevalue = 0.0;

				if (edge_isboundarynode == true)
				{
					if (edge_isFieldBC == true)
					{
						// Linear interpolation between start and end field values
						edge_node_fieldvalue = 0.5 * ((1 - xi) * edge_startnode_fieldvalue +
							(1 + xi) * edge_endnode_fieldvalue);
					}

					// Linear interpolation for source value
					edge_node_sourcevalue = 0.5 * ((1 - xi) * edge_startnode_sourcevalue +
						(1 + xi) * edge_endnode_sourcevalue);
				}


				create_spectral_nodes(node_id,
					x, y, edge_isboundarynode, edge_isFieldBC, edge_node_fieldvalue, edge_node_sourcevalue); // Create edge node and store it

				edge_internal_node_ids.push_back(node_id); // Add to this edge's internal node IDs
				edge_node_ids[i].push_back(node_id); // Add to edge node IDs

			}

			// Add spectral edge to the spectral edge list
			create_spectral_edges(edge, startnodeid, endnodeid, leftfaceid, rightfaceid, edge_internal_node_ids);
			//
		}

		// Create the corner nodes
		 // Corner nodes

		//_____________________________________________________________________________
		// Node 1 (corner)
		corner_node_ids.push_back(n1.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n1.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n1.node_id,
				n1.x_coord, n1.y_coord, n1.isboundarynode, n1.isFieldBC,
				n1.fieldvalue, n1.sourcevalue); // Create corner node and store it
		}
		//
		//_____________________________________________________________________________
		// Node 2 (corner)
		corner_node_ids.push_back(n2.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n2.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n2.node_id,
				n2.x_coord, n2.y_coord, n2.isboundarynode, n2.isFieldBC,
				n2.fieldvalue, n2.sourcevalue); // Create corner node and store it

		}
		//
		//_____________________________________________________________________________
		// Node 3 (corner)
		corner_node_ids.push_back(n3.node_id); // Add to corner node IDs

		// Check if the spectral node already exists to avoid duplicates
		if (spectral_node_list.find(n3.node_id) == spectral_node_list.end())
		{
			// If the spectral node does not exists, create it and store it
			create_spectral_nodes(n3.node_id,
				n3.x_coord, n3.y_coord, n3.isboundarynode, n3.isFieldBC,
				n3.fieldvalue, n3.sourcevalue); // Create corner node and store it
		}
		//



		// Create the internal nodes for the triangular element  
		// IMA Journal of Applied Mathematics Advance Access published March 16, 2005   
		// A Lobatto interpolation grid over the triangle   
		// M.G. Blyth and C. Pozrikidis

		for (int i = 1; i < spectral_order; i++)
		{
			for (int j = 1; j < spectral_order - i; j++)
			{
				// Internal nodes
				int k = spectral_order - i - j;

				double vi = (gll_locations[i] + 1.0) * 0.5;
				double vj = (gll_locations[j] + 1.0) * 0.5;
				double vk = (gll_locations[k] + 1.0) * 0.5;

				// Find the double xi and eta (local coordinates)
				double xi = (1 / 3.0) * (1.0 + (2.0 * vj) - vi - vk);
				double eta = (1 / 3.0) * (1.0 + (2.0 * vi) - vk - vj);

				// Converted to global coordinates
				double l1 = 1.0 - xi - eta;
				double l2 = xi;
				double l3 = eta;

				int node_id = node_id_control.get_unique_id();

				double x = l1 * n1.x_coord + l2 * n2.x_coord + l3 * n3.x_coord;
				double y = l1 * n1.y_coord + l2 * n2.y_coord + l3 * n3.y_coord;

				create_spectral_nodes(node_id, x, y, false, false, 0.0, 0.0);

				internal_node_ids.push_back(node_id);
				//
			}
			//
		}

		// Store spectral tri element
		spectral_trielement_store spec_tri;

		spec_tri.tri_id = tri_elm.tri_id;
		spec_tri.materialid = tri_elm.materialid;

		// Assigne the edge ids
		spec_tri.edge_ids[0] = edge1_id;
		spec_tri.edge_ids[1] = edge2_id;
		spec_tri.edge_ids[2] = edge3_id;

		spec_tri.corner_nodes = corner_node_ids;
		spec_tri.edge_node_ids = edge_node_ids;
		spec_tri.internal_nodes = internal_node_ids;

		// Create the renderer nodes
		create_renderer_nodes(added_nodes, corner_node_ids, edge_node_ids, internal_node_ids);

		// Create the renderer triangle
		create_spectraltri_renderer_triangles(spec_tri);

		// Create the rendere edges
		std::vector<int> edge_ids = { edge1_id, edge2_id, edge3_id };
		create_renderer_edges(added_edges, corner_node_ids, edge_node_ids,
			internal_node_ids, edge_ids, spec_tri.renderer_tri_elements);


		spectral_trielement_list[spec_tri.tri_id] = spec_tri;
		//
	}
	// End of triangular element loop

	//
}






int spectral_mesh2d::get_edge_id(const int& startnodeid, const int& endnodeid)
{

	// Get the connected edges to start node
	const std::vector<int>& connected_edges = this->linear_mesh.node_edge_map[startnodeid];

	for (const int& edge_id : connected_edges)
	{
		const auto& edge = this->linear_mesh.edge_list[edge_id];
		if ((edge.startnodeid == startnodeid && edge.endnodeid == endnodeid) ||
			(edge.startnodeid == endnodeid && edge.endnodeid == startnodeid))
		{
			// Line with the same start and end nodes
			return edge_id;
		}

	}

	return -1;

}



void spectral_mesh2d::create_spectral_nodes(int node_id,
	double x_coord,
	double y_coord,
	bool isboundarynode,
	bool isFieldBC,
	double fieldvalue,
	double sourcevalue)
{

	// Create spectral node and store it
	spectral_node_store spec_node;
	spec_node.node_id = node_id; // Get a unique node ID
	spec_node.x_coord = x_coord;
	spec_node.y_coord = y_coord;

	spec_node.isboundarynode = isboundarynode;
	spec_node.isFieldBC = isFieldBC;
	spec_node.fieldvalue = fieldvalue;
	spec_node.sourcevalue = sourcevalue;

	spectral_node_list[spec_node.node_id] = spec_node; // Store the node
	//
}



void spectral_mesh2d::create_spectral_edges(edge_store edge,
	const int& startnodeid,
	const int& endnodeid,
	const int& leftfaceid,
	const int& rightfaceid,
	const std::vector<int>& edge_internal_node_ids)
{
	// Create spectral edge and store it
	spectral_edge_store spec_edge;

	spec_edge.edge_id = edge.edge_id;
	spec_edge.startnodeid = startnodeid;
	spec_edge.endnodeid = endnodeid;

	spec_edge.edge_internal_node_ids = edge_internal_node_ids;

	spec_edge.leftfaceid = leftfaceid;
	spec_edge.rightfaceid = rightfaceid;

	spec_edge.isboundaryedge = edge.isboundaryedge;
	spec_edge.isSommerfieldBC = edge.isSommerfieldBC;
	spec_edge.isFieldBC = edge.isFieldBC;
	spec_edge.isDerivFieldBC = edge.isDerivFieldBC;
	spec_edge.fieldvalue = edge.fieldvalue;
	spec_edge.normalderivfieldvalue = edge.normalderivfieldvalue;

	spectral_edge_list[spec_edge.edge_id] = spec_edge; // Store the edge
	//
}



void spectral_mesh2d::create_spectralquad_renderer_triangles(spectral_quadelement_store& spec_quad)
{
	// Get the spectral order
	int order = this->spectral_order;

	// Layer count 
	int layer_count = order + 1;

	// Clear the row ordered nodes
	spec_quad.row_ordered_node_ids.clear();


	auto create_renderer_triangles = [this](const std::vector<int>& layer_0, const std::vector<int>& layer_1,
		spectral_quadelement_store& spec_quad)
		{
			// layer_1    c -- d
			//            |    |
			// layer_0    a -- b

			int count = static_cast<int>(layer_0.size());

			for (int i = 0; i < count - 1; i++)
			{
				renderer_triangle tri1{
					layer_0[i],
					layer_0[i + 1],
					layer_1[i]
				};

				spec_quad.renderer_tri_elements.push_back(tri1);
				this->renderer_element_triangles.push_back(tri1);

				renderer_triangle tri2{
					layer_0[i + 1],
					layer_1[i + 1],
					layer_1[i]
				};

				spec_quad.renderer_tri_elements.push_back(tri2);
				this->renderer_element_triangles.push_back(tri2);
			}

			for (const auto& id : layer_0)
			{
				spec_quad.row_ordered_node_ids.push_back(id);
			}
			//
		};

	// Set the first layer nodes
	std::vector<int> layer_0_nodes;

	layer_0_nodes.push_back(spec_quad.corner_nodes[0]);

	for (const auto& edge0_id : spec_quad.edge_node_ids[0])
	{

		// First edge nodes (Node 0 -> Node 1)
		layer_0_nodes.push_back(edge0_id);
	}

	layer_0_nodes.push_back(spec_quad.corner_nodes[1]);


	int interior_node_index = 0;
	std::vector<int> layer_1_nodes;


	for (int i = 1; i < order; i++)
	{
		layer_1_nodes.clear();

		// Start is edge 4 node (Node 3 -> Node 0)
		layer_1_nodes.push_back(spec_quad.edge_node_ids[3][order - i - 1]);

		// Interior nodes layer 1
		for (int j = 0; j < order - 1; j++)
		{
			layer_1_nodes.push_back(spec_quad.internal_nodes[interior_node_index]);

			interior_node_index++;
		}

		// End is edge 2 node (Node 1 -> Node 2)
		layer_1_nodes.push_back(spec_quad.edge_node_ids[1][i - 1]);

		// Using layer_0 and layer 1 create the triangles
		create_renderer_triangles(layer_0_nodes, layer_1_nodes, spec_quad);

		layer_0_nodes = layer_1_nodes;
		//
	}




	// Final layer is the final corner node
	layer_1_nodes.clear();

	layer_1_nodes.push_back(spec_quad.corner_nodes[3]);

	// Add in reverse
	for (const auto& edge2_id : std::vector<int>(spec_quad.edge_node_ids[2].rbegin(), spec_quad.edge_node_ids[2].rend()))
	{

		// First edge nodes (Node 1 -> Node 2)
		layer_1_nodes.push_back(edge2_id);
	}

	layer_1_nodes.push_back(spec_quad.corner_nodes[2]);


	// Using layer_0 and layer 1 create the triangles
	create_renderer_triangles(layer_0_nodes, layer_1_nodes, spec_quad);


	// Final layer for row ordered node ids
	for (const auto& id : layer_1_nodes)
	{
		spec_quad.row_ordered_node_ids.push_back(id);
	}

	//
}



void spectral_mesh2d::create_spectraltri_renderer_triangles(spectral_trielement_store& spec_tri)
{
	// Get the spectral order
	int order = this->spectral_order;

	// Layer count 
	// int layer_count = order + 1;


	auto create_renderer_triangles = [this](const std::vector<int>& layer_0, const std::vector<int>& layer_1,
		spectral_trielement_store& spec_tri)
		{
			// layer_1    c -- d
			//            |    |
			// layer_0    a -- b

			int count = static_cast<int>(layer_0.size());

			for (int i = 0; i < count - 1; i++)
			{
				renderer_triangle tri1{
					layer_0[i],
					layer_0[i + 1],
					layer_1[i]
				};

				spec_tri.renderer_tri_elements.push_back(tri1);
				this->renderer_element_triangles.push_back(tri1);

				if (i < count - 2)
				{
					renderer_triangle tri2{
						layer_0[i + 1],
						layer_1[i + 1],
						layer_1[i]
					};

					spec_tri.renderer_tri_elements.push_back(tri2);
					this->renderer_element_triangles.push_back(tri2);
				}
			}
			//
		};


	// Set the first layer nodes
	std::vector<int> layer_0_nodes;

	layer_0_nodes.push_back(spec_tri.corner_nodes[0]);

	for (const auto& edge0_id : spec_tri.edge_node_ids[0])
	{

		// First edge nodes (Node 0 -> Node 1) [Edge 0]
		layer_0_nodes.push_back(edge0_id);
	}

	layer_0_nodes.push_back(spec_tri.corner_nodes[1]);


	int interior_node_index = 0;
	std::vector<int> layer_1_nodes;

	//if (order > 1)
	//{
	for (int i = 1; i < order; i++)
	{
		layer_1_nodes.clear();

		// Start is edge node (Node 2 -> Node 0) [Edge 2]
		layer_1_nodes.push_back(spec_tri.edge_node_ids[2][order - i - 1]);

		// Interior nodes layer 1
		for (int j = 0; j < order - i - 1; j++)
		{
			layer_1_nodes.push_back(spec_tri.internal_nodes[interior_node_index]);

			interior_node_index++;
		}

		// End is edge node (Node 1 -> Node 2) [Edge 1]
		layer_1_nodes.push_back(spec_tri.edge_node_ids[1][i - 1]);

		// Using layer_0 and layer 1 create the triangles
		create_renderer_triangles(layer_0_nodes, layer_1_nodes, spec_tri);

		layer_0_nodes = layer_1_nodes;
		//
	}
	//}

	// Final layer is the final corner node
	layer_1_nodes.clear();
	layer_1_nodes.push_back(spec_tri.corner_nodes[2]);

	// Using layer_0 and layer 1 create the triangles
	create_renderer_triangles(layer_0_nodes, layer_1_nodes, spec_tri);


	//____________________________________________________________________________________________________________
	// Create the node ordering
	spec_tri.lexi_ordered_node_ids.clear();

	// Corner nodes
	spec_tri.lexi_ordered_node_ids.push_back(spec_tri.corner_nodes[0]);
	spec_tri.lexi_ordered_node_ids.push_back(spec_tri.corner_nodes[1]);
	spec_tri.lexi_ordered_node_ids.push_back(spec_tri.corner_nodes[2]);

	// Edge nodes
	for (int i = 0; i < 3; i++)
	{
		for (const auto& edge_nd_id : spec_tri.edge_node_ids[i])
		{

			// edge nodes (Node 0 -> Node 1), (Node 1 -> Node 2), (Node 2 -> Node 3)
			spec_tri.lexi_ordered_node_ids.push_back(edge_nd_id);
		}
	}

	// Interior nodes
	for (const auto& interior_nd_id : spec_tri.internal_nodes)
	{
		spec_tri.lexi_ordered_node_ids.push_back(interior_nd_id);
	}



	//
}




void spectral_mesh2d::create_renderer_nodes(std::unordered_set<int>& added_nodes,
	const std::vector<int>& corner_nodes,
	const std::vector<std::vector<int>>& edge_node_ids,
	const std::vector<int>& internal_nodes)
{
	// --- Corner nodes ---
	// Add the corner nodes
	for (const auto& c_node : corner_nodes)
	{
		if (added_nodes.insert(c_node).second)
		{
			// get the spectral node from the Node id
			const auto& node = spectral_node_list.at(c_node);

			renderer_node r_node;
			r_node.n_id = c_node;
			r_node.x = node.x_coord;
			r_node.y = node.y_coord;

			renderer_node_points.push_back(r_node);
		}
	}

	// --- Edge nodes ---
	// Add the edge node
	for (const auto& e_node_list : edge_node_ids)
	{
		for (const auto& e_node : e_node_list)
		{
			if (added_nodes.insert(e_node).second)
			{
				// get the spectral node from the Node id
				const auto& node = spectral_node_list.at(e_node);

				renderer_node r_node;
				r_node.n_id = e_node;
				r_node.x = node.x_coord;
				r_node.y = node.y_coord;

				renderer_node_points.push_back(r_node);
			}
		}
	}

	// --- Internal nodes (already unique) ---
	// Add the internal nodes
	for (const auto& i_node : internal_nodes)
	{
		// Internal nodes are unique and no need to check for existing
		// get the spectral node from the Node id
		const auto& node = spectral_node_list.at(i_node);

		renderer_node r_node;
		r_node.n_id = i_node;
		r_node.x = node.x_coord;
		r_node.y = node.y_coord;

		renderer_node_points.push_back(r_node);
	}
	//
}




void spectral_mesh2d::create_renderer_edges(std::unordered_set<int>& added_edges,
	const std::vector<int>& corner_nodes,
	const std::vector<std::vector<int>>& edge_node_ids,
	const std::vector<int>& internal_nodes,
	const std::vector<int>& edge_ids,
	const std::vector<renderer_triangle>& renderer_tri_elements)
{

	struct EdgeHash
	{
		size_t operator()(const renderer_edge& e) const
		{
			return std::hash<int>()(e.nstart) ^ (std::hash<int>()(e.nend) << 1);
		}
	};

	// Store the local edge
	std::unordered_set<renderer_edge, EdgeHash> edge_set;

	auto make_edge = [](int a, int b)
		{
			return (a < b) ? renderer_edge{ a, b } : renderer_edge{ b, a };
		};


	// --- 1. Boundary edges ---
	for (int i = 0; i < 3; i++)
	{
		int start = corner_nodes[i];
		int end = corner_nodes[(i + 1) % 3];

		int current = start;

		if (added_edges.insert(edge_ids[i]).second)
		{
			for (const auto& interior_node : edge_node_ids[i])
			{
				renderer_edge e = make_edge(current, interior_node);

				if (edge_set.insert(e).second)
				{
					renderer_edge_lines.push_back({ current, interior_node });
				}

				current = interior_node;
			}

			// close edge
			renderer_edge e = make_edge(current, end);

			if (edge_set.insert(e).second)
			{
				renderer_edge_lines.push_back({ current, end });
			}
		}
	}


	// --- 2. Internal triangle edges ---
	for (const auto& tri : renderer_tri_elements)
	{
		renderer_edge e1 = make_edge(tri.n1, tri.n2);
		renderer_edge e2 = make_edge(tri.n2, tri.n3);
		renderer_edge e3 = make_edge(tri.n3, tri.n1);

		if (edge_set.insert(e1).second)
			renderer_edge_lines.push_back({ tri.n1, tri.n2 });

		if (edge_set.insert(e2).second)
			renderer_edge_lines.push_back({ tri.n2, tri.n3 });

		if (edge_set.insert(e3).second)
			renderer_edge_lines.push_back({ tri.n3, tri.n1 });
	}
	//
}



void spectral_mesh2d::create_local_id_structure(int order)
{

	quad_element_id_structure.clear();
	tri_element_id_structure.clear();


	// Create the quadrilateral element ID structure
	int idx = 0;
	int n_edge_nodes = order - 1;
	int n_internal = (order - 1) * (order - 1);

	// === Bottom edge (corner 0 -> edge nodes -> corner 1) ===
	// Corner 0
	quad_element_id_structure.corner_nodes.push_back(idx++);

	for (int i = 0; i < n_edge_nodes; i++)
	{

		// First edge nodes (Node 0 -> Node 1) // Edge 0
		quad_element_id_structure.edge_node_ids[0].push_back(idx++);
	}

	// Corner 1
	quad_element_id_structure.corner_nodes.push_back(idx++);


	// === Internal rows (each row has edge nodes on left/right) ===
	for (int row = 0; row < n_edge_nodes; row++)
	{
		// Left edge node (edge 3, from bottom to top)
		// Start is edge 3 node (Node 3 -> Node 0)
		quad_element_id_structure.edge_node_ids[3].push_back(idx++);

		// Interior nodes in this row
		for (int col = 0; col < n_edge_nodes; col++)
		{
			quad_element_id_structure.internal_nodes.push_back(idx++);
		}

		// Right edge node (edge 1, from bottom to top)
		quad_element_id_structure.edge_node_ids[1].push_back(idx++);
	}


	// === Top edge (corner 2 -> edge nodes -> corner 3) ===
	// Corner 2
	quad_element_id_structure.corner_nodes.push_back(idx++);

	for (int j = 0; j < order - 1; j++)
	{
		// Note: Edge 2 nodes go from right to left or left to right?
		// Third edge (edge[2]) nodes (Node 4 -> Node 3)
		quad_element_id_structure.edge_node_ids[2].push_back(idx++);
	}

	// Corner 3
	quad_element_id_structure.corner_nodes.push_back(idx++);



	//__________________________________________________________________________________________________
	//__________________________________________________________________________________________________
	// Create the triangle element ID structure
	idx = 0;

	// Corner nodes
	tri_element_id_structure.corner_nodes.push_back(idx++);
	tri_element_id_structure.corner_nodes.push_back(idx++);
	tri_element_id_structure.corner_nodes.push_back(idx++);

	// Edge nodes
	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < n_edge_nodes; j++)
		{
			// edge nodes (Node 0 -> Node 1), (Node 1 -> Node 2), (Node 2 -> Node 3)
			tri_element_id_structure.edge_node_ids[i].push_back(idx++);
		}
	}

	n_internal = (order - 2) * (order - 1) * 0.5;

	// Interior nodes
	for (int j = 0; j < n_internal; j++)
	{
		tri_element_id_structure.internal_nodes.push_back(idx++);
	}

	//
}






