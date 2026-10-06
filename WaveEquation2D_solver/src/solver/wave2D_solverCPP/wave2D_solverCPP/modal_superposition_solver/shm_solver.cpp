#include "shm_solver.h"



shm_solver::shm_solver()
{
	// Empty constructor

}

void shm_solver::get_steady_state_initial_condition_soln(double& displ, 
	double& velo, double& accl, 
	double time_t, double mass_m, double stiff_k, 
	double initial_displ, double initial_velo)
{
	// Return the steady state solution for the intial displacment and initial velocity
	double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n

	displ = (initial_displ * std::cos(modal_omega_n * time_t)) +
		((initial_velo / modal_omega_n) * std::sin(modal_omega_n * time_t));

	velo = (-modal_omega_n * initial_displ * std::sin(modal_omega_n * time_t)) +
		(initial_velo * std::cos(modal_omega_n * time_t));

	accl = (-modal_omega_n * modal_omega_n * initial_displ * std::cos(modal_omega_n * time_t)) -
		(modal_omega_n * initial_velo * std::sin(modal_omega_n * time_t));

	//
}



void shm_solver::get_steady_state_half_sine_pulse_soln(double& displ, 
	double& velo, double& accl, 
	double time_t, double mass_m, double stiff_k, 
	double force_ampl, double force_starttime, double force_endtime)
{
    // Return the steady state solution for the half sine pulse
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);

    // natural time period
    double T_n = (2.0 * M_PI) / modal_omega_n;
    // Force period
    double t_d = (force_endtime - force_starttime);
    // time at
    double t_at = 0.0;

    // Check whether the current time is greater than the force start time
    if (time_t >= force_starttime)
    {
        t_at = time_t - force_starttime;
        if (time_t <= force_endtime)
        {
            // current time is within the force period
            if (std::abs((t_d / T_n) - 0.5) < 0.000001)
            {
                // Resonance case
                double k_fact = (force_ampl / (2.0 * stiff_k));

                displ = k_fact * (std::sin(modal_omega_n * t_at) -
                    (modal_omega_n * t_at * std::cos(modal_omega_n * t_at)));

                velo = k_fact * (modal_omega_n * modal_omega_n * t_at * std::sin(modal_omega_n * t_at));

                accl = k_fact * ((modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at)) +
                    (modal_omega_n * modal_omega_n * modal_omega_n * t_at * std::cos(modal_omega_n * t_at)));

            }
            else
            {
                // Normal case
                double const1 = M_PI / (modal_omega_n * t_d);
                double const2 = 1.0 - std::pow(const1, 2);
                double k_fact = (force_ampl / stiff_k) * (1 / const2);

                double pi_td = (M_PI / t_d);

                displ = k_fact * (std::sin(pi_td * t_at) - (const1 * std::sin(modal_omega_n * t_at)));


                velo = k_fact * (pi_td * std::cos(pi_td * t_at) -
                    (const1 * modal_omega_n * std::cos(modal_omega_n * t_at)));

                accl = k_fact * (-pi_td * pi_td * std::sin(pi_td * t_at) +
                    (const1 * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at)));

            }
        }
        else if (time_t > force_endtime)
        {
            // current time is over the force period
            if (std::abs((t_d / T_n) - 0.5) < 0.000001)
            {
                // Resonance case
                double k_fact = ((force_ampl * M_PI) / (2.0 * stiff_k));

                displ = k_fact * std::cos((modal_omega_n * t_at) - M_PI);

                velo = -k_fact * modal_omega_n * std::sin((modal_omega_n * t_at) - M_PI);

                accl = -k_fact * modal_omega_n * modal_omega_n * std::cos((modal_omega_n * t_at) - M_PI);

            }
            else
            {
                // Normal case
                double const1 = M_PI / (modal_omega_n * t_d);
                double const2 = std::pow(const1, 2) - 1.0;
                double k_fact = (force_ampl / stiff_k) * ((2 * const1) / const2) * std::cos(modal_omega_n * t_d * 0.5);

                displ = k_fact * std::sin(modal_omega_n * (t_at - (t_d * 0.5)));

                velo = k_fact * modal_omega_n * std::cos(modal_omega_n * (t_at - (t_d * 0.5)));

                accl = -k_fact * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * (t_at - (t_d * 0.5)));

            }
        }
    }

    //
}




void shm_solver::get_steady_state_rectangular_pulse_soln(double& displ, 
    double& velo, double& accl, 
    double time_t, double mass_m, double stiff_k, 
    double force_ampl, double force_starttime, double force_endtime)
{
    // Return the steady state solution for the Rectangular pulse
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);

    // natural time period
    double T_n = (2.0 * M_PI) / modal_omega_n;
    // Force period
    double t_d = (force_endtime - force_starttime);
    // time at
    double t_at = 0.0;


    // Check whether the current time is greater than the force start time
    if (time_t >= force_starttime)
    {
        t_at = time_t - force_starttime;
        double k_fact = (force_ampl / stiff_k);

        if (time_t <= force_endtime)
        {

            // current time is within the force period
            displ = k_fact * (1.0 - std::cos(modal_omega_n * t_at));

            velo = k_fact * modal_omega_n * std::sin(modal_omega_n * t_at);

            accl = k_fact * modal_omega_n * modal_omega_n * std::cos(modal_omega_n * t_at);

        }
        else if (time_t > force_endtime)
        {
            // current time is over the force period
            displ = k_fact * (std::cos(modal_omega_n * (t_at - t_d)) - std::cos(modal_omega_n * t_at));

            velo = k_fact * (-modal_omega_n * std::sin(modal_omega_n * (t_at - t_d)) +
                modal_omega_n * std::sin(modal_omega_n * t_at));

            accl = k_fact * (-modal_omega_n * modal_omega_n * std::cos(modal_omega_n * (t_at - t_d)) +
                modal_omega_n * modal_omega_n * std::cos(modal_omega_n * t_at));

        }
    }

    //
}



void shm_solver::get_steady_state_triangular_pulse_soln(double& displ, 
    double& velo, double& accl, 
    double time_t, double mass_m, double stiff_k, 
    double force_ampl, double force_starttime, double force_endtime)
{
    // Return the steady state solution for the Triangular pulse
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);

    // natural time period
    double T_n = (2.0 * M_PI) / modal_omega_n;
    // Force period
    double t_d = (force_endtime - force_starttime);
    // time at
    double t_at = 0.0;

    // Check whether the current time is greater than the force start time
    if (time_t >= force_starttime)
    {
        t_at = time_t - force_starttime;

        if (time_t <= force_endtime)
        {
            // current time is within the force period
            if (t_at < (t_d * 0.5))
            {
                double k_fact = ((2.0 * force_ampl) / stiff_k);
                double const1 = (1.0 / (t_d * modal_omega_n));

                displ = k_fact * ((t_at / t_d) - (const1 * std::sin(modal_omega_n * t_at)));

                velo = k_fact * ((1.0 / t_d) - (const1 * modal_omega_n * std::cos(modal_omega_n * t_at)));

                accl = k_fact * (const1 * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at));

            }
            else
            {
                double k_fact = ((2.0 * force_ampl) / stiff_k);
                double const1 = (1.0 / (t_d * modal_omega_n));
                double tau = (t_at - (0.5 * t_d));

                displ = k_fact * (1.0 - (t_at / t_d) + const1 * ((2.0 * std::sin(modal_omega_n * tau)) -
                    std::sin(modal_omega_n * t_at)));

                velo = k_fact * (-(1.0 / t_d) + const1 * ((2.0 * modal_omega_n * std::cos(modal_omega_n * tau)) -
                    modal_omega_n * std::cos(modal_omega_n * t_at)));

                accl = k_fact * (const1 * ((-2.0 * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * tau)) +
                    modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at)));

            }

        }
        else if (time_t > force_endtime)
        {
            // current time is over the force period

            double k_fact = ((2.0 * force_ampl) / stiff_k);
            double const1 = (1.0 / (t_d * modal_omega_n));
            double tau1 = (t_at - (0.5 * t_d));
            double tau2 = (t_at - t_d);

            double factor1 = 2.0 * std::sin(modal_omega_n * tau1);
            double factor2 = std::sin(modal_omega_n * tau2);

            displ = k_fact * const1 * (2.0 * std::sin(modal_omega_n * tau1) -
                std::sin(modal_omega_n * tau2) - std::sin(modal_omega_n * t_at));

            velo = k_fact * const1 * modal_omega_n * (2.0 * std::cos(modal_omega_n * tau1) -
                std::cos(modal_omega_n * tau2) - std::cos(modal_omega_n * t_at));

            accl = k_fact * const1 * modal_omega_n * modal_omega_n * (-2.0 * std::sin(modal_omega_n * tau1) +
                std::sin(modal_omega_n * tau2) + std::sin(modal_omega_n * t_at));

        }
    }

    //
}



void shm_solver::get_steady_state_stepforce_finiterise_soln(double& displ, 
    double& velo, double& accl, 
    double time_t, double mass_m, double stiff_k, 
    double force_ampl, double force_starttime, double force_endtime)
{
    // Return the steady state solution for the Step Force with Finite Rise
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);

    // natural time period
    double T_n = (2.0 * M_PI) / modal_omega_n;
    // Force period
    double t_d = (force_endtime - force_starttime);
    // time at
    double t_at = 0.0;

    // Check whether the current time is greater than the force start time
    if (time_t >= force_starttime)
    {
        t_at = time_t - force_starttime;
        double k_fact = (force_ampl / stiff_k);
        double const1 = (1.0 / (t_d * modal_omega_n));


        if (time_t <= force_endtime)
        {

            // current time is within the force period
            displ = k_fact * ((t_at / t_d) - (const1 * std::sin(modal_omega_n * t_at)));

            velo = k_fact * ((1.0 / t_d) - (const1 * modal_omega_n * std::cos(modal_omega_n * t_at)));

            accl = k_fact * (const1 * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at));

        }
        else if (time_t > force_endtime)
        {

            // current time is over the force period
            displ = k_fact * (1.0 + const1 * (std::sin(modal_omega_n * (t_at - t_d)) - std::sin(modal_omega_n * t_at)));

            velo = k_fact * (const1 * modal_omega_n * (std::cos(modal_omega_n * (t_at - t_d)) - std::cos(modal_omega_n * t_at)));

            accl = k_fact * (const1 * modal_omega_n * modal_omega_n * (-std::sin(modal_omega_n * (t_at - t_d)) +
                std::sin(modal_omega_n * t_at)));

        }
    }
    //
}



void shm_solver::get_total_harmonic_soln(double& displ, 
    double& velo, double& accl, 
    double time_t, double mass_m, double stiff_k, 
    double force_ampl, double force_starttime, double force_endtime)
{
    // Return the Total solution (Transient + steady state) for the Harmonic excitation
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);


    if (std::abs(modal_omega_f - modal_omega_n) < 0.001)
    {
        // Resonance case

        double force_factor = (force_ampl / (2.0 * stiff_k));

        displ = force_factor * ((modal_omega_n * time_t * std::cos(modal_omega_n * time_t))
            - std::sin(modal_omega_n * time_t));

        velo = force_factor * ((modal_omega_n * std::cos(modal_omega_n * time_t)) -
            (modal_omega_n * modal_omega_n * time_t * std::sin(modal_omega_n * time_t))
            - modal_omega_n * std::cos(modal_omega_n * time_t));

        accl = force_factor * ((-modal_omega_n * modal_omega_n * std::sin(modal_omega_n * time_t)) -
            (modal_omega_n * modal_omega_n * std::sin(modal_omega_n * time_t)) +
            (modal_omega_n * modal_omega_n * modal_omega_n * time_t * std::cos(modal_omega_n * time_t))
            + modal_omega_n * modal_omega_n * std::sin(modal_omega_n * time_t));

    }
    else
    {
        // Regular case
        double force_factor = (force_ampl / stiff_k);
        double freq_ratio = modal_omega_f / modal_omega_n;
        double freq_factor = (1.0 - (freq_ratio * freq_ratio));

        double const1 = force_factor * (1.0 / freq_factor);


        displ = const1 * (std::sin(modal_omega_f * time_t) - freq_ratio * std::sin(modal_omega_n * time_t));

        velo = const1 * (modal_omega_f * std::cos(modal_omega_f * time_t) -
            freq_ratio * modal_omega_n * std::cos(modal_omega_n * time_t));

        accl = const1 * (-modal_omega_f * modal_omega_f * std::sin(modal_omega_f * time_t) +
            freq_ratio * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * time_t));

    }

    //
}



void shm_solver::get_steady_state_full_sine_pulse_soln(double& displ, 
    double& velo, double& accl, 
    double time_t, double mass_m, double stiff_k, 
    double force_ampl, double force_starttime, double force_endtime)
{
    // Return the steady state solution for the full sine pulse
    double modal_omega_n = std::sqrt(stiff_k / mass_m); // Modal omega n
    double modal_omega_f = M_PI / (force_endtime - force_starttime);


    // natural time period
    double T_n = (2.0 * M_PI) / modal_omega_n;
    // Force period
    double t_d = (force_endtime - force_starttime);
    // time at
    double t_at = 0.0;

    // Check whether the current time is greater than the force start time
    if (time_t >= force_starttime)
    {
        t_at = time_t - force_starttime;
        if (time_t <= force_endtime)
        {
            // current time is within the force period
            if (std::abs((t_d / T_n) - 0.5) < 0.000001)
            {
                // Resonance case
                double k_fact = (force_ampl / (2.0 * stiff_k));

                displ = k_fact * (std::sin(modal_omega_n * t_at) -
                    (modal_omega_n * t_at * std::cos(modal_omega_n * t_at)));

                velo = k_fact * (modal_omega_n * modal_omega_n * t_at * std::sin(modal_omega_n * t_at));

                accl = k_fact * ((modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at)) +
                    (modal_omega_n * modal_omega_n * modal_omega_n * t_at * std::cos(modal_omega_n * t_at)));

            }
            else
            {
                // Normal case
                double const1 = M_PI / (modal_omega_n * t_d);
                double const2 = 1.0 - std::pow(const1, 2);
                double k_fact = (force_ampl / stiff_k) * (1 / const2);

                double pi_td = (M_PI / t_d);

                displ = k_fact * (std::sin(pi_td * t_at) - (const1 * std::sin(modal_omega_n * t_at)));


                velo = k_fact * (pi_td * std::cos(pi_td * t_at) -
                    (const1 * modal_omega_n * std::cos(modal_omega_n * t_at)));

                accl = k_fact * (-pi_td * pi_td * std::sin(pi_td * t_at) +
                    (const1 * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * t_at)));

            }
        }
        else if (time_t > force_endtime)
        {
            // current time is over the force period
            if (std::abs((t_d / T_n) - 0.5) < 0.000001)
            {
                // Resonance case
                double k_fact = ((force_ampl * M_PI) / (2.0 * stiff_k));

                displ = k_fact * std::cos((modal_omega_n * t_at) - M_PI);

                velo = -k_fact * modal_omega_n * std::sin((modal_omega_n * t_at) - M_PI);

                accl = -k_fact * modal_omega_n * modal_omega_n * std::cos((modal_omega_n * t_at) - M_PI);

            }
            else
            {
                // Normal case
                double const1 = M_PI / (modal_omega_n * t_d);
                double const2 = std::pow(const1, 2) - 1.0;
                double k_fact = (force_ampl / stiff_k) * ((2 * const1) / const2) * std::cos(modal_omega_n * t_d * 0.5);

                displ = k_fact * std::sin(modal_omega_n * (t_at - (t_d * 0.5)));

                velo = k_fact * modal_omega_n * std::cos(modal_omega_n * (t_at - (t_d * 0.5)));

                accl = -k_fact * modal_omega_n * modal_omega_n * std::sin(modal_omega_n * (t_at - (t_d * 0.5)));

            }
        }
    }

    //
}





