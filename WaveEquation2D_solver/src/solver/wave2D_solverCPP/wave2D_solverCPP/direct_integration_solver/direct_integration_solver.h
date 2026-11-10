#pragma once
#include "../system_store/wave2d_system_store.h"
#include "../system_store/stopwatch_events.h"
#include "direct_integration_matrices.h"

#include <fstream>

struct direct_integration_nodal_results_store
{
	int node_id = 0;

	std::vector<double> field_vector; // Displacement or field values at the node over time
	std::vector<double> field_derivative_vector; // Velocity or field derivative values at the node over time
	std::vector<double> field_acceleration_vector; // Acceleration or field second derivative values at the node over time
};





class direct_integration_solver
{

public:
	direct_integration_solver();
	~direct_integration_solver() = default;


	void init(wave2d_system_store* wave_2dsystem_ptr,
		const char* output_file_char, stopwatch_events* stopwatch, void(*callback)(const char*));


	bool perform_direct_integration_solve(int number_of_intermediatesteps, double global_damping_ratio,
		double TotalSimulationTime, double TimeIncrement);


private:
	wave2d_system_store* wave_2dsystem_ptr;
	stopwatch_events* m_stopwatch;

	std::string output_file;


	std::vector<double> time_vector;

	std::string msg = "";


	void store_results_at_timestep(int main_step, const std::vector<int>& free_dofs, const std::vector<int>& fixed_dofs,
		const Eigen::VectorXcd& u, const Eigen::VectorXcd& v, const Eigen::VectorXcd& a,
		std::unordered_map<int, direct_integration_nodal_results_store>& nodal_results);

	void store_results(const spectral_mesh2d& spec_mesh2d, 
		const std::unordered_map<int, direct_integration_nodal_results_store>& nodal_results);


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);



};