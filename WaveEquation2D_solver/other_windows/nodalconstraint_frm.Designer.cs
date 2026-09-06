namespace WaveEquation2D_solver.other_windows
{
    partial class nodalconstraint_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(nodalconstraint_frm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_source = new System.Windows.Forms.RadioButton();
            this.radioButton_dirichlet = new System.Windows.Forms.RadioButton();
            this.textBox_source = new System.Windows.Forms.TextBox();
            this.label_source = new System.Windows.Forms.Label();
            this.textBox_dirichlet = new System.Windows.Forms.TextBox();
            this.label_dirichlet = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_selectednodes = new System.Windows.Forms.TextBox();
            this.button_applyconstraint = new System.Windows.Forms.Button();
            this.button_deleteconstraint = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.rectangleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.circleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView_ConstraintList = new System.Windows.Forms.DataGridView();
            this.Column1_constraintid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2_nodeids = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3_fieldvalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4_sourcevalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ConstraintList)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_source);
            this.groupBox1.Controls.Add(this.radioButton_dirichlet);
            this.groupBox1.Controls.Add(this.textBox_source);
            this.groupBox1.Controls.Add(this.label_source);
            this.groupBox1.Controls.Add(this.textBox_dirichlet);
            this.groupBox1.Controls.Add(this.label_dirichlet);
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(287, 195);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Nodal Constraint Data: ";
            // 
            // radioButton_source
            // 
            this.radioButton_source.AutoSize = true;
            this.radioButton_source.Location = new System.Drawing.Point(11, 119);
            this.radioButton_source.Name = "radioButton_source";
            this.radioButton_source.Size = new System.Drawing.Size(180, 19);
            this.radioButton_source.TabIndex = 7;
            this.radioButton_source.TabStop = true;
            this.radioButton_source.Text = "Source/ External Excitation: ";
            this.radioButton_source.UseVisualStyleBackColor = true;
            this.radioButton_source.CheckedChanged += new System.EventHandler(this.radioButton_source_CheckedChanged);
            // 
            // radioButton_dirichlet
            // 
            this.radioButton_dirichlet.AutoSize = true;
            this.radioButton_dirichlet.Checked = true;
            this.radioButton_dirichlet.Location = new System.Drawing.Point(11, 39);
            this.radioButton_dirichlet.Name = "radioButton_dirichlet";
            this.radioButton_dirichlet.Size = new System.Drawing.Size(262, 19);
            this.radioButton_dirichlet.TabIndex = 6;
            this.radioButton_dirichlet.TabStop = true;
            this.radioButton_dirichlet.Text = "Essential or Dirichlet Boundary Condition: ";
            this.radioButton_dirichlet.UseVisualStyleBackColor = true;
            this.radioButton_dirichlet.CheckedChanged += new System.EventHandler(this.radioButton_dirichlet_CheckedChanged);
            // 
            // textBox_source
            // 
            this.textBox_source.Location = new System.Drawing.Point(93, 144);
            this.textBox_source.Name = "textBox_source";
            this.textBox_source.Size = new System.Drawing.Size(100, 23);
            this.textBox_source.TabIndex = 5;
            this.textBox_source.Text = "0";
            // 
            // label_source
            // 
            this.label_source.AutoSize = true;
            this.label_source.Location = new System.Drawing.Point(40, 147);
            this.label_source.Name = "label_source";
            this.label_source.Size = new System.Drawing.Size(47, 15);
            this.label_source.TabIndex = 4;
            this.label_source.Text = "f(x,y) =";
            // 
            // textBox_dirichlet
            // 
            this.textBox_dirichlet.Location = new System.Drawing.Point(93, 64);
            this.textBox_dirichlet.Name = "textBox_dirichlet";
            this.textBox_dirichlet.Size = new System.Drawing.Size(100, 23);
            this.textBox_dirichlet.TabIndex = 2;
            this.textBox_dirichlet.Text = "0";
            // 
            // label_dirichlet
            // 
            this.label_dirichlet.AutoSize = true;
            this.label_dirichlet.Location = new System.Drawing.Point(61, 67);
            this.label_dirichlet.Name = "label_dirichlet";
            this.label_dirichlet.Size = new System.Drawing.Size(26, 15);
            this.label_dirichlet.TabIndex = 1;
            this.label_dirichlet.Text = "ϕ =";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 238);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Selected Nodes: ";
            // 
            // textBox_selectednodes
            // 
            this.textBox_selectednodes.Location = new System.Drawing.Point(12, 256);
            this.textBox_selectednodes.Multiline = true;
            this.textBox_selectednodes.Name = "textBox_selectednodes";
            this.textBox_selectednodes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectednodes.Size = new System.Drawing.Size(287, 101);
            this.textBox_selectednodes.TabIndex = 3;
            // 
            // button_applyconstraint
            // 
            this.button_applyconstraint.Location = new System.Drawing.Point(365, 313);
            this.button_applyconstraint.Name = "button_applyconstraint";
            this.button_applyconstraint.Size = new System.Drawing.Size(134, 35);
            this.button_applyconstraint.TabIndex = 4;
            this.button_applyconstraint.Text = "Apply Constraint";
            this.button_applyconstraint.UseVisualStyleBackColor = true;
            this.button_applyconstraint.Click += new System.EventHandler(this.button_applyconstraint_Click);
            // 
            // button_deleteconstraint
            // 
            this.button_deleteconstraint.Location = new System.Drawing.Point(547, 313);
            this.button_deleteconstraint.Name = "button_deleteconstraint";
            this.button_deleteconstraint.Size = new System.Drawing.Size(130, 35);
            this.button_deleteconstraint.TabIndex = 5;
            this.button_deleteconstraint.Text = "Delete Constraint";
            this.button_deleteconstraint.UseVisualStyleBackColor = true;
            this.button_deleteconstraint.Click += new System.EventHandler(this.button_deleteconstraint_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rectangleSelectionToolStripMenuItem,
            this.circleSelectionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(754, 24);
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
            this.Column4_sourcevalue});
            this.dataGridView_ConstraintList.Location = new System.Drawing.Point(306, 40);
            this.dataGridView_ConstraintList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView_ConstraintList.MultiSelect = false;
            this.dataGridView_ConstraintList.Name = "dataGridView_ConstraintList";
            this.dataGridView_ConstraintList.ReadOnly = true;
            this.dataGridView_ConstraintList.RowHeadersWidth = 62;
            this.dataGridView_ConstraintList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_ConstraintList.Size = new System.Drawing.Size(435, 251);
            this.dataGridView_ConstraintList.TabIndex = 7;
            // 
            // Column1_constraintid
            // 
            this.Column1_constraintid.FillWeight = 80F;
            this.Column1_constraintid.HeaderText = "Node Constraint ID";
            this.Column1_constraintid.MinimumWidth = 8;
            this.Column1_constraintid.Name = "Column1_constraintid";
            this.Column1_constraintid.ReadOnly = true;
            this.Column1_constraintid.Width = 80;
            // 
            // Column2_nodeids
            // 
            this.Column2_nodeids.HeaderText = "Node IDs";
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
            // Column4_sourcevalue
            // 
            this.Column4_sourcevalue.FillWeight = 80F;
            this.Column4_sourcevalue.HeaderText = "Source (f)";
            this.Column4_sourcevalue.MinimumWidth = 8;
            this.Column4_sourcevalue.Name = "Column4_sourcevalue";
            this.Column4_sourcevalue.ReadOnly = true;
            this.Column4_sourcevalue.Width = 80;
            // 
            // nodalconstraint_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 371);
            this.Controls.Add(this.dataGridView_ConstraintList);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button_deleteconstraint);
            this.Controls.Add(this.button_applyconstraint);
            this.Controls.Add(this.textBox_selectednodes);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(780, 420);
            this.MinimumSize = new System.Drawing.Size(770, 410);
            this.Name = "nodalconstraint_frm";
            this.Opacity = 0.85D;
            this.Text = "Nodal Constraints";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.nodalconstraint_frm_FormClosing);
            this.Load += new System.EventHandler(this.nodalconstraint_frm_Load);
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
        private System.Windows.Forms.TextBox textBox_source;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.TextBox textBox_dirichlet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_selectednodes;
        private System.Windows.Forms.Button button_applyconstraint;
        private System.Windows.Forms.Button button_deleteconstraint;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rectangleSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem circleSelectionToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView_ConstraintList;
        private System.Windows.Forms.RadioButton radioButton_source;
        private System.Windows.Forms.RadioButton radioButton_dirichlet;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1_constraintid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2_nodeids;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3_fieldvalue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4_sourcevalue;
    }
}