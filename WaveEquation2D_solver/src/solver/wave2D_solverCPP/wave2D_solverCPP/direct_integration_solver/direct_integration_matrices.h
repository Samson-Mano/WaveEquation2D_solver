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

#pragma warning(pop)



#include "../system_store/wave2d_system_store.h"
#include "../spectral_elements/spectral_mesh2d.h"
#include "../system_store/stopwatch_events.h"


#include "../spectral_elements/spectral_lib/gll_utility.h"
#include "../spectral_elements/spectral_lib/spectral_quad_element.h"
#include "../spectral_elements/spectral_lib/spectral_tri_element.h"


class nodal_load_store
{

public:
	nodal_load_store() = default;
	~nodal_load_store() = default;

	int node_id = -1;

	bool isSource = false;


	void add_loads(int load_set_id, const load_vector_store& load_values);

	double get_load_amplitude_at_time(double time) const;

private:
	const double M_PI = 3.1415926535897932384626433;

	// Source information for the node
	std::unordered_map<int, load_vector_store> source_values; // Source values for the node




};



class direct_integration_matrices
{

public:
	direct_integration_matrices();
	~direct_integration_matrices() = default;


	void init(wave2d_system_store* wave_2dsystem_ptr,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));


	const Eigen::SparseMatrix<double>& getGlobalMMatrix() const
	{
		return global_m_matrix;
	}


	const Eigen::SparseMatrix<double>& getGlobalKMatrix() const
	{
		return global_k_matrix;
	}

	const Eigen::SparseMatrix<double>& getGlobalCMatrix() const
	{
		return global_c_matrix;
	}


	const Eigen::VectorXi& getGlobalDirichletBCFlagsVector() const
	{
		return global_dirichlet_BC_flags_vector;
	}


	const Eigen::VectorXd& getGlobalInitialFieldVector() const
	{
		return global_initial_field_vector;
	}

	
	const Eigen::VectorXd& getGlobalInitialDerivFieldVector() const
	{
		return global_initial_derivfield_vector;
	}

	const Eigen::VectorXd& getGlobalLoadVector(double time) const;


	void create_global_matrices();

	void create_initialcondition_load_vector();

	void create_global_damping_matrix(double global_damping_ratio);


private:
	wave2d_system_store* wave_2dsystem_ptr;
	spectral_mesh2d spec_mesh2d;

	stopwatch_events* m_stopwatch;


	int numDOF = 0;
	Eigen::SparseMatrix<double> global_k_matrix; // Global ke Matrix [Ke]
	Eigen::SparseMatrix<double> global_m_matrix; // Global Me Matrix [Me]
	Eigen::SparseMatrix<double> global_c_matrix; // Global Ce Matrix [Ce]

	Eigen::VectorXi global_dirichlet_BC_flags_vector; // Global boundary condition Vector (To track the nodes where prescribed field is applied)

	Eigen::VectorXd global_initial_field_vector; // Global initial condition field vector 
	Eigen::VectorXd global_initial_derivfield_vector; // Global initial condition derivative field vector

	std::unordered_map<int, nodal_load_store> load_maps; // Load maps for the nodes


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


	//________________________________________________________________________________________________

	void get_quadelement_k_grad_k_mass_matrix(const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::MatrixXd& element_k_matrix,
		Eigen::MatrixXd& element_m_matrix);


	void get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
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



	void get_trielement_field_vector(const spectral_trielement_store& tri_elm,
		const std::vector<int>& elem_nodes,
		Eigen::VectorXd& initial_field_vector,
		Eigen::VectorXi& initial_BC_fieldcount_vector);



	void get_trielement_normderivfield_vector(const spectral_trielement_store& tri_elm,
		const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::VectorXd& initial_derivfield_vector,
		Eigen::VectorXi& initial_BC_derivfieldcount_vector);


	void get_trielement_source_vector(const spectral_trielement_store& tri_elm,
		const std::vector<int>& elem_nodes,
		std::unordered_map<int, nodal_load_store>& load_maps);


	void get_quadelement_field_vector(const spectral_quadelement_store& quad_elm,
		const std::vector<int>& elem_nodes,
		Eigen::VectorXd& initial_field_vector,
		Eigen::VectorXi& initial_BC_fieldcount_vector);



	void get_quadelement_normderivfield_vector(const spectral_quadelement_store& quad_elm,
		const std::vector<int>& elem_nodes,
		const std::vector<Eigen::Vector2d>& elem_coords,
		Eigen::VectorXd& initial_derivfield_vector,
		Eigen::VectorXi& initial_BC_derivfieldcount_vector);


	void get_quadelement_source_vector(const spectral_quadelement_store& quad_elm,
		const std::vector<int>& elem_nodes,
		std::unordered_map<int, nodal_load_store>& load_maps);



	//________________________________________________________________________________________________







	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);




};