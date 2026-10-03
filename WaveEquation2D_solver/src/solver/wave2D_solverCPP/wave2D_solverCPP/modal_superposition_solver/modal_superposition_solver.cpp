#include "modal_superposition_solver.h"

modal_superposition_solver::modal_superposition_solver()
{
	// Empty constructor
}


void modal_superposition_solver::init(wave2d_system_store* wave_2dsystem_ptr,
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




bool modal_superposition_solver::perform_modal_superposition_solve(int inpt_num_modes, double global_damping_ratio,
	double geom_min_x, double geom_min_y, double scale_value)
{
	
	
	// Step 1: Modal Analysis
	modal_spectral_solver modal_spec_solver;

	modal_spec_solver.init(wave_2dsystem_ptr, m_stopwatch, m_callback);

	// Create spectral mesh and global matrices
	modal_spec_solver.create_global_matrices();

	// Perform ARPACK modal analysis solve
	bool modalAnalysisStatus = modal_spec_solver.solve_modal_analysis(inpt_num_modes, geom_min_x, geom_min_y, scale_value);
	
	if (modalAnalysisStatus)
	{
		report("Modal analysis completed successfully.");
	}
	else
	{
		report("Modal analysis failed.");
		return false;
	}


	// Checks for the maximum frequency
	double max_natural_frequency = modal_spec_solver.getNaturalFrequencies().back();

	// Get the maximum load (source) frequency from the system
	double max_source_frequency = 0.0;

	for (const auto& [id, edge_constraint] : wave_2dsystem_ptr->edge_constraint_list)
	{
		if (edge_constraint.isSource == true) // Source constraint
		{
			// Get the maximum source frequency
			max_source_frequency = std::max(max_source_frequency, edge_constraint.sourcefrequency);

		}
	}

	for (const auto& [id, node_constraint] : wave_2dsystem_ptr->node_constraint_list)
	{
		if (node_constraint.isSource == true) // Source constraint
		{
			// Get the maximum source frequency
			max_source_frequency = std::max(max_source_frequency, node_constraint.sourcefrequency);
		}
	}


	// Check if the maximum source frequency exceeds the maximum natural frequency
	if (max_source_frequency > (0.8333333 * max_natural_frequency))
	{
		std::string error_msg = "Error: 1.2 x Maximum source frequency (" + std::to_string(max_source_frequency) +
			" Hz) exceeds maximum natural frequency (" + std::to_string(max_natural_frequency) + " Hz). " +
			"Consider increasing the number of modes for correct results.";
		report(error_msg.c_str());

		return false; // Indicate that the modal superposition solve may not be accurate
	}
	else
	{
		std::string error_msg = "Maximum source frequency = " + std::to_string(max_source_frequency) +
			" Hz, Maximum natural frequency = " + std::to_string(max_natural_frequency) + " Hz.";
		report("Maximum source frequency is within the range of natural frequencies.");
	}

	






	return false; // Remove this after the actual modal superposition solve logic is implemented.


}





void modal_superposition_solver::report(const char* msg)
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



