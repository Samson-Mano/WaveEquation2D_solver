using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WaveEquation2D_solver.src.solver
{
    public static class wave2DSolverInterop
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SolverSettings
        {
            public int SolverType;           // 0 = Elimination, 1 = Lagrange
            public int HRefinement;          // 0, 1, 2
            public int SpectralOrderN;          //  3, 4, 5, 6, 7, 8, 9, 10
            public double TotalSimulationTime; // Total simulation time
            public double TimeIncrement;         // Time increment for the simulation
            public int NumberOfModes;          // Number of modes to consider in the analysis

            public int ExtendConstraints; // 0 or 1
            public int ImportModalAnalysisResults; // 0 or 1
            public int SaveHRefinedModel; // 0 or 1
        }




        // Declare the callback delegate
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallbackDelegate([MarshalAs(UnmanagedType.LPStr)] string message);



        // Import the DLL function (updated to accept callback)
        [DllImport("wave2D_solverCPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void solve_2DwaveanalysisCPP(
            [MarshalAs(UnmanagedType.LPStr)] string inputPath,
            [MarshalAs(UnmanagedType.LPStr)] string outputPath,
            ref SolverSettings settings,
            ref bool isAnalysisSuccess,
            CallbackDelegate callback
        );






    }
}
