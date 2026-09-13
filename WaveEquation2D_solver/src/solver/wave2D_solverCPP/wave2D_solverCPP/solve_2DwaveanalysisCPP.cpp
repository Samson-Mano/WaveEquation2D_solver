#include <iostream>
#include <fstream>
#include <vector>
#include <cmath>
#include <string>
#include <cstdint>
#include <iomanip>
#include <sstream>

#include "h_refinement/h_refinement_store.h"
#include "system_store/wave2d_system_store.h"
#include "system_store/stopwatch_events.h"
// #include "solver/wave2d_solver.h"


#pragma pack(push, 1)
struct SolverSettings
{
	int SolverType;
	int HRefinement; // 0, 1, 2
	int SpectralOrderN; // 3, 4, 5, 6, 7, 8, 9, 10

	double TotalSimulationTime; // Total simulation time
	double TimeIncrement;         // Time increment for the simulation
	int NumberOfModes;          // Number of modes to consider in the analysis

	int ExtendConstraints; // 0 or 1
	int ImportModalAnalysisResults; // 0 or 1
	int SaveHRefinedModel; // 0 or 1

};
#pragma pack(pop)



// Function to solve the system setting from C# or Python
extern "C" __declspec(dllexport) void solve_2DwaveanalysisCPP(const char* input_file,
	const char* output_file,
	SolverSettings* settings,
	bool* isAnalysisSuccess,
	void(*callback)(const char*))
{


	if (callback) callback("Initializing solver...");
	(*isAnalysisSuccess) = false;



	std::string msg = "";

	if (!settings)
	{
		msg = "Solver settings error";
		if (callback) callback(msg.c_str());

		return;
	}


	// Example placeholder
	std::ifstream infile(input_file, std::ios::binary);
	std::ofstream outfile(output_file, std::ios::binary);

	// Read the solver settings
	int SolverType = settings->SolverType;
	int HRefinement = settings->HRefinement; // 0, 1, 2
	int SpectralOrderN = settings->SpectralOrderN; // 3, 4, 5, 6, 7, 8, 9, 10

	double TotalSimulationTime = settings->TotalSimulationTime; // Total simulation time
	double TimeIncrement = settings->TimeIncrement;         // Time increment for the simulation
	int NumberOfModes = settings->NumberOfModes;          // Number of modes to consider in the analysis

	int ExtendConstraints = settings->ExtendConstraints; // 0 or 1
	int ImportModalAnalysisResults = settings->ImportModalAnalysisResults; // 0 or 1
	int SaveHRefinedModel = settings->SaveHRefinedModel; // 0 or 1

	msg = ", H_refinement order = " + std::to_string(HRefinement) + ", Spectral order = " +
		std::to_string(SpectralOrderN);

	if (callback) callback(msg.c_str());


	stopwatch_events stopwatch;
	std::stringstream stopwatch_elapsed_str;

	// Start the solver stop watch
	stopwatch.start();



	if (!infile.is_open())
	{
		msg = "Error: Unable to open input file: " + std::string(input_file);
		if (callback) callback(msg.c_str());

		(*isAnalysisSuccess) = false;

		// std::cerr << "Error: Unable to open input file: " << input_file << std::endl;
		return;
	}


	if (!outfile.is_open())
	{
		msg = "Error: Unable to open output file: " + std::string(output_file);
		if (callback) callback(msg.c_str());

		(*isAnalysisSuccess) = false;

		// std::cerr << "Error: Unable to open output file: " << output_file << std::endl;
		return;
	}



	//_______________________________________________________________________________________
	// Read the elements for H Refinement module

	h_refinement_store h_refinement_model;

	// ---------- Nodes ----------
	int32_t nodeCount;
	infile.read(reinterpret_cast<char*>(&nodeCount), 4);

	for (int i = 0; i < nodeCount; i++)
	{
		int32_t node_id = 0; double x_coord = 0.0, y_coord = 0.0;

		infile.read(reinterpret_cast<char*>(&node_id), 4);
		infile.read(reinterpret_cast<char*>(&x_coord), 8);
		infile.read(reinterpret_cast<char*>(&y_coord), 8);

		// Add node to the H Refinement system store
		h_refinement_model.add_node(node_id, x_coord, y_coord);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading nodes at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());



	// ---------- Tri Elements ----------
	int32_t triCount;
	infile.read(reinterpret_cast<char*>(&triCount), 4);

	for (int i = 0; i < triCount; i++)
	{
		int32_t tri_id = 0, nodeid1 = 0, nodeid2 = 0, nodeid3 = 0, materialid = 0;

		infile.read(reinterpret_cast<char*>(&tri_id), 4);
		infile.read(reinterpret_cast<char*>(&nodeid1), 4);
		infile.read(reinterpret_cast<char*>(&nodeid2), 4);
		infile.read(reinterpret_cast<char*>(&nodeid3), 4);
		infile.read(reinterpret_cast<char*>(&materialid), 4);

		// Add tri element to the H Refinement system store
		h_refinement_model.add_trielement(tri_id, nodeid1, nodeid2, nodeid3, materialid);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading triangular elements at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());


	// ---------- Quad Elements ----------
	int32_t quadCount;
	infile.read(reinterpret_cast<char*>(&quadCount), 4);

	for (int i = 0; i < quadCount; i++)
	{
		int32_t quad_id = 0, nodeid1 = 0, nodeid2 = 0, nodeid3 = 0, nodeid4 = 0, materialid = 0;

		infile.read(reinterpret_cast<char*>(&quad_id), 4);
		infile.read(reinterpret_cast<char*>(&nodeid1), 4);
		infile.read(reinterpret_cast<char*>(&nodeid2), 4);
		infile.read(reinterpret_cast<char*>(&nodeid3), 4);
		infile.read(reinterpret_cast<char*>(&nodeid4), 4);
		infile.read(reinterpret_cast<char*>(&materialid), 4);

		// Add quad element to the H Refinement system store
		h_refinement_model.add_quadelement(quad_id, nodeid1, nodeid2, nodeid3, nodeid4, materialid);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading quadrilateral elements at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());



	// ---------- Materials ----------
	int32_t matCount;
	infile.read(reinterpret_cast<char*>(&matCount), 4);

	for (int i = 0; i < matCount; i++)
	{
		int32_t materialid = 0, numelement = 0;
		double material_density = 0.0, youngs_modulus = 0.0, poissons_ratio = 0.0;
		double yield_point = 0.0, thickness = 0.0;

		infile.read(reinterpret_cast<char*>(&materialid), 4);

		int32_t nameLen;
		infile.read(reinterpret_cast<char*>(&nameLen), 4);

		std::string matname(nameLen, '\0');
		infile.read(&matname[0], nameLen);

		infile.read(reinterpret_cast<char*>(&material_density), 8);
		infile.read(reinterpret_cast<char*>(&youngs_modulus), 8);
		infile.read(reinterpret_cast<char*>(&poissons_ratio), 8);
		infile.read(reinterpret_cast<char*>(&yield_point), 8);
		infile.read(reinterpret_cast<char*>(&thickness), 8);

		// Add material to the H Refinement system store
		h_refinement_model.add_material(materialid, youngs_modulus, material_density, poissons_ratio,
			yield_point, thickness);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading materials at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());




	// ---------- Node Constraints ----------
	int32_t ndCnstCount;
	infile.read(reinterpret_cast<char*>(&ndCnstCount), 4);

	for (int i = 0; i < ndCnstCount; i++)
	{
		int32_t nodeConstraintsetid = 0;
		double fieldvalue = 0.0 , sourcevalue = 0.0, sourcefrequency = 0.0, sourcestarttime = 0.0;
		int32_t sourcetype = -1;
		bool isFieldBC = false;

		infile.read(reinterpret_cast<char*>(&nodeConstraintsetid), 4);
		infile.read(reinterpret_cast<char*>(&fieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&sourcevalue), 8);
		infile.read(reinterpret_cast<char*>(&sourcefrequency), 8);
		infile.read(reinterpret_cast<char*>(&sourcetype), 4);
		infile.read(reinterpret_cast<char*>(&sourcestarttime), 8);
		infile.read(reinterpret_cast<char*>(&isFieldBC), 1);

		int32_t nodeidCount;
		infile.read(reinterpret_cast<char*>(&nodeidCount), 4);

		std::vector<int> node_id_list;

		for (int j = 0; j < nodeidCount; j++)
		{
			int32_t node_id = 0;
			infile.read(reinterpret_cast<char*>(&node_id), 4);

			// Update the constraint of the node where constraints are applied
			node_id_list.push_back(node_id);
		}

		// Add node constraints to the H Refinement system store
		h_refinement_model.add_nodeconstraint(nodeConstraintsetid, node_id_list, isFieldBC, 
			fieldvalue, sourcevalue, sourcefrequency, sourcetype, sourcestarttime);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading node constraints at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());




	// ---------- Edge Constraints ----------
	int32_t edCnstCount;
	infile.read(reinterpret_cast<char*>(&edCnstCount), 4);

	for (int i = 0; i < edCnstCount; i++)
	{
		int32_t edgeConstraintsetid = 0;
		double fieldvalue = 0.0, normalderivfieldvalue = 0.0;
		double sourcevalue = 0.0, sourcefrequency = 0.0, sourcestarttime = 0.0;
		int32_t sourcetype = -1;
		bool isFieldBC = false, isSommerfieldBC = false, isDerivFieldBC = false, isSource = false;

		infile.read(reinterpret_cast<char*>(&edgeConstraintsetid), 4);
		infile.read(reinterpret_cast<char*>(&fieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&normalderivfieldvalue), 8);
		infile.read(reinterpret_cast<char*>(&sourcevalue), 8);
		infile.read(reinterpret_cast<char*>(&sourcefrequency), 8);
		infile.read(reinterpret_cast<char*>(&sourcetype), 4);
		infile.read(reinterpret_cast<char*>(&sourcestarttime), 8);
		infile.read(reinterpret_cast<char*>(&isFieldBC), 1);
		infile.read(reinterpret_cast<char*>(&isSommerfieldBC), 1);
		infile.read(reinterpret_cast<char*>(&isDerivFieldBC), 1);
		infile.read(reinterpret_cast<char*>(&isSource), 1);

		int32_t edgeidCount;
		infile.read(reinterpret_cast<char*>(&edgeidCount), 4);

		std::vector<int> edge_id_list;
		std::vector<int> edge_startpt_id_list;
		std::vector<int> edge_endpt_id_list;

		for (int j = 0; j < edgeidCount; j++)
		{
			int32_t edge_id = 0;
			infile.read(reinterpret_cast<char*>(&edge_id), 4);
			edge_id_list.push_back(edge_id);

			int32_t startnode_id = 0;
			infile.read(reinterpret_cast<char*>(&startnode_id), 4);
			edge_startpt_id_list.push_back(startnode_id);		

			int32_t endnode_id = 0;
			infile.read(reinterpret_cast<char*>(&endnode_id), 4);
			edge_endpt_id_list.push_back(endnode_id);

		}


		// Add edge constraints to the H Refinement system store
		h_refinement_model.add_edgeconstraint(edgeConstraintsetid, edge_id_list, edge_startpt_id_list, edge_endpt_id_list, 
			isSommerfieldBC, isFieldBC, isDerivFieldBC, isSource,
			fieldvalue, normalderivfieldvalue, sourcevalue, sourcefrequency, sourcetype, sourcestarttime);

	}

	stopwatch_elapsed_str.str("");       // clear the string content
	stopwatch_elapsed_str.clear();       // clear any error flags
	stopwatch_elapsed_str << std::fixed << std::setprecision(6) << stopwatch.elapsed();

	msg = "Finished reading edge constraints at " + stopwatch_elapsed_str.str() + " secs";
	if (callback) callback(msg.c_str());



	// Create the edge
	h_refinement_model.create_edge_wireframe();

	// Preform refinement
	h_refinement_model.perform_refinement(HRefinement, ExtendConstraints, &stopwatch, callback);


	// Print the H Refined binary file for testing
	if (SaveHRefinedModel == true && HRefinement > 0)
	{
		h_refinement_model.save_hrefined_model();
	}

	//______________________________________________________________________________________
	// Copy H Refined Mesh to the wave analyzer
	wave2d_system_store wave_system;

	wave_system.spectral_order = SpectralOrderN;

	wave_system.node_list = std::move(h_refinement_model.node_list);
	wave_system.edge_list = std::move(h_refinement_model.edge_list);
	wave_system.trielement_list = std::move(h_refinement_model.trielement_list);
	wave_system.quadelement_list = std::move(h_refinement_model.quadelement_list);

	wave_system.material_list = std::move(h_refinement_model.material_list);

	wave_system.node_constraint_list = std::move(h_refinement_model.node_constraint_list);
	wave_system.edge_constraint_list = std::move(h_refinement_model.edge_constraint_list);

	wave_system.node_edge_map = std::move(h_refinement_model.node_edge_map);



	if (ImportModalAnalysisResults == 0)
	{
		// Perform modal analysis


	}
	else
	{
		// Read modal analysis results from file


	}





}




