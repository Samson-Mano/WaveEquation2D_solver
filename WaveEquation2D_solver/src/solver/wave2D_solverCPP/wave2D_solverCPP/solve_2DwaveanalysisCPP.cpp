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




}




