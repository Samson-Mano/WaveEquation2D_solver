namespace WaveEquation2D_solver.other_windows
{
    partial class edgeconstraint_frm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(edgeconstraint_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_neumann = new System.Windows.Forms.CheckBox();
            this.checkBox_dirichlet = new System.Windows.Forms.CheckBox();
            this.radioButton_boundaryconditions = new System.Windows.Forms.RadioButton();
            this.label_sommerfield = new System.Windows.Forms.Label();
            this.radioButton_sommerfield = new System.Windows.Forms.RadioButton();
            this.textBox_neumann = new System.Windows.Forms.TextBox();
            this.label_neumann = new System.Windows.Forms.Label();
            this.textBox_dirichlet = new System.Windows.Forms.TextBox();
            this.label_dirichlet = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_selectededges = new System.Windows.Forms.TextBox();
            this.button_applyconstraint = new System.Windows.Forms.Button();
            this.button_deleteconstraint = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.rectangleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.circleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView_ConstraintList = new System.Windows.Forms.DataGridView();
            this.Column1_constraintid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2_nodeids = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3_fieldvalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4_derivfieldvalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4_isSommerfield = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ConstraintList)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_neumann);
            this.groupBox1.Controls.Add(this.checkBox_dirichlet);
            this.groupBox1.Controls.Add(this.radioButton_boundaryconditions);
            this.groupBox1.Controls.Add(this.label_sommerfield);
            this.groupBox1.Controls.Add(this.radioButton_sommerfield);
            this.groupBox1.Controls.Add(this.textBox_neumann);
            this.groupBox1.Controls.Add(this.label_neumann);
            this.groupBox1.Controls.Add(this.textBox_dirichlet);
            this.groupBox1.Controls.Add(this.label_dirichlet);
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 262);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Edge Constraint Data: ";
            // 
            // checkBox_neumann
            // 
            this.checkBox_neumann.AutoSize = true;
            this.checkBox_neumann.Location = new System.Drawing.Point(29, 114);
            this.checkBox_neumann.Name = "checkBox_neumann";
            this.checkBox_neumann.Size = new System.Drawing.Size(261, 19);
            this.checkBox_neumann.TabIndex = 12;
            this.checkBox_neumann.Text = "Natural or Newmann Boundary Condition: ";
            this.checkBox_neumann.UseVisualStyleBackColor = true;
            this.checkBox_neumann.CheckedChanged += new System.EventHandler(this.checkBox_neumann_CheckedChanged);
            // 
            // checkBox_dirichlet
            // 
            this.checkBox_dirichlet.AutoSize = true;
            this.checkBox_dirichlet.Checked = true;
            this.checkBox_dirichlet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_dirichlet.Location = new System.Drawing.Point(29, 60);
            this.checkBox_dirichlet.Name = "checkBox_dirichlet";
            this.checkBox_dirichlet.Size = new System.Drawing.Size(263, 19);
            this.checkBox_dirichlet.TabIndex = 11;
            this.checkBox_dirichlet.Text = "Essential or Dirichlet Boundary Condition: ";
            this.checkBox_dirichlet.UseVisualStyleBackColor = true;
            this.checkBox_dirichlet.CheckedChanged += new System.EventHandler(this.checkBox_dirichlet_CheckedChanged);
            // 
            // radioButton_boundaryconditions
            // 
            this.radioButton_boundaryconditions.AutoSize = true;
            this.radioButton_boundaryconditions.Checked = true;
            this.radioButton_boundaryconditions.Location = new System.Drawing.Point(11, 35);
            this.radioButton_boundaryconditions.Name = "radioButton_boundaryconditions";
            this.radioButton_boundaryconditions.Size = new System.Drawing.Size(145, 19);
            this.radioButton_boundaryconditions.TabIndex = 10;
            this.radioButton_boundaryconditions.TabStop = true;
            this.radioButton_boundaryconditions.Text = "Boundary Conditions:";
            this.radioButton_boundaryconditions.UseVisualStyleBackColor = true;
            this.radioButton_boundaryconditions.CheckedChanged += new System.EventHandler(this.radioButton_boundaryconditions_CheckedChanged);
            // 
            // label_sommerfield
            // 
            this.label_sommerfield.AutoSize = true;
            this.label_sommerfield.Location = new System.Drawing.Point(37, 222);
            this.label_sommerfield.Name = "label_sommerfield";
            this.label_sommerfield.Size = new System.Drawing.Size(93, 15);
            this.label_sommerfield.TabIndex = 9;
            this.label_sommerfield.Text = "∂ϕ/∂n - ikϕ = 0";
            // 
            // radioButton_sommerfield
            // 
            this.radioButton_sommerfield.AutoSize = true;
            this.radioButton_sommerfield.Location = new System.Drawing.Point(11, 191);
            this.radioButton_sommerfield.Name = "radioButton_sommerfield";
            this.radioButton_sommerfield.Size = new System.Drawing.Size(256, 19);
            this.radioButton_sommerfield.TabIndex = 8;
            this.radioButton_sommerfield.Text = "ABC or Sommerfield Radiation Condition: ";
            this.radioButton_sommerfield.UseVisualStyleBackColor = true;
            this.radioButton_sommerfield.CheckedChanged += new System.EventHandler(this.radioButton_sommerfield_CheckedChanged);
            // 
            // textBox_neumann
            // 
            this.textBox_neumann.Location = new System.Drawing.Point(93, 139);
            this.textBox_neumann.Name = "textBox_neumann";
            this.textBox_neumann.Size = new System.Drawing.Size(100, 23);
            this.textBox_neumann.TabIndex = 5;
            this.textBox_neumann.Text = "0";
            // 
            // label_neumann
            // 
            this.label_neumann.AutoSize = true;
            this.label_neumann.Location = new System.Drawing.Point(37, 142);
            this.label_neumann.Name = "label_neumann";
            this.label_neumann.Size = new System.Drawing.Size(50, 15);
            this.label_neumann.TabIndex = 4;
            this.label_neumann.Text = "∂ϕ/∂n=";
            // 
            // textBox_dirichlet
            // 
            this.textBox_dirichlet.Location = new System.Drawing.Point(93, 85);
            this.textBox_dirichlet.Name = "textBox_dirichlet";
            this.textBox_dirichlet.Size = new System.Drawing.Size(100, 23);
            this.textBox_dirichlet.TabIndex = 2;
            this.textBox_dirichlet.Text = "0";
            // 
            // label_dirichlet
            // 
            this.label_dirichlet.AutoSize = true;
            this.label_dirichlet.Location = new System.Drawing.Point(61, 88);
            this.label_dirichlet.Name = "label_dirichlet";
            this.label_dirichlet.Size = new System.Drawing.Size(26, 15);
            this.label_dirichlet.TabIndex = 1;
            this.label_dirichlet.Text = "ϕ =";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 314);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Selected Edges: ";
            // 
            // textBox_selectededges
            // 
            this.textBox_selectededges.Location = new System.Drawing.Point(12, 332);
            this.textBox_selectededges.Multiline = true;
            this.textBox_selectededges.Name = "textBox_selectededges";
            this.textBox_selectededges.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectededges.Size = new System.Drawing.Size(310, 101);
            this.textBox_selectededges.TabIndex = 3;
            // 
            // button_applyconstraint
            // 
            this.button_applyconstraint.Location = new System.Drawing.Point(386, 365);
            this.button_applyconstraint.Name = "button_applyconstraint";
            this.button_applyconstraint.Size = new System.Drawing.Size(134, 35);
            this.button_applyconstraint.TabIndex = 4;
            this.button_applyconstraint.Text = "Apply Constraint";
            this.button_applyconstraint.UseVisualStyleBackColor = true;
            this.button_applyconstraint.Click += new System.EventHandler(this.button_applyconstraint_Click);
            // 
            // button_deleteconstraint
            // 
            this.button_deleteconstraint.Location = new System.Drawing.Point(560, 365);
            this.button_deleteconstraint.Name = "button_deleteconstraint";
            this.button_deleteconstraint.Size = new System.Drawing.Size(134, 35);
            this.button_deleteconstraint.TabIndex = 5;
            this.button_deleteconstraint.Text = "Delete Constraint";
            this.button_deleteconstraint.UseVisualStyleBackColor = true;
            this.button_deleteconstraint.Click += new System.EventHandler(this.button_deleteconstraint_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rectangleSelectionToolStripMenuItem,
            this.circleSelectionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(804, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // rectangleSelectionToolStripMenuItem
            // 
            this.rectangleSelectionToolStripMenuItem.Checked = true;
            this.rectangleSelectionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rectangleSelectionToolStripMenuItem.Name = "rectangleSelectionToolStripMenuItem";
            this.rectangleSelectionToolStripMenuItem.Size = new System.Drawing.Size(122, 20);
            this.rectangleSelectionToolStripMenuItem.Text = "Rectangle Selection";
            this.rectangleSelectionToolStripMenuItem.Click += new System.EventHandler(this.rectangleSelectionToolStripMenuItem_Click);
            // 
            // circleSelectionToolStripMenuItem
            // 
            this.circleSelectionToolStripMenuItem.Name = "circleSelectionToolStripMenuItem";
            this.circleSelectionToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
            this.circleSelectionToolStripMenuItem.Text = "Circle Selection";
            this.circleSelectionToolStripMenuItem.Click += new System.EventHandler(this.circleSelectionToolStripMenuItem_Click);
            // 
            // dataGridView_ConstraintList
            // 
            this.dataGridView_ConstraintList.AllowUserToAddRows = false;
            this.dataGridView_ConstraintList.AllowUserToDeleteRows = false;
            this.dataGridView_ConstraintList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_ConstraintList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_ConstraintList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1_constraintid,
            this.Column2_nodeids,
            this.Column3_fieldvalue,
            this.Column4_derivfieldvalue,
            this.Column4_isSommerfield});
            this.dataGridView_ConstraintList.Location = new System.Drawing.Point(314, 40);
            this.dataGridView_ConstraintList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView_ConstraintList.MultiSelect = false;
            this.dataGridView_ConstraintList.Name = "dataGridView_ConstraintList";
            this.dataGridView_ConstraintList.ReadOnly = true;
            this.dataGridView_ConstraintList.RowHeadersWidth = 62;
            this.dataGridView_ConstraintList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_ConstraintList.Size = new System.Drawing.Size(477, 262);
            this.dataGridView_ConstraintList.TabIndex = 8;
            // 
            // Column1_constraintid
            // 
            this.Column1_constraintid.FillWeight = 80F;
            this.Column1_constraintid.HeaderText = "Edge Constraint ID";
            this.Column1_constraintid.MinimumWidth = 8;
            this.Column1_constraintid.Name = "Column1_constraintid";
            this.Column1_constraintid.ReadOnly = true;
            this.Column1_constraintid.Width = 80;
            // 
            // Column2_nodeids
            // 
            this.Column2_nodeids.HeaderText = "Edge IDs";
            this.Column2_nodeids.MinimumWidth = 8;
            this.Column2_nodeids.Name = "Column2_nodeids";
            this.Column2_nodeids.ReadOnly = true;
            // 
            // Column3_fieldvalue
            // 
            this.Column3_fieldvalue.FillWeight = 80F;
            this.Column3_fieldvalue.HeaderText = "Field value (ϕ)";
            this.Column3_fieldvalue.MinimumWidth = 8;
            this.Column3_fieldvalue.Name = "Column3_fieldvalue";
            this.Column3_fieldvalue.ReadOnly = true;
            this.Column3_fieldvalue.Width = 80;
            // 
            // Column4_derivfieldvalue
            // 
            this.Column4_derivfieldvalue.FillWeight = 80F;
            this.Column4_derivfieldvalue.HeaderText = "Derivative Field value (∂ϕ/∂n)";
            this.Column4_derivfieldvalue.MinimumWidth = 8;
            this.Column4_derivfieldvalue.Name = "Column4_derivfieldvalue";
            this.Column4_derivfieldvalue.ReadOnly = true;
            this.Column4_derivfieldvalue.Width = 80;
            // 
            // Column4_isSommerfield
            // 
            this.Column4_isSommerfield.FillWeight = 65F;
            this.Column4_isSommerfield.HeaderText = "ABC";
            this.Column4_isSommerfield.Name = "Column4_isSommerfield";
            this.Column4_isSommerfield.ReadOnly = true;
            this.Column4_isSommerfield.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column4_isSommerfield.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column4_isSommerfield.Width = 65;
            // 
            // edgeconstraint_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 451);
            this.Controls.Add(this.dataGridView_ConstraintList);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button_deleteconstraint);
            this.Controls.Add(this.button_applyconstraint);
            this.Controls.Add(this.textBox_selectededges);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(820, 500);
            this.MinimumSize = new System.Drawing.Size(810, 490);
            this.Name = "edgeconstraint_frm";
            this.Opacity = 0.85D;
            this.Text = "Edge Constraints";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.edgeconstraint_frm_FormClosing);
            this.Load += new System.EventHandler(this.edgeconstraint_frm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ConstraintList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label_dirichlet;
        private System.Windows.Forms.TextBox textBox_neumann;
        private System.Windows.Forms.Label label_neumann;
        private System.Windows.Forms.TextBox textBox_dirichlet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_selectededges;
        private System.Windows.Forms.Button button_applyconstraint;
        private System.Windows.Forms.Button button_deleteconstraint;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rectangleSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circleSelectionToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButton_sommerfield;
        private System.Windows.Forms.Label label_sommerfield;
        private System.Windows.Forms.RadioButton radioButton_boundaryconditions;
        private System.Windows.Forms.CheckBox checkBox_neumann;
        private System.Windows.Forms.CheckBox checkBox_dirichlet;
        private System.Windows.Forms.DataGridView dataGridView_ConstraintList;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1_constraintid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2_nodeids;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3_fieldvalue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4_derivfieldvalue;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column4_isSommerfield;
    }
}