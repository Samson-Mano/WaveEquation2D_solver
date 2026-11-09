#include "direct_integration_solver.h"

direct_integration_solver::direct_integration_solver()
{
	// Empty constructor
}




void direct_integration_solver::init(wave2d_system_store * wave_2dsystem_ptr, 
	const char* output_file_char, 
	stopwatch_events * stopwatch, void(*callback)(const char*))
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

	msg = "Output file set to: " + this->output_file;
	report(msg.c_str());

}





bool direct_integration_solver::perform_direct_integration_solve(int number_of_intermediatesteps, 
	double global_damping_ratio, double TotalSimulationTime, double TimeIncrement, 
	double geom_min_x, double geom_min_y, double scale_value)
{

	// Step 1: Create the direct integration global matrices
	direct_integration_matrices direct_integration_matrices_solver;

	direct_integration_matrices_solver.init(wave_2dsystem_ptr, m_stopwatch, m_callback);

	// Create spectral mesh and global matrices
	direct_integration_matrices_solver.create_global_matrices();

	// Create the initial condition field vector
	direct_integration_matrices_solver.create_initialcondition_load_vector();

	// Create the global damping matrix
	direct_integration_matrices_solver.create_global_damping_matrix(global_damping_ratio);

	//___________________________________________________________________________________________________________
	Eigen::VectorXd initial_displacement = direct_integration_matrices_solver.getGlobalInitialFieldVector();
	Eigen::VectorXd initial_velocity = direct_integration_matrices_solver.getGlobalInitialDerivFieldVector();

	Eigen::SparseMatrix<std::complex<double>> global_k_matrix = direct_integration_matrices_solver.getGlobalKMatrix();
	Eigen::SparseMatrix<std::complex<double>> global_m_matrix = direct_integration_matrices_solver.getGlobalMMatrix();
	Eigen::SparseMatrix<std::complex<double>> global_c_matrix = direct_integration_matrices_solver.getGlobalCMatrix();

	Eigen::VectorXi global_dirichlet_BC_flags_vector = direct_integration_matrices_solver.getGlobalDirichletBCFlagsVector();

	// Reduce the global matrices and vectors based on Dirichlet boundary conditions
	// Find the fixed and free nodes dof
	std::vector<int> free_dofs;
	std::vector<int> fixed_dofs;

	const int numDOF = direct_integration_matrices_solver.getNumDOF();

	for (int i = 0; i < numDOF; ++i)
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


	std::vector<Eigen::Triplet<std::complex<double>>> Kt, Mt, Ct;
	int n_free = static_cast<int>(free_dofs.size());
	int n_fixed = static_cast<int>(fixed_dofs.size());

	// Extract K_ff
	for (int k = 0; k < global_k_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_k_matrix, k); it; ++it)
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
		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_m_matrix, k); it; ++it)
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

	// Extract C_ff
	for (int k = 0; k < global_c_matrix.outerSize(); ++k)
	{
		for (Eigen::SparseMatrix<std::complex<double>>::InnerIterator it(global_c_matrix, k); it; ++it)
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

				Ct.emplace_back(ii, jj, it.value());
			}
		}
	}


	Eigen::SparseMatrix<std::complex<double>> K_ff(n_free, n_free);
	Eigen::SparseMatrix<std::complex<double>> M_ff(n_free, n_free);
	Eigen::SparseMatrix<std::complex<double>> C_ff(n_free, n_free);
	K_ff.setFromTriplets(Kt.begin(), Kt.end());
	M_ff.setFromTriplets(Mt.begin(), Mt.end());
	C_ff.setFromTriplets(Ct.begin(), Ct.end());

	// Compress matrices for better performance
	K_ff.makeCompressed();
	M_ff.makeCompressed();
	C_ff.makeCompressed();


	report("Matrices created successfully_____________________________________");


	Eigen::VectorXd initial_displacement_free(n_free);
	Eigen::VectorXd initial_velocity_free(n_free);

	for (int k = 0; k < n_free; ++k)
	{
		int global_idx = free_dofs[k];

		initial_displacement_free(k) = initial_displacement(global_idx);
		initial_velocity_free(k) = initial_velocity(global_idx);
	}

	Eigen::VectorXcd load_vector_at_t(n_free);

	direct_integration_matrices_solver.getReducedGlobalLoadVector(0.0, load_vector_at_t);

	report("Reduced matrices created successfully");
	//___________________________________________________________________________________________________________
	std::unordered_map<int, direct_integration_nodal_results_store> nodal_results; // Map to store nodal results

	int number_of_mainsteps = static_cast<int>(TotalSimulationTime / TimeIncrement) + 1;
	
	for (int node_id = 0; node_id < numDOF; ++node_id)
	{
		auto& nr = nodal_results[node_id];
		nr.node_id = node_id;

		nr.field_vector.resize(number_of_mainsteps + 1);
		nr.field_derivative_vector.resize(number_of_mainsteps + 1);
		nr.field_acceleration_vector.resize(number_of_mainsteps + 1);
	}

	this->time_vector.resize(number_of_mainsteps + 1);

	// Newmark-beta method parameters 
	// Linear acceleration method (unconditionally stable)
	const double sub_dt = TimeIncrement / number_of_intermediatesteps;

	const double gamma = 0.5;   // Newmark gamma
	const double beta = 1.0 / 6.0;  // Newmark beta

	// --- Initial acceleration: solve M a0 = F0 - C v0 - K u0 ---
	Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> M_solver;
	M_solver.analyzePattern(M_ff);
	M_solver.factorize(M_ff);

	Eigen::VectorXcd u = initial_displacement_free.cast<std::complex<double>>();
	Eigen::VectorXcd v = initial_velocity_free.cast<std::complex<double>>();
	Eigen::VectorXcd F0 = load_vector_at_t;
	Eigen::VectorXcd a = M_solver.solve(F0 - C_ff * v - K_ff * u);

	// Precompute Newmark scalars
	const double inv_beta_dt2 = 1.0 / (beta * sub_dt * sub_dt);
	const double inv_beta_dt = 1.0 / (beta * sub_dt);
	const double half_beta_m1 = (1.0 / (2.0 * beta)) - 1.0;

	const double dt_gammainv2beta_m1 = sub_dt * ((gamma / (2.0 * beta)) - 1.0);
	const double gammainvbeta_m1 = (gamma / beta) - 1.0;
	const double gammainvbetadt = (gamma / (beta * sub_dt));


	Eigen::SparseMatrix<std::complex<double>> a1 = inv_beta_dt2 * M_ff + gammainvbetadt * C_ff;
	Eigen::SparseMatrix<std::complex<double>> a2 = inv_beta_dt * M_ff + gammainvbeta_m1 * C_ff;
	Eigen::SparseMatrix<std::complex<double>> a3 = half_beta_m1 * M_ff + dt_gammainv2beta_m1 * C_ff;

	// Calculate the effective stiffness matrix
	Eigen::SparseMatrix<std::complex<double>> K_eff = K_ff + a1;
	K_eff.makeCompressed();

	// Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> K_solver;
	Eigen::PardisoLU<Eigen::SparseMatrix<std::complex<double>>> K_solver;
	K_solver.analyzePattern(K_eff);
	K_solver.factorize(K_eff);


	// Store results at initial time step (t=0)
	store_results_at_timestep(0, free_dofs, fixed_dofs, u, v, a, nodal_results);

	// --- Time loop ---
	// Start of time integration loop
	double current_time = 0.0;
	time_vector[0] = current_time;

	report("Starting time integration loop_____________________________________");
	Eigen::VectorXcd rhs(n_free), u_next(n_free), v_next(n_free), a_next(n_free);


	int next_milestone = 10;  // Start with 10 %

	for (int main_step = 0; main_step < number_of_mainsteps; ++main_step)
	{
		for (int sub = 0; sub < number_of_intermediatesteps; ++sub)
		{
			const double t_next = current_time + sub_dt;

			direct_integration_matrices_solver.getReducedGlobalLoadVector(t_next, load_vector_at_t);

			rhs = load_vector_at_t;
			rhs.noalias() += a1 * u;
			rhs.noalias() += a2 * v;
			rhs.noalias() += a3 * a;
			

			u_next.noalias() = K_solver.solve(rhs);

			v_next = gammainvbetadt * (u_next - u) - gammainvbeta_m1 * v - dt_gammainv2beta_m1 * a;
			a_next = inv_beta_dt2 * (u_next - u) - inv_beta_dt * v - half_beta_m1 * a;

			u.swap(u_next);
			v.swap(v_next);
			a.swap(a_next);

			current_time = t_next;
		}

		// store results at index main_step + 1 ...
		store_results_at_timestep(main_step + 1, free_dofs, fixed_dofs, u, v, a, nodal_results);

		// Add to time vector
		time_vector[main_step + 1] = current_time;

		// Report progress
		int progress = static_cast<int>(100.0 * (main_step + 1) / number_of_mainsteps);
		if (progress >= next_milestone)
		{
			msg = "Time integration progress: " + std::to_string(progress) + "%";
			report(msg.c_str());
			next_milestone += 10;  // Update next milestone
		}

	}


	// Report completion
	msg = "Direct time integration analysis completed successfully.";
	report(msg.c_str());

	const spectral_mesh2d& spec_mesh2d = direct_integration_matrices_solver.getSpectralMesh2D();
	
	store_results(spec_mesh2d, nodal_results, geom_min_x, geom_min_y, scale_value);

	msg = "Direct time integration results stored successfully in: " + this->output_file;
	report(msg.c_str());

	return true;
}


void direct_integration_solver::store_results_at_timestep(int main_step, 
	const std::vector<int>& free_dofs, const std::vector<int>& fixed_dofs,
	const Eigen::VectorXcd& u, const Eigen::VectorXcd& v, const Eigen::VectorXcd& a,
	std::unordered_map<int, direct_integration_nodal_results_store>& nodal_results)
{

	// Store results for free DOFs
	for (int k = 0; k < free_dofs.size(); ++k)
	{
		int global_idx = free_dofs[k];
		auto& nodal_result = nodal_results[global_idx];
		nodal_result.node_id = global_idx;
		nodal_result.field_vector[main_step] = u(k).real();
		nodal_result.field_derivative_vector[main_step] = v(k).real();
		nodal_result.field_acceleration_vector[main_step] = a(k).real();
	}

	// Store results for fixed DOFs (assumed to be zero)
	for (int k = 0; k < fixed_dofs.size(); ++k)
	{
		int global_idx = fixed_dofs[k];
		auto& nodal_result = nodal_results[global_idx];
		nodal_result.node_id = global_idx;
		nodal_result.field_vector[main_step] = 0.0;
		nodal_result.field_derivative_vector[main_step] = 0.0;
		nodal_result.field_acceleration_vector[main_step] = 0.0;
	}
}


void direct_integration_solver::store_results(const spectral_mesh2d& spec_mesh2d, 
	const std::unordered_map<int, direct_integration_nodal_results_store>& nodal_results,
	double geom_min_x, double geom_min_y, double scale_value)
{

	std::ofstream bin_file(this->output_file.c_str(), std::ios::binary);

	if (!bin_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + this->output_file;
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}


	int32_t node_points_count = static_cast<int32_t>(spec_mesh2d.renderer_node_points.size());
	bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));

	// Write the nodes
	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		double scaled_x = (node.x / scale_value) + geom_min_x;
		double scaled_y = (node.y / scale_value) + geom_min_y;

		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&scaled_x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&scaled_y), sizeof(double));

	}

	report("Results: Nodes written");

	int32_t edge_lines_count = static_cast<int32_t>(spec_mesh2d.renderer_edge_lines.size());
	bin_file.write(reinterpret_cast<const char*>(&edge_lines_count), sizeof(int32_t));

	// Write the edges
	for (const auto& edge : spec_mesh2d.renderer_edge_lines)
	{
		int32_t start_nodeid = static_cast<int32_t>(edge.nstart);
		int32_t end_nodeid = static_cast<int32_t>(edge.nend);

		bin_file.write(reinterpret_cast<const char*>(&start_nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&end_nodeid), sizeof(int32_t));
	}

	report("Results: Edges written");

	int32_t triangles_count = static_cast<int32_t>(spec_mesh2d.renderer_element_triangles.size());
	bin_file.write(reinterpret_cast<const char*>(&triangles_count), sizeof(int32_t));

	// Write the triangles
	for (const auto& tri : spec_mesh2d.renderer_element_triangles)
	{
		int32_t n1 = static_cast<int32_t>(tri.n1);
		int32_t n2 = static_cast<int32_t>(tri.n2);
		int32_t n3 = static_cast<int32_t>(tri.n3);

		bin_file.write(reinterpret_cast<const char*>(&n1), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n2), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&n3), sizeof(int32_t));
	}

	report("Results: Triangles written");

	int32_t time_vector_size = static_cast<int32_t>(time_vector.size());
	bin_file.write(reinterpret_cast<const char*>(&time_vector_size), sizeof(int32_t));


	// Write the time vector
	bin_file.write(reinterpret_cast<const char*>(this->time_vector.data()),
		static_cast<std::streamsize>(this->time_vector.size() * sizeof(double)));


	report("Time vectors written");


	for (const auto& node : spec_mesh2d.renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		// Write the node ID
		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));

		// Get the nodal results for the current node
		const direct_integration_nodal_results_store& n_result = nodal_results.at(node.n_id);

		bin_file.write(reinterpret_cast<const char*>(n_result.field_vector.data()),
			static_cast<std::streamsize>(n_result.field_vector.size() * sizeof(double)));
		bin_file.write(reinterpret_cast<const char*>(n_result.field_derivative_vector.data()),
			static_cast<std::streamsize>(n_result.field_derivative_vector.size() * sizeof(double)));
		bin_file.write(reinterpret_cast<const char*>(n_result.field_acceleration_vector.data()),
			static_cast<std::streamsize>(n_result.field_acceleration_vector.size() * sizeof(double)));


	}

	report("Nodal results written");



	bin_file.flush();

	auto file_size = bin_file.tellp();  // tellp() for output file (tellg() is for input)

	bin_file.close();

	// Report Success and file size
	std::string success_msg = "Results stored successfully: " +
		this->output_file +
		" (" + std::to_string(node_points_count) + " nodes, " +
		std::to_string(triangles_count) + " triangles)";
	report(success_msg.c_str());

	//

}




void direct_integration_solver::report(const char* msg)
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


