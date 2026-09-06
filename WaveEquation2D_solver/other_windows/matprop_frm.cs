using WaveEquation2D_solver.src.global_variables;
using WaveEquation2D_solver.src.model_store;
using WaveEquation2D_solver.src.model_store.fe_objects;
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
    public partial class matprop_frm : Form
    {
        private modeldata_store modeldata;

        public matprop_frm(ref modeldata_store modeldata)
        {
            InitializeComponent();

            this.modeldata = modeldata;
        }



        public void update_material_data()
        {
            // Get the fe materials
            List<material_data> fe_materials = this.modeldata.fe_data.fe_materials.Values.ToList();

            // Clear existing rows 
            dataGridView_MaterialList.Rows.Clear();

            // Add rows manually
            foreach (material_data mat in fe_materials)
            {
                int rowIndex = dataGridView_MaterialList.Rows.Add(
                    mat.material_id.ToString(),
                    mat.material_name,
                    mat.youngs_modulus.ToString("G"),
                    mat.material_density.ToString("G"),
                    mat.poissons_ratio.ToString("G"),
                    mat.yield_point.ToString("G"),
                    mat.thickness.ToString("G")
                );

                // Get the newly added row
                DataGridViewRow row = dataGridView_MaterialList.Rows[rowIndex];

                // Set the color for the material color column (Column6_materialcolor)
                Color materialColor = gvariables_static.ColorUtils.GetRandomColor(mat.material_id);

                // row.DefaultCellStyle.BackColor = materialColor;
                // row.DefaultCellStyle.ForeColor = Color.Black; // Text color

                row.Cells["Column8_materialcolor"].Style.BackColor = materialColor;
                row.Cells["Column8_materialcolor"].Style.SelectionBackColor = materialColor;

                // Optional: Store the material_id in the row's Tag for later use
               // row.Tag = mat.material_id;
            }

        }


        private void dataGridView_MaterialList_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];


                // Check if the selected row is the first row (index 0)
                if (selectedRow.Index == 0)
                {
                    // First row update and delete not allowed
                    button_update.Enabled = false;
                    button_delete.Enabled = false;

                }
                else
                {
                    // Enable the update and delete for all other rows
                    button_update.Enabled = true;
                    button_delete.Enabled = true;

                }

                textBox_materialname.Text = selectedRow.Cells["Column2_materialname"].Value?.ToString();
                textBox_youngsmodulus.Text = selectedRow.Cells["Column3_youngsmodulus"].Value?.ToString();
                textBox_density.Text = selectedRow.Cells["Column4_density"].Value?.ToString();
                textBox_poissonsratio.Text = selectedRow.Cells["Column5_poissonsratio"].Value?.ToString();
                textBox_yieldpoint.Text = selectedRow.Cells["Column6_yieldpoint"].Value?.ToString();
                textBox_thickness.Text = selectedRow.Cells["Column7_thickness"].Value?.ToString();

            }

        }


        private void matprop_frm_Load(object sender, EventArgs e)
        {
            // Initialize selection state from global variable
            SetSelectionMode(gvariables_static.is_RectangleSelection);
        }


        private void button_create_Click(object sender, EventArgs e)
        {

            // Generate a unique material ID
            int material_id = gvariables_static.get_unique_id(modeldata.fe_data.materialids);

            // Read and validate input from text boxes
            string material_name = textBox_materialname.Text.Trim();
            if (string.IsNullOrWhiteSpace(material_name))
            {
                MessageBox.Show("Material name cannot be empty.");
                return;
            }

            // Test the data
            if (!double.TryParse(textBox_youngsmodulus.Text, out double youngsmodulus) ||
                !double.TryParse(textBox_density.Text, out double density) ||
                !double.TryParse(textBox_poissonsratio.Text, out double poissonsratio) ||
                !double.TryParse(textBox_yieldpoint.Text, out double yieldpoint) ||
                !double.TryParse(textBox_thickness.Text, out double thickness))
            {
                MessageBox.Show("Please enter valid numeric values for youngs modulus, density, and poissons ratio.");
                return;
            }

            // Add a new row to the DataGridView
            int rowIndex = dataGridView_MaterialList.Rows.Add(
                material_id,
                material_name,
                youngsmodulus.ToString("G"),
                density.ToString("G"),
                poissonsratio.ToString("G"),
                yieldpoint.ToString("G"),
                thickness.ToString("G")
            );

            // Get the newly added row
            DataGridViewRow row = dataGridView_MaterialList.Rows[rowIndex];

            // Set the color for the material color column (Column6_materialcolor)
            Color materialColor = gvariables_static.ColorUtils.GetRandomColor(material_id);

            //// Option A: Set entire row color
            // row.DefaultCellStyle.BackColor = materialColor;
            // row.DefaultCellStyle.ForeColor = Color.Black; // Text color


            row.Cells["Column8_materialcolor"].Style.BackColor = materialColor;
            row.Cells["Column8_materialcolor"].Style.SelectionBackColor = materialColor;



            // Create and store the material object
            var newMaterial = new material_data
            {
                material_id = material_id,
                material_name = material_name,
                youngs_modulus = youngsmodulus,
                material_density = density,
                poissons_ratio = poissonsratio,
                yield_point = yieldpoint,
                thickness = thickness
            };

            modeldata.fe_data.fe_materials[material_id] = newMaterial;
            modeldata.fe_data.materialids.Add(material_id);

            // modeldata.fe_data.updateMaterialIDLabels();

            // Call the main form for refresh
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_matprop_frm();
            }

        }

        private void button_update_Click(object sender, EventArgs e)
        {
            // Update the material data
            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];

                // Safely Retrieve the material ID
                string idString = selectedRow.Cells["Column1_materialid"].Value?.ToString();

                if (!int.TryParse(idString, out int material_id))
                {
                    // MessageBox.Show("Invalid material ID.");
                    return;
                }


                // Read and validate input from text boxes
                string material_name = textBox_materialname.Text.Trim();
                if (string.IsNullOrWhiteSpace(material_name))
                {
                    MessageBox.Show("Material name cannot be empty.");
                    return;
                }

                // Test the data
                if (!double.TryParse(textBox_youngsmodulus.Text, out double youngsmodulus) ||
                    !double.TryParse(textBox_density.Text, out double density) ||
                    !double.TryParse(textBox_poissonsratio.Text, out double poissonsratio) ||
                    !double.TryParse(textBox_yieldpoint.Text, out double yieldpoint) ||
                    !double.TryParse(textBox_thickness.Text, out double thickness))
                {
                    MessageBox.Show("Please enter valid numeric values for youngs modulus, density, and poissons ratio.");
                    return;
                }

                // update the material data in the dictionary
                modeldata.fe_data.fe_materials[material_id].material_name = material_name;
                modeldata.fe_data.fe_materials[material_id].youngs_modulus = youngsmodulus;
                modeldata.fe_data.fe_materials[material_id].material_density = density;
                modeldata.fe_data.fe_materials[material_id].poissons_ratio = poissonsratio;
                modeldata.fe_data.fe_materials[material_id].yield_point = yieldpoint;
                modeldata.fe_data.fe_materials[material_id].thickness = thickness;
             
                // Update the DataGridView row
                selectedRow.Cells["Column2_materialname"].Value = material_name;
                selectedRow.Cells["Column3_youngsmodulus"].Value = youngsmodulus.ToString("G");
                selectedRow.Cells["Column4_density"].Value = density.ToString("G");
                selectedRow.Cells["Column5_poissonsratio"].Value = poissonsratio.ToString("G");
                selectedRow.Cells["Column6_yieldpoint"].Value = yieldpoint.ToString("G");
                selectedRow.Cells["Column7_thickness"].Value = thickness.ToString("G");


                // Set the row background color
                Color rowColor = gvariables_static.ColorUtils.GetRandomColor(material_id);

                //// Option A: Set entire row color
                // selectedRow.DefaultCellStyle.BackColor = rowColor;
                // selectedRow.DefaultCellStyle.ForeColor = Color.Black; // Text color

                //// Option B: Set only the material color cell
                //selectedRow.Cells["Column6_materialcolor"].Style.BackColor = rowColor;
                //selectedRow.Cells["Column6_materialcolor"].Style.ForeColor = Color.Black;

                // Option C: If you want to show the color as a colored box in the cell
                selectedRow.Cells["Column8_materialcolor"].Style.BackColor = rowColor;
                selectedRow.Cells["Column8_materialcolor"].Style.SelectionBackColor = rowColor;


                // fe_data.updateMaterialIDLabels();

                // Call the main form for refresh
                if (this.Owner is main_frm mainForm)
                {
                    mainForm.CallFrom_matprop_frm();
                }

            }

        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            // Delete the material
            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];

                // Safely Retrieve the material ID
                string idString = selectedRow.Cells["Column1_materialid"].Value?.ToString();

                if (!int.TryParse(idString, out int material_id))
                {
                    // MessageBox.Show("Invalid material ID.");
                    return;
                }

                // Call the main form
                if (this.Owner is main_frm mainForm)
                {
                    mainForm.CallFrom_matprop_frm();
                }


                // Remove from the dictionary
                modeldata.fe_data.fe_materials.Remove(material_id);
                modeldata.fe_data.materialids.Remove(material_id);

                // remove the row from the data grid view
                dataGridView_MaterialList.Rows.Remove(selectedRow);

                // Update the material
                modeldata.fe_data.execute_delete_material(material_id);
                modeldata.fe_data.clear_selected_mesh();

                // Call the main form for refresh
                if (this.Owner is main_frm mainForm1)
                {
                    mainForm1.CallFrom_matprop_frm();
                }

            }

        }

        private void button_assignmaterial_Click(object sender, EventArgs e)
        {

            if (dataGridView_MaterialList.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView_MaterialList.SelectedRows[0];
                // Safely Retrieve the material ID
                string idString = selectedRow.Cells["Column1_materialid"].Value?.ToString();

                if (!int.TryParse(idString, out int material_id))
                {
                    // MessageBox.Show("Invalid material ID.");
                    return;
                }

                // Update the material ID 
                modeldata.fe_data.updateMaterial(material_id);
                modeldata.fe_data.clear_selected_mesh();

                // Call the main form
                if (this.Owner is main_frm mainForm)
                {
                    mainForm.CallFrom_matprop_frm();
                }

                update_selected_element_list();
            }

        }



        public void update_selected_element_list()
        {
            // Clear the text box
            textBox_selectedelements.Clear();

            List<int> all_selected_ids = new List<int>();

           all_selected_ids.AddRange(modeldata.fe_data.selected_tri_ids);
           all_selected_ids.AddRange(modeldata.fe_data.selected_quad_ids);

            textBox_selectedelements.Text = string.Join(", ", all_selected_ids);

            textBox_selectedelements.Invalidate();

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

        private void matprop_frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Control the flag
            modeldata.isMaterialUpdateInProgress = false;
            modeldata.fe_data.clear_selected_mesh();

            // Call the main form
            if (this.Owner is main_frm mainForm)
            {
                mainForm.CallFrom_matprop_frm();
            }
        }
    }
}
