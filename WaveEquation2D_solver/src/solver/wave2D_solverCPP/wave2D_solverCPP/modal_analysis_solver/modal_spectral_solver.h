#pragma once

#include <cmath>

#pragma warning(push)
#pragma warning (disable : 26451)
#pragma warning (disable : 26495)
#pragma warning (disable : 6255)
#pragma warning (disable : 6294)
#pragma warning (disable : 26813)
#pragma warning (disable : 26454)
#pragma warning (disable : 4244)
#pragma warning (disable : 4091)
#pragma warning (disable : 6011)
#pragma warning (disable : 6387)

// Optimization for Eigen Library
// 1) OpenMP (Yes (/openmp)
//	 Solution Explorer->Configuration Properties -> C/C++ -> Language -> Open MP Support
// 2) For -march=native, choose "AVX2" or the latest supported instruction set.
//   Solution Explorer->Configuration Properties -> C/C++ -> Code Generation -> Enable Enhanced Instruction Set 
#include <iostream>

#include <Eigen/Dense>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>
#include <Eigen/Eigenvalues>
// Define the sparse matrix type for the reduced global stiffness matrix
typedef Eigen::SparseMatrix<double> SparseMatrix;

#include <Eigen/Core>
#include <Eigen/SparseCore>

// Spectra
#include <Spectra/SymEigsSolver.h>
#include <Spectra/GenEigsSolver.h>
#include <Spectra/MatOp/SparseSymMatProd.h>
#include <Spectra/MatOp/SparseCholesky.h>
#include <Spectra/MatOp/SparseGenMatProd.h>
#include <Spectra/MatOp/SparseRegularInverse.h>

using namespace Spectra;

// ARPACK
#include <Eigen/Sparse>
#include <Eigen/ArpackSupport>


#pragma warning(pop)


#include "../system_store/wave2d_system_store.h"
#include "../spectral_elements/spectral_mesh2d.h"
#include "../system_store/stopwatch_events.h"


#include "../spectral_elements/spectral_lib/gll_utility.h"
#include "../spectral_elements/spectral_lib/spectral_quad_element.h"
#include "../spectral_elements/spectral_lib/spectral_tri_element.h"


#include <iomanip> // to get std::setprecision()


#include "MinvKOp.h"

#include <vector>
#include <string>
#include <fstream>
#include <cstdint>





class modal_spectral_solver
{
public:
	modal_spectral_solver();
	~modal_spectral_solver() = default;

	void init(wave2d_system_store* wave_2dsystem_ptr,
		const char* output_file_char,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));

	void create_global_matrices();

	bool solve_modal_analysis(int inpt_num_modes, int solver_type);


	enum SolverType
	{
		SOLVER_SPECTRA = 1,
		SOLVER_ARPACK = 2
	};


	void store_matrices_text_debug();

	void store_results_text_debug();

private:
	const double M_PI = 3.1415926535897932384626433;

	wave2d_system_store* wave_2dsystem_ptr;
	spectral_mesh2d spec_mesh2d;

	stopwatch_events* m_stopwatch;

	std::string output_file;


	int numDOF = 0;
	std::unordered_map<int, int> nodeid_map; // Node ID map


	Eigen::SparseMatrix<double> global_k_matrix; // Global ke Matrix [Ke]
	Eigen::SparseMatrix<double> global_m_matrix; // Global Me Matrix [Me]

	Eigen::VectorXi global_dirichlet_BC_flags_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)

	// Solution
	std::vector<double> natural_frequencies;
	Eigen::MatrixXd natural_modes;



	// Quadrature points for triangle element
	std::vector<spectral_point> triangle_quadrature_points;

	// Triangle basis term
	std::vector<proriol_basis_term> triangle_basis_terms;

	// Inverse Vandermonde matrix
	Eigen::MatrixXd inv_vandermonde_matrix;

	// 1D GLL points
	std::vector<double> gll_locations;
	std::vector<double> gll_weights;


	// Quadrature points for quadrilateral element
	std::vector<spectral_point> quadrilateral_quadrature_points;



	//________________________________________________________________________________________________

	void get_trielement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_matrix,
		Eigen::MatrixXd& element_m_matrix);


	void get_trielement_field_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag);


	void get_trielement_source_vector(const spectral_trielement_store& tri_elm,
		Eigen::VectorXi& dirichlet_BC_flag);


	//________________________________________________________________________________________________

	void set_global_matrix(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::MatrixXd& element_k_matrix,
		const Eigen::MatrixXd& element_m_matrix,
		std::vector<Eigen::Triplet<double>>& k_triplets,
		std::vector<Eigen::Triplet<double>>& m_triplets);



	void set_global_BC_flag_vector(const std::vector<int>& elem_nodes,
		int nen,
		const Eigen::VectorXi& element_BC_flag_vector, Eigen::VectorXi& global_BC_flag_vector);



	void report_vandermondematrix_conditioning(const Eigen::MatrixXd& invVanderMondematrix);



	//________________________________________________________________________________________________

	void get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_matrix,
		Eigen::MatrixXd& element_m_matrix);


	void get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag);



	void get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
		Eigen::VectorXi& dirichlet_BC_flag);

	//________________________________________________________________________________________________


	bool solveWithSpectra(int num_modes,
		const Eigen::SparseMatrix<double>& K_ff,
		const Eigen::SparseMatrix<double>& M_ff,
		Eigen::VectorXd& eigenvalues,
		Eigen::MatrixXd& eigenvectors);



	bool solveWithARPACK(int num_modes,
		const Eigen::SparseMatrix<double>& K_ff,
		const Eigen::SparseMatrix<double>& M_ff,
		Eigen::VectorXd& eigenvalues,
		Eigen::MatrixXd& eigenvectors);


	void store_results_with_index();



	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);






};









