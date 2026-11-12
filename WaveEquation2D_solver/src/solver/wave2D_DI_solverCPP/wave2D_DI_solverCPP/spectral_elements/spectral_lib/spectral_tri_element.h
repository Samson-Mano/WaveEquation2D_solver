#pragma once
#include <numeric>

#include <vector>
#include <cmath>

#include <Eigen/Dense>
#include <stdexcept>
#include "gll_utility.h"


// To evaculate legendre and jacobi polynomial for Proriol basis phi
#include <boost/math/special_functions/legendre.hpp>
#include <boost/math/special_functions/jacobi.hpp>



static struct proriol_basis_term
{
	int a; // power of xi
	int b; // power of eta
};


class spectral_tri_element
{
public:
	static std::vector<spectral_point> get_triangle_quadrature(int spectral_order);

	static std::vector<proriol_basis_term> proriol_modes(int spectral_order);

	static Eigen::MatrixXd get_inverse_vandermonde_matrix(int spectral_order, std::vector<proriol_basis_term> p_modes);


	static void evaluate_triangle_shape_functions(double quadraturept_xi,
		double quadraturept_eta, int spectral_order, const Eigen::MatrixXd& invVmatirx,
		const std::vector<proriol_basis_term>& proriol_modes,
		Eigen::VectorXd& N,
		Eigen::VectorXd& dN_dxi,
		Eigen::VectorXd& dN_deta);



private:
	static constexpr double eps = 1e-10;
	static constexpr double m_pi = 3.1415926535897932384626433832795028841971;


	static int get_dunavant_rule_for_order(int spectral_order);

	static std::vector<spectral_point> get_triangle_spectral_element(int spectral_order);

	static double proriol_basis_phi(double xi, double eta, int a, int b);


	static void proriol_basis_phi_derivatives(double xi, double eta, int a, int b,
		double& phi,
		double& dphi_dxi, double& dphi_deta);


};