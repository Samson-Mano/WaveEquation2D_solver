#include "wave2d_system_store.h"


wave2d_system_store::wave2d_system_store()
{
	// Empty constructor
}



uint64_t wave2d_system_store::get_model_signature() const
{
	// Create a hash signature for the model based on its components
	// Distinct seeds so that different sections can't accidentally collide
	// when their contents happen to permute.
	constexpr std::uint64_t kSeedNodes = 0x01;
	constexpr std::uint64_t kSeedQuads = 0x02;
	constexpr std::uint64_t kSeedTris = 0x03;
	constexpr std::uint64_t kSeedMaterial = 0x04;
	constexpr std::uint64_t kSeedNodeBC = 0x05;
	constexpr std::uint64_t kSeedEdgeBC = 0x06;
	constexpr std::uint64_t kSeedTop = 0x07;

	// ---------------------------------------------------------------- nodes
	std::uint64_t node_h = kSeedNodes;
	node_h = fnv_mix(node_h, static_cast<std::uint64_t>(node_list.size()));

	for (const auto& [key, nd] : node_list)
	{
		node_h = fnv_mix(node_h, nd.node_id);

		// Round to 8 decimal places to avoid floating-point precision issues
		double x_coord_rounded = std::round(nd.x_coord * 1e8) / 1e8;
		double y_coord_rounded = std::round(nd.y_coord * 1e8) / 1e8;

		node_h = fnv_mix(node_h, std::bit_cast<std::uint64_t>(x_coord_rounded));
		node_h = fnv_mix(node_h, std::bit_cast<std::uint64_t>(y_coord_rounded));
	}

	// --------------------------------------------------------------- quads
	std::uint64_t quadelem_h = kSeedQuads;
	quadelem_h = fnv_mix(quadelem_h,
		static_cast<std::uint64_t>(quadelement_list.size()));

	for (const auto& [key, quad] : quadelement_list)
	{
		quadelem_h = fnv_mix(quadelem_h, quad.quad_id);
		quadelem_h = fnv_mix(quadelem_h, quad.nodeid1);
		quadelem_h = fnv_mix(quadelem_h, quad.nodeid2);
		quadelem_h = fnv_mix(quadelem_h, quad.nodeid3);
		quadelem_h = fnv_mix(quadelem_h, quad.nodeid4);
		quadelem_h = fnv_mix(quadelem_h, quad.materialid);
	}

	// ---------------------------------------------------------------- tris
	std::uint64_t trielem_h = kSeedTris;
	trielem_h = fnv_mix(trielem_h,
		static_cast<std::uint64_t>(trielement_list.size()));

	for (const auto& [key, tri] : trielement_list)
	{
		trielem_h = fnv_mix(trielem_h, tri.tri_id);
		trielem_h = fnv_mix(trielem_h, tri.nodeid1);
		trielem_h = fnv_mix(trielem_h, tri.nodeid2);
		trielem_h = fnv_mix(trielem_h, tri.nodeid3);
		trielem_h = fnv_mix(trielem_h, tri.materialid);
	}

	// ------------------------------------------------------------ material
	std::uint64_t material_h = kSeedMaterial;
	material_h = fnv_mix(material_h,
		static_cast<std::uint64_t>(material_list.size()));

	for (const auto& [key, material] : material_list)
	{
		material_h = fnv_mix(material_h, material.materialid);
		material_h = fnv_mix(material_h, std::bit_cast<std::uint64_t>(material.matdensity));
		material_h = fnv_mix(material_h, std::bit_cast<std::uint64_t>(material.youngsmodulus));
		material_h = fnv_mix(material_h, std::bit_cast<std::uint64_t>(material.poissonsratio));
		material_h = fnv_mix(material_h, std::bit_cast<std::uint64_t>(material.yieldpoint));
		material_h = fnv_mix(material_h, std::bit_cast<std::uint64_t>(material.thickness));
	}


	// ------------------------------------------------- node constraints BC
	// Source BC is intentionally excluded.
	std::uint64_t node_constraint_h = kSeedNodeBC;
	node_constraint_h = fnv_mix(node_constraint_h,
		static_cast<std::uint64_t>(node_constraint_list.size()));

	for (const auto& [key, node_cnstr] : node_constraint_list)
	{
		if (!node_cnstr.isFieldBC) continue;

		node_constraint_h = fnv_mix(node_constraint_h,
			node_cnstr.node_constraint_set_id);

		node_constraint_h = fnv_mix(node_constraint_h,
			std::bit_cast<std::uint64_t>(node_cnstr.fieldvalue));

		// Sort the node IDs so the hash is independent of container order.
		std::vector<std::uint64_t> ids(node_cnstr.constraint_node_ids.begin(),
			node_cnstr.constraint_node_ids.end());
		std::sort(ids.begin(), ids.end());
		node_constraint_h = fnv_mix(node_constraint_h,
			static_cast<std::uint64_t>(ids.size()));
		for (auto id : ids)
			node_constraint_h = fnv_mix(node_constraint_h, id);
	}

	// ------------------------------------------------- edge constraints BC
	std::uint64_t edge_constraint_h = kSeedEdgeBC;
	edge_constraint_h = fnv_mix(edge_constraint_h,
		static_cast<std::uint64_t>(edge_constraint_list.size()));

	for (const auto& [key, edge_cnstr] : edge_constraint_list)
	{
		if (!edge_cnstr.isFieldBC && !edge_cnstr.isSommerfieldBC) continue;

		edge_constraint_h = fnv_mix(edge_constraint_h,
			edge_cnstr.edge_constraint_set_id);
		edge_constraint_h = fnv_mix(edge_constraint_h,
			std::bit_cast<std::uint64_t>(edge_cnstr.fieldvalue));
		edge_constraint_h = fnv_mix(edge_constraint_h,
			std::bit_cast<std::uint64_t>(edge_cnstr.normalderivfieldvalue));

		auto mix_sorted = [&](auto const& container)
			{
				using value_t = typename std::decay_t<decltype(container)>::value_type;
				std::vector<value_t> ids(container.begin(), container.end());
				std::sort(ids.begin(), ids.end());
				edge_constraint_h = fnv_mix(edge_constraint_h,
					static_cast<std::uint64_t>(ids.size()));
				for (auto id : ids)
					edge_constraint_h = fnv_mix(edge_constraint_h, id);
			};

		mix_sorted(edge_cnstr.constraint_edge_ids);
		mix_sorted(edge_cnstr.constraint_edge_startpt_ids);
		mix_sorted(edge_cnstr.constraint_edge_endpt_ids);
	}

	// ------------------------------------------------------------- combine
	std::uint64_t h = kSeedTop;
	h = fnv_mix(h, static_cast<std::uint64_t>(spectral_order));
	h = fnv_mix(h, node_h);
	h = fnv_mix(h, quadelem_h);
	h = fnv_mix(h, trielem_h);
	h = fnv_mix(h, material_h);
	h = fnv_mix(h, node_constraint_h);
	h = fnv_mix(h, edge_constraint_h);
	return h;

}





