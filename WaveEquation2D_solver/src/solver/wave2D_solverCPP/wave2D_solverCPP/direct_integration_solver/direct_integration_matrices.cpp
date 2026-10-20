#include "direct_integration_matrices.h"

direct_integration_matrices::direct_integration_matrices()
{
	// Empty constructor

}


void direct_integration_matrices::init(wave2d_system_store * wave_2dsystem_ptr, 
	stopwatch_events * stopwatch, void(*callback)(const char*))
{
	// Set the initialized system ptr
	this->wave_2dsystem_ptr = wave_2dsystem_ptr;

	// Set the stopwatch
	this->m_stopwatch = stopwatch;


	// Store callback locally
	this->m_callback = callback;

}


void direct_integration_matrices::create_global_matrices()
{

	// Create the spectral mesh
	wave2d_system_store wave_2dsystem = (*this->wave_2dsystem_ptr);

	// Create the spectral mesh
	this->spec_mesh2d.generate_spectral_mesh(wave_2dsystem);

	report("Spectral mesh created");



	// Set the number of DOF
	this->numDOF = static_cast<int>(spec_mesh2d.spectral_node_list.size());

	// Global k Matrix [ke]
	global_k_matrix.resize(numDOF, numDOF);
	global_k_matrix.setZero();

	// Global m Matrix [Me]
	global_m_matrix.resize(numDOF, numDOF);
	global_m_matrix.setZero();

	// Global boundary condition Vector (To track the nodes where prescribed field is applied)
	global_dirichlet_BC_flags_vector.setZero(numDOF);

	// get the quadrature points and the basis term
	int spectral_order = spec_mesh2d.spectral_order;

	std::vector<Eigen::Triplet<double>> k_triplets;
	std::vector<Eigen::Triplet<double>> m_triplets;



	if (static_cast<int>(spec_mesh2d.spectral_trielement_list.size()) > 0)
	{
		this->triangle_quadrature_points = spectral_tri_element::get_triangle_quadrature(spectral_order);
		this->triangle_basis_terms = spectral_tri_element::proriol_modes(spectral_order);

		report("Triangle Element Quadrature Points Created");

		this->inv_vandermonde_matrix = spectral_tri_element::get_inverse_vandermonde_matrix(spectral_order, this->triangle_basis_terms);

		// Callback the rank and conditioning of inverse Vandermonde matrix
		report_vandermondematrix_conditioning(this->inv_vandermonde_matrix);

		// Get the gll locations and gll weights for the given spectral order 
		this->gll_locations = gll_utility::get_gll_locations(spectral_order);
		this->gll_weights = gll_utility::get_gll_weights(spectral_order, gll_locations);


		// Triangle elements
		for (auto& tri_elm_m : spec_mesh2d.spectral_trielement_list)
		{
			// get the element
			const spectral_trielement_store& tri_elm = tri_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
			std::vector<int> elem_nodes = tri_elm.lexi_ordered_node_ids;


			// Get node coordinates ________________________________________________
			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list.at(nid);
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}


			//________________________________________________________________________________________________
			// Step 2: Create element k_grad matrix
			int nen = static_cast<int>(elem_nodes.size());

			Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
			Eigen::MatrixXd element_m_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element mass matrix


			get_trielement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
				element_k_matrix, element_m_matrix);

			int material_id = tri_elm.materialid;

			if (material_id == 0)
			{
				// Perfectly matched layer (PML) material, set the normalized wave speed to default material id 1
				material_id = 1;
			}

			double wave_speed = spec_mesh2d.material_list[material_id].material_wave_speed; // get the normalized material wave speed
			element_k_matrix = (wave_speed * wave_speed) * element_k_matrix;

			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag

			get_trielement_field_vector(tri_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 4: Set the global matrix and global vector

			set_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_m_matrix,
				k_triplets,
				m_triplets);



			set_global_BC_flag_vector(elem_nodes, nen,
				element_field_BC_flag_vector, global_dirichlet_BC_flags_vector);

		}

		report("Triangle Spectral Elements Global Matrices Created");
		//
	}


	if (static_cast<int>(spec_mesh2d.spectral_quadelement_list.size()) > 0)
	{
		this->quadrilateral_quadrature_points = spectral_quad_element::get_quadrilateral_quadrature(spectral_order);

		// Get the gll locations and gll weights for the given spectral order 
		this->gll_locations = gll_utility::get_gll_locations(spectral_order);
		this->gll_weights = gll_utility::get_gll_weights(spectral_order, gll_locations);

		report("Quadrilateral Element Quadrature Points Created");

		// Quadrilateral elements
		for (auto& quad_elm_m : spec_mesh2d.spectral_quadelement_list)
		{
			// get the element
			const spectral_quadelement_store& quad_elm = quad_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
			std::vector<int> elem_nodes = quad_elm.row_ordered_node_ids;

			// Get node coordinates ________________________________________________
			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list.at(nid);
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}


			//________________________________________________________________________________________________
			// Step 2: Create element k_grad matrix
			int nen = static_cast<int>(elem_nodes.size());

			Eigen::MatrixXd element_k_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k grad matrix
			Eigen::MatrixXd element_m_matrix = Eigen::MatrixXd::Zero(nen, nen); // Element k mass matrix


			get_quadelement_k_grad_k_mass_matrix(elem_nodes, elem_coords,
				element_k_matrix, element_m_matrix);


			int material_id = quad_elm.materialid;

			if (material_id == 0)
			{
				// Perfectly matched layer (PML) material, set the normalized wave speed to default material id 1
				material_id = 1;
			}

			double wave_speed = spec_mesh2d.material_list[material_id].material_wave_speed; // get the normalized material wave speed
			element_k_matrix = (wave_speed * wave_speed) * element_k_matrix;


			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag

			get_quadelement_field_vector(quad_elm, element_field_BC_flag_vector);


			//________________________________________________________________________________________________
			// Step 4: Set the global matrix and global vector

			set_global_matrix(elem_nodes, nen,
				element_k_matrix,
				element_m_matrix,
				k_triplets,
				m_triplets);


			set_global_BC_flag_vector(elem_nodes, nen,
				element_field_BC_flag_vector, global_dirichlet_BC_flags_vector);

		}

		report("Quadrilateral Spectral Elements Global Matrices Created");
		//
	}

	//__________________________________________________________________________________________________________
	// Set the global k and m matrix
	global_k_matrix.setFromTriplets(k_triplets.begin(), k_triplets.end());
	global_m_matrix.setFromTriplets(m_triplets.begin(), m_triplets.end());

	// Create the message string and convert to const char*
	std::string sizeMsg = "Global K matrix created. Size: " +
		std::to_string(global_k_matrix.rows()) + "x" +
		std::to_string(global_k_matrix.cols()) +
		", Non-zeros: " + std::to_string(global_k_matrix.nonZeros());

	report(sizeMsg.c_str());

	sizeMsg = "Global M matrix created. Size: " +
		std::to_string(global_m_matrix.rows()) + "x" +
		std::to_string(global_m_matrix.cols()) +
		", Non-zeros: " + std::to_string(global_m_matrix.nonZeros());

	report(sizeMsg.c_str());

	//
}


void direct_integration_matrices::create_initialcondition_load_vector()
{

	// Global initial field Vector [d0]
	global_initial_field_vector.resize(numDOF);
	global_initial_field_vector.setZero();

	// Global initial derivative field Vector [v0]
	global_initial_derivfield_vector.resize(numDOF);
	global_initial_derivfield_vector.setZero();

	Eigen::VectorXi initial_BC_fieldcount_vector(numDOF);
	Eigen::VectorXi initial_BC_derivfieldcount_vector(numDOF);

	// Clear the load maps
	load_maps.clear();


	if (static_cast<int>(spec_mesh2d.spectral_trielement_list.size()) > 0)
	{

		// Triangle elements
		for (auto& tri_elm_m : spec_mesh2d.spectral_trielement_list)
		{
			// get the element
			const spectral_trielement_store& tri_elm = tri_elm_m.second;


			// Get node coordinates ________________________________________________
			std::vector<int> elem_nodes = tri_elm.lexi_ordered_node_ids;

			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list[nid];
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}


			//________________________________________________________________________________________________
			// Element field vector

			get_trielement_field_vector(tri_elm, elem_nodes, global_initial_field_vector, initial_BC_fieldcount_vector);

			//________________________________________________________________________________________________
			// Element normal derivative field vector

			get_trielement_normderivfield_vector(tri_elm, elem_nodes, elem_coords, global_initial_derivfield_vector, initial_BC_derivfieldcount_vector);


			//________________________________________________________________________________________________
			// Element source term vector

			get_trielement_source_vector(tri_elm, elem_nodes, load_maps);

		}

	}


	if (static_cast<int>(spec_mesh2d.spectral_quadelement_list.size()) > 0)
	{

		// Quadrilateral elements
		for (auto& quad_elm_m : spec_mesh2d.spectral_quadelement_list)
		{
			// get the element
			const spectral_quadelement_store& quad_elm = quad_elm_m.second;

			//________________________________________________________________________________________________
			// Step 1: Create local node & node coordinate list
			// Build local node list _______________________________________________
			std::vector<int> elem_nodes = quad_elm.row_ordered_node_ids;

			// Get node coordinates ________________________________________________
			std::vector<Eigen::Vector2d> elem_coords;

			for (int nid : elem_nodes)
			{
				const auto& node = spec_mesh2d.spectral_node_list.at(nid);
				elem_coords.emplace_back(node.x_coord, node.y_coord);
			}



			//________________________________________________________________________________________________
			// Element field vector

			get_quadelement_field_vector(quad_elm, elem_nodes, global_initial_field_vector, initial_BC_fieldcount_vector);

			//________________________________________________________________________________________________
			// Element normal derivative field vector

			get_quadelement_normderivfield_vector(quad_elm, elem_nodes, elem_coords, global_initial_derivfield_vector, initial_BC_derivfieldcount_vector);


			//________________________________________________________________________________________________
			// Element source term vector

			get_quadelement_source_vector(quad_elm, elem_nodes, load_maps);

		}

	}

	// Normalize the global initial field vector and global initial derivative field vector
	for (int i = 0; i < numDOF; i++)
	{
		if (initial_BC_fieldcount_vector(i) > 0)
		{
			global_initial_field_vector(i) /= initial_BC_fieldcount_vector(i);
		}
		if (initial_BC_derivfieldcount_vector(i) > 0)
		{
			global_initial_derivfield_vector(i) /= initial_BC_derivfieldcount_vector(i);
		}
	}
//
}



void direct_integration_matrices::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_matrix,
	Eigen::MatrixXd& element_m_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// Get quadrature points
	const auto& quadrature_points = this->triangle_quadrature_points;

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(quadrature_points.size()); q++)
	{
		double quadraturept_xi = quadrature_points[q].xi;
		double quadraturept_eta = quadrature_points[q].eta;
		double wt = quadrature_points[q].weight; // weights are normalized to 1.0

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
		Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

		spectral_tri_element::evaluate_triangle_shape_functions(quadraturept_xi, quadraturept_eta, spec_mesh2d.spectral_order,
			this->inv_vandermonde_matrix, this->triangle_basis_terms,
			N, dN_dxi, dN_deta);


		// --- 3. Compute Jacobian ---
		Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

		for (int i = 0; i < nen; i++)
		{
			double x = elem_coords[i].x();
			double y = elem_coords[i].y();

			J(0, 0) += dN_dxi(i) * x; // dx/dxi
			J(0, 1) += dN_deta(i) * x; // dx/deta
			J(1, 0) += dN_dxi(i) * y; // dy/dxi
			J(1, 1) += dN_deta(i) * y; // dy/deta
		}

		double detJ = J.determinant();
		Eigen::Matrix2d invJ = J.inverse();

		// --- 4. Transform gradients ---
		// [dN/dx; dN/dy] = invJ^T * [dN/dxi; dN/deta]
		Eigen::MatrixXd dN_dx(nen, 2);  // Each row: [dN_i/dx, dN_i/dy]

		for (int i = 0; i < nen; i++)
		{
			// Gradient in reference coordinates
			Eigen::Vector2d grad_ref(dN_dxi(i), dN_deta(i));

			// Transform to physical coordinates
			Eigen::Vector2d grad_phys = invJ.transpose() * grad_ref;

			dN_dx(i, 0) = grad_phys(0); // dN_i/dx
			dN_dx(i, 1) = grad_phys(1); // dN_i/dy
		}

		// Quadrature weight including Jacobian determinant
		double dV = detJ * wt;


		// --- 5. Assemble matrices ---
		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				double grad_dot = dN_dx.row(i).dot(dN_dx.row(j));

				// Gradient (stiffness)
				element_k_matrix(i, j) += grad_dot * dV;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * dV;
			}
		}
	}
	//
}




void direct_integration_matrices::get_trielement_field_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	// Edge Boundary Condition 
	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true && edge.fieldvalue == 0.0)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];

			dirichlet_BC_flag(local_idx) = 1;
		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = tri_elm.corner_nodes;

	for (int i = 0; i < 3; i++)
	{
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		// Corner 1
		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			if (nd1.isFieldBC == true && nd1.fieldvalue == 0.0)
			{
				// Apply field value at the node
				dirichlet_BC_flag(local_idx) = 1;
			}
		}

		// edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.tri_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : tri_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				int local_idx = edge_node_local_ids[j];

				if (edge_nd.isFieldBC == true && edge_nd.fieldvalue == 0.0)
				{
					// Apply field value at the node
					dirichlet_BC_flag(local_idx) = 1;
				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 3]];
		if (nd2.isboundarynode == true)
		{
			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];
			if (nd2.isFieldBC == true && nd2.fieldvalue == 0.0)
			{
				// Apply field value at the node
				dirichlet_BC_flag(local_idx) = 1;
			}
		}


	}
	//
}




//________________________________________________________________________________________________

void direct_integration_matrices::get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::MatrixXd& element_k_matrix,
	Eigen::MatrixXd& element_m_matrix)
{
	int nen = static_cast<int>(elem_nodes.size());

	// Get quadrature points
	const auto& quadrature_points = this->quadrilateral_quadrature_points;

	// --- 1. Loop over quadrature points ---
	for (int q = 0; q < static_cast<int>(quadrature_points.size()); q++)
	{
		double quadraturept_xi = quadrature_points[q].xi;
		double quadraturept_eta = quadrature_points[q].eta;
		double wt = quadrature_points[q].weight;

		// --- 2. Evaluate shape functions ---
		Eigen::VectorXd N(nen);
		Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
		Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

		spectral_quad_element::evaluate_quadrilateral_shape_functions(quadraturept_xi, quadraturept_eta,
			spec_mesh2d.spectral_order, gll_locations,
			N, dN_dxi, dN_deta);

		// --- 3. Compute Jacobian ---
		Eigen::Matrix2d J = Eigen::Matrix2d::Zero();

		for (int i = 0; i < nen; i++)
		{
			double x = elem_coords[i].x();
			double y = elem_coords[i].y();

			J(0, 0) += dN_dxi(i) * x; // dx/dxi
			J(0, 1) += dN_deta(i) * x; // dx/deta
			J(1, 0) += dN_dxi(i) * y; // dy/dxi
			J(1, 1) += dN_deta(i) * y; // dy/deta
		}

		double detJ = J.determinant();
		Eigen::Matrix2d invJ = J.inverse();

		// --- 4. Transform gradients ---
		Eigen::MatrixXd dN_dx(nen, 2);

		for (int i = 0; i < nen; i++)
		{
			// Gradient in reference coordinates
			Eigen::Vector2d grad_ref(dN_dxi(i), dN_deta(i));

			// Transform to physical coordinates
			Eigen::Vector2d grad_phys = invJ.transpose() * grad_ref;

			dN_dx(i, 0) = grad_phys(0); // dN_i/dx
			dN_dx(i, 1) = grad_phys(1); // dN_i/dy
		}

		// Quadrature weight including Jacobian determinant
		double dV = detJ * wt;

		// --- 5. Assemble matrices ---
		// element_k_grad_matrix += (dN_dx * dN_dx.transpose()) * detJ * w;
		// element_k_mass_matrix += (N * N.transpose()) * detJ * w;

		for (int i = 0; i < nen; i++)
		{
			for (int j = 0; j < nen; j++)
			{
				double grad_dot = dN_dx.row(i).dot(dN_dx.row(j));

				// Gradient (stiffness)
				element_k_matrix(i, j) += grad_dot * dV;

				// Mass
				element_m_matrix(i, j) += (N(i) * N(j)) * dV;
			}
		}
	}
	//
}





void direct_integration_matrices::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	// Edge Boundary Condition 
	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true && edge.fieldvalue == 0.0)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];

			dirichlet_BC_flag(local_idx) = 1;

		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		// Corner 1
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			if (nd1.isFieldBC == true && nd1.fieldvalue == 0.0)
			{
				// Apply field value at the node
				dirichlet_BC_flag(local_idx) = 1;
			}
		}

		// Edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.quad_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : quad_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				int local_idx = edge_node_local_ids[j];

				if (edge_nd.isFieldBC == true && edge_nd.fieldvalue == 0.0)
				{
					// Apply field value at the node
					dirichlet_BC_flag(local_idx) = 1;
				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 4]];

		if (nd2.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];

			if (nd2.isFieldBC == true && nd2.fieldvalue == 0.0)
			{
				// Apply field value at the node
				dirichlet_BC_flag(local_idx) = 1;
			}
		}

	}
	//
}




//________________________________________________________________________________________________


void direct_integration_matrices::set_global_matrix(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::MatrixXd& element_k_matrix,
	const Eigen::MatrixXd& element_m_matrix,
	std::vector<Eigen::Triplet<double>>& k_triplets,
	std::vector<Eigen::Triplet<double>>& m_triplets)
{

	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = elem_nodes[i];

		for (int j = 0; j < nen; j++)
		{
			// get the global map id
			int j_node_map = elem_nodes[j];

			double k_val = element_k_matrix(i, j);
			double m_val = element_m_matrix(i, j);


			k_triplets.emplace_back(i_node_map, j_node_map, k_val);
			m_triplets.emplace_back(i_node_map, j_node_map, m_val);

			// Note: Triplets don’t accumulate — Eigen accumulates when building the sparse matrix.
		}
	}
	//

}



void direct_integration_matrices::set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector)
{
	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = elem_nodes[i];

		global_BC_flag_vector(i_node_map) = element_BC_flag_vector(i);
	}
	//
}



void direct_integration_matrices::report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix)
{
	std::stringstream ss;

	// Compute condition number using singular value decomposition
	Eigen::JacobiSVD<Eigen::MatrixXd> svd(invVanderMondematrix);
	Eigen::VectorXd singular_values = svd.singularValues();

	double max_sv = singular_values(0);
	double min_sv = singular_values(singular_values.size() - 1);
	double cond_number = max_sv / min_sv;
	double log10_cond = std::log10(cond_number);

	// Compute numerical rank (singular values > tolerance)
	double tolerance = std::max(invVanderMondematrix.rows(), invVanderMondematrix.cols()) *
		singular_values(0) * std::numeric_limits<double>::epsilon();

	int rank = 0;
	for (int i = 0; i < singular_values.size(); ++i)
	{
		if (singular_values(i) > tolerance)
		{
			rank++;
		}
	}

	// Compute determinant (log scale to avoid overflow)
	double log_det = 0.0;
	for (int i = 0; i < rank; ++i)
	{
		log_det += std::log(singular_values(i));
	}


	int rows = static_cast<int>(invVanderMondematrix.rows());
	int cols = static_cast<int>(invVanderMondematrix.cols());

	// Additional statistics
	double min_sv_db = 20.0 * std::log10(min_sv);
	double max_sv_db = 20.0 * std::log10(max_sv);
	double dynamic_range_db = max_sv_db - min_sv_db;

	ss << "\n========================================";
	ss << "\n" << "Inverse VanderMonde Matrix Analysis : ";
	ss << "\n  Dimensions: " << rows << "x" << cols;
	ss << "\n  Rank: " << rank << "/" << std::min(rows, cols);
	ss << "\n  Condition number: " << std::scientific << std::setprecision(6) << cond_number;
	ss << " (10^" << std::fixed << std::setprecision(2) << log10_cond << ")";
	ss << "\n  Singular values:";
	ss << "\n    Max: " << std::scientific << std::setprecision(4) << max_sv;
	ss << " (" << std::fixed << std::setprecision(1) << max_sv_db << " dB)";
	ss << "\n    Min: " << std::scientific << std::setprecision(4) << min_sv;
	ss << " (" << std::fixed << std::setprecision(1) << min_sv_db << " dB)";
	ss << "\n    Dynamic range: " << std::fixed << std::setprecision(1) << dynamic_range_db << " dB";
	ss << "\n  Log determinant: " << std::scientific << std::setprecision(4) << log_det;




	// Warning for ill-conditioned matrix
	if (cond_number > 1e8)
	{
		ss << " [WARNING: Matrix is ill-conditioned!]";
	}
	else
	{
		ss << "\n  [OK] Matrix is well-conditioned";
	}
	ss << "\n========================================";


	report(ss.str().c_str());

}



//_______________________________________________________________________________________________________________________


void direct_integration_matrices::get_trielement_field_vector(const spectral_trielement_store& tri_elm,
	const std::vector<int>& elem_nodes,
	Eigen::VectorXd& initial_field_vector,
	Eigen::VectorXi& initial_BC_fieldcount_vector)
{
	int local_idx = -1;
	int global_idx = -1;


	// Edge Boundary Condition 
	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true && edge.fieldvalue != 0.0)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			initial_field_vector(global_idx) += edge.fieldvalue;
			initial_BC_fieldcount_vector(global_idx) += 1;

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;
				global_idx = elem_nodes[local_idx];

				initial_field_vector(global_idx) += edge.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;

			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];
			global_idx = elem_nodes[local_idx];

			initial_field_vector(global_idx) += edge.fieldvalue;
			initial_BC_fieldcount_vector(global_idx) += 1;

		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = tri_elm.corner_nodes;

	for (int i = 0; i < 3; i++)
	{
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		// Corner 1
		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			if (nd1.isFieldBC == true && nd1.fieldvalue != 0.0)
			{
				// Apply field value at the node
				initial_field_vector(global_idx) += nd1.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;

			}
		}

		// edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.tri_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : tri_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				local_idx = edge_node_local_ids[j];
				global_idx = elem_nodes[local_idx];

				if (edge_nd.isFieldBC == true && edge_nd.fieldvalue != 0.0)
				{
					// Apply field value at the node
					initial_field_vector(global_idx) += edge_nd	.fieldvalue;
					initial_BC_fieldcount_vector(global_idx) += 1;

				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 3]];
		if (nd2.isboundarynode == true)
		{
			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];
			global_idx = elem_nodes[local_idx];

			if (nd2.isFieldBC == true && nd2.fieldvalue != 0.0)
			{
				// Apply field value at the node
				initial_field_vector(global_idx) += nd2.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;
			}
		}
	}
	//
}



void direct_integration_matrices::get_trielement_normderivfield_vector(const spectral_trielement_store& tri_elm,
	const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::VectorXd& initial_derivfield_vector,
	Eigen::VectorXi& initial_BC_derivfieldcount_vector)
{
	int nen = static_cast<int>(elem_nodes.size());

	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isDerivFieldBC == true)
		{

			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds)
				{
					if (i == 0)
					{      // Bottom edge
						xi = s;  eta = 0.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1)
					{ // Diagonal edge
						xi = 1.0 - s;  eta = s;
						dxi_ds = -1.0;  deta_ds = 1.0;
					}
					else
					{             // Left edge
						xi = 0.0;  eta = 1.0 - s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};



			for (int j = 0; j < n_gll_points; j++)
			{
				// Map the 1D GLL point to [0, 1]
				double s = (this->gll_locations[j] + 1.0) * 0.5;

				// Get the weigths
				double wt = this->gll_weights[j] * 0.5;

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);


				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen); // [dN/dxi, dN/deta]
				Eigen::VectorXd dN_deta(nen); // [dN/dxi, dN/deta]

				spectral_tri_element::evaluate_triangle_shape_functions(xi, eta, spec_mesh2d.spectral_order,
					this->inv_vandermonde_matrix, this->triangle_basis_terms,
					N, dN_dxi, dN_deta);


				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * wt;

				// --- Assembly ---
				// Neumann (flux) BC contribution
				double dq_edge = edge.normalderivfieldvalue;  // flux value

				for (int a = 0; a < nen; a++)
				{
					double neumann_vector_value = dq_edge * N(a) * dV;

					int local_idx = a;
					int global_idx = elem_nodes[local_idx];

					initial_derivfield_vector(global_idx) += neumann_vector_value;
					initial_BC_derivfieldcount_vector(global_idx) += 1;
				}
			}
		}
	}
	//
}



void direct_integration_matrices::get_trielement_source_vector(const spectral_trielement_store& tri_elm,
	const std::vector<int>& elem_nodes,
	std::unordered_map<int, nodal_load_store>& load_maps)
{
	int local_idx = -1;
	int global_idx = -1;

	auto add_sources = [&](int global_idx,
		const std::unordered_map<int, load_vector_store>& src)
		{
			if (src.empty()) return;
			auto& lm = load_maps[global_idx];
			lm.node_id = global_idx;
			for (const auto& [set_id, lv] : src)
			{
				lm.add_loads(set_id, lv);

				// Policy: same set_id at same node should assert.
				// auto [it, inserted] = lm.source_values.try_emplace(set_id, lv);
				//if (!inserted) 
				//{
				//	// either accumulate, or assert:
				//	// it->second.sourceamplitude += lv.sourceamplitude;
				//	assert(false && "duplicate source set on node");
				//}
			}
		};

	// Edge Boundary Condition 
	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isSource == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			add_sources(global_idx, edge.source_values);

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;
				global_idx = elem_nodes[local_idx];

				add_sources(global_idx, edge.source_values);

			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];
			global_idx = elem_nodes[local_idx];

			add_sources(global_idx, edge.source_values);

		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = tri_elm.corner_nodes;

	for (int i = 0; i < 3; i++)
	{
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		// Corner 1
		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			if (nd1.isSource == true)
			{
				// Apply source value at the node
				add_sources(global_idx, nd1.source_values);
			}
		}

		// edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.tri_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : tri_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				local_idx = edge_node_local_ids[j];
				global_idx = elem_nodes[local_idx];

				if (edge_nd.isSource == true)
				{
					// Apply source value at the node
					add_sources(global_idx, edge_nd.source_values);
				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 3]];
		if (nd2.isboundarynode == true)
		{
			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];
			global_idx = elem_nodes[local_idx];

			if (nd2.isSource == true)
			{
				// Apply source value at the node
				add_sources(global_idx, nd2.source_values);
			}
		}
	}
	//

}


void direct_integration_matrices::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	const std::vector<int>& elem_nodes,
	Eigen::VectorXd& initial_field_vector,
	Eigen::VectorXi& initial_BC_fieldcount_vector)
{
	int local_idx = -1;
	int global_idx = -1;

	// Edge Boundary Condition 
	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true && edge.fieldvalue != 0.0)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			initial_field_vector(global_idx) += edge.fieldvalue;
			initial_BC_fieldcount_vector(global_idx) += 1;

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;
				global_idx = elem_nodes[local_idx];

				initial_field_vector(global_idx) += edge.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;
			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];
			global_idx = elem_nodes[local_idx];

			initial_field_vector(global_idx) += edge.fieldvalue;
			initial_BC_fieldcount_vector(global_idx) += 1;

		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		// Corner 1
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			if (nd1.isFieldBC == true && nd1.fieldvalue != 0.0)
			{
				// Apply field value at the node
				initial_field_vector(global_idx) += nd1.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;
			}
		}

		// Edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.quad_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : quad_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				local_idx = edge_node_local_ids[j];
				global_idx = elem_nodes[local_idx];

				if (edge_nd.isFieldBC == true && edge_nd.fieldvalue != 0.0)
				{
					// Apply field value at the node
					initial_field_vector(global_idx) += edge_nd.fieldvalue;
					initial_BC_fieldcount_vector(global_idx) += 1;
				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 4]];

		if (nd2.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];
			global_idx = elem_nodes[local_idx];

			if (nd2.isFieldBC == true && nd2.fieldvalue != 0.0)
			{
				// Apply field value at the node
				initial_field_vector(global_idx) += nd2.fieldvalue;
				initial_BC_fieldcount_vector(global_idx) += 1;
			}
		}

	}
	//
}



void direct_integration_matrices::get_quadelement_normderivfield_vector(const spectral_quadelement_store& quad_elm,
	const std::vector<int>& elem_nodes,
	const std::vector<Eigen::Vector2d>& elem_coords,
	Eigen::VectorXd& initial_derivfield_vector,
	Eigen::VectorXi& initial_BC_derivfieldcount_vector)
{
	int nen = static_cast<int>(elem_nodes.size());

	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isDerivFieldBC == true)
		{
			int n_gll_points = spec_mesh2d.spectral_order + 1;

			// Pre-compute edge mapping parameters
			auto get_edge_coordinates = [i](double s, double& xi, double& eta,
				double& dxi_ds, double& deta_ds)
				{
					if (i == 0)
					{      // Edge 1 (-1,-1) to (1,-1) bottom edge
						xi = s;  eta = -1.0;
						dxi_ds = 1.0;  deta_ds = 0.0;
					}
					else if (i == 1)
					{	// Edge 2 (1,-1) to (1, 1) right edge
						xi = 1.0;  eta = s;
						dxi_ds = 0.0;  deta_ds = 1.0;
					}
					else if (i == 2)
					{  // Edge 3 (1,1) to (-1,1) top edge
						xi = -s;  eta = 1.0;
						dxi_ds = -1.0;  deta_ds = 0.0;
					}
					else
					{  // Edge 4 (-1,1) to (-1,-1) left edge
						xi = -1.0;  eta = -s;
						dxi_ds = 0.0;  deta_ds = -1.0;
					}
				};


			for (int j = 0; j < n_gll_points; j++)
			{
				// Get the 1D GLL point [-1, 1]
				double s = this->gll_locations[j];

				// Get the weigths
				double wt = this->gll_weights[j];

				double xi, eta, dxi_ds, deta_ds;
				get_edge_coordinates(s, xi, eta, dxi_ds, deta_ds);

				// --- Shape functions ---
				Eigen::VectorXd N(nen);
				Eigen::VectorXd dN_dxi(nen);
				Eigen::VectorXd dN_deta(nen);

				spectral_quad_element::evaluate_quadrilateral_shape_functions(xi, eta, spec_mesh2d.spectral_order, gll_locations,
					N, dN_dxi, dN_deta);

				// --- Compute physical edge length Jacobian ---
				Eigen::Vector2d dx_ds = Eigen::Vector2d::Zero();

				for (int k = 0; k < nen; k++)
				{
					double dN_ds = dN_dxi(k) * dxi_ds + dN_deta(k) * deta_ds;

					dx_ds += dN_ds * elem_coords[k];
				}

				double J_edge = dx_ds.norm();
				double dV = J_edge * wt;

				// --- Assembly ---
				// Neumann (flux) BC contribution
				double dq_edge = edge.normalderivfieldvalue;  // flux value

				for (int a = 0; a < nen; a++)
				{
					double neumann_vector_value = dq_edge * N(a) * dV;

					int local_idx = a;
					int global_idx = elem_nodes[local_idx];

					initial_derivfield_vector(global_idx) += neumann_vector_value;
					initial_BC_derivfieldcount_vector(global_idx) += 1;

				}
			}
		}
	}
	//
}


void direct_integration_matrices::get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
	const std::vector<int>& elem_nodes,
	std::unordered_map<int, nodal_load_store>& load_maps)
{
	int local_idx = -1;
	int global_idx = -1;

	auto add_sources = [&](int global_idx,
		const std::unordered_map<int, load_vector_store>& src)
		{
			if (src.empty()) return;
			auto& lm = load_maps[global_idx];
			lm.node_id = global_idx;
			for (const auto& [set_id, lv] : src)
			{
				lm.add_loads(set_id, lv);

				// Policy: same set_id at same node should assert.
				// auto [it, inserted] = lm.source_values.try_emplace(set_id, lv);
				//if (!inserted) 
				//{
				//	// either accumulate, or assert:
				//	// it->second.sourceamplitude += lv.sourceamplitude;
				//	assert(false && "duplicate source set on node");
				//}
			}
		};

	// Edge Boundary Condition 
	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isSource == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			add_sources(global_idx, edge.source_values);

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;
				global_idx = elem_nodes[local_idx];

				add_sources(global_idx, edge.source_values);

			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];
			global_idx = elem_nodes[local_idx];

			add_sources(global_idx, edge.source_values);

		}
	}
	// ___________________________________________________

	// Node Boundary Condition
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		// Corner 1
		const spectral_node_store& nd1 = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd1.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];
			global_idx = elem_nodes[local_idx];

			if (nd1.isSource == true)
			{
				// Apply field value at the node
				add_sources(global_idx, nd1.source_values);
			}
		}

		// Edge nodes
		const std::vector<int>& edge_node_local_ids = spec_mesh2d.quad_element_id_structure.edge_node_ids[i];
		int j = 0;
		for (const int& edge_nd_id : quad_elm.edge_node_ids[i])
		{

			const spectral_node_store& edge_nd = spec_mesh2d.spectral_node_list[edge_nd_id];

			if (edge_nd.isboundarynode == true)
			{
				local_idx = edge_node_local_ids[j];
				global_idx = elem_nodes[local_idx];

				if (edge_nd.isSource == true)
				{
					// Apply field value at the node
					add_sources(global_idx, edge_nd.source_values);
				}
			}

			j++;
		}

		// Corner 2
		const spectral_node_store& nd2 = spec_mesh2d.spectral_node_list[corner_nodes[(i + 1) % 4]];

		if (nd2.isboundarynode == true)
		{
			// int local_idx = (i * spec_mesh2d.spectral_order);
			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];
			global_idx = elem_nodes[local_idx];

			if (nd2.isSource == true)
			{
				// Apply field value at the node
				add_sources(global_idx, nd2.source_values);
			}
		}

	}
	//

}


const Eigen::VectorXd& direct_integration_matrices::getGlobalLoadVector(double time) const
{
	Eigen::VectorXd global_load_vector(numDOF);
	global_load_vector.setZero();

	for (const auto& [i, load_store] : load_maps)
	{
		double amplitude = load_store.get_load_amplitude_at_time(time);
		global_load_vector(i) += amplitude;
	}

	return global_load_vector;
}



void nodal_load_store::add_loads(int load_set_id, const load_vector_store& load_values)
{

	// Emplace the load values into the source_values map
	source_values.emplace(load_set_id, load_values);

}



double nodal_load_store::get_load_amplitude_at_time(double time) const
{
	// Source term types
	// 0 = Half sine pulse
	// 1 = Rectangular pulse
	// 2 = Triangular pulse
	// 3 = Step force with finite rise
	// 4 = Full sine pulse
	// 5 = Harmonic/ periodic excitation
	double source_amplitude = 0.0;

	for (const auto& [set_id, load] : source_values)
	{
		if (time < load.sourcestarttime)
		{
			continue; // Skip this load if the current time is before the load's start time
		}

		// Time since the load started
		double time_sst = time - load.sourcestarttime;
		double amplitude = load.sourceamplitude;
		double frequency = load.sourcefrequency;
		double duration = 1.0 / frequency;

		if (load.sourcetype == 0)
		{
			// Half sine pulse
			if (time_sst <= duration)
			{
				source_amplitude += amplitude * std::sin(M_PI * time_sst * frequency);
			}
		}
		else if (load.sourcetype == 1)
		{
			// Rectangular pulse
			if (time_sst <= duration)
			{
				source_amplitude += amplitude;
			}
		}
		else if (load.sourcetype == 2)
		{
			// Triangular pulse
			if (time_sst <= duration)
			{
				source_amplitude += amplitude * (1.0 - std::abs(2.0 * time_sst / duration - 1.0));
			}
		}
		else if (load.sourcetype == 3)
		{
			// Step force with finite rise
			if (time_sst <= duration)
			{
				source_amplitude += amplitude * (time_sst / duration);
			}
			else
			{
				source_amplitude += amplitude;
			}
		}
		else if (load.sourcetype == 4)
		{
			// Full sine pulse
			if (time_sst <= duration)
			{
				source_amplitude += amplitude * std::sin(2.0 * M_PI * frequency * time_sst);
			}
		}
		else if (load.sourcetype == 5)
		{
			// Harmonic/ periodic excitation
			source_amplitude += amplitude * std::sin(2.0 * M_PI * frequency * time_sst);

		}
	}

	return source_amplitude;
}



void direct_integration_matrices::report(const char* msg)
{
	std::stringstream stopwatch_elapsed_str;

	stopwatch_elapsed_str << std::fixed << std::setprecision(6)
		<< this->m_stopwatch->elapsed();

	std::string final_msg = std::string(msg) + " at " +
		stopwatch_elapsed_str.str() +
		" secs";

	if (m_callback)
		m_callback(final_msg.c_str());
	//
}

