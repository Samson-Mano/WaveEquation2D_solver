#pragma once
#include "../system_store/wave2d_system_store.h"
#include "../system_store/stopwatch_events.h"

#include "modal_spectral_solver.h"

class modal_superposition_solver
{
public:
	modal_superposition_solver();
	~modal_superposition_solver() = default;


	void init(wave2d_system_store* wave_2dsystem_ptr,
		const char* output_file_char, stopwatch_events* stopwatch, void(*callback)(const char*));

	bool perform_modal_superposition_solve(int inpt_num_modes, double global_damping_ratio, 
		double geom_min_x, double geom_min_y, double scale_value);


private:

	wave2d_system_store* wave_2dsystem_ptr;
	stopwatch_events* m_stopwatch;

	std::string output_file;


	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);


};
