#pragma once
#include <utility>
#include <Eigen/Dense>

#include "../spectral_elements/spectral_mesh2d.h"


struct regionEdgeStore
{
	// always stored with the smaller node id first => canonical representation
	int a = -1, b = -1;

	// canonical: a < b
	bool operator==(const regionEdgeStore&) const noexcept = default;

};


struct regionEdgeHash
{
	std::size_t operator()(const regionEdgeStore& e) const noexcept
	{
		std::size_t h = std::hash<int>{}(e.a);
		h ^= std::hash<int>{}(e.b) + 0x9e3779b9u + (h << 6) + (h >> 2);
		return h;
	}
};


struct regionBoundary
{
	// std::vector<regionEdgeStore> boundaryedges;


	std::vector<regionEdgeStore> inner_edges; // PML <=> non-PML
	std::vector<regionEdgeStore> outer_edges; // PML <=> nothing


	std::vector<int> pml_trielements;
	std::vector<int> pml_quadelements;

};

struct pmlDistanceStore
{

	double d = 0.0;   // distance to inner interface
	double L = 0.0;   // local PML thickness along the interface normal at the closest point
	
	double dx = 0.0, dy = 0.0;   // d split by normal (optional)
	double Lx = 0.0, Ly = 0.0;   // L split by normal (optional)
	
	Eigen::Vector2d n{ 0,0 };      // outward interface normal at the closest poin

};



class pml_region_calculator
{
public:
	static constexpr int PML_MATERIAL_ID = 0;

	pml_region_calculator() = default;
	~pml_region_calculator() = default;


	void set_pml_region(const spectral_mesh2d& spec_mesh2d, std::unordered_map<int, pmlDistanceStore>& pml_distances);


private:

	std::vector<regionBoundary> pml_regions;

	std::vector<std::vector<regionEdgeStore>> getRegionBoundaries(const std::vector<regionEdgeStore>& edges); // get the closed boundaries from the edges

	std::vector<std::vector<regionEdgeStore>> getRegionBoundaries(const std::vector<regionEdgeStore>& edges,
		const std::unordered_map<int, Eigen::Vector2d>& node_pos);

	void setPMLRegionBoundaries(spectral_mesh2d spec_mesh2d,
		const std::vector<std::vector<regionEdgeStore>>& region_boundaries,
		const std::unordered_map<int, Eigen::Vector2d>& node_pos,
		const std::unordered_set<int>& pml_trielements,
		const std::unordered_set<int>& pml_quadelements);


	bool pointInPolygon(const Eigen::Vector2d& p,
		const std::vector<regionEdgeStore>& loop,
		const std::unordered_map<int, Eigen::Vector2d>& pos);


	void setPMLDistances(const spectral_mesh2d& spec_mesh2d,
		const std::unordered_map<int, Eigen::Vector2d>& node_pos,
		std::unordered_map<int, pmlDistanceStore>& pml_distances);


	double raySegment(const Eigen::Vector2d& p, const Eigen::Vector2d& dhat,
		const Eigen::Vector2d& a, const Eigen::Vector2d& b);


	std::pair<Eigen::Vector2d, double> closestOnSegment(const Eigen::Vector2d& p, 
		const Eigen::Vector2d& a, const Eigen::Vector2d& b);

};


