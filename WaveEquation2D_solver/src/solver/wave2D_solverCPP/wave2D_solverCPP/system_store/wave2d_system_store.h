#pragma once
#include "hash_utils.h"

#include <Eigen/Dense>
#include <unordered_map>


#include <cstdint>
#include <bit>          // std::bit_cast (C++20)
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <type_traits>

using hash_utils::fnv_mix;


struct node_store
{
	int node_id = 0;
	double x_coord = 0.0;
	double y_coord = 0.0;

	bool isboundarynode = false;

	//bool isFieldBC = false;
	//double fieldvalue = 0.0; // Field value in the node
	//
	//double sourcevalue = 0.0; // Source value in the node
	//double sourcefrequency = 0.0; // Source frequency in the node
	//int sourcetype = -1; // Source type in the node
	//double sourcestarttime = 0.0; // Source start time in the node	

};



struct edge_store
{
	int edge_id = 0;
	int startnodeid = 0;
	int endnodeid = 0;

	int leftfaceid = -1; // The face on the left side of the edge (when looking from start node to end node)
	int rightfaceid = -1; // The face on the right side of the edge (when looking from start node to end node)

	bool isboundaryedge = false;
	//bool isSommerfieldBC = false;
	//bool isFieldBC = false;
	//bool isDerivFieldBC = false;
	//bool isSource = false;

	//double fieldvalue = 0.0;
	//double normalderivfieldvalue = 0.0;

	//double sourcevalue = 0.0; // Source value in the node
	//double sourcefrequency = 0.0; // Source frequency in the node
	//int sourcetype = -1; // Source type in the node
	//double sourcestarttime = 0.0; // Source start time in the node	

};


struct trielement_store
{
	int tri_id = 0;
	int nodeid1 = 0;
	int nodeid2 = 0;
	int nodeid3 = 0;
	int materialid = 0;

};

struct quadelement_store
{
	int quad_id = 0;
	int nodeid1 = 0;
	int nodeid2 = 0;
	int nodeid3 = 0;
	int nodeid4 = 0;
	int materialid = 0;

};


struct node_constraint_store
{
	int node_constraint_set_id = 0;
	std::vector<int> constraint_node_ids;

	bool isFieldBC = false;
	double fieldvalue = 0.0; // Field value in the node

	double sourcevalue = 0.0; // Source value in the node
	double sourcefrequency = 0.0; // Source frequency in the node
	int sourcetype = -1; // Source type in the node
	double sourcestarttime = 0.0; // Source start time in the node	

};


struct edge_constraint_store
{
	int edge_constraint_set_id = 0;

	std::vector<int> constraint_edge_startpt_ids;
	std::vector<int> constraint_edge_endpt_ids;
	std::vector<int> constraint_edge_ids;

	bool isSommerfieldBC = false;
	bool isFieldBC = false;
	bool isDerivFieldBC = false;
	bool isSource = false;

	double fieldvalue = 0.0;
	double normalderivfieldvalue = 0.0;

	double sourcevalue = 0.0; // Source value in the node
	double sourcefrequency = 0.0; // Source frequency in the node
	int sourcetype = -1; // Source type in the node
	double sourcestarttime = 0.0; // Source start time in the node	

};



struct material_store
{
	int materialid = 0;
	double youngsmodulus = 0.0;
	double matdensity = 0.0;
	double poissonsratio = 0.0;
	double yieldpoint = 0.0;
	double thickness = 0.0;

};


class wave2d_system_store
{
public:
	int spectral_order = -1; // Spectral order of the finite element method (3, 4, 5, 6, 7, 8, 9, 10)
	std::unordered_map<int, node_store> node_list;
	std::unordered_map<int, edge_store> edge_list;
	std::unordered_map<int, trielement_store> trielement_list;
	std::unordered_map<int, quadelement_store> quadelement_list;

	std::unordered_map<int, material_store> material_list;


	std::unordered_map<int, node_constraint_store> node_constraint_list;
	std::unordered_map<int, edge_constraint_store> edge_constraint_list;	

	std::unordered_map<int, std::vector<int>> node_edge_map;


	wave2d_system_store();
	~wave2d_system_store() = default;


	uint64_t get_model_signature() const;

private:

};



