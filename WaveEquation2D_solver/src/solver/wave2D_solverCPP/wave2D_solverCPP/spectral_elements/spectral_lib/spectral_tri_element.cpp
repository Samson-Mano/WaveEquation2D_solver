
#include "spectral_tri_element.h"



std::vector<spectral_point> spectral_tri_element::get_triangle_quadrature(int spectral_order)
{
	std::vector<spectral_point> quadrature_points;

	// Map spectral order to Dunavant rule number
	int rule = get_dunavant_rule_for_order(spectral_order);

	// Get the number of quadrature points for this rule
	int order_num = dunavant_order_num(rule);

	// Allocate arrays for points and weights
	// Note: xy array needs to be of size 2*order_num for x and y coordinates
	std::vector<double> xy(2 * order_num);
	std::vector<double> w(order_num);

	// Call dunavant_rule
	dunavant_rule(rule, order_num, xy.data(), w.data());

	// Convert to spectral_point format
	for (int i = 0; i < order_num; i++)
	{
		double L1 = xy[2 * i];      // x coordinate (L1)
		double L2 = xy[2 * i + 1];  // y coordinate (L2)
		double L3 = 1.0 - L1 - L2; // L3 is determined

		quadrature_points.push_back({ L1, L2, w[i] });
	}

	return quadrature_points;
}




int spectral_tri_element::get_dunavant_rule_for_order(int spectral_order)
{
	// Minimum rule numbers needed for exact integration of polynomials of degree 2p
	// where p = 2 * spectral_order
	int p = 2 * spectral_order;

	switch (p)
	{
	case 1:  return 1;   // 1 point, degree 1
	case 2:  return 2;   // 3 points, degree 2  
	case 3:  return 3;   // 4 points, degree 3
	case 4:  return 4;   // 6 points, degree 4
	case 5:  return 5;   // 7 points, degree 5
	case 6:  return 6;   // 12 points, degree 6
	case 7:  return 7;   // 16 points, degree 7
	case 8:  return 8;   // 19 points, degree 8
	case 9:  return 9;   // 25 points, degree 9
	case 10: return 10;  // 31 points, degree 10
	case 11: return 11;  // 37 points, degree 11
	case 12: return 12;  // 43 points, degree 12
	case 13: return 13;  // 49 points, degree 13
	case 14: return 14;  // 55 points, degree 14
	case 15: return 15;  // 61 points, degree 15
	case 16: return 16;  // 67 points, degree 16
	case 17: return 17;  // 73 points, degree 17
	case 18: return 18;  // 79 points, degree 18
	case 19: return 19;  // 85 points, degree 19
	case 20: return 20;  // 91 points, degree 20
	default: return 20;  // Default to order 20 rule, 91 points, degree 20 
	}
	//
}




std::vector<spectral_point> spectral_tri_element::get_triangle_spectral_element(int spectral_order)
{
	// 3
	// |\
    // | \
    // |  \
    // 1---2


	// Get the GLL points of 1D edges
	std::vector<double> gll_points = gll_utility::get_gll_locations(spectral_order);


	// Triangle spectral element points
	std::vector<spectral_point> tri_spectral_points;

	// Create the corner points
	std::vector<spectral_point> corner_points;

	corner_points.emplace_back(spectral_point{ 0.0, 0.0, 1.0 }); // Point 1
	corner_points.emplace_back(spectral_point{ 1.0, 0.0, 1.0 }); // Point 2
	corner_points.emplace_back(spectral_point{ 0.0, 1.0, 1.0 }); // Point 3

	// Add the corner
	tri_spectral_points.emplace_back(spectral_point{ corner_points[0].xi, corner_points[0].eta, corner_points[0].weight });
	tri_spectral_points.emplace_back(spectral_point{ corner_points[1].xi, corner_points[1].eta, corner_points[1].weight });
	tri_spectral_points.emplace_back(spectral_point{ corner_points[2].xi, corner_points[2].eta, corner_points[2].weight });

	for (int i = 0; i < 3; i++)
	{
		spectral_point v_start = corner_points[i];
		spectral_point v_end = corner_points[(i + 1) % 3];

		// Add the edges
		for (int j = 1; j < spectral_order; j++) // Exclude the end point -1 and 1
		{
			// Map [-1, 1] to [0, 1]
			double s = (gll_points[j] + 1.0) / 2.0;

			double x_coord = ((1.0 - s) * v_start.xi) + (s * v_end.xi);
			double y_coord = ((1.0 - s) * v_start.eta) + (s * v_end.eta);

			tri_spectral_points.emplace_back(spectral_point{ x_coord, y_coord, 1.0 });
		}

	}

	// Populate Interior(Lobatto interpolation)
	// IMA Journal of Applied Mathematics Advance Access published March 16, 2005
	// A Lobatto interpolation grid over the triangle
	// M.G.Blyth and C.Pozrikidis

	if (spectral_order > 2)
	{
		for (int i = 1; i < spectral_order; i++)
		{
			for (int j = 1; j < spectral_order - i; j++)
			{
				int k = spectral_order - i - j;

				double vi = (gll_points[i] + 1.0) / 2.0;
				double vj = (gll_points[j] + 1.0) / 2.0;
				double vk = (gll_points[k] + 1.0) / 2.0;

				double x_coord = (1.0 / 3.0) * (1.0 + (2.0 * vj) - vk - vi);
				double y_coord = (1.0 / 3.0) * (1.0 + (2.0 * vi) - vk - vj);

				tri_spectral_points.emplace_back(spectral_point{ x_coord, y_coord, 1.0 });
			}
		}
		//
	}

	return tri_spectral_points;
	//
}



std::vector<proriol_basis_term> spectral_tri_element::proriol_modes(int spectral_order)
{

	std::vector<proriol_basis_term> basis_terms;

	for (int i = 0; i <= spectral_order; ++i)
	{
		for (int j = 0; j <= spectral_order - i; ++j)
		{
			basis_terms.push_back({ i, j });
		}
	}

	return basis_terms;

}


double spectral_tri_element::proriol_basis_phi(double xi, double eta, int a, int b)
{
	
	// Prevent division by zero with epsilon
	double den = std::max(1.0 - eta, eps);

	// Collapsed coordinates
	double xi_bar = ((2.0 * xi) / den) - 1.0;
	double eta_bar = (2.0 * eta) - 1.0;

	// Polynomials
	// Legendre polynomial - P_n(x)
	double Li = boost::math::legendre_p(a, xi_bar);

	// Jacobi polynomial - P_n^(alpha,beta)(x)
	double Pj = boost::math::jacobi(b, 2.0 * a + 1.0, 0.0, eta_bar);

	// Weight (stable form)
	double w = std::pow((1.0 - eta_bar) / 2.0, a);

	// Normalization
	double c = std::sqrt(((2.0 * a) + 1.0) * (a + b + 1.0) * 0.5);

	return c * Li * Pj * w;

}


void spectral_tri_element::proriol_basis_phi_derivatives(double xi, double eta, int a, int b,
	double& phi,
	double& dphi_dxi, double& dphi_deta)
{

	// Prevent division by zero with epsilon
	double den = std::max(1.0 - eta, eps);

	// Collapsed coordinates
	double xi_bar = ((2.0 * xi) / den) - 1.0;
	double eta_bar = (2.0 * eta) - 1.0;

	// ========== Polynomial Values ==========
   // Legendre polynomial P_a(xi_bar)
	double Li = boost::math::legendre_p(a, xi_bar);
	// Jacobi polynomial P_b^(2a+1,0)(eta_bar)
	double Pj = boost::math::jacobi(b, 2.0 * a + 1.0, 0.0, eta_bar);


	// ========== Polynomial Derivatives ==========
	// Legendre derivative d/d(xi_bar) P_a(xi_bar)
	double dLi_dxi_bar = boost::math::legendre_p_prime(a, xi_bar);


	// Jacobi derivative d/d(eta_bar) P_b^(2a+1,0)(eta_bar)
	// double dPj_deta_bar = boost::math::jacobi_derivative(b, 2.0 * a + 1.0, 0.0, eta_bar, 1);
	// Alternative: use jacobi_prime for first derivative only
	double dPj_deta_bar = boost::math::jacobi_prime(b, 2.0 * a + 1.0, 0.0, eta_bar);


	// ========== Weight Term ==========
	double w = std::pow((1.0 - eta_bar) / 2.0, a);
	double dw_deta_bar = 0.0;
	if (a > 0) {
		dw_deta_bar = -a * std::pow((1.0 - eta_bar) / 2.0, a - 1.0) / 2.0;
	}

	// ========== Normalization ==========
	double c = std::sqrt(((2.0 * a) + 1.0) * (a + b + 1.0) * 0.5);

	// ========== Basis Function Value ==========
	phi = c * Li * Pj * w;


	// ========== Chain Rule for Derivatives ==========
	// Derivatives with respect to collapsed coordinates
	double dphi_dxi_bar = c * dLi_dxi_bar * Pj * w;
	double dphi_deta_bar = c * Li * (dPj_deta_bar * w + Pj * dw_deta_bar);

	// Transform derivatives to physical coordinates
	double dxi_bar_dxi = 2.0 / den;
	double dxi_bar_deta = (2.0 * xi) / (den * den);
	double deta_bar_deta = 2.0;

	dphi_dxi = dphi_dxi_bar * dxi_bar_dxi;
	dphi_deta = dphi_dxi_bar * dxi_bar_deta + dphi_deta_bar * deta_bar_deta;

}



Eigen::MatrixXd spectral_tri_element::get_inverse_vandermonde_matrix(int spectral_order, std::vector<proriol_basis_term> p_modes)
{
	// Build Vandermonde matrix V where V_{p,q} = phi_q(xi_p, eta_p)

	// Get the reference coordinates of Lobatto triangle
	std::vector<spectral_point> ref_coords = get_triangle_spectral_element(spectral_order);

	int nen = ((spectral_order + 1) * (spectral_order + 2)) / 2;

	//if (ref_coords.size() != nen) 
	//{
	//	throw std::runtime_error("Number of reference points does not match number of basis functions");
	//}
	//if (p_modes.size() != nen) 
	//{
	//	throw std::runtime_error("Number of modes does not match expected size for spectral order");
	//}

	// Build Vandermonde matrix - use direct construction
	Eigen::MatrixXd vandermonde_matrix(nen, nen);

	// Pre-allocate and fill row by row
	for (int p = 0; p < nen; ++p) 
	{
		const auto& coords = ref_coords[p];

		for (int q = 0; q < nen; ++q) 
		{
			const auto& mode = p_modes[q];

			vandermonde_matrix(p, q) = proriol_basis_phi(coords.xi, coords.eta, mode.a, mode.b);
		}
		//
	}


	//// Manual Scalling of Vandermonde
	// // Build matrix with row/column scaling
	//Eigen::VectorXd row_scale(nen);
	//Eigen::VectorXd col_scale(nen);

	//// Compute scaling factors
	//for (int i = 0; i < nen; ++i) {
	//	row_scale(i) = 1.0 / vandermonde_matrix.row(i).norm();
	//	col_scale(i) = 1.0 / vandermonde_matrix.col(i).norm();
	//}

	//// Scale matrix: A_scaled = D_row * A * D_col
	//Eigen::MatrixXd scaled_matrix = row_scale.asDiagonal() *
	//	vandermonde_matrix *
	//	col_scale.asDiagonal();

	//// Compute inverse of scaled system
	//Eigen::MatrixXd inv_scaled = scaled_matrix.inverse();

	//// Unscale: A^{-1} = D_col * inv_scaled * D_row
	//Eigen::MatrixXd inv_vandermonde_matrix = col_scale.asDiagonal() *
	//	inv_scaled *
	//	row_scale.asDiagonal();


	// Compute inverse more robustly
	// Option 1: Use LU decomposition with partial pivoting (faster for well-conditioned)
	Eigen::MatrixXd inv_vandermonde_matrix = vandermonde_matrix.inverse();

	// Option 2: Use QR decomposition for better numerical stability
	// (uncomment for more robust approach)
	// Eigen::MatrixXd inv_vandermonde_matrix = vandermonde_matrix
	//     .colPivHouseholderQr()
	//     .solve(Eigen::MatrixXd::Identity(nen, nen));

	return inv_vandermonde_matrix;

}



void spectral_tri_element::evaluate_triangle_shape_functions(double quadraturept_xi,
	double quadraturept_eta, int spectral_order, const Eigen::MatrixXd& invVmatrix,
	const std::vector<proriol_basis_term>& proriol_modes,
	Eigen::VectorXd& N,
	Eigen::VectorXd& dN_dxi,
	Eigen::VectorXd& dN_deta)
{
	//  Evaluate shape functions and their derivatives at point(xi, eta).

	//	The shape functions N(xi, eta) are defined such that :
	//  N(xi_p, eta_p) = e_p(unit vector) at node p

	//	We have : V * c = f(where c are modal coefficients, f are nodal values)
	//	So for a given nodal values f, the modal coefficients are : c = invV @ f

	//	For shape function p(which is 1 at node p and 0 elsewhere),
	//	the nodal values are e_p, so the modal coefficients are invV[:, p].

	//	Therefore, shape function p at(xi, eta) is:
	//  N_p(xi, eta) = sum_q(invV[q, p] * phi_q(xi, eta))
	//	= (basis_vals @ invV)[p]


	int nen = ((spectral_order + 1) * (spectral_order + 2)) / 2;


	// Evaluate all basis functions at the point
	Eigen::VectorXd basis_vals(nen);
	Eigen::VectorXd dphi_dxi(nen);
	Eigen::VectorXd dphi_deta(nen);

	// Evaluate each basis function and its derivatives
	for (int q = 0; q < nen; q++) {
		int a = proriol_modes[q].a;
		int b = proriol_modes[q].b;

		double phi, dphi_dxi_val, dphi_deta_val;
		
		proriol_basis_phi_derivatives(quadraturept_xi, quadraturept_eta, a, b, phi,
			dphi_dxi_val, dphi_deta_val);

		basis_vals(q) = phi;
		dphi_dxi(q) = dphi_dxi_val;
		dphi_deta(q) = dphi_deta_val;
	}

	// Shape functions and their derivatives via inverse Vandermonde
	N = basis_vals.transpose() * invVmatrix;
	dN_dxi = dphi_dxi.transpose() * invVmatrix;
	dN_deta = dphi_deta.transpose() * invVmatrix;

}








