#pragma once
#include <utility>
#include <Eigen/Dense>

#include "../spectral_elements/spectral_mesh2d.h"


struct regionEdgeStore
{
	// always stored with the smaller node id first => canonical representation
	int a, b;

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





class pml_region_calculator
{
public:
	static constexpr int PML_MATERIAL_ID = 0;

	pml_region_calculator() = default;
	~pml_region_calculator() = default;


	void set_pml_region(const spectral_mesh2d& spec_mesh2d);


private:




};


