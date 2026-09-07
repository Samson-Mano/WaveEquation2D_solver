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
using System.Windows.Forms.VisualStyles;
using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store;
using WaveEquation2D_solver.src.model_store.fe_objects;

namespace WaveEquation2D_solver.other_windows
{
    public partial class edgeconstraint_frm : Form
    {
        private modeldata_store modeldata;

        public edgeconstraint_frm(ref modeldata_store modeldata)
        {
            InitializeComponent();

            this.modeldata = modeldata;

            UpdateEnabledStateUI();
            UpdateEnabledStateUI2();

        }

        private void edgeconstraint_frm_Load(object sender, EventArgs e)
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
            if (modeldata.fe_data.selected_edge_ids.Count == 0)
                return;



            if(radioButton_boundaryconditions.Checked == true)
            {
                // Test the data
                if (!double.TryParse(textBox_dirichlet.Text, out double field_value) ||
                    !double.TryParse(textBox_neumann.Text, out double normalderiv_value))
                {
                    MessageBox.Show("Please enter valid numeric values for field value, and normal derivative field value.");
                    return;
                }

                if (checkBox_dirichlet.Checked == false && checkBox_neumann.Checked == false)
                {
                    MessageBox.Show("Please select field value, and normal derivative field value.");
                    return;
                }

                // Get the edges
                // Get the start and end point locations
                List<int> constraint_edge_startpt_ids = new List<int>();
                List<int> constraint_edge_endpt_ids = new List<int>();

                List<Vector2> constraint_edge_startpts = new List<Vector2>();
                List<Vector2> constraint_edge_endpts = new List<Vector2>();

                int i = 0;

                foreach (int edgeid in modeldata.fe_data.selected_edge_ids)
                {
                    elementedge_store elementedge = modeldata.fe_data.fe_edges.FirstOrDefault(l => l.edge_id == edgeid);

                    int start_pt_id = elementedge.start_nodeid;
                    int end_pt_id = elementedge.end_nodeid;

                    // Add the edge end point ids
                    constraint_edge_startpt_ids.Add(start_pt_id);
                    constraint_edge_endpt_ids.Add(end_pt_id);

                    node_store nd1 = modeldata.fe_data.fe_nodes.nodeMap[start_pt_id];
                    node_store nd2 = modeldata.fe_data.fe_nodes.nodeMap[end_pt_id];

                    // Add edge end points
                    constraint_edge_startpts.Add(new Vector2((float)nd1.node_pt_x_coord,
                        (float)nd1.node_pt_y_coord));

                    constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                        (float)nd2.node_pt_y_coord));

                    i++;
                }

                if (checkBox_dirichlet.Checked == false)
                    field_value = 0.0;

                if (checkBox_neumann.Checked == false)
                    normalderiv_value = 0.0;

                // Add the edge constraint
                modeldata.fe_data.fe_edgeconstraints.add_edgeconstraint(modeldata.fe_data.selected_edge_ids.ToList(),
                    constraint_edge_startpt_ids, constraint_edge_endpt_ids,
                    constraint_edge_startpts, constraint_edge_endpts,
                    field_value, normalderiv_value, checkBox_dirichlet.Checked, checkBox_neumann.Checked,false);

            }
            else
            {

                // Get the edges
                // Get the start and end point locations
                List<int> constraint_edge_startpt_ids = new List<int>();
                List<int> constraint_edge_endpt_ids = new List<int>();

                List<Vector2> constraint_edge_startpts = new List<Vector2>();
                List<Vector2> constraint_edge_endpts = new List<Vector2>();

                int i = 0;

                foreach (int edgeid in modeldata.fe_data.selected_edge_ids)
                {
                    elementedge_store elementedge = modeldata.fe_data.fe_edges.FirstOrDefault(l => l.edge_id == edgeid);

                    int start_pt_id = elementedge.start_nodeid;
                    int end_pt_id = elementedge.end_nodeid;

                    // Add the edge end point ids
                    constraint_edge_startpt_ids.Add(start_pt_id);
                    constraint_edge_endpt_ids.Add(end_pt_id);

                    node_store nd1 = modeldata.fe_data.fe_nodes.nodeMap[start_pt_id];
                    node_store nd2 = modeldata.fe_data.fe_nodes.nodeMap[end_pt_id];

                    // Add edge end points
                    constraint_edge_startpts.Add(new Vector2((float)nd1.node_pt_x_coord,
                        (float)nd1.node_pt_y_coord));

                    constraint_edge_endpts.Add(new Vector2((float)nd2.node_pt_x_coord,
                        (float)nd2.node_pt_y_coord));

                    i++;
                }

                // Add the edge constraint
                modeldata.fe_data.fe_edgeconstraints.add_edgeconstraint(modeldata.fe_data.selected_edge_ids.ToList(),
                    constraint_edge_startpt_ids, constraint_edge_endpt_ids,
                    constraint_edge_startpts, constraint_edge_endpts,
                    0.0, 0.0, false, false, true);

            }

            // Clear the selected edge ids
           modeldata.fe_data.clear_selected_edges();

            update_selected_edge_list();

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
                modeldata.fe_data.fe_edgeconstraints.delete_edgeconstraint(constraint_id);

                update_dataGridView();

                // Clear the selected edge ids
                modeldata.fe_data.clear_selected_edges();

                update_selected_edge_list();

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


            foreach (var cnst_m in modeldata.fe_data.fe_edgeconstraints.edgecnstMap)
            {
                edgecnst_store cnst = cnst_m.Value;

                // Convert edge IDs list to a short string, e.g. "1, 2, 3 ..."
                string edgeIdsPreview;
                int previewCount = 15; // how many IDs to show
                if (cnst.constraint_edge_ids.Count > previewCount)
                {
                    edgeIdsPreview = string.Join(", ", cnst.constraint_edge_ids.Take(previewCount)) + " ...";
                }
                else
                {
                    edgeIdsPreview = string.Join(", ", cnst.constraint_edge_ids);
                }

                dataGridView_ConstraintList.Rows.Add(
                    cnst.edgecnst_set_id,
                    edgeIdsPreview,   // show some of constraint nodes as string here
                    cnst.field_value.ToString("G"),
                    cnst.normalderivfield_value.ToString("G"),
                    cnst.isSommerfieldBC
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




        public void update_selected_edge_list()
        {
            // Clear the text box
            textBox_selectededges.Clear();

            List<int> all_selected_edges = new List<int>();

            all_selected_edges.AddRange(modeldata.fe_data.selected_edge_ids);

            textBox_selectededges.Text = string.Join(", ", all_selected_edges);

            textBox_selectededges.Invalidate();

        }


        private void edgeconstraint_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            modeldata.isEdgeConstraintUpdateInProgress = false;
            modeldata.fe_data.clear_selected_edges();

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_constraint_frm();
            }

        }



        private void radioButton_boundaryconditions_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }


        private void radioButton_sommerfield_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI();

        }


        private void UpdateEnabledStateUI()
        {
            bool isBoundartConditionSelected = radioButton_boundaryconditions.Checked;

            // Boundary condition
            checkBox_dirichlet.Enabled = isBoundartConditionSelected;
            textBox_dirichlet.Enabled = isBoundartConditionSelected;
            label_dirichlet.Enabled = isBoundartConditionSelected;

            checkBox_neumann.Enabled = isBoundartConditionSelected;
            textBox_neumann.Enabled= isBoundartConditionSelected;
            label_neumann.Enabled = isBoundartConditionSelected;


            // ABC Sommerfield
            label_sommerfield.Enabled = !isBoundartConditionSelected;

            if(isBoundartConditionSelected == true)
            {
                UpdateEnabledStateUI2();
            }
   
        }

        private void checkBox_dirichlet_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI2();

        }

        private void checkBox_neumann_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStateUI2();

        }


        private void UpdateEnabledStateUI2()
        {
            bool isDirichletSelected = checkBox_dirichlet.Checked;

            label_dirichlet.Enabled = isDirichletSelected;
            textBox_dirichlet.Enabled = isDirichletSelected;


            bool isNeumannSelected = checkBox_neumann.Checked;

            label_neumann.Enabled = isNeumannSelected;
            textBox_neumann.Enabled = isNeumannSelected;

        }



    }
}
