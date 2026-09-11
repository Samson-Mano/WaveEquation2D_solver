#pragma once
#include <Eigen/Dense>
#include <unordered_map>
#include <unordered_set>
#include "../system_store/helmholtz_system_store.h"
#include "unique_id_control.h"
#include "spectral_lib/gll_utility.h"



// Renderer Triangle
struct renderer_triangle
{
	int n1, n2, n3;
};


// Renderer Edge
struct renderer_edge
{
	int nstart, nend;

	bool operator==(const renderer_edge& other) const
	{
		return nstart == other.nstart && nend == other.nend;
	}
};


// Renderer Node
struct renderer_node
{
	int n_id;
	double x, y;
	double r1, r2, r3, r4; // Scalar Result data
};



struct spectral_node_store
{
	int node_id = 0;
	double x_coord = 0.0;
	double y_coord = 0.0;

	bool isboundarynode = false;
	bool isFieldBC = false;
	double fieldvalue = 0.0; // Field value in the node
	double sourcevalue = 0.0; // Source value in the node

};



struct spectral_edge_store
{
	int edge_id = 0;
	int startnodeid = 0;
	int endnodeid = 0;

	std::vector<int> edge_internal_node_ids; // Internal node IDs on the edge (for higher-order spectral elements)

	int leftfaceid = -1; // The face on the left side of the edge (when looking from start node to end node)
	int rightfaceid = -1; // The face on the right side of the edge (when looking from start node to end node)

	bool isboundaryedge = false;
	bool isSommerfieldBC = false;
	bool isFieldBC = false;
	bool isDerivFieldBC = false;
	double fieldvalue = 0.0; // Dirichlet boundary condition
	double normalderivfieldvalue = 0.0; // Neumann boundary condition

};



struct local_idx_structure
{
	// Local matrix index structure for a single element
	std::vector<int> corner_nodes; // 4 corner nodes of the quadrilateral element

	// edge_node_ids[0] for edge 1, edge_node_ids[1] for edge 2, edge_node_ids[2] for edge 3, edge_node_ids[3] for edge 4
	std::vector<std::vector<int>> edge_node_ids;

	std::vector<int> internal_nodes; // Internal nodes of the quadrialteral element (for higher-order spectral elements)

	// Constructor to initialize with number of edges
	explicit local_idx_structure(int num_edges) : edge_node_ids(num_edges) {}

	void clear()
	{
		corner_nodes.clear();

		for (auto& edge : edge_node_ids)
		{
			edge.clear();
		}

		internal_nodes.clear();
	}


	// x --- @ --- @ --- x
	// |                 |
	// |                 |
	// @     O     O     @
	// |                 |
	// |                 | 
	// @     O     O     @
	// |                 |
	// |                 |
	// x --- @ --- @ --- x


	// x
	// | \
	// |   \
	// @  0  @
	// |       \
	// |         \
	// @   0  0    @
	// |             \
	// |               \
	// x --- @ --- @ --- x

};


struct spectral_trielement_store
{
	int tri_id = 0;

	double tri_area = 0.0;

	std::vector<int> corner_nodes; // 3 corner nodes of the triangle element

	// edge_node_ids[0] for edge 1, edge_node_ids[1] for edge 2, edge_node_ids[2] for edge 3
	std::vector<std::vector<int>> edge_node_ids{ 3 };

	std::vector<int> internal_nodes; // Internal nodes of the triangle element (for higher-order spectral elements)

	// lexi ordered nodes
	std::vector<int> lexi_ordered_node_ids; // lexicographic pattern based on barycentric coordinates

	int materialid = 0;

	// Store the edge ids
	int edge_ids[3] = { 0, 0, 0 };

	// Store the renderer triangle ID
	std::vector<renderer_triangle> renderer_tri_elements;

};

struct spectral_quadelement_store
{
	int quad_id = 0;

	std::vector<int> corner_nodes; // 4 corner nodes of the quadrilateral element

	// edge_node_ids[0] for edge 1, edge_node_ids[1] for edge 2, edge_node_ids[2] for edge 3, edge_node_ids[3] for edge 4
	std::vector<std::vector<int>> edge_node_ids{ 4 };

	std::vector<int> internal_nodes; // Internal nodes of the quadrialteral element (for higher-order spectral elements)

	// Row ordered nodes
	std::vector<int> row_ordered_node_ids; // Row ordered node ids

	int materialid = 0;

	// Store the edge ids
	int edge_ids[4] = { 0, 0, 0, 0 };

	// Store the renderer triangle ID
	std::vector<renderer_triangle> renderer_tri_elements;

};





class spectral_mesh2d
{
public:
	int spectral_order = 1; // Spectral order of the finite element method (1 for linear, 2 for quadratic, etc.)
	std::unordered_map<int, spectral_node_store> spectral_node_list;
	std::unordered_map<int, spectral_edge_store> spectral_edge_list;
	std::unordered_map<int, spectral_trielement_store> spectral_trielement_list;
	std::unordered_map<int, spectral_quadelement_store> spectral_quadelement_list;
	std::unordered_map<int, material_store> material_list;

	std::unordered_map<int, std::vector<int>> spectral_node_edge_map;


	// Store the renderer data
	std::vector<renderer_edge> renderer_edge_lines;
	std::vector<renderer_node> renderer_node_points;
	std::vector<renderer_triangle> renderer_element_triangles;


	// Store the Local IDX for the single element
	local_idx_structure quad_element_id_structure{ 4 };
	local_idx_structure tri_element_id_structure{ 3 };


	spectral_mesh2d();
	~spectral_mesh2d() = default;


	void generate_spectral_mesh(const helmholtz_system_store& linear_mesh);



private:
	helmholtz_system_store linear_mesh;


	void create_spectral_nodes(int node_id,
		double x_coord,
		double y_coord,
		bool isboundarynode,
		bool isFieldBC,
		double fieldvalue,
		double sourcevalue);


	void create_spectral_edges(edge_store edge,
		const int& startnodeid,
		const int& endnodeid,
		const int& leftfaceid,
		const int& rightfaceid,
		const std::vector<int>& edge_internal_node_ids);


	int get_edge_id(const int& startnodeid, const int& endnodeid);


	void create_spectralquad_renderer_triangles(spectral_quadelement_store& spec_quad);


	void create_spectraltri_renderer_triangles(spectral_trielement_store& spec_tri);


	void create_renderer_nodes(std::unordered_set<int>& added_nodes,
		const std::vector<int>& corner_nodes,
		const std::vector<std::vector<int>>& edge_node_ids,
		const std::vector<int>& internal_nodes);


	void create_renderer_edges(std::unordered_set<int>& added_edges,
		const std::vector<int>& corner_nodes,
		const std::vector<std::vector<int>>& edge_node_ids,
		const std::vector<int>& internal_nodes,
		const std::vector<int>& edge_ids,
		const std::vector<renderer_triangle>& renderer_tri_elements);


	void create_local_id_structure(int order);


};




