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

	std::string msg = "Output file set to: " + this->output_file;
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

	for (int i = 0; i < direct_integration_matrices_solver.getNumDOF(); ++i)
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


	Eigen::VectorXd initial_displacement_free(n_free);
	Eigen::VectorXd initial_velocity_free(n_free);

	for (int k = 0; k < n_free; ++k)
	{
		int global_idx = free_dofs[k];

		initial_displacement_free(k) = initial_displacement(global_idx);
		initial_velocity_free(k) = initial_velocity(global_idx);
	}

	Eigen::VectorXd load_vector_free = direct_integration_matrices_solver.getReducedGlobalLoadVector(0.0);

	report("Reduced matrices created successfully");
	//___________________________________________________________________________________________________________

	const double sub_dt = TimeIncrement / number_of_intermediatesteps;

	// Newmark-beta method parameters 
	// Linear acceleration method (unconditionally stable)
	const double gamma = 0.5;   // Newmark gamma
	const double beta = 1.0 / 6.0;  // Newmark beta

	// Initial calculation of the effective stiffness matrix
	// Inverse of mass matrix
	Eigen::SparseMatrix<std::complex<double>> M_ff_inv(n_free, n_free);
	{
		Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> solver;
		solver.analyzePattern(M_ff);
		solver.factorize(M_ff);
		std::vector<Eigen::Triplet<std::complex<double>>> inv_triplets;
		for (int k = 0; k < n_free; ++k)
		{
			Eigen::VectorXcd e_k = Eigen::VectorXcd::Zero(n_free);
			e_k(k) = 1.0;
			Eigen::VectorXcd x_k = solver.solve(e_k);
			for (int j = 0; j < n_free; ++j)
			{
				if (std::abs(x_k(j)) > 1e-12)
				{
					inv_triplets.emplace_back(j, k, x_k(j));
				}
			}
		}
		M_ff_inv.setFromTriplets(inv_triplets.begin(), inv_triplets.end());
	}

	// Iniital acceleration
	Eigen::VectorXcd initial_acceleration_free = M_ff_inv * (load_vector_free.cast<std::complex<double>>() - C_ff * initial_velocity_free.cast<std::complex<double>>() - K_ff * initial_displacement_free.cast<std::complex<double>>());

	Eigen::SparseMatrix<std::complex<double>> a1 = (1.0 / (beta * sub_dt * sub_dt)) * M_ff + (gamma / (beta * sub_dt)) * C_ff + K_ff;
	Eigen::SparseMatrix<std::complex<double>> a2 = (1.0 / (beta * sub_dt)) * M_ff + ((gamma / beta) - 1.0) * C_ff;
	Eigen::SparseMatrix<std::complex<double>> a3 = ((1.0 / (2.0 * beta)) - 1.0) * M_ff + sub_dt * (gamma / (2.0 * beta) - 1.0) * C_ff;

	// Calculate the effective stiffness matrix
	Eigen::SparseMatrix<std::complex<double>> effective_stiffness_matrix = K_ff + a1;

	// Invert the effective stiffness matrix
	Eigen::SparseMatrix<std::complex<double>> effective_stiffness_matrix_inv(n_free, n_free);
	{
		Eigen::SparseLU<Eigen::SparseMatrix<std::complex<double>>> solver;
		solver.analyzePattern(effective_stiffness_matrix);
		solver.factorize(effective_stiffness_matrix);
		std::vector<Eigen::Triplet<std::complex<double>>> inv_triplets;
		for (int k = 0; k < n_free; ++k)
		{
			Eigen::VectorXcd e_k = Eigen::VectorXcd::Zero(n_free);
			e_k(k) = 1.0;
			Eigen::VectorXcd x_k = solver.solve(e_k);
			for (int j = 0; j < n_free; ++j)
			{
				if (std::abs(x_k(j)) > 1e-12)
				{
					inv_triplets.emplace_back(j, k, x_k(j));
				}
			}
		}
		effective_stiffness_matrix_inv.setFromTriplets(inv_triplets.begin(), inv_triplets.end());
	}

	// Results store
	Eigen::VectorXcd displacement_free = initial_displacement_free;
	Eigen::VectorXcd velocity_free = initial_velocity_free;
	Eigen::VectorXcd acceleration_free = initial_acceleration_free;


	std::unordered_map<int, direct_integration_nodal_results_store> nodal_results; // Map to store nodal results

	// Store the results for the initial time step
	for (int k = 0; k < n_free; ++k)
	{
		int global_idx = free_dofs[k];
		auto& nodal_result = nodal_results[global_idx];
		
		nodal_result.node_id = global_idx;

		nodal_result.field_vector[0] = displacement_free(k).real();
		nodal_result.field_derivative_vector[0] = velocity_free(k).real();
		nodal_result.field_acceleration_vector[0] = acceleration_free(k).real();
	}


	// Start of time integration loop
	int number_of_mainsteps = static_cast<int>(TotalSimulationTime / TimeIncrement);
	double current_time = 0.0;

	for (int main_step = 0; main_step < number_of_mainsteps; ++main_step)
	{
		for (int sub = 0; sub < number_of_intermediatesteps; ++sub)
		{
			double t_next = current_time + sub_dt;

			// Get the load vector at the next time step
			Eigen::VectorXd load_vector_next = direct_integration_matrices_solver.getReducedGlobalLoadVector(t_next);

			Eigen::VectorXcd effective_load_vector = load_vector_next.cast<std::complex<double>>() + a1 * displacement_free + a2 * velocity_free + a3 * acceleration_free;

			// Solve for the next displacement
			Eigen::VectorXcd displacement_free_next = effective_stiffness_matrix_inv * effective_load_vector;

			// Update velocity and acceleration
			Eigen::VectorXcd acceleration_free_next = (1.0 / (beta * sub_dt * sub_dt)) * (displacement_free_next - displacement_free) - (1.0 / (beta * sub_dt)) * velocity_free - ((1.0 / (2.0 * beta)) - 1.0) * acceleration_free;
			Eigen::VectorXcd velocity_free_next = velocity_free + sub_dt * ((1.0 - gamma) * acceleration_free + gamma * acceleration_free_next);

			// Update the current state
			displacement_free = displacement_free_next;
			velocity_free = velocity_free_next;
			acceleration_free = acceleration_free_next;

			current_time = t_next;	
		}

		// After each main step, store the results to the output file



	}


	return false;
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


