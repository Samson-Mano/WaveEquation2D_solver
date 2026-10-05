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
	double TotalSimulationTime, double TimeIncrement,
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

		return false; // Indicate that the modal superposition fails
	}
	else
	{
		std::string error_msg = "Maximum source frequency = " + std::to_string(max_source_frequency) +
			" Hz, Maximum natural frequency = " + std::to_string(max_natural_frequency) + " Hz.";
		report("Maximum source frequency is within the range of natural frequencies.");
	}


	//____________________________________________________________________________________________________________________

	int number_of_modes = modal_spec_solver.getNumberofModes();
	int numDOF = modal_spec_solver.getNaturalModes().rows();

	int mode_start_index = modal_spec_solver.getIsRigid() == true ? 1 : 0; // Starting index for modes (0 or 1)
	int mode_end_index = number_of_modes; // Ending index for modes (number_of_modes)

	int modal_dof = mode_end_index - mode_start_index; // Number of modes to consider for modal superposition

	// Step 2: Modal Superposition Solve
	// Create the global initial condition vectors
	const Eigen::SparseMatrix<double>& Mff = modal_spec_solver.getGlobalMMatrix();	

	Eigen::VectorXd initial_field_vector = Eigen::VectorXd::Zero(numDOF);
	Eigen::VectorXd initial_field_derivative_vector = Eigen::VectorXd::Zero(numDOF);

	// Create the source load vectors for the modal superposition solve
	source_load_vectors.clear();


	for (const auto& [node_id, loads] : modal_spec_solver.getLoadMaps())
	{
		if (loads.isFieldBC)
		{
			// Initial field value is set to the corresponding node's initial field value
			initial_field_vector(node_id) = loads.initial_field_value;
		}

		if (loads.isFieldDerivativeBC)
		{
			// Initial field derivative value is set to the corresponding node's initial field derivative value
			initial_field_derivative_vector(node_id) = loads.initial_field_derivative_value;
		}

		if (loads.isSource)
		{
			// Handle source boundary conditions if needed
			for (const auto& [load_id, load] : loads.source_values)
			{
				auto [it, inserted] = source_load_vectors.try_emplace(load_id);
				auto& source_load = it->second;

				if (inserted)
				{
					source_load.load_id = load_id;
					source_load.load_type = load.sourcetype;
					source_load.load_start_time = load.sourcestarttime;
					source_load.load_end_time = load.sourcestarttime + (1.0 / load.sourcefrequency);

					source_load.LoadAmplitudeVector = Eigen::VectorXd::Zero(numDOF);
					source_load.modal_LoadAmplitudeVector = Eigen::VectorXd::Zero(modal_dof);
				}

				source_load.LoadAmplitudeVector(node_id) = load.sourceamplitude; // Set the load amplitude for the corresponding node
			}
		}

	}

	// Perform the modal superposition solve using the initial conditions and source load vectors

	Eigen::VectorXd modal_initial_field_vector = Eigen::VectorXd::Zero(modal_dof);
	Eigen::VectorXd modal_initial_field_derivative_vector = Eigen::VectorXd::Zero(modal_dof);

	for (int i = mode_start_index; i < mode_end_index; ++i)
	{
		// Get the mode shape
		Eigen::VectorXd mode_shape_phi = modal_spec_solver.getNaturalModes().col(i);

		modal_initial_field_vector(i - mode_start_index) = mode_shape_phi.dot(Mff * initial_field_vector);
		modal_initial_field_derivative_vector(i - mode_start_index) = mode_shape_phi.dot(Mff * initial_field_derivative_vector);
	}


	// Perform the modal superposition for source load vectors
	for (auto& [load_id, source_load] : source_load_vectors)
	{
		Eigen::VectorXd Mff_times_load = Mff * source_load.LoadAmplitudeVector;

		for (int i = mode_start_index; i < mode_end_index; ++i)
		{
			// Get the mode shape
			Eigen::VectorXd mode_shape_phi = modal_spec_solver.getNaturalModes().col(i);

			source_load.modal_LoadAmplitudeVector(i - mode_start_index) = mode_shape_phi.dot(Mff_times_load);
		}

	}


	//____________________________________________________________________________________________________________________




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



