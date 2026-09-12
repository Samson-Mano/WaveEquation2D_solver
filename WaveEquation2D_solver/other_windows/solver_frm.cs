using WaveEquation2D_solver.src.events_handler;
using WaveEquation2D_solver.src.model_store;
using WaveEquation2D_solver.src.model_store.rslt_objects;
using WaveEquation2D_solver.src.solver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WaveEquation2D_solver.other_windows
{
    public partial class solver_frm : Form
    {

        private modeldata_store modeldata;

        int refined_nodecount = 0;
        int refined_edgecount = 0;
        int refined_tricount = 0;
        int refined_quadcount = 0;

        public solver_frm(ref modeldata_store modeldata)
        {
            InitializeComponent();

            this.modeldata = modeldata;
        }

        private void solver_frm_Load(object sender, EventArgs e)
        {
            comboBox_solvertype.SelectedIndex = Properties.Settings.Default.Sett_solver_type;
            comboBox_HRefinement.SelectedIndex = Properties.Settings.Default.Sett_Hrefine;
            comboBox_spectralorderN.SelectedIndex = Properties.Settings.Default.Sett_Prefine-3;
            textBox_totalsimulationtime.Text = Properties.Settings.Default.Sett_totalsimulationtime.ToString();
            textBox_timeinterval.Text = Properties.Settings.Default.Sett_timeinterval.ToString();
            textBox_numberofmodes.Text = Properties.Settings.Default.Sett_numofmodes.ToString();

            checkBox_extendconstraints.Checked = true;
            checkBox_loadmodalanalysis.Checked = true;
            checkBox_saveHrefinedmodel.Checked = false;

        }

        public void updateTextBox()
        {

            double x_extent = modeldata.geom_bounds.X;
            double y_extent = modeldata.geom_bounds.Y;

            // Use general format: no decimals for large values, scientific for small (<1)
            string formatValue(double v)
            {
                if (Math.Abs(v) >= 1.0)
                    return v.ToString("F0");  // 0 digits after decimal
                else
                    return v.ToString("0.####E+0");  // scientific notation, 4 significant digits
            }


            textBox_xyextent.Text = $"[{formatValue(x_extent)}, {formatValue(y_extent)}]";

        }


        private async void button_solve_Click(object sender, EventArgs e)
        {

            try
            {
                // Check the inputs (Whether the boundary condition is applied or not)
                if (modeldata.fe_data.fe_nodeconstraints.ndcnst_set_count + modeldata.fe_data.fe_edgeconstraints.edgecnst_count == 0 )
                {

                    richTextBox_AnalysisUpdate.Clear();
                    AppendStatus("No boundary conditions applied...\n");

                    return;
                }


                calculate_solver_model_size();

                // Calculate the total size of the refined model
                AppendStatus($"Total number of nodes = {this.refined_nodecount} \n");
                AppendStatus($"Total triangle element count = {this.refined_tricount} \n");
                AppendStatus($"Total quadrilateral element count = {this.refined_quadcount} \n");

                // Continue Analysis
                if (this.refined_nodecount > 200000)
                {
                    string message = $"Refined Model Size:\n" +
                                     $"• Nodes: {this.refined_nodecount:N0}\n" +
                                     $"• Triangles: {this.refined_tricount:N0}\n" +
                                     $"• Quads: {this.refined_quadcount:N0}\n" +
                                     $"• Edges: {this.refined_edgecount:N0}\n\n" +
                                     $"This exceeds the recommended limit of 200,000 nodes.\n" +
                                     $"The analysis may be slow or run out of memory.\n\n" +
                                     $"Continue with analysis?";

                    DialogResult result = MessageBox.Show(
                        message,
                        "Large Model Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                        return;
                }


                // Run the solver
                AppendStatus("\nStarting wave2D analysis...\n");
                await RunSolverAsync();

            }
            catch (Exception ex)
            {
                AppendStatus($"\n❌ Unexpected error: {ex.Message}\n");
                AppendStatus($"Stack trace: {ex.StackTrace}\n");
                MessageBox.Show($"An unexpected error occurred:\n\n{ex.Message}",
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button
                button_solve.Enabled = true;
            }

        }



        private async Task RunSolverAsync()
        {
            try
            {

                // input files
                string inputPath = Path.Combine(Application.StartupPath, "wave2d_input.bin");
                string outputPath = Path.Combine(Application.StartupPath, "wave2d_output.bin");

                //// Delete existing input file if it exists
                //if (File.Exists(inputPath))
                //{
                //    try
                //    {
                //        File.Delete(inputPath);
                //        richTextBox_AnalysisUpdate.AppendText("Deleted existing input file.\n");
                //    }
                //    catch (IOException ex)
                //    {
                //        richTextBox_AnalysisUpdate.AppendText($"Warning: Could not delete existing input file: {ex.Message}\n");
                //        // Try to force garbage collection to release any locks
                //        GC.Collect();
                //        GC.WaitForPendingFinalizers();
                //        File.Delete(inputPath);
                //    }
                //}

                //// Delete existing output file if it exists
                //if (File.Exists(outputPath))
                //{
                //    try
                //    {
                //        File.Delete(outputPath);
                //        richTextBox_AnalysisUpdate.AppendText("Deleted existing output file.\n");
                //    }
                //    catch (IOException ex)
                //    {
                //        richTextBox_AnalysisUpdate.AppendText($"Warning: Could not delete existing output file: {ex.Message}\n");
                //        GC.Collect();
                //        GC.WaitForPendingFinalizers();
                //        File.Delete(outputPath);
                //    }
                //}



                // C# GUI exports model to a .bin file.
                // C# calls your C++ DLL (using P/Invoke).
                // C++ DLL reads the.bin file, performs the simulation, and writes results to another .bin.
                // C# re-imports and displays results.

                // Write the binary file
                file_events.export_binary_mesh(inputPath, modeldata.fe_data);

                // Test the data
                if (!double.TryParse(textBox_totalsimulationtime.Text, out double totalsimulationtime) ||
                    !double.TryParse(textBox_timeinterval.Text, out double timestep))
                {
                    AppendStatus("Invalid total simulation time or timestep.\n");
                    return;
                }


                if (!int.TryParse(textBox_numberofmodes.Text, out int numberofmodes))
                {
                    AppendStatus("Invalid number of modes.\n");
                    return;
                }

        
                // Write input file
                wave2DSolverInterop.SolverSettings solver_settings = new wave2DSolverInterop.SolverSettings();
                solver_settings.SolverType = comboBox_solvertype.SelectedIndex; // 0 = Elimination method, 1 = Lagrange method
                solver_settings.HRefinement = comboBox_HRefinement.SelectedIndex; // 0, 1, 2
                solver_settings.SpectralOrderN = comboBox_spectralorderN.SelectedIndex + 3; // 3, 4, 5, 6, ...
                solver_settings.TotalSimulationTime = totalsimulationtime; // 0, 1
                solver_settings.TimeIncrement = timestep;
                solver_settings.NumberOfModes = numberofmodes;
                solver_settings.ExtendConstraints = checkBox_extendconstraints.Checked == false ? 0 : 1;
                solver_settings.ImportModalAnalysisResults = checkBox_loadmodalanalysis.Checked == false ? 0 : 1;
                solver_settings.SaveHRefinedModel = checkBox_saveHrefinedmodel.Checked == false ? 0 : 1;
          


                bool isAnalysisSuccess = false;


                // Run solver asynchronously
                await Task.Run(() =>
                {
                    // Call C++ solver
                    wave2DSolverInterop.solve_2DwaveanalysisCPP(inputPath, outputPath, 
                        ref solver_settings,
                        ref isAnalysisSuccess, OnStatusUpdate);

                });


                if (isAnalysisSuccess)
                {
                    richTextBox_AnalysisUpdate.AppendText("Solver completed successfully!\n");

                    // Process results...
                    process_results(outputPath);

                }
                else
                {
                    richTextBox_AnalysisUpdate.AppendText("Solver failed.\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox_AnalysisUpdate.AppendText($"Unexpected error: {ex.Message}\n");
            }

        }



        private void process_results(string outputPath)
        {
            // Read the binary result file
            if (!File.Exists(outputPath))
            {
                AppendStatus("Result file not found: " + outputPath + "\n");
                return;
            }

            try
            {
                using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        modeldata.rslt_data = new rsltdata_store();

                        // Read number of nodes
                        int node_points_count = reader.ReadInt32();
                        AppendStatus($"Reading results started...\n");

                        for (int i = 0; i < node_points_count; i++)
                        {
                            int node_id = reader.ReadInt32();
                            double node_xcoord = reader.ReadDouble();
                            double node_ycoord = reader.ReadDouble();

                            // Displacement at the node
                            double displ_x = reader.ReadDouble();
                            double displ_y = reader.ReadDouble();

                            // Reaction at the node
                            int constraint_type = reader.ReadInt32(); // 0 = free, 1 = pinned, 2 = roller
                            double constraint_value = reader.ReadDouble();
                            double reaction_x = reader.ReadDouble();
                            double reaction_y = reader.ReadDouble();

                            // Stress results at the node
                            double sigma_x = reader.ReadDouble();
                            double sigma_y = reader.ReadDouble();
                            double tau_xy = reader.ReadDouble();

                            double sigma_1 = reader.ReadDouble();
                            double sigma_2 = reader.ReadDouble();

                            double von_mises = reader.ReadDouble();
                            double max_shear = reader.ReadDouble();
                            double theta_p = reader.ReadDouble();

                            double streamfunction_tension = reader.ReadDouble();
                            double streamfunction_compression = reader.ReadDouble();


                            modeldata.rslt_data.add_point(node_id,
                                node_xcoord, 
                                node_ycoord,
                                displ_x,
                                displ_y,
                                constraint_type,
                                constraint_value,
                                reaction_x,
                                reaction_y,
                                sigma_x,
                                sigma_y,
                                tau_xy,
                                sigma_1,
                                sigma_2,
                                von_mises,
                                max_shear,
                                theta_p,
                                streamfunction_tension,
                                streamfunction_compression
                                );

                        }

                        AppendStatus($"Reading results for {node_points_count} nodes complete \n");


                        // Read number of edges
                        int edge_lines_count = reader.ReadInt32();

                        for (int i = 0; i < edge_lines_count; i++)
                        {
                            int start_nodeid = reader.ReadInt32();
                            int end_nodeid = reader.ReadInt32();

                            modeldata.rslt_data.add_wireframe_line(i, start_nodeid, end_nodeid);

                        }

                        AppendStatus($"Reading results for {edge_lines_count} edges complete \n");


                        // Read number of triangles
                        int triangles_count = reader.ReadInt32();

                        for (int i = 0; i < triangles_count; i++)
                        {
                            int n1 = reader.ReadInt32();
                            int n2 = reader.ReadInt32();
                            int n3 = reader.ReadInt32();

                            modeldata.rslt_data.add_tri(i, n1, n2, n3);

                        }

                        AppendStatus($"Reading results for {triangles_count} triangles complete \n");

                        // Set the Result mesh
                        bool isValid = modeldata.rslt_data.set_result_extremes();

                        if(!isValid)
                        {
                            modeldata.rslt_data = new rsltdata_store();

                            AppendStatus("Error: Invalid result extremes detected.\n");
                            MessageBox.Show("Error: Invalid result extremes detected. Please check the inputs.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }


                        modeldata.rslt_data.create_buffer_data();
                        modeldata.IsResultSet = true;

                        // Call the main form
                        if (this.Owner is main_frm mainForm)
                        {
                            // mainForm.set_ResultOption(1);
                        }

                        AppendStatus("Results read complete!\n");

                        // MessageBox.Show("Solve completed successfully!", "Success",
                        //     MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
            }
            catch (Exception ex)
            {
                AppendStatus("Error reading binary results: " + ex.Message + "\n");
                MessageBox.Show("Error reading results file:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //________________________
        }





        private void calculate_solver_model_size()
        {
            int num_of_nodecount = modeldata.fe_data.fe_nodes.node_count;
            int num_of_edges = modeldata.fe_data.number_of_edges;
            int num_of_trielements = modeldata.fe_data.fe_tris.elementtri_count;
            int num_of_quadelements = modeldata.fe_data.fe_quads.elementquad_count;

            // Get refinement levels (0-based indexing)
            int h_refinement = comboBox_HRefinement.SelectedIndex; // 0, 1, 2
            int p_refinement = comboBox_spectralorderN.SelectedIndex + 3; // 3, 4, 5, 6, ...

            // Initialize with original mesh
            int h_refined_nodecount = num_of_nodecount;
            int h_refined_edgecount = num_of_edges;
            int h_refined_tricount = num_of_trielements;
            int h_refined_quadcount = num_of_quadelements;

            // === H-REFINEMENT ===
            if (h_refinement == 0) // Original mesh
            {
                // No change
            }
            else if (h_refinement == 1) // Split into 4 (h=2)
            {
                // Elements multiply by 4
                h_refined_tricount = num_of_trielements * 4;
                h_refined_quadcount = num_of_quadelements * 4;

                // Edges: Each original edge gets split into 2
                h_refined_edgecount = num_of_edges * 2;

                // Nodes: Original nodes + edge nodes + quad center nodes
                int edgeNodes = num_of_edges * 1;        // 1 new node per edge
                int quadCenterNodes = num_of_quadelements * 1; // 1 new node per quad
                                                               // Note: Triangles don't add center nodes for h=2 refinement
                h_refined_nodecount = num_of_nodecount + edgeNodes + quadCenterNodes;
            }
            else if (h_refinement == 2) // Split into 16 (h=4)
            {
                // Elements multiply by 16
                h_refined_tricount = num_of_trielements * 16;
                h_refined_quadcount = num_of_quadelements * 16;

                // Edges: Each original edge gets split into 4
                h_refined_edgecount = num_of_edges * 4;

                // Nodes: Original nodes + edge nodes + triangle internal nodes + quad internal nodes
                int edgeNodes = num_of_edges * 3;         // 3 new nodes per original edge
                int triInternalNodes = num_of_trielements * 3;  // 3 internal nodes per triangle
                int quadInternalNodes = num_of_quadelements * 9; // 9 internal nodes per quad
                h_refined_nodecount = num_of_nodecount + edgeNodes + triInternalNodes + quadInternalNodes;
            }

            // === P-REFINEMENT ===
            int p_refined_nodecount = h_refined_nodecount;
            int p_refined_edgecount = h_refined_edgecount;
            int p_refined_tricount = h_refined_tricount;
            int p_refined_quadcount = h_refined_quadcount;

             if (p_refinement == 3) // p=3 (Spectral order = 3)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 2)    // Edge nodes (2 per edge)
                                    + (h_refined_tricount * 1)    // Triangle internal nodes (1 per tri)
                                    + (h_refined_quadcount * 4);  // Quad internal nodes (4 per quad)
            }
            else if (p_refinement == 4) // p=4 (Spectral order = 4)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 3)    // Edge nodes (3 per edge)
                                    + (h_refined_tricount * 3)    // Triangle internal nodes (3 per tri)
                                    + (h_refined_quadcount * 9);  // Quad internal nodes (9 per quad)
            }
            else if (p_refinement == 5) // p=5 (Spectral order = 5)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 4)    // Edge nodes (4 per edge)
                                    + (h_refined_tricount * 6)    // Triangle internal nodes (6 per tri)
                                    + (h_refined_quadcount * 16);  // Quad internal nodes (16 per quad)
            }
            else if (p_refinement == 6) // p=6 (Spectral order = 6)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 5)    // Edge nodes (5 per edge)
                                    + (h_refined_tricount * 10)    // Triangle internal nodes (10 per tri)
                                    + (h_refined_quadcount * 25);  // Quad internal nodes (25 per quad)
            }
            else if (p_refinement == 7) // p=7 (Spectral order = 7)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 6)    // Edge nodes (6 per edge)
                                    + (h_refined_tricount * 15)    // Triangle internal nodes (15 per tri)
                                    + (h_refined_quadcount * 36);  // Quad internal nodes (36 per quad)
            }
            else if (p_refinement == 8) // p=8 (Spectral order = 8)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 7)    // Edge nodes (7 per edge)
                                    + (h_refined_tricount * 21)    // Triangle internal nodes (21 per tri)
                                    + (h_refined_quadcount * 49);  // Quad internal nodes (49 per quad)
            }
            else if (p_refinement == 9) // p=9 (Spectral order = 9)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 8)    // Edge nodes (8 per edge)
                                    + (h_refined_tricount * 28)    // Triangle internal nodes (28 per tri)
                                    + (h_refined_quadcount * 64);  // Quad internal nodes (64 per quad)
            }
            else if (p_refinement == 10) // p=10 (Spectral order = 10)
            {
                p_refined_nodecount = h_refined_nodecount
                                    + (h_refined_edgecount * 9)    // Edge nodes (9 per edge)
                                    + (h_refined_tricount * 36)    // Triangle internal nodes (36 per tri)
                                    + (h_refined_quadcount * 81);  // Quad internal nodes (81 per quad)
            }


            // Store results
            this.refined_nodecount = p_refined_nodecount;
            this.refined_edgecount = p_refined_edgecount;
            this.refined_tricount = p_refined_tricount;
            this.refined_quadcount = p_refined_quadcount;
        }


        private void save_defaults()
        {
            Properties.Settings.Default.Sett_solver_type = comboBox_solvertype.SelectedIndex;
            Properties.Settings.Default.Sett_Hrefine = comboBox_HRefinement.SelectedIndex;
            Properties.Settings.Default.Sett_Prefine = comboBox_spectralorderN.SelectedIndex + 3;
            Properties.Settings.Default.Sett_totalsimulationtime = double.Parse(textBox_totalsimulationtime.Text);
            Properties.Settings.Default.Sett_timeinterval = double.Parse(textBox_timeinterval.Text);
            Properties.Settings.Default.Sett_numofmodes = int.Parse(textBox_numberofmodes.Text);
            
            Properties.Settings.Default.Save();
        }


        private void OnStatusUpdate(string message)
        {
            // Marshal back to UI thread safely
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendStatus(message + "\n")));
            }
            else
            {
                AppendStatus(message + "\n");
            }
        }

        private void AppendStatus(string text)
        {
            richTextBox_AnalysisUpdate.AppendText(text);
            richTextBox_AnalysisUpdate.ScrollToCaret();
        }

    

    }
}
