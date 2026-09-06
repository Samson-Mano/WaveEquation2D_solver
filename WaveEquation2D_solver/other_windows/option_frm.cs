using WaveEquation2D_solver.src.global_variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveEquation2D_solver.other_windows
{
    public partial class option_frm : Form
    {

        // Define enum for paint options
        private enum PaintOption
        {
            MeshPoints = 1,
            Mesh = 2,
            MeshBoundaries = 3,
            ShrunkTriangle = 4,
            Loads = 5,
            Constraints = 6
        }

        public option_frm()
        {
            InitializeComponent();

            // Initialize checkboxes from current state
            LoadSettings();

        }

        private void LoadSettings()
        {
            checkBox_paintmeshpoints.Checked = gvariables_static.is_paint_meshpoints;
            checkBox_paintmesh.Checked = gvariables_static.is_paint_mesh;
            checkBox_paintmeshboundaries.Checked = gvariables_static.is_paint_mesh_boundaries;
            checkBox_paintshrinkmesh.Checked = gvariables_static.is_paint_shrunk_triangle;
            checkBox_paintloads.Checked = gvariables_static.is_paint_loads;
            checkBox_paintconstraints.Checked = gvariables_static.is_paint_constraints;
        }


        private void button_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox_paintmeshpoints_CheckedChanged(object sender, EventArgs e)
            => SetPaintOption(PaintOption.MeshPoints, checkBox_paintmeshpoints.Checked);

        private void checkBox_paintmesh_CheckedChanged(object sender, EventArgs e) 
            => SetPaintOption(PaintOption.Mesh, checkBox_paintmesh.Checked);

        private void checkBox_paintmeshboundaries_CheckedChanged(object sender, EventArgs e) 
            => SetPaintOption(PaintOption.MeshBoundaries, checkBox_paintmeshboundaries.Checked);

        private void checkBox_paintshrinkmesh_CheckedChanged(object sender, EventArgs e) 
            => SetPaintOption(PaintOption.ShrunkTriangle, checkBox_paintshrinkmesh.Checked);

        private void checkBox_paintloads_CheckedChanged(object sender, EventArgs e) 
            => SetPaintOption(PaintOption.Loads, checkBox_paintloads.Checked);

        private void checkBox_paintconstraints_CheckedChanged(object sender, EventArgs e) 
            => SetPaintOption(PaintOption.Constraints, checkBox_paintconstraints.Checked);



        private void SetPaintOption(PaintOption option, bool isChecked)
        {
            switch (option)
            {
                case PaintOption.MeshPoints:
                    gvariables_static.is_paint_meshpoints = isChecked;
                    break;
                case PaintOption.Mesh:
                    gvariables_static.is_paint_mesh = isChecked;
                    break;
                case PaintOption.MeshBoundaries:
                    gvariables_static.is_paint_mesh_boundaries = isChecked;
                    break;
                case PaintOption.ShrunkTriangle:
                    gvariables_static.is_paint_shrunk_triangle = isChecked;
                    break;
                case PaintOption.Loads:
                    gvariables_static.is_paint_loads = isChecked;
                    break;
                case PaintOption.Constraints:
                    gvariables_static.is_paint_constraints = isChecked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }

            // Optional: Trigger immediate redraw
            // Application.DoEvents(); or raise an event

            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_option_frm();
            }
        }


    }
}
