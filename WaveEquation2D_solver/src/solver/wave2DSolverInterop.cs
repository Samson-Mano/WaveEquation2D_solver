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
            public int PRefinement;          // 0, 1, 2, 3
            public int Formulation;          // 0, 1
            public double ExtendConstraints; // 0.0 or 1.0
            public double ExtendLoads;       // 0.0 or 1.0
            public double SaveHRefinedModel; // 0.0 or 1.0
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
