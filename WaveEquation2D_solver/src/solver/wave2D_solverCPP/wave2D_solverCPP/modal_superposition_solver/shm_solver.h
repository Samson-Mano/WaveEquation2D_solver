#pragma once
#include <iostream>


class shm_solver
{
public:
	const double M_PI = 3.1415926535897932384626433;

	shm_solver();
	~shm_solver() = default;


    void get_steady_state_initial_condition_soln(
        double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double initial_displ,
        double initial_velo);



    void get_steady_state_half_sine_pulse_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);



    void get_steady_state_rectangular_pulse_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);




    void get_steady_state_triangular_pulse_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);



    void get_steady_state_stepforce_finiterise_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);




    void get_total_harmonic_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);



    void get_steady_state_full_sine_pulse_soln(double& displ,
        double& velo,
        double& accl,
        double time_t,
        double mass_m,
        double stiff_k,
        double force_ampl,
        double force_starttime,
        double force_endtime);


};

