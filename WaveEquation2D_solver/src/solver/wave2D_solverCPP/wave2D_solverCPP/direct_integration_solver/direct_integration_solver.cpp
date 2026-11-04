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

	const double sub_dt = TimeIncrement / number_of_intermediatesteps;

	// Newmark-beta method parameters 
	// Linear acceleration method (unconditionally stable)
	const double gamma = 0.5;   // Newmark gamma
	const double beta = 1.0 / 6.0;  // Newmark beta

	// Initial calculation of the effective stiffness matrix
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





	// Start of time integration loop
	int number_of_mainsteps = static_cast<int>(TotalSimulationTime / TimeIncrement);
	double current_time = 0.0;

	for (int main_step = 0; main_step < number_of_mainsteps; ++main_step)
	{
		for (int sub = 0; sub < number_of_intermediatesteps; ++sub)
		{
			double t_next = current_time + sub_dt;




			current_time = t_next;	
		}
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


