#pragma once
#include <numeric>

#include <vector>
#include <cmath>

#include <Eigen/Dense>
#include <stdexcept>
#include "gll_utility.h"


struct GaussQuadrature
{
	std::vector<double> points;
	std::vector<double> weights;
};



class spectral_quad_element
{
public:
	static std::vector<spectral_point> get_quadrilateral_quadrature(int spectral_order);


	static void evaluate_quadrilateral_shape_functions(double quadraturept_xi,
		double quadraturept_eta, int spectral_order, const std::vector<double>& gll_locations,
		Eigen::VectorXd& N,
		Eigen::VectorXd& dN_dxi,
		Eigen::VectorXd& dN_deta);

private:
	static constexpr double tol = 1e-12;
	static constexpr int max_iter = 100;
	static constexpr double m_pi = 3.1415926535897932384626433832795028841971; // 3.1415926535897932384626433832795028841971


	static GaussQuadrature get_gauss_quadrature(int n);


	//static std::vector<double> get_gauss_points(int n);

	//static std::vector<double> get_gauss_weights(int n);

	static std::vector<spectral_point> get_quadrilateral_spectral_element(int spectral_order);

	static void gauss_legendre(int n, std::vector<double>& points, std::vector<double>& weights);

};