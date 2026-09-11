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
            this.textBox_sourcestarttime = new System.Windows.Forms.TextBox();
            this.label_sourcestarttime = new System.Windows.Forms.Label();
            this.comboBox_sourcetype = new System.Windows.Forms.ComboBox();
            this.label_sourcetype = new System.Windows.Forms.Label();
            this.textBox_sourcefreq = new System.Windows.Forms.TextBox();
            this.label_sourcefreq = new System.Windows.Forms.Label();
            this.radioButton_source = new System.Windows.Forms.RadioButton();
            this.textBox_sourceampl = new System.Windows.Forms.TextBox();
            this.label_source = new System.Windows.Forms.Label();
            this.checkBox_neumann = new System.Windows.Forms.CheckBox();
            this.checkBox_dirichlet = new System.Windows.Forms.CheckBox();
            this.radioButton_boundaryconditions = new System.Windows.Forms.RadioButton();
            this.label_sommerfield = new System.Windows.Forms.Label();
            this.radioButton_sommerfield = new System.Windows.Forms.RadioButton();
            this.textBox_neumann = new System.Windows.Forms.TextBox();
            this.label_neumann = new System.Windows.Forms.Label();
            this.textBox_dirichlet = new System.Windows.Forms.TextBox();
            this.label_dirichlet = new System.Windows.Forms.Label();
            this.label_selectedEdgeCount = new System.Windows.Forms.Label();
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
            this.Column6_sourcevalue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7_sourcefrequency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ConstraintList)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_sourcestarttime);
            this.groupBox1.Controls.Add(this.label_sourcestarttime);
            this.groupBox1.Controls.Add(this.comboBox_sourcetype);
            this.groupBox1.Controls.Add(this.label_sourcetype);
            this.groupBox1.Controls.Add(this.textBox_sourcefreq);
            this.groupBox1.Controls.Add(this.label_sourcefreq);
            this.groupBox1.Controls.Add(this.radioButton_source);
            this.groupBox1.Controls.Add(this.textBox_sourceampl);
            this.groupBox1.Controls.Add(this.label_source);
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
            this.groupBox1.Size = new System.Drawing.Size(295, 472);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Edge Constraint Data: ";
            // 
            // textBox_sourcestarttime
            // 
            this.textBox_sourcestarttime.Location = new System.Drawing.Point(117, 410);
            this.textBox_sourcestarttime.Name = "textBox_sourcestarttime";
            this.textBox_sourcestarttime.Size = new System.Drawing.Size(76, 23);
            this.textBox_sourcestarttime.TabIndex = 21;
            this.textBox_sourcestarttime.Text = "0";
            // 
            // label_sourcestarttime
            // 
            this.label_sourcestarttime.AutoSize = true;
            this.label_sourcestarttime.Location = new System.Drawing.Point(23, 413);
            this.label_sourcestarttime.Name = "label_sourcestarttime";
            this.label_sourcestarttime.Size = new System.Drawing.Size(91, 15);
            this.label_sourcestarttime.TabIndex = 20;
            this.label_sourcestarttime.Text = "Start time t_s = ";
            // 
            // comboBox_sourcetype
            // 
            this.comboBox_sourcetype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_sourcetype.FormattingEnabled = true;
            this.comboBox_sourcetype.Items.AddRange(new object[] {
            "Half sine pulse",
            "Rectangular pulse",
            "Triangular pulse",
            "Step force with finite rise",
            "Full sine pulse",
            "Harmonic/ periodic excitation"});
            this.comboBox_sourcetype.Location = new System.Drawing.Point(117, 381);
            this.comboBox_sourcetype.Name = "comboBox_sourcetype";
            this.comboBox_sourcetype.Size = new System.Drawing.Size(133, 23);
            this.comboBox_sourcetype.TabIndex = 19;
            // 
            // label_sourcetype
            // 
            this.label_sourcetype.AutoSize = true;
            this.label_sourcetype.Location = new System.Drawing.Point(66, 384);
            this.label_sourcetype.Name = "label_sourcetype";
            this.label_sourcetype.Size = new System.Drawing.Size(48, 15);
            this.label_sourcetype.TabIndex = 18;
            this.label_sourcetype.Text = "Type = ";
            // 
            // textBox_sourcefreq
            // 
            this.textBox_sourcefreq.Location = new System.Drawing.Point(117, 349);
            this.textBox_sourcefreq.Name = "textBox_sourcefreq";
            this.textBox_sourcefreq.Size = new System.Drawing.Size(76, 23);
            this.textBox_sourcefreq.TabIndex = 17;
            this.textBox_sourcefreq.Text = "0";
            // 
            // label_sourcefreq
            // 
            this.label_sourcefreq.AutoSize = true;
            this.label_sourcefreq.Location = new System.Drawing.Point(75, 352);
            this.label_sourcefreq.Name = "label_sourcefreq";
            this.label_sourcefreq.Size = new System.Drawing.Size(36, 15);
            this.label_sourcefreq.TabIndex = 16;
            this.label_sourcefreq.Text = "ω_f =";
            // 
            // radioButton_source
            // 
            this.radioButton_source.AutoSize = true;
            this.radioButton_source.Location = new System.Drawing.Point(11, 295);
            this.radioButton_source.Name = "radioButton_source";
            this.radioButton_source.Size = new System.Drawing.Size(180, 19);
            this.radioButton_source.TabIndex = 15;
            this.radioButton_source.TabStop = true;
            this.radioButton_source.Text = "Source/ External Excitation: ";
            this.radioButton_source.UseVisualStyleBackColor = true;
            this.radioButton_source.CheckedChanged += new System.EventHandler(this.radioButton_source_CheckedChanged);
            // 
            // textBox_sourceampl
            // 
            this.textBox_sourceampl.Location = new System.Drawing.Point(117, 320);
            this.textBox_sourceampl.Name = "textBox_sourceampl";
            this.textBox_sourceampl.Size = new System.Drawing.Size(76, 23);
            this.textBox_sourceampl.TabIndex = 14;
            this.textBox_sourceampl.Text = "0";
            // 
            // label_source
            // 
            this.label_source.AutoSize = true;
            this.label_source.Location = new System.Drawing.Point(64, 323);
            this.label_source.Name = "label_source";
            this.label_source.Size = new System.Drawing.Size(47, 15);
            this.label_source.TabIndex = 13;
            this.label_source.Text = "f(x,y) =";
            // 
            // checkBox_neumann
            // 
            this.checkBox_neumann.AutoSize = true;
            this.checkBox_neumann.Location = new System.Drawing.Point(29, 123);
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
            this.checkBox_dirichlet.Location = new System.Drawing.Point(29, 69);
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
            this.radioButton_boundaryconditions.Location = new System.Drawing.Point(11, 44);
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
            this.label_sommerfield.Location = new System.Drawing.Point(75, 247);
            this.label_sommerfield.Name = "label_sommerfield";
            this.label_sommerfield.Size = new System.Drawing.Size(89, 15);
            this.label_sommerfield.TabIndex = 9;
            this.label_sommerfield.Text = "∂u/∂n - iku = 0";
            // 
            // radioButton_sommerfield
            // 
            this.radioButton_sommerfield.AutoSize = true;
            this.radioButton_sommerfield.Location = new System.Drawing.Point(11, 214);
            this.radioButton_sommerfield.Name = "radioButton_sommerfield";
            this.radioButton_sommerfield.Size = new System.Drawing.Size(256, 19);
            this.radioButton_sommerfield.TabIndex = 8;
            this.radioButton_sommerfield.Text = "ABC or Sommerfield Radiation Condition: ";
            this.radioButton_sommerfield.UseVisualStyleBackColor = true;
            this.radioButton_sommerfield.CheckedChanged += new System.EventHandler(this.radioButton_sommerfield_CheckedChanged);
            // 
            // textBox_neumann
            // 
            this.textBox_neumann.Location = new System.Drawing.Point(117, 148);
            this.textBox_neumann.Name = "textBox_neumann";
            this.textBox_neumann.Size = new System.Drawing.Size(76, 23);
            this.textBox_neumann.TabIndex = 5;
            this.textBox_neumann.Text = "0";
            // 
            // label_neumann
            // 
            this.label_neumann.AutoSize = true;
            this.label_neumann.Location = new System.Drawing.Point(61, 151);
            this.label_neumann.Name = "label_neumann";
            this.label_neumann.Size = new System.Drawing.Size(48, 15);
            this.label_neumann.TabIndex = 4;
            this.label_neumann.Text = "∂u/∂n=";
            // 
            // textBox_dirichlet
            // 
            this.textBox_dirichlet.Location = new System.Drawing.Point(117, 94);
            this.textBox_dirichlet.Name = "textBox_dirichlet";
            this.textBox_dirichlet.Size = new System.Drawing.Size(76, 23);
            this.textBox_dirichlet.TabIndex = 2;
            this.textBox_dirichlet.Text = "0";
            // 
            // label_dirichlet
            // 
            this.label_dirichlet.AutoSize = true;
            this.label_dirichlet.Location = new System.Drawing.Point(85, 97);
            this.label_dirichlet.Name = "label_dirichlet";
            this.label_dirichlet.Size = new System.Drawing.Size(24, 15);
            this.label_dirichlet.TabIndex = 1;
            this.label_dirichlet.Text = "u =";
            // 
            // label_selectedEdgeCount
            // 
            this.label_selectedEdgeCount.AutoSize = true;
            this.label_selectedEdgeCount.Location = new System.Drawing.Point(313, 337);
            this.label_selectedEdgeCount.Name = "label_selectedEdgeCount";
            this.label_selectedEdgeCount.Size = new System.Drawing.Size(93, 15);
            this.label_selectedEdgeCount.TabIndex = 1;
            this.label_selectedEdgeCount.Text = "Selected Edges: ";
            // 
            // textBox_selectededges
            // 
            this.textBox_selectededges.Location = new System.Drawing.Point(314, 355);
            this.textBox_selectededges.Multiline = true;
            this.textBox_selectededges.Name = "textBox_selectededges";
            this.textBox_selectededges.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectededges.Size = new System.Drawing.Size(648, 101);
            this.textBox_selectededges.TabIndex = 3;
            // 
            // button_applyconstraint
            // 
            this.button_applyconstraint.Location = new System.Drawing.Point(474, 477);
            this.button_applyconstraint.Name = "button_applyconstraint";
            this.button_applyconstraint.Size = new System.Drawing.Size(134, 35);
            this.button_applyconstraint.TabIndex = 4;
            this.button_applyconstraint.Text = "Apply Constraint";
            this.button_applyconstraint.UseVisualStyleBackColor = true;
            this.button_applyconstraint.Click += new System.EventHandler(this.button_applyconstraint_Click);
            // 
            // button_deleteconstraint
            // 
            this.button_deleteconstraint.Location = new System.Drawing.Point(648, 477);
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
            this.menuStrip1.Size = new System.Drawing.Size(974, 24);
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
            this.Column4_isSommerfield,
            this.Column6_sourcevalue,
            this.Column7_sourcefrequency});
            this.dataGridView_ConstraintList.Location = new System.Drawing.Point(314, 40);
            this.dataGridView_ConstraintList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView_ConstraintList.MultiSelect = false;
            this.dataGridView_ConstraintList.Name = "dataGridView_ConstraintList";
            this.dataGridView_ConstraintList.ReadOnly = true;
            this.dataGridView_ConstraintList.RowHeadersWidth = 62;
            this.dataGridView_ConstraintList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_ConstraintList.Size = new System.Drawing.Size(649, 286);
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
            this.Column4_isSommerfield.MinimumWidth = 6;
            this.Column4_isSommerfield.Name = "Column4_isSommerfield";
            this.Column4_isSommerfield.ReadOnly = true;
            this.Column4_isSommerfield.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column4_isSommerfield.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column4_isSommerfield.Width = 65;
            // 
            // Column6_sourcevalue
            // 
            this.Column6_sourcevalue.FillWeight = 80F;
            this.Column6_sourcevalue.HeaderText = "Source (f)";
            this.Column6_sourcevalue.Name = "Column6_sourcevalue";
            this.Column6_sourcevalue.ReadOnly = true;
            this.Column6_sourcevalue.Width = 80;
            // 
            // Column7_sourcefrequency
            // 
            this.Column7_sourcefrequency.FillWeight = 80F;
            this.Column7_sourcefrequency.HeaderText = "Source frequency (ω_f)";
            this.Column7_sourcefrequency.Name = "Column7_sourcefrequency";
            this.Column7_sourcefrequency.ReadOnly = true;
            this.Column7_sourcefrequency.Width = 80;
            // 
            // edgeconstraint_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 531);
            this.Controls.Add(this.dataGridView_ConstraintList);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.button_deleteconstraint);
            this.Controls.Add(this.button_applyconstraint);
            this.Controls.Add(this.textBox_selectededges);
            this.Controls.Add(this.label_selectedEdgeCount);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(990, 570);
            this.MinimumSize = new System.Drawing.Size(980, 560);
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
        private System.Windows.Forms.Label label_selectedEdgeCount;
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
        private System.Windows.Forms.ComboBox comboBox_sourcetype;
        private System.Windows.Forms.Label label_sourcetype;
        private System.Windows.Forms.TextBox textBox_sourcefreq;
        private System.Windows.Forms.Label label_sourcefreq;
        private System.Windows.Forms.RadioButton radioButton_source;
        private System.Windows.Forms.TextBox textBox_sourceampl;
        private System.Windows.Forms.Label label_source;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1_constraintid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2_nodeids;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3_fieldvalue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4_derivfieldvalue;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Column4_isSommerfield;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6_sourcevalue;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7_sourcefrequency;
        private System.Windows.Forms.TextBox textBox_sourcestarttime;
        private System.Windows.Forms.Label label_sourcestarttime;
    }
}