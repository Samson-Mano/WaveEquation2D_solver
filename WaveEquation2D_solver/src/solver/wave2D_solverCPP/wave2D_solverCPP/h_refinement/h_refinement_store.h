#pragma once
#include <Eigen/Dense>
#include <unordered_map>
#include <unordered_set>

#include <fstream>

#include <iomanip> // to get std::setprecision()

#include "../system_store/stopwatch_events.h"
#include "../system_store/wave2d_system_store.h"

class h_refinement_store
{
public:

	std::unordered_map<int, node_store> node_list;
	std::unordered_map<int, edge_store> edge_list;
	std::unordered_map<int, trielement_store> trielement_list;
	std::unordered_map<int, quadelement_store> quadelement_list;
	std::unordered_map<int, material_store> material_list;

	std::unordered_map<int, node_constraint_store> node_constraint_list;
	std::unordered_map<int, edge_constraint_store> edge_constraint_list;

	std::unordered_map<int, std::vector<int>> node_edge_map;


	h_refinement_store();
	~h_refinement_store() = default;


	void add_node(const int& node_id,
		const double& x_coord,
		const double& y_coord);


	void add_trielement(const int& tri_id,
		const int& nodeid1,
		const int& nodeid2,
		const int& nodeid3,
		const int& materialid);

	void add_quadelement(const int& quad_id,
		const int& nodeid1,
		const int& nodeid2,
		const int& nodeid3,
		const int& nodeid4,
		const int& materialid);

	void create_edge_wireframe();


	void add_material(const int& materialid,
		const double& youngsmodulus,
		const double& matdensity,
		const double& poissonsratio,
		const double& yieldpoint, 
		const double& thickness);


	void add_nodeconstraint(const int& node_constraint_set_id,
		std::vector<int>& node_ids,
		const bool& isFieldBC,
		const double& fieldvalue,
		const double& sourcevalue,
		const double& sourcefrequency,
		const int& sourcetype,
		const double& sourcestarttime);


	void add_edgeconstraint(const int& edge_constraint_set_id,
		std::vector<int>& constraint_edge_startpt_ids,
		std::vector<int>& constraint_edge_endpt_ids,
		std::vector<int>& constraint_edge_ids,
		const bool& isSommerfieldBC,
		const bool& isFieldBC,
		const bool& isDerivFieldBC,
		const bool& isSource,
		const double& fieldvalue,
		const double& normalderivfieldvalue,
		const double& sourcevalue,
		const double& sourcefrequency,
		const int& sourcetype,
		const double& sourcestarttime);



	void perform_refinement(int h_refinement, bool isConstraintExtend,
		stopwatch_events* stopwatch,
		void(*callback)(const char*));

	void save_hrefined_model();


private:

	bool isConstraintExtend = false;

	stopwatch_events* m_stopwatch;


	void set_edge_faceid(const int& startnodeid, const int& endnodeid, const int& face_id);

	int get_edge_id(const int& startnodeid, const int& endnodeid);

	void renumber_model();

	void refine_elements();

	void extend_nodeconstraints_to_midnodes(const std::unordered_map<int, int>& edge_to_node_ids);


	void recreate_edges();

	void(*m_callback)(const char*) = nullptr;

	void report(const char* msg);

	// void refine_elements1();

};

