// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WaveEquation2D_solver.other_windows;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store;
using WaveEquation2D_solver.src.model_store.geom_objects;



namespace WaveEquation2D_solver
{
    public partial class main_frm : Form
    {

        // main model data store
        public modeldata_store modeldata;

        // Zoom To Fit 
        private Timer zoomToFitTimer;

        // Refreh and FPS Tracking variables
        // private Timer refreshStatusResetTimer;
        private Stopwatch fpsStopwatch = new Stopwatch();


        // Forms
        private option_frm option_Form;
        private matprop_frm matprop_Form;
        private edgeconstraint_frm edgeconstraint_Form;
        private nodalconstraint_frm nodalconstraint_Form;

        private solver_frm solver_Form;
        //private rsltoption_frm rsltoption_Form;
        //private annotate_frm annotate_Form;

        // Drawing area Axis data store
        public axisdata_store axisdata;



        public main_frm()
        {
            InitializeComponent();

            modeldata = new modeldata_store();
            axisdata = new axisdata_store();


            zoomToFitTimer = new Timer();
            zoomToFitTimer.Interval = 10;
            zoomToFitTimer.Tick += ZoomToFitTimer_Tick;

            Application.Idle += OnApplicationIdle;

        }

        private void main_frm_Load(object sender, EventArgs e)
        {

            // Initialize the GLControl in the Load event
            // Fill the gcontrol panel
            glControl_main_panel.BorderStyle = BorderStyle.Fixed3D;
            glControl_main_panel.Dock = DockStyle.Fill;


            // Create the main font atlas
            modeldata.InitializeModelGeom();

            gvariables_static.main_font.CreateAtlas("Calibri");

            axisdata.InitializeAxisData(glControl_main_panel.Width, glControl_main_panel.Height);

        }



        #region "glControl Main Panel Events"

        private void glControl_main_panel_Load(object sender, EventArgs e)
        {
            glControl_main_panel.MakeCurrent();


            // Paint the background
            Color clr_bg = gvariables_static.glcontrol_background_color;
            GL.ClearColor(((float)clr_bg.R / 255.0f),
                ((float)clr_bg.G / 255.0f),
                ((float)clr_bg.B / 255.0f),
                ((float)clr_bg.A / 255.0f));


            fpsStopwatch.Start();

            // Refresh the controller (doesnt do much.. nothing to draw)
            glControl_main_panel.Invalidate();

        }


        private void glControl_main_panel_Paint(object sender, PaintEventArgs e)
        {
            // Paint the drawing area (glControl_main)
            // Tell OpenGL to use MyGLControl
            glControl_main_panel.MakeCurrent();

            // GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(0, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Clear the background
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            modeldata.paint_model();

            // Draw the axis arrows
            axisdata.draw_axis_arrows();


            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.
            glControl_main_panel.SwapBuffers();

            // Update the zoom value
            double zm_val = modeldata.graphic_events_control.zoom_val;
            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(zm_val * 100))).ToString() + "%";

            // Update FPS every second
            if (fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                fpsStopwatch.Restart();

                // SetRefreshStatus(true); // Update status bar
            }

        }

        private void glControl_main_panel_SizeChanged(object sender, EventArgs e)
        {
            // Note: SizeChanged can fire before the OpenGL context exists (e.g., during form initialization, Load etc).
            if (glControl_main_panel == null || modeldata == null)
                return;

            // Update the size of the drawing area
            modeldata.graphic_events_control.update_drawing_area_size(glControl_main_panel.Width,
                glControl_main_panel.Height);

            axisdata.UpdateAxisArrowCenter(glControl_main_panel.Width, glControl_main_panel.Height);
            modeldata.update_contour_bar_position(glControl_main_panel.Width, glControl_main_panel.Height);

            toolStripStatusLabel_zoom_value.Text = "Zoom: " + (gvariables_static.RoundOff((int)(1.0f * 100))).ToString() + "%";

            // Refresh the painting area
            glControl_main_panel.Invalidate();
        }

        private void glControl_main_panel_MouseEnter(object sender, EventArgs e)
        {
            // set the focus to enable zoom/ pan & zoom to fit
            glControl_main_panel.Focus();

        }

        private void glControl_main_panel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Left button down
                modeldata.graphic_events_control.handleMouseLeftButtonClick(true, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button down
                modeldata.graphic_events_control.handleMouseRightButtonClick(true, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse wheel
            modeldata.graphic_events_control.handleMouseScroll(e.Delta, e.X, e.Y);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse move 
            modeldata.graphic_events_control.handleMouseMove(e.X, e.Y);

            glControl_main_panel.Invalidate();

        }

        private void glControl_main_panel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Left button up
                modeldata.graphic_events_control.handleMouseLeftButtonClick(false, e.X, e.Y);

            }
            else if (e.Button == MouseButtons.Right)
            {
                // Right button up
                modeldata.graphic_events_control.handleMouseRightButtonClick(false, e.X, e.Y);

            }

            glControl_main_panel.Invalidate();

            // Update the Material Property Form data
            if (modeldata.isMaterialUpdateInProgress == true)
            {
                matprop_Form.update_selected_element_list();

            }

            // Update the Edge Constraint Form data
            if (modeldata.isEdgeConstraintUpdateInProgress == true)
            {
                edgeconstraint_Form.update_selected_edge_list();
            }

            // Update the Nodal Constraint Form data
            if (modeldata.isNodeConstraintUpdateInProgress == true)
            {
                nodalconstraint_Form.update_selected_node_list();

            }

            //// Update the Result Annotation Form data
            //if (modeldata.isAnnotateResultInProgress == true)
            //{
            //    // Update the selected result points
            //    annotate_Form.updateSelectedResultPointsDataGridView();

            //}

        }

        private void glControl_main_panel_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Key Down
            modeldata.graphic_events_control.handleKeyboardAction(true, e.KeyValue);

            glControl_main_panel.Invalidate();

        }


        private void glControl_main_panel_KeyUp(object sender, KeyEventArgs e)
        {
            // Keyboard Key Up
            modeldata.graphic_events_control.handleKeyboardAction(false, e.KeyValue);

            glControl_main_panel.Invalidate();

            // If zoom-to-fit started, start the timer
            if (modeldata.graphic_events_control.isZoomToFitInProgress == true)
            {
                // Start the zoomToFit timer
                if (!zoomToFitTimer.Enabled)
                    zoomToFitTimer.Start();

            }


        }


        private void ZoomToFitTimer_Tick(object sender, EventArgs e)
        {
            glControl_ZoomToFitOperation();

        }

        private void glControl_ZoomToFitOperation()
        {
            // Refresh the glControl_main_panel as the zoom to fit operation in progress
            glControl_main_panel.Invalidate();

            if (modeldata.graphic_events_control.isZoomToFitInProgress == false)
            {
                // End the zoom to fit operation
                // Stop zoom-to-fit operation once done
                zoomToFitTimer.Stop();

            }

        }


        private bool IsApplicationIdle()
        {
            Message msg;
            return !gvariables_static.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (IsApplicationIdle())
            {
                modeldata.update_result_animation();   // Update animation
                glControl_main_panel.Invalidate(); // Redraw
            }
        }








        #endregion

        #region "Menu Events"

        #region "File Menu"


        private void importTXTFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import Model File",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    modeldata.importFile(filePath, 0);

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading text file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);


            // set_ResultOption(0); // Reset result option to hide results


            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();

        }

        private void importModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Import Model File",
                Filter = "Text Files (*.bin)|*.bin|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {

                    modeldata.importFile(filePath, 1);

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading binary file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            // set_ResultOption(0); // Reset result option to hide results

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();

        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export Model File",
                Filter = "Bindary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                try
                {

                    modeldata.exportBINFile(filePath);

                    // Do something with the file content, e.g., parse the model
                    // MessageBox.Show("Model file exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            glControl_main_panel_SizeChanged(sender, e);

            glControl_main_panel.Refresh();
            glControl_main_panel.Invalidate();
        }


        private void CenterFormOnParent(Form childForm)
        {
            // Helper method to center a form on its owner
            if (childForm.Owner == null)
                return;

            // Get the screen bounds of the parent form
            Screen parentScreen = Screen.FromControl(childForm.Owner);
            Rectangle parentBounds = childForm.Owner.Bounds;

            // Calculate center position relative to the parent form
            int x = parentBounds.X + (parentBounds.Width - childForm.Width) / 2;
            int y = parentBounds.Y + (parentBounds.Height - childForm.Height) / 2;

            // Ensure the form stays within the screen bounds
            Rectangle screenBounds = parentScreen.WorkingArea;

            // Adjust if the form would go off-screen
            if (x < screenBounds.Left)
                x = screenBounds.Left;
            if (y < screenBounds.Top)
                y = screenBounds.Top;
            if (x + childForm.Width > screenBounds.Right)
                x = screenBounds.Right - childForm.Width;
            if (y + childForm.Height > screenBounds.Bottom)
                y = screenBounds.Bottom - childForm.Height;

            childForm.Location = new Point(x, y);
        }


        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modeldata.IsModelSet == false)
                return;

            // Check if option_Form is null or disposed
            if (option_Form == null || option_Form.IsDisposed)
            {
                option_Form = new option_frm();

                // Make it behave like a tool window
                option_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                option_Form.ShowInTaskbar = false;
                option_Form.TopLevel = true;
                option_Form.Owner = this;

                // Set the start position to manual so we can control placement
                option_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(option_Form);
            }

            // Show the form
            option_Form.Show(this);
            option_Form.BringToFront();

            glControl_main_panel.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exit application
            this.Close();

        }


        #endregion

        #region "Load and Boundary Conditions Menu"

        private void addNodalConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modeldata.IsModelSet == false)
                return;

            // Check if nodalconstraint_Form is null or disposed
            if (nodalconstraint_Form == null || nodalconstraint_Form.IsDisposed)
            {
                nodalconstraint_Form = new nodalconstraint_frm(ref modeldata);

                // Make it behave like a tool window
                nodalconstraint_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                nodalconstraint_Form.ShowInTaskbar = false;
                nodalconstraint_Form.TopLevel = true;
                nodalconstraint_Form.Owner = this;


                // Set the start position to manual so we can control placement
                nodalconstraint_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(nodalconstraint_Form);
            }

            // Turn on Flag Nodal Constraint update form is open
            modeldata.isNodeConstraintUpdateInProgress = true;
            modeldata.fe_data.clear_selected_nodes();

            // Show the form
            if (!nodalconstraint_Form.Visible)
            {
                nodalconstraint_Form.update_dataGridView();
                nodalconstraint_Form.update_selected_node_list();
                nodalconstraint_Form.Show(this);
            }

            nodalconstraint_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }



        private void addEdgeConstraintsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modeldata.IsModelSet == false)
                return;

            // Check if edgeconstraint_Form is null or disposed
            if (edgeconstraint_Form == null || edgeconstraint_Form.IsDisposed)
            {
                edgeconstraint_Form = new edgeconstraint_frm(ref modeldata);

                // Make it behave like a tool window
                edgeconstraint_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                edgeconstraint_Form.ShowInTaskbar = false;
                edgeconstraint_Form.TopLevel = true;
                edgeconstraint_Form.Owner = this;


                // Set the start position to manual so we can control placement
                edgeconstraint_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(edgeconstraint_Form);
            }

            // Turn on Flag Edge Constraint update form is open
            modeldata.isEdgeConstraintUpdateInProgress = true;
            modeldata.fe_data.clear_selected_edges();

            // Show the form
            if (!edgeconstraint_Form.Visible)
            {
                edgeconstraint_Form.update_dataGridView();
                edgeconstraint_Form.update_selected_edge_list();
                edgeconstraint_Form.Show(this);
            }

            edgeconstraint_Form.BringToFront();

            glControl_main_panel.Invalidate();

        }



        private void mediumPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modeldata.IsModelSet == false)
                return;

            // Check if matprop_Form is null or disposed
            if (matprop_Form == null || matprop_Form.IsDisposed)
            {
                matprop_Form = new matprop_frm(ref modeldata);

                // Make it behave like a tool window
                matprop_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                matprop_Form.ShowInTaskbar = false;
                matprop_Form.TopLevel = true;
                // matprop_Form.MdiParent = this;
                matprop_Form.Owner = this;


                // Set the start position to manual so we can control placement
                matprop_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(matprop_Form);

            }

            // Turn on Flag Material update form is open
            modeldata.isMaterialUpdateInProgress = true;
            modeldata.fe_data.clear_selected_mesh();

            // Show the form
            if (!matprop_Form.Visible)
            {
                matprop_Form.update_material_data();
                matprop_Form.update_selected_element_list();
                matprop_Form.Show(this);
            }
            matprop_Form.BringToFront();

            glControl_main_panel.Invalidate();
        }

        #endregion


        #region "Solver Menu"

        private void dWaveEquationSolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (modeldata.IsModelSet == false)
                return;

            // Check if solver_Form is null or disposed
            if (solver_Form == null || solver_Form.IsDisposed)
            {
                solver_Form = new solver_frm(ref modeldata);

                // Make it behave like a tool window
                solver_Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                solver_Form.ShowInTaskbar = false;
                solver_Form.TopLevel = true;
                solver_Form.Owner = this;

                // Set the start position to manual so we can control placement
                solver_Form.StartPosition = FormStartPosition.Manual;

                // Center the form on the parent
                CenterFormOnParent(solver_Form);

            }

            if (!solver_Form.Visible)
            {
                solver_Form.Show(this);
            }
            solver_Form.BringToFront();

            glControl_main_panel.Invalidate();


        }

        #endregion


        #region "Call from Child Forms"

        public void CallFrom_matprop_frm()
        {
            modeldata.update_openTK_uniforms();

            glControl_main_panel.Invalidate();
        }


        public void CallFrom_constraint_frm()
        {
            modeldata.update_openTK_uniforms();

            glControl_main_panel.Invalidate();
        }

        public void CallFrom_option_frm()
        {
            modeldata.update_openTK_uniforms();

            glControl_main_panel.Invalidate();
        }


        public void CallFrom_rsltoption_frm()
        {
            modeldata.update_openTK_uniforms();
            glControl_main_panel.Invalidate();
        }



        public void callFrom_annotate_frm()
        {
            modeldata.update_openTK_uniforms();

            glControl_main_panel.Invalidate();
        }




        #endregion

        #endregion

    }
}
