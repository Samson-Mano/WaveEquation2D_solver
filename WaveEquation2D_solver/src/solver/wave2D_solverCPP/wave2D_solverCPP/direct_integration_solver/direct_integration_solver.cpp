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


