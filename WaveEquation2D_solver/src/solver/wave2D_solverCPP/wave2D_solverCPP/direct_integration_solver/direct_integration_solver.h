#pragma once
#include "../system_store/wave2d_system_store.h"
#include "../system_store/stopwatch_events.h"
#include "direct_integration_matrices.h"


class direct_integration_solver
{

public:
	direct_integration_solver();
	~direct_integration_solver() = default;


	void init(wave2d_system_store* wave_2dsystem_ptr,
		const char* output_file_char, stopwatch_events* stopwatch, void(*callback)(const char*));


	bool perform_direct_integration_solve(int number_of_intermediatesteps, double global_damping_ratio,
		double TotalSimulationTime, double TimeIncrement,
		double geom_min_x, double geom_min_y, double scale_value);


private:

	wave2d_system_store* wave_2dsystem_ptr;
	stopwatch_events* m_stopwatch;

	std::string output_file;


	std::vector<double> time_vector;


	//void store_results(const modal_spectral_solver& modal_spec_solver,
	//	const std::unordered_map<int, nodal_results_store>& nodal_results,
	//	double geom_min_x, double geom_min_y, double scale_value);


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);



};