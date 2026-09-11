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
	int solverType;
	int hRefinement;
	int pRefinement;
	int formulation;
	double extendConstraints;
	double extendLoads;
	double saveHRefinedModel;

};
#pragma pack(pop)







