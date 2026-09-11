#pragma once
#include <numeric>

#include <vector>
#include <cmath>

#include <Eigen/Dense>
#include <stdexcept>

#include "../triangle_duvant_rule/triangle_dunavant_rule.hpp"


static struct spectral_point
{
	double xi;
	double eta;
	double weight;
};


//static struct basis_term
//{
//	int a; // power of xi
//	int b; // power of eta
//};



class gll_utility
{
public:

	static std::vector<double> get_gll_locations(int spectral_order);

	static std::vector<double> get_gll_weights(int spectral_order, const std::vector<double>& gll_points_xi);


	// static std::vector<spectral_point> get_triangle_quadrature(int spectral_order);

	// static std::vector<spectral_point> get_quadrilateral_quadrature(int spectral_order);


	// static Eigen::MatrixXd get_inverse_vandermonde_matrix(int spectral_order);


	//static void evaluate_basis_derivatives(
	//	double xi,
	//	double eta,
	//	const std::vector<basis_term>& basis_terms,
	//	Eigen::VectorXd& dphi_dxi,
	//	Eigen::VectorXd& dphi_deta);


	//static void evaluate_basis_phi(double xi, double eta,
	//	const std::vector<basis_term>& basis_terms,
	//	Eigen::VectorXd& phi);


	static void evaluate_lagrange_1D(
		double x,
		const std::vector<double>& nodes,
		std::vector<double>& L,
		std::vector<double>& dL);


	// static std::vector<basis_term> build_basis_terms(int spectral_order);


private:
	static constexpr double tol = 1e-12;
	static constexpr int max_iter = 100;
	static constexpr double m_pi = 3.1415926535897932384626433832795028841971; // 3.1415926535897932384626433832795028841971


	//static std::vector<spectral_point> get_triangle_spectral_element(int spectral_order);

	// static std::vector<spectral_point> get_quadrilateral_spectral_element(int spectral_order);

	// static std::vector<double> get_gauss_points(int n);

	// static std::vector<double> get_gauss_weights(int n);


	// static int get_dunavant_rule_for_order(int spectral_order);

	static std::vector<spectral_point> get_triangle_quadrature_manual(int spectral_order);

	static std::vector<spectral_point> get_unsorted_triangle_quadrature(int spectral_order);

	// static void gauss_legendre(int n, std::vector<double>& points, std::vector<double>& weights);

};




