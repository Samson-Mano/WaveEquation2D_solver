using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store.fe_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WaveEquation2D_solver.src.model_store;

namespace WaveEquation2D_solver.other_windows
{
    public partial class nodalconstraint_frm : Form
    {
        private modeldata_store model_data;

        public nodalconstraint_frm(ref modeldata_store model_data)
        {
            InitializeComponent();

            this.model_data = model_data;

            UpdateEnabledStateUI();

        }


        private void nodalconstraint_frm_Load(object sender, EventArgs e)
        {
            // Initialize selection state from global variable
            SetSelectionMode(gvariables_static.is_RectangleSelection);

        }



        private void rectangleSelectionToolStripMenuItem_Click(object sender, EventArgs e) => SetSelectionMode(true);


        private void circleSelectionToolStripMenuItem_Click(object sender, EventArgs e) => SetSelectionMode(false);




        private void SetSelectionMode(bool isRectangle)
        {

            gvariables_static.is_RectangleSelection = isRectangle;

            rectangleSelectionToolStripMenuItem.Checked = isRectangle;
            circleSelectionToolStripMenuItem.Checked = !isRectangle;


            rectangleSelectionToolStripMenuItem.BackColor = isRectangle ? Color.LightBlue : SystemColors.Control;
            circleSelectionToolStripMenuItem.BackColor = !isRectangle ? Color.LightBlue : SystemColors.Control;

        }



        private void button_applyconstraint_Click(object sender, EventArgs e)
        {
            if (model_data.fe_data.selected_node_ids.Count == 0)
                return;


            // Test the data
            if (!double.TryParse(textBox_dirichlet.Text, out double field_value) ||
                !double.TryParse(textBox_source.Text, out double source_value))
            {
                MessageBox.Show("Please enter valid numeric values for field value, and source value.");
                return;
            }

            // Get the point locations
            List<Vector2> constraint_node_pts = new List<Vector2>();

            foreach(int nd_id in model_data.fe_data.selected_node_ids)
            {
                node_store nd = model_data.fe_data.fe_nodes.nodeMap[nd_id];
                
                constraint_node_pts.Add(new Vector2((float)nd.node_pt_x_coord,
                    (float)nd.node_pt_y_coord));
            }

            bool isField = radioButton_dirichlet.Checked;

            // Add the constraint
            model_data.fe_data.fe_nodeconstraints.add_nodeconstraint(model_data.fe_data.selected_node_ids.ToList(), 
                constraint_node_pts, field_value, source_value, isField);

            // Clear the selected point ids
            model_data.fe_data.clear_selected_nodes();

            update_selected_node_list();

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_constraint_frm();
            }

            // Update the data grid view
            update_dataGridView();

        }


        private void button_deleteconstraint_Click(object sender, EventArgs e)
        {
            if (dataGridView_ConstraintList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_ConstraintList.SelectedRows[0];

                // Safely Retrieve the Constraint ID
                string idString = selectedRow.Cells["Column1_constraintid"].Value?.ToString();

                if (!int.TryParse(idString, out int constraint_id))
                {
                    // MessageBox.Show("Invalid constraint ID.");
                    return;
                }


                // Delete the selected constraint
                model_data.fe_data.fe_nodeconstraints.delete_nodeconstraint(constraint_id);

                update_dataGridView();

                // Clear the selected point ids
                model_data.fe_data.clear_selected_nodes();

                update_selected_node_list();

                // Call the main form
                if (this.Owner is main_frm mainForm)
                {
                    mainForm.CallFrom_constraint_frm();
                }

            }

        }


        public void update_dataGridView()
        {

            // refresh the Constraint list data grid view
            dataGridView_ConstraintList.Rows.Clear();


            foreach (var cnst_m in model_data.fe_data.fe_nodeconstraints.ndcnstMap)
            {
                nodecnst_data cnst = cnst_m.Value;

                // Convert node IDs list to a short string, e.g. "1, 2, 3 ..."
                string nodeIdsPreview;
                int previewCount = 15; // how many IDs to show
                if (cnst.constraint_node_ids.Count > previewCount)
                {
                    nodeIdsPreview = string.Join(", ", cnst.constraint_node_ids.Take(previewCount)) + " ...";
                }
                else
                {
                    nodeIdsPreview = string.Join(", ", cnst.constraint_node_ids);
                }

                dataGridView_ConstraintList.Rows.Add(
                    cnst.ndcnst_set_id,
                    nodeIdsPreview,   // show some of constraint nodes as string here
                    cnst.field_value.ToString("G"),
                    cnst.source_value.ToString("G")
                    );

            }

            if (dataGridView_ConstraintList.Rows.Count > 0)
            {
                // Move the index to the last index
                int lastIndex = dataGridView_ConstraintList.Rows.Count - 1;
                dataGridView_ConstraintList.ClearSelection();
                dataGridView_ConstraintList.Rows[lastIndex].Selected = true;

            }

            dataGridView_ConstraintList.Invalidate();

        }



        public void update_selected_node_list()
        {
            // Clear the text box
            textBox_selectednodes.Clear();

            List<int> all_selected_nodes = new List<int>();

            all_selected_nodes.AddRange(model_data.fe_data.selected_node_ids);

            textBox_selectednodes.Text = string.Join(", ", all_selected_nodes);

            textBox_selectednodes.Invalidate();

        }


        private void nodalconstraint_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            model_data.isNodeConstraintUpdateInProgress = false;
            model_data.fe_data.clear_selected_nodes();

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_constraint_frm();
            }

        }



        private void radioButton_dirichlet_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }

        private void radioButton_source_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }


        private void UpdateEnabledStateUI()
        {
            bool isSourceSelected = radioButton_source.Checked;

            textBox_source.Enabled = isSourceSelected;
            label_source.Enabled = isSourceSelected;

            textBox_dirichlet.Enabled = !isSourceSelected;
            label_dirichlet.Enabled = !isSourceSelected;
        }


    }
}
