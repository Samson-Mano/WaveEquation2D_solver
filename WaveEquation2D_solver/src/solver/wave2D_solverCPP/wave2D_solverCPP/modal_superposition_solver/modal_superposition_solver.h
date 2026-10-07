#pragma once
#include "../system_store/wave2d_system_store.h"
#include "../system_store/stopwatch_events.h"

#include "modal_spectral_solver.h"
#include "shm_solver.h"



struct source_load_vector_data
{
	int load_id = 0;


	// Source term types
	// 0 = Half sine pulse
	// 1 = Rectangular pulse
	// 2 = Triangular pulse
	// 3 = Step force with finite rise
	// 4 = Full sine pulse
	// 5 = Harmonic/ periodic excitation
	int load_type = -1;

	double load_start_time = 0.0;
	double load_end_time = 0.0;


	Eigen::VectorXd modal_LoadAmplitudeVector;
	Eigen::VectorXd LoadAmplitudeVector;
};



struct nodal_results_store
{
	int node_id = 0;
	
	std::vector<double> field_vector; // Displacement or field values at the node over time
	std::vector<double> field_derivative_vector; // Velocity or field derivative values at the node over time
	std::vector<double> field_acceleration_vector; // Acceleration or field second derivative values at the node over time
};



class modal_superposition_solver
{
public:
	modal_superposition_solver();
	~modal_superposition_solver() = default;


	void init(wave2d_system_store* wave_2dsystem_ptr,
		const char* output_file_char, stopwatch_events* stopwatch, void(*callback)(const char*));

	bool perform_modal_superposition_solve(int inpt_num_modes, double global_damping_ratio, 
		double TotalSimulationTime,	double TimeIncrement,
		double geom_min_x, double geom_min_y, double scale_value);


private:

	wave2d_system_store* wave_2dsystem_ptr;
	stopwatch_events* m_stopwatch;
	shm_solver m_shm_solver;

	std::string output_file;

	std::vector<double> time_vector;

	// std::unordered_map<int, source_load_vector_data> source_load_vectors;


	void store_results(const modal_spectral_solver& modal_spec_solver,
		const std::unordered_map<int, nodal_results_store>& nodal_results, 
		double geom_min_x, double geom_min_y, double scale_value);


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);


};
