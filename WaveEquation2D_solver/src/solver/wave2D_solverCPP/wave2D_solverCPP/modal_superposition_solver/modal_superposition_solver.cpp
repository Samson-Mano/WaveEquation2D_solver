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
	std::unordered_map<int, source_load_vector_data> source_load_vectors;
	// source_load_vectors.clear();


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

					// // Scale the load frequency based on the modal spectral solver's frequency scale factor
					// double load_freq = load.sourcefrequency * modal_spec_solver.getFreqScaleFactor();
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

	std::unordered_map<int, nodal_results_store> nodal_results; // Map to store nodal results

	const int num_steps =
		static_cast<int>(TotalSimulationTime / TimeIncrement) + 1;

	for (int node_id = 0; node_id < numDOF; ++node_id)
	{
		auto& nr = nodal_results[node_id];
		nr.node_id = node_id;

		nr.field_vector.resize(num_steps);
		nr.field_derivative_vector.resize(num_steps);
		nr.field_acceleration_vector.resize(num_steps);
	}

	this->time_vector.resize(num_steps);

	// Get the modal mass and stiffness vectors
	const std::vector<double>& modal_m_vector = modal_spec_solver.getModalMVector();
	const std::vector<double>& modal_k_vector = modal_spec_solver.getModalKVector();

	// Get the mode shape block for transforming modal coordinates back to global coordinates
	// for a block spanning all rows and given columns
	const Eigen::MatrixXd& mode_shape_block = modal_spec_solver.getNaturalModes().middleCols(mode_start_index, modal_dof);


	for (int step = 0; step < num_steps; ++step)
	{
		const double time_t = step * TimeIncrement;

		// For each time step, compute the modal response and then reconstruct the nodal response

		Eigen::VectorXd modal_displresponse_vector = Eigen::VectorXd::Zero(modal_dof);
		Eigen::VectorXd modal_veloresponse_vector = Eigen::VectorXd::Zero(modal_dof);
		Eigen::VectorXd modal_acclresponse_vector = Eigen::VectorXd::Zero(modal_dof);


		for (int i = mode_start_index; i < mode_end_index; ++i)
		{
			const int local_i = i - mode_start_index;   // <-- local index

			double displ_resp_initial = 0.0; // Displacement response due to initial condition
			double velo_resp_initial = 0.0; // Velocity response due to initial condition
			double accl_resp_initial = 0.0; // Acceleration response due to initial condition

			// Get the modal mass and modal stiffness for the current mode
			double modal_mass = modal_m_vector[i] * std::pow(modal_spec_solver.getFreqScaleFactor(), 2);
			double modal_stiffness = modal_k_vector[i];


			m_shm_solver.get_steady_state_initial_condition_soln(displ_resp_initial,
				velo_resp_initial,
				accl_resp_initial,
				time_t,
				modal_mass,
				modal_stiffness,
				modal_initial_field_vector[local_i],
				modal_initial_field_derivative_vector[local_i]);

			//_______________________________________________________________________
			double displ_resp_force = 0.0; // Displacement response due to pulse force
			double velo_resp_force = 0.0; // Velocity response due to pulse force
			double accl_resp_force = 0.0; // Acceleration response due to pulse force

			// get all the loads
			for (const auto& [load_id, source_load] : source_load_vectors)
			{
				// Go through all the force
				double at_force_displ_resp = 0.0;
				double at_force_velo_resp = 0.0;
				double at_force_accl_resp = 0.0;

				if (source_load.load_type == 0)
				{
					// Half sine pulse

					m_shm_solver.get_steady_state_half_sine_pulse_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);

				}
				else if (source_load.load_type == 1)
				{
					// Rectangular pulse

					m_shm_solver.get_steady_state_rectangular_pulse_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);

				}
				else if (source_load.load_type == 2)
				{
					// Triangular pulse

					m_shm_solver.get_steady_state_triangular_pulse_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);

				}
				else if (source_load.load_type == 3)
				{
					// Step force with finite rise

					m_shm_solver.get_steady_state_stepforce_finiterise_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);


				}
				else if (source_load.load_type == 4)
				{
					// Full sine pulse
					m_shm_solver.get_steady_state_full_sine_pulse_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);


				}
				else if (source_load.load_type == 5)
				{
					// Harmonic Excitation
					m_shm_solver.get_total_harmonic_soln(at_force_displ_resp,
						at_force_velo_resp,
						at_force_accl_resp,
						time_t,
						modal_mass,
						modal_stiffness,
						source_load.modal_LoadAmplitudeVector[local_i],
						source_load.load_start_time,
						source_load.load_end_time);

				}

				displ_resp_force = displ_resp_force + at_force_displ_resp;
				velo_resp_force = velo_resp_force + at_force_velo_resp;
				accl_resp_force = accl_resp_force + at_force_accl_resp;

			}

			// Add to the modal displ, velo, and accl matrices
			modal_displresponse_vector[local_i] = displ_resp_initial + displ_resp_force;
			modal_veloresponse_vector[local_i] = velo_resp_initial + velo_resp_force;
			modal_acclresponse_vector[local_i] = accl_resp_initial + accl_resp_force;

		}

		// Transform modal space responses back to global coordinate (nodal) responses at time t

		Eigen::VectorXd globalcoord_displresponse_vector = Eigen::VectorXd::Zero(numDOF);
		Eigen::VectorXd globalcoord_veloresponse_vector = Eigen::VectorXd::Zero(numDOF);
		Eigen::VectorXd globalcoord_acclresponse_vector = Eigen::VectorXd::Zero(numDOF);


		globalcoord_displresponse_vector.noalias() = mode_shape_block * modal_displresponse_vector;
		globalcoord_veloresponse_vector.noalias() = mode_shape_block * modal_veloresponse_vector;
		globalcoord_acclresponse_vector.noalias() = mode_shape_block * modal_acclresponse_vector;


		// Store the results in the nodal_results map
		for (int node_id = 0; node_id < numDOF; ++node_id)
		{
			auto& nodal_result = nodal_results[node_id];

			nodal_result.node_id = node_id;
			
			nodal_result.field_vector[step] = globalcoord_displresponse_vector[node_id];
			nodal_result.field_derivative_vector[step] = globalcoord_veloresponse_vector[node_id];
			nodal_result.field_acceleration_vector[step] = globalcoord_acclresponse_vector[node_id];
		}

		// Add to time vector
		time_vector[step] = time_t;
	}

	// Store the results to the output file
	store_results(modal_spec_solver, nodal_results, geom_min_x, geom_min_y, scale_value);


	return true; // Remove this after the actual modal superposition solve logic is implemented.

}


void modal_superposition_solver::store_results(const modal_spectral_solver& modal_spec_solver,
	const std::unordered_map<int, nodal_results_store>& nodal_results,
	double geom_min_x, double geom_min_y, double scale_value)
{
	std::ofstream bin_file(this->output_file.c_str(), std::ios::binary);

	if (!bin_file.is_open())
	{
		std::string error_msg = "Failed to open output file: " + this->output_file;
		report(error_msg.c_str());
		throw std::runtime_error(error_msg);
	}


	int32_t node_points_count = static_cast<int32_t>(modal_spec_solver.getSpectralMesh2D().renderer_node_points.size());
	bin_file.write(reinterpret_cast<const char*>(&node_points_count), sizeof(int32_t));

	// Write the nodes
	for (const auto& node : modal_spec_solver.getSpectralMesh2D().renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		double scaled_x = (node.x / scale_value) + geom_min_x;
		double scaled_y = (node.y / scale_value) + geom_min_y;

		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&scaled_x), sizeof(double));
		bin_file.write(reinterpret_cast<const char*>(&scaled_y), sizeof(double));
	}

	report("Results: Nodes written");

	int32_t edge_lines_count = static_cast<int32_t>(modal_spec_solver.getSpectralMesh2D().renderer_edge_lines.size());
	bin_file.write(reinterpret_cast<const char*>(&edge_lines_count), sizeof(int32_t));

	// Write the edges
	for (const auto& edge : modal_spec_solver.getSpectralMesh2D().renderer_edge_lines)
	{
		int32_t start_nodeid = static_cast<int32_t>(edge.nstart);
		int32_t end_nodeid = static_cast<int32_t>(edge.nend);

		bin_file.write(reinterpret_cast<const char*>(&start_nodeid), sizeof(int32_t));
		bin_file.write(reinterpret_cast<const char*>(&end_nodeid), sizeof(int32_t));
	}

	report("Results: Edges written");

	int32_t triangles_count = static_cast<int32_t>(modal_spec_solver.getSpectralMesh2D().renderer_element_triangles.size());
	bin_file.write(reinterpret_cast<const char*>(&triangles_count), sizeof(int32_t));

	// Write the triangles
	for (const auto& tri : modal_spec_solver.getSpectralMesh2D().renderer_element_triangles)
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


	for (const auto& node : modal_spec_solver.getSpectralMesh2D().renderer_node_points)
	{
		int32_t nodeid = static_cast<int32_t>(node.n_id);
		// Write the node ID
		bin_file.write(reinterpret_cast<const char*>(&nodeid), sizeof(int32_t));

		// Get the nodal results for the current node
		const nodal_results_store& n_result =   nodal_results.at(node.n_id);

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



