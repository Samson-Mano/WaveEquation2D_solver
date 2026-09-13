#include "modal_spectral_solver.h"

modal_spectral_solver::modal_spectral_solver()
{
	// Empty constructor
}



void modal_spectral_solver::init(wave2d_system_store* wave_2dsystem_ptr,
	const char* output_file_char, stopwatch_events* stopwatch, void(*callback)(const char*))
{
	// Set the initialized system ptr
	this->wave_2dsystem_ptr = wave_2dsystem_ptr;

	// Set the stopwatch
	this->m_stopwatch = stopwatch;


	// Store callback locally
	this->m_callback = callback;

	// Store the output file name
	// CRITICAL: Copy the string to std::string for permanent storage
	this->output_file = std::string(output_file_char);

	std::string msg = "Output file set to: " + this->output_file;
	report(msg.c_str());


}



void modal_spectral_solver::create_global_matrices()
{
	// Create the spectral mesh
	wave2d_system_store wave_2dsystem = (*this->wave_2dsystem_ptr);

	// Create the spectral mesh
	this->spec_mesh2d.generate_spectral_mesh(wave_2dsystem);

	report("Spectral mesh created");


	// Create a node ID map (to create a nodes as ordered and numbered from 0,1,2...n)
	int i = 0;
	for (auto& nd : this->spec_mesh2d.spectral_node_list)
	{
		nodeid_map[nd.second.node_id] = i;
		i++;
	}


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
			spectral_trielement_store tri_elm = tri_elm_m.second;

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


			// double wave_speed = spec_mesh2d.material_list[tri_elm.materialid].wave_speed; // get the material wave speed
			// element_k_matrix = (wave_speed * wave_speed) * element_k_matrix;

			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			// Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector


			get_trielement_field_vector(tri_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 4: Create Element source vector
			// Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_trielement_source_vector(tri_elm, element_field_BC_flag_vector);


			//________________________________________________________________________________________________
			// Step 5: Set the global matrix and global vector

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
			spectral_quadelement_store quad_elm = quad_elm_m.second;

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


			// double wave_speed = spec_mesh2d.material_list[quad_elm.materialid].wave_speed; // get the material wave speed
			// element_k_matrix = (wave_speed * wave_speed) * element_k_matrix;


			//________________________________________________________________________________________________
			// Step 3: Create Element field vector
			Eigen::VectorXi element_field_BC_flag_vector = Eigen::VectorXi::Zero(nen); // Element field vector BC flag
			// Eigen::VectorXd element_field_vector = Eigen::VectorXd::Zero(nen); // Element field vector

			get_quadelement_field_vector(quad_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 4: Create Element source vector
			// Eigen::VectorXd element_source_vector = Eigen::VectorXd::Zero(nen); // Element source vector

			get_quadelement_source_vector(quad_elm, element_field_BC_flag_vector);

			//________________________________________________________________________________________________
			// Step 5: Set the global matrix and global vector

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




bool modal_spectral_solver::solve_modal_analysis(int inpt_num_modes, int solver_type)
{

	auto start_time = std::chrono::high_resolution_clock::now();


	// Find the fixed and free nodes dof
	std::vector<int> free_dofs;
	std::vector<int> fixed_dofs;

	for (int i = 0; i < this->numDOF; ++i)
	{
		if (global_dirichlet_BC_flags_vector(i))
			fixed_dofs.push_back(i);
		else
			free_dofs.push_back(i);
	}

	// Map free DOF to Local indices
	std::unordered_map<int, int> free_map;

	for (int i = 0; i < free_dofs.size(); ++i)
	{
		free_map[free_dofs[i]] = i;
	}


	// 2) Extract reduced matrices Kff, Mff
	int n_free = static_cast<int>(free_dofs.size());
	int num_modes = std::min(inpt_num_modes, n_free);

	std::string msg = "Number of free DOFs: " + std::to_string(n_free);
	report(msg.c_str());
	msg = "Number of modes to compute: " + std::to_string(num_modes);
	report(msg.c_str());

	if (num_modes == 0)
	{
		report("Warning: No free DOFs found!");
		return false;
	}


	// Reserve space for triplets (estimate ~20 entries per DOF)
	std::vector<Eigen::Triplet<double>> Kt, Mt;
	// Kt.reserve(n_free * 15);
	// Mt.reserve(n_free * 15);

	// Extract K_ff
	for (int k = 0; k < global_k_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_k_matrix, k); it; ++it)
		{
			int i = static_cast<int>(it.row());
			int j = static_cast<int>(it.col());

			// keep only free-free block
			if (global_dirichlet_BC_flags_vector(i) == 0 &&
				global_dirichlet_BC_flags_vector(j) == 0)
			{
				// Get the local indices of free nodes
				int ii = free_map[i];
				int jj = free_map[j];

				Kt.emplace_back(ii, jj, it.value());
			}
		}
	}

	// Extract M_ff
	for (int k = 0; k < global_m_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_m_matrix, k); it; ++it)
		{
			int i = static_cast<int>(it.row());
			int j = static_cast<int>(it.col());

			// keep only free-free block
			if (global_dirichlet_BC_flags_vector(i) == 0 &&
				global_dirichlet_BC_flags_vector(j) == 0)
			{
				// Get the local indices of free nodes
				int ii = free_map[i];
				int jj = free_map[j];

				Mt.emplace_back(ii, jj, it.value());
			}
		}
	}


	Eigen::SparseMatrix<double> K_ff(n_free, n_free);
	Eigen::SparseMatrix<double> M_ff(n_free, n_free);
	K_ff.setFromTriplets(Kt.begin(), Kt.end());
	M_ff.setFromTriplets(Mt.begin(), Mt.end());

	// Compress matrices for better performance
	K_ff.makeCompressed();
	M_ff.makeCompressed();

	report("Reduced matrices created successfully");

	// 3) Solve eigenvalue problem
	Eigen::VectorXd eigenvalues;
	Eigen::MatrixXd eigenvectors;

	bool isSolveSuccessful = false;

	if (solver_type == SOLVER_SPECTRA)
	{
		isSolveSuccessful = solveWithSpectra(num_modes, K_ff, M_ff, eigenvalues, eigenvectors);
	}
	else if (solver_type == SOLVER_ARPACK)
	{
		isSolveSuccessful = solveWithARPACK(num_modes, K_ff, M_ff, eigenvalues, eigenvectors);
	}

	if (!isSolveSuccessful)
	{
		report("Eigenvalue solver failed!");
		return false;
	}

	// 4) Convert eigenvalues to frequencies
	this->natural_frequencies.clear();
	// this->natural_frequencies.reserve(eigenvalues.size());

	for (int i = 0; i < eigenvalues.size(); ++i)
	{
		double lambda = eigenvalues[i];
		if (lambda > 1e-12)
		{  // Positive definite check
			double omega = std::sqrt(lambda);
			double freq = omega / (2.0 * M_PI);
			this->natural_frequencies.push_back(freq);
		}
		else
		{
			// Rigid body mode
			this->natural_frequencies.push_back(0.0);
		}
	}

	// 5) Normalize mode shapes (mass-normalized)
	for (int i = 0; i < eigenvectors.cols(); ++i)
	{
		Eigen::VectorXd phi = eigenvectors.col(i);
		double norm = std::sqrt(phi.transpose() * M_ff * phi);

		if (norm > 1e-12)
		{
			eigenvectors.col(i) /= norm;
		}
	}

	// 6) Reconstruct full mode shapes
	int total_dofs = static_cast<int>(global_k_matrix.rows());
	this->natural_modes.resize(total_dofs, eigenvectors.cols());
	this->natural_modes.setZero();

	for (int i = 0; i < n_free; ++i)
	{
		this->natural_modes.row(free_dofs[i]) = eigenvectors.row(i);
	}

	auto end_time = std::chrono::high_resolution_clock::now();
	auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
	msg = "Modal analysis completed in " + std::to_string(duration.count()) + " ms";
	report(msg.c_str());

	// Store results
	store_results_with_index();

	return true;
}





void modal_spectral_solver::get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
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




void modal_spectral_solver::get_trielement_field_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	for (int i = 0; i < 3; i++)
	{
		// Get the edge id
		int edge_id = tri_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution
			double q_edge = edge.fieldvalue;  // field value

			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.tri_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[(i + 1) % 3];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;
			//
		}
		//
	}
	//
}


void modal_spectral_solver::get_trielement_source_vector(const spectral_trielement_store& tri_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{
	// Get the corner nodes
	const std::vector<int>& corner_nodes = tri_elm.corner_nodes;

	for (int i = 0; i < 3; i++)
	{
		const spectral_node_store& nd = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd.isboundarynode == true)
		{
			int local_idx = spec_mesh2d.tri_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				// dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
			}
			else
			{
				// Apply source value at the node
				// source_vector(local_idx) = nd.sourcevalue;
			}
		}

	}
	//
}




//________________________________________________________________________________________________

void modal_spectral_solver::get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
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





void modal_spectral_solver::get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{

	for (int i = 0; i < 4; i++)
	{
		// Get the edge id
		int edge_id = quad_elm.edge_ids[i];

		const spectral_edge_store& edge = spec_mesh2d.spectral_edge_list[edge_id];

		if (edge.isboundaryedge == true && edge.isFieldBC == true)
		{
			// --- Assembly ---
			// Dirichlet (field) BC contribution
			double q_edge = edge.fieldvalue;  // field value

			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;

			for (const int& j : spec_mesh2d.quad_element_id_structure.edge_node_ids[i])
			{
				local_idx = j;

				// dirichlet_vector(local_idx) = q_edge;
				dirichlet_BC_flag(local_idx) = 1;
			}

			local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[(i + 1) % 4];

			// dirichlet_vector(local_idx) = q_edge;
			dirichlet_BC_flag(local_idx) = 1;
			//
		}
		//
	}
	//
}



void modal_spectral_solver::get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
	Eigen::VectorXi& dirichlet_BC_flag)
{
	// Get the corner nodes
	const std::vector<int>& corner_nodes = quad_elm.corner_nodes;

	for (int i = 0; i < 4; i++)
	{
		const spectral_node_store& nd = spec_mesh2d.spectral_node_list[corner_nodes[i]];

		if (nd.isboundarynode == true)
		{
			int local_idx = spec_mesh2d.quad_element_id_structure.corner_nodes[i];

			if (nd.isFieldBC == true)
			{
				// Apply field value at the node
				// dirichlet_vector(local_idx) = nd.fieldvalue;
				dirichlet_BC_flag(local_idx) = 1;
			}
			else
			{
				// Apply source value at the node
				// source_vector(local_idx) = nd.sourcevalue;
			}
		}

	}
	//
}






//________________________________________________________________________________________________


void modal_spectral_solver::set_global_matrix(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::MatrixXd& element_k_matrix,
	const Eigen::MatrixXd& element_m_matrix,
	std::vector<Eigen::Triplet<double>>& k_triplets,
	std::vector<Eigen::Triplet<double>>& m_triplets)
{

	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		for (int j = 0; j < nen; j++)
		{
			// get the global map id
			int j_node_map = this->nodeid_map[elem_nodes[j]];

			double k_val = element_k_matrix(i, j);
			double m_val = element_m_matrix(i, j);


			k_triplets.emplace_back(i_node_map, j_node_map, k_val);
			m_triplets.emplace_back(i_node_map, j_node_map, m_val);

			// Note: Triplets don’t accumulate — Eigen accumulates when building the sparse matrix.
		}
	}
	//

}



void modal_spectral_solver::set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
	int nen,
	const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector)
{
	for (int i = 0; i < nen; i++)
	{
		// get the global map id
		int i_node_map = this->nodeid_map[elem_nodes[i]];

		global_BC_flag_vector(i_node_map) = element_BC_flag_vector(i);
	}
	//
}




void modal_spectral_solver::report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix)
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



void modal_spectral_solver::report(const char* msg)
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




bool modal_spectral_solver::solveWithSpectra(int num_modes,
	const Eigen::SparseMatrix<double>& K_ff,
	const Eigen::SparseMatrix<double>& M_ff,
	Eigen::VectorXd& eigenvalues,
	Eigen::MatrixXd& eigenvectors)
{

	report("Solving with Spectra solver...");
	auto start_time = std::chrono::high_resolution_clock::now();

	int n = static_cast<int>(K_ff.rows());
	int ncv = std::min(2 * num_modes + 1, n);  // Number of Lanczos vectors

	// Method 1: Using Cholesky to convert to standard eigenvalue problem
	Eigen::SimplicialLLT<Eigen::SparseMatrix<double>> chol(M_ff);


	if (chol.info() != Eigen::Success)
	{
		auto end_time = std::chrono::high_resolution_clock::now();
		auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);

		std::string msg = "Mass matrix is not positive definite, Solver failed in " + std::to_string(duration.count()) + " ms";
		report(msg.c_str());

		return false;
		// throw std::runtime_error("Mass matrix is not positive definite");
	}

	// Create the operator for (M^{-1} * K)
	MinvKOp op(K_ff, chol);

	// Create the solver for standard eigenvalue problem
	// Note: Spectra::SymEigsSolver expects only the operator type
	Spectra::SymEigsSolver<MinvKOp> eigs(op, num_modes, ncv);

	// Initialize and compute
	eigs.init();

	// Compute eigenvalues (smallest algebraic values for lowest frequencies)
	int nconv = static_cast<int>(eigs.compute(Spectra::SortRule::SmallestAlge));

	if (eigs.info() == Spectra::CompInfo::Successful)
	{
		eigenvalues = eigs.eigenvalues();
		eigenvectors = eigs.eigenvectors();

		// Convert eigenvalues from standard to generalized
		// For standard problem: K*x = λ*M*x, we solved M^{-1}K*x = λ*x
		// So λ are the same
		//for (int i = 0; i < eigenvalues.size(); ++i) 
		//{
		//	if (eigenvalues(i) < 0) 
		//	{
		//		report("Warning: Negative eigenvalue detected");
		//	}
		//}
	}
	else
	{
		auto end_time = std::chrono::high_resolution_clock::now();
		auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);

		std::string msg = "Spectra solver failed to converge in " + std::to_string(duration.count()) + " ms";
		report(msg.c_str());

		return false;

		// throw std::runtime_error("Spectra solver failed to converge");
	}

	auto end_time = std::chrono::high_resolution_clock::now();
	auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
	std::string msg = "Spectra solver completed in " + std::to_string(duration.count()) + " ms";
	report(msg.c_str());

	return true;
}





bool modal_spectral_solver::solveWithARPACK(int num_modes,
	const Eigen::SparseMatrix<double>& K_ff,
	const Eigen::SparseMatrix<double>& M_ff,
	Eigen::VectorXd& eigenvalues,
	Eigen::MatrixXd& eigenvectors)
{
	report("Solving with ARPACK (sparse mode via Eigen wrapper)...");
	auto start_time = std::chrono::high_resolution_clock::now();

	int nev = num_modes; // std::min(static_cast<int>(K.rows()), 20); // Don't compute all eigenvalues! Use a reasonable number
	int ncv = std::min(2 * nev + 1, static_cast<int>(K_ff.rows())); // Number of Lanczos vectors

	// Validate inputs
	if (nev <= 0)
	{
		report("Number of modes must be positive");
		return false;
	}

	std::string solver_msg;

	// Matrix sizes
	solver_msg = "Kff size: " + std::to_string(K_ff.rows()) + " x " + std::to_string(K_ff.cols());
	report(solver_msg.c_str());

	solver_msg = "Mff size: " + std::to_string(M_ff.rows()) + " x " + std::to_string(M_ff.cols());
	report(solver_msg.c_str());

	// Eigenvalue info
	solver_msg = "Computing " + std::to_string(nev) + " eigenvalues";
	report(solver_msg.c_str());


	// Use the correct solver type
	Eigen::ArpackGeneralizedSelfAdjointEigenSolver<Eigen::SparseMatrix<double>> solver;


	// The compute() method expects: matrix K, matrix M, number of eigenvalues, 
	// spectrum type, and optionally ncv
	solver.compute(K_ff, M_ff, nev, "SM", Eigen::ComputeEigenvectors, ncv);

	// Check ARPACK's convergence status
	if (solver.info() != Eigen::Success)
	{
		std::string error_msg = "ARPACK failed to converge. ";
		error_msg += "Number of converged eigenvalues: " + std::to_string(solver.getNbrConvergedEigenValues());
		report(error_msg.c_str());
		return false;
	}


	// Check if eigenvectors were computed
	if (!solver.eigenvectors().size() == 0)
	{
		solver_msg = "Eigenvectors not computed." + solver.info();
		report(solver_msg.c_str());

		return false;
	}


	eigenvalues = solver.eigenvalues();
	eigenvectors = solver.eigenvectors();

	//// Post-processing: ensure eigenvalues are sorted (should be, but just in case)
 //  // and remove any negative eigenvalues near zero (numerical noise)
	//for (int i = 0; i < eigenvalues.size(); ++i)
	//{
	//	if (eigenvalues(i) < 0 && eigenvalues(i) > -1e-9)
	//	{
	//		eigenvalues(i) = 0.0;  // Treat as zero (rigid body mode)
	//		report("Warning: Near-zero eigenvalue detected (rigid body mode)");
	//	}
	//	else if (eigenvalues(i) < 0)
	//	{
	//		report("Warning: Negative eigenvalue detected. Check matrix definiteness.");
	//	}
	//}

	auto end_time = std::chrono::high_resolution_clock::now();
	auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
	solver_msg = "ARPACK solver completed in " + std::to_string(duration.count()) + " ms";
	report(solver_msg.c_str());

	// Report convergence statistics
	solver_msg = "Converged eigenvalues: " + std::to_string(solver.getNbrConvergedEigenValues()) +
		" / " + std::to_string(nev);
	report(solver_msg.c_str());

	return true;

}





void modal_spectral_solver::store_results_with_index()
{
	// Open file in binary mode
	std::ofstream bin_file(output_file, std::ios::binary);

	if (!bin_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + this->output_file;
		report(error_msg.c_str());
		throw std::runtime_error("Failed to open output file: " + output_file);
	}

	// Get counts
	int32_t num_nodes = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	int32_t num_modes = static_cast<int32_t>(natural_frequencies.size());
	uint32_t num_edges = static_cast<uint32_t>(spec_mesh2d.renderer_edge_lines.size());
	uint32_t num_triangles = static_cast<uint32_t>(spec_mesh2d.renderer_element_triangles.size());

	// Calculate offsets (will be updated after writing)
	uint64_t header_size = sizeof(BinaryFileHeader);
	uint64_t node_data_offset = header_size;

	// Node data size
	uint64_t node_data_size = num_nodes * (sizeof(int32_t) + 2 * sizeof(double));
	uint64_t edge_data_offset = node_data_offset + node_data_size;

	// Edge data size
	uint64_t edge_data_size = spec_mesh2d.renderer_edge_lines.size() * (2 * sizeof(int32_t));
	uint64_t triangle_data_offset = edge_data_offset + edge_data_size;

	// Triangle data size
	uint64_t triangle_data_size = spec_mesh2d.renderer_element_triangles.size() * (3 * sizeof(int32_t));
	uint64_t mode_index_offset = triangle_data_offset + triangle_data_size;

	// Mode index table size
	uint64_t mode_index_size = num_modes * (sizeof(uint32_t) + sizeof(double) + sizeof(uint64_t) + sizeof(uint64_t));
	uint64_t mode_data_offset = mode_index_offset + mode_index_size;

	std::string success_msg = "";

	// Write header
	BinaryFileHeader header;
	std::memcpy(header.magic, "SEMF", 4);
	header.version = 2;
	header.num_modes = num_modes;
	header.num_nodes = num_nodes;
	header.num_edges = num_edges;
	header.num_triangles = num_triangles;
	header.mode_data_offset = mode_data_offset;
	header.mode_index_offset = mode_index_offset;

	bin_file.write(reinterpret_cast<const char*>(&header), sizeof(header));


	// Write nodes
	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t node_id = static_cast<int32_t>(node.n_id);
		bin_file.write(reinterpret_cast<const char*>(&node_id), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&node.x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&node.y), sizeof(double));

	}

	report("Result mesh: Nodes written");

	// Write edges
	for (const auto& edge : spec_mesh2d.renderer_edge_lines)
	{
		int32_t start_id = static_cast<int32_t>(edge.nstart);
		int32_t end_id = static_cast<int32_t>(edge.nend);
		bin_file.write(reinterpret_cast<const char*>(&start_id), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&end_id), sizeof(int32_t));

	}

	report("Result mesh: Edges written");

	// Write triangles
	for (const auto& tri : spec_mesh2d.renderer_element_triangles)
	{
		int32_t n1 = static_cast<int32_t>(tri.n1);
		int32_t n2 = static_cast<int32_t>(tri.n2);
		int32_t n3 = static_cast<int32_t>(tri.n3);
		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));

	}

	report("Result mesh: Triangles written");

	std::vector<ModeIndexEntry> mode_index;
	uint64_t current_offset = mode_data_offset;

	// Write mode index table - field by field 
	for (int32_t i = 0; i < num_modes; i++)
	{
		uint32_t mode_id = i;
		double frequency = natural_frequencies[i];
		uint64_t file_offset = current_offset;
		uint64_t data_size = sizeof(int32_t) + (num_nodes * (sizeof(int32_t) + sizeof(double)));

		// Write each field individually
		bin_file.write(reinterpret_cast<const char*>(&mode_id), sizeof(uint32_t));
		bin_file.write(reinterpret_cast<const char*>(&frequency), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&file_offset), sizeof(uint64_t));
		bin_file.write(reinterpret_cast<const char*>(&data_size), sizeof(uint64_t));

		mode_index.push_back({ mode_id, frequency, file_offset, data_size });
		current_offset += data_size;
	}

	report("Mode index table written");

	// Write mode data (each mode separately)
	for (int32_t mode_id = 0; mode_id < num_modes; mode_id++)
	{
		// Write mode ID first (for redundancy)
		bin_file.write(reinterpret_cast<const char*>(&mode_id), sizeof(int32_t));

		// Write mode shape values for each node
		for (const auto& node : spec_mesh2d.renderer_node_points)
		{
			int32_t node_id = static_cast<int32_t>(node.n_id);
			int nd_idx = nodeid_map[node.n_id];
			double mode_value = natural_modes(nd_idx, mode_id);

			bin_file.write(reinterpret_cast<const char*>(&node_id), sizeof(int32_t));
			bin_file.write(reinterpret_cast<const char*>(&mode_value), sizeof(double));
		}
	}

	bin_file.flush();
	bin_file.close();

	success_msg = "Modal results stored successfully: " + this->output_file +
		" (" + std::to_string(num_nodes) + " nodes, " +
		std::to_string(num_modes) + " modes)";

	report(success_msg.c_str());

}


void modal_spectral_solver::store_matrices_text_debug()
{
	std::string text_file_name = "debug_matrices.txt";
	std::ofstream text_file(text_file_name);

	if (!text_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + text_file_name;
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}

	// Print the global K and M matrices
	// Only print 200 x 200, inform if the matrix size exceed 200 x 200

	text_file << "# Modal Analysis Solver - Ke & Me matrix\n";
	text_file << "# Format: Debug Text Output\n";
	text_file << "# Generated: " << __DATE__ << " " << __TIME__ << "\n\n";

	int max_print_size = 200;
	int matrix_rows = global_k_matrix.rows();
	int matrix_cols = global_k_matrix.cols();

	// Write Ke Matrix
	text_file << "=== Ke Matrix ===\n";
	text_file << "Size: " << matrix_rows << " x " << matrix_cols << "\n";

	if (matrix_rows > max_print_size || matrix_cols > max_print_size)
	{
		text_file << "WARNING: Matrix size exceeds " << max_print_size
			<< " x " << max_print_size << ". Printing only the first "
			<< max_print_size << " x " << max_print_size << " block.\n\n";

		// Print only the top-left corner
		for (int i = 0; i < std::min(max_print_size, matrix_rows); i++)
		{
			for (int j = 0; j < std::min(max_print_size, matrix_cols); j++)
			{
				text_file << std::setw(15) << std::setprecision(6) << global_k_matrix.coeff(i, j) << " ";
			}
			text_file << "\n";
		}
	}
	else
	{
		// Print full matrix
		for (int i = 0; i < matrix_rows; i++)
		{
			for (int j = 0; j < matrix_cols; j++)
			{
				text_file << std::setw(15) << std::setprecision(6) << global_k_matrix.coeff(i, j) << " ";
			}
			text_file << "\n";
		}
	}
	text_file << "\n";

	// Write Me Matrix
	text_file << "=== Me Matrix ===\n";
	text_file << "Size: " << matrix_rows << " x " << matrix_cols << "\n";

	if (matrix_rows > max_print_size || matrix_cols > max_print_size)
	{
		text_file << "WARNING: Matrix size exceeds " << max_print_size
			<< " x " << max_print_size << ". Printing only the first "
			<< max_print_size << " x " << max_print_size << " block.\n\n";

		// Print only the top-left corner
		for (int i = 0; i < std::min(max_print_size, matrix_rows); i++)
		{
			for (int j = 0; j < std::min(max_print_size, matrix_cols); j++)
			{
				text_file << std::setw(15) << std::setprecision(6) << global_m_matrix.coeff(i, j) << " ";
			}
			text_file << "\n";
		}
	}
	else
	{
		// Print full matrix
		for (int i = 0; i < matrix_rows; i++)
		{
			for (int j = 0; j < matrix_cols; j++)
			{
				text_file << std::setw(15) << std::setprecision(6) << global_m_matrix.coeff(i, j) << " ";
			}
			text_file << "\n";
		}
	}
	text_file << "\n";

	// Optional: Print matrix statistics
	text_file << "=== Matrix Statistics ===\n";

	// K matrix statistics
	double k_min = 0, k_max = 0, k_sum = 0;
	int k_nonzero = 0;
	for (int k = 0; k < global_k_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_k_matrix, k); it; ++it)
		{
			double val = it.value();
			if (k_nonzero == 0) {
				k_min = val;
				k_max = val;
			}
			k_min = std::min(k_min, val);
			k_max = std::max(k_max, val);
			k_sum += std::abs(val);
			k_nonzero++;
		}
	}

	text_file << "Ke (Stiffness) Matrix:\n";
	text_file << "  Non-zero entries: " << k_nonzero << "\n";
	text_file << "  Density: " << (100.0 * k_nonzero / (matrix_rows * matrix_cols)) << "%\n";
	text_file << "  Min value: " << k_min << "\n";
	text_file << "  Max value: " << k_max << "\n";
	text_file << "  Mean absolute value: " << (k_nonzero > 0 ? k_sum / k_nonzero : 0) << "\n\n";

	// M matrix statistics
	double m_min = 0, m_max = 0, m_sum = 0;
	int m_nonzero = 0;
	for (int k = 0; k < global_m_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<double>::InnerIterator it(global_m_matrix, k); it; ++it)
		{
			double val = it.value();
			if (m_nonzero == 0) {
				m_min = val;
				m_max = val;
			}
			m_min = std::min(m_min, val);
			m_max = std::max(m_max, val);
			m_sum += std::abs(val);
			m_nonzero++;
		}
	}

	text_file << "Me (Mass) Matrix:\n";
	text_file << "  Non-zero entries: " << m_nonzero << "\n";
	text_file << "  Density: " << (100.0 * m_nonzero / (matrix_rows * matrix_cols)) << "%\n";
	text_file << "  Min value: " << m_min << "\n";
	text_file << "  Max value: " << m_max << "\n";
	text_file << "  Mean absolute value: " << (m_nonzero > 0 ? m_sum / m_nonzero : 0) << "\n\n";

	// Check for symmetry
	bool k_symmetric = global_k_matrix.isApprox(global_k_matrix.transpose());
	bool m_symmetric = global_m_matrix.isApprox(global_m_matrix.transpose());

	text_file << "=== Matrix Properties ===\n";
	text_file << "Ke is symmetric: " << (k_symmetric ? "YES" : "NO") << "\n";
	text_file << "Me is symmetric: " << (m_symmetric ? "YES" : "NO") << "\n";

	text_file.close();

	std::string msg = "Debug matrices written to: " + text_file_name;
	report(msg.c_str());


}



void modal_spectral_solver::store_results_text_debug()
{
	std::string text_file_name = "_debug_results.txt";
	std::ofstream text_file(text_file_name);

	if (!text_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + text_file_name + ".txt";
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}

	// Get counts
	int32_t num_nodes = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	int32_t num_modes = static_cast<int32_t>(natural_frequencies.size());

	// Write header information
	text_file << "# Modal Analysis Results\n";
	text_file << "# Format: Debug Text Output\n";
	text_file << "# Generated: " << __DATE__ << " " << __TIME__ << "\n\n";

	text_file << "=== HEADER ===\n";
	text_file << "Magic: SEMF\n";
	text_file << "Version: 2 (Debug Text)\n";
	text_file << "Number of Modes: " << num_modes << "\n";
	text_file << "Number of Nodes: " << num_nodes << "\n\n";

	// Write nodes
	text_file << "=== NODES ===\n";
	text_file << "Format: NodeID, X, Y\n";
	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t node_id = static_cast<int32_t>(node.n_id);
		text_file << node_id << ", " << node.x << ", " << node.y << "\n";
	}
	text_file << "\n";

	report("Result mesh: Nodes written to text file");

	// Write edges
	text_file << "=== EDGES ===\n";
	text_file << "Format: StartNodeID, EndNodeID\n";
	for (const auto& edge : spec_mesh2d.renderer_edge_lines)
	{
		int32_t start_id = static_cast<int32_t>(edge.nstart);
		int32_t end_id = static_cast<int32_t>(edge.nend);
		text_file << start_id << ", " << end_id << "\n";
	}
	text_file << "\n";

	report("Result mesh: Edges written to text file");

	// Write triangles
	text_file << "=== TRIANGLES ===\n";
	text_file << "Format: Node1, Node2, Node3\n";
	for (const auto& tri : spec_mesh2d.renderer_element_triangles)
	{
		int32_t n1 = static_cast<int32_t>(tri.n1);
		int32_t n2 = static_cast<int32_t>(tri.n2);
		int32_t n3 = static_cast<int32_t>(tri.n3);
		text_file << n1 << ", " << n2 << ", " << n3 << "\n";
	}
	text_file << "\n";

	report("Result mesh: Triangles written to text file");

	// Write mode results
	text_file << "=== MODE RESULTS ===\n";
	for (int32_t mode_id = 0; mode_id < num_modes; mode_id++)
	{
		text_file << "\n--- Mode " << mode_id + 1 << " ---\n";
		text_file << "Frequency: " << natural_frequencies[mode_id] << " Hz\n";
		text_file << "Mode Shape Values (NodeID, Value):\n";

		// Write mode shape values for each node
		for (const auto& node : spec_mesh2d.renderer_node_points)
		{
			int32_t node_id = static_cast<int32_t>(node.n_id);
			int nd_idx = nodeid_map[node.n_id];
			double mode_value = natural_modes(nd_idx, mode_id);

			text_file << "  " << node_id << ", " << mode_value << "\n";
		}
	}

	text_file << "\n=== SUMMARY ===\n";
	text_file << "Total Nodes: " << num_nodes << "\n";
	text_file << "Total Modes: " << num_modes << "\n";
	text_file << "Total Edges: " << spec_mesh2d.renderer_edge_lines.size() << "\n";
	text_file << "Total Triangles: " << spec_mesh2d.renderer_element_triangles.size() << "\n";

	text_file.flush();
	text_file.close();


	std::string msg = "Debug text output written to: " + text_file_name;
	report(msg.c_str());
}


