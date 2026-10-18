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

			double norm_wave_speed = spec_mesh2d.material_list[material_id].norm_wave_speed; // get the normalized material wave speed
			element_k_matrix = (norm_wave_speed * norm_wave_speed) * element_k_matrix;

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

			double norm_wave_speed = spec_mesh2d.material_list[material_id].norm_wave_speed; // get the normalized material wave speed
			element_k_matrix = (norm_wave_speed * norm_wave_speed) * element_k_matrix;


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

