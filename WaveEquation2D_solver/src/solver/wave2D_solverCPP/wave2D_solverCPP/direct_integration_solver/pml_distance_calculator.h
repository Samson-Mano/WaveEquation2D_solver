#pragma once
#include <algorithm>
#include <cassert>
#include <cstddef>
#include <limits>
#include <unordered_map>
#include <unordered_set>
#include <vector>
#include <queue>
#include <utility>
#include <Eigen/Dense>

#include "../spectral_elements/spectral_mesh2d.h"


struct meshEdgeStore
{
	// always stored with the smaller node id first => canonical representation
	int a, b;

	// canonical: a < b
	bool operator==(const meshEdgeStore&) const noexcept = default;

};


struct meshEdgeHash 
{
	std::size_t operator()(const meshEdgeStore& e) const noexcept
	{
		std::size_t h = std::hash<int>{}(e.a);
		h ^= std::hash<int>{}(e.b) + 0x9e3779b9u + (h << 6) + (h >> 2);
		return h;
	}
};


struct InterfaceEdge 
{
	meshEdgeStore         edge;
	int                   nonpml_elem_id;  // seed for the Dijkstra
	Eigen::Vector2d       a, b;            // cached endpoints (from node_pos)
};

struct Neighbour 
{
	int element_id;               // neighbour element index
	double weight;               // Euclidean distance between centroids
};

enum class EdgeDir { Vertical, Horizontal, Oblique };

class pml_distance_calculator
{
public:
	static constexpr int PML_MATERIAL_ID = 0; 

	pml_distance_calculator() = default;
	~pml_distance_calculator() = default;


	void calculate_pml_distances(const spectral_mesh2d& spec_mesh2d, std::unordered_map<int, Eigen::Vector2d>& pml_dist);

private:
	static  meshEdgeStore makeEdge(int i, int j);

	static  double point_to_segment_distance(const Eigen::Vector2d& p, const Eigen::Vector2d& a, const Eigen::Vector2d& b);

	static  Eigen::Vector2d centroid(const std::vector<int>& node_ids, const std::unordered_map<int, Eigen::Vector2d>& node_pos);

	static  EdgeDir classify(const Eigen::Vector2d& a, const Eigen::Vector2d& b, double tol = 1e-10);

	static	std::unordered_map<int, double>	run_dijkstra(const std::unordered_map<int, std::vector<Neighbour>>& adj, const std::vector<int>& seeds);

};