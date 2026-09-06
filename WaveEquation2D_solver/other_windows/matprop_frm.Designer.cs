namespace WaveEquation2D_solver.other_windows
{
    partial class matprop_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(matprop_frm));
            this.circleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.rectangleSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button_assignmaterial = new System.Windows.Forms.Button();
            this.textBox_selectedelements = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_delete = new System.Windows.Forms.Button();
            this.button_update = new System.Windows.Forms.Button();
            this.button_create = new System.Windows.Forms.Button();
            this.textBox_poissonsratio = new System.Windows.Forms.TextBox();
            this.textBox_density = new System.Windows.Forms.TextBox();
            this.textBox_youngsmodulus = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_materialname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridView_MaterialList = new System.Windows.Forms.DataGridView();
            this.textBox_yieldpoint = new System.Windows.Forms.TextBox();
            this.textBox_thickness = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.Column1_materialid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2_materialname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3_youngsmodulus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4_density = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5_poissonsratio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6_yieldpoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7_thickness = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8_materialcolor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MaterialList)).BeginInit();
            this.SuspendLayout();
            // 
            // circleSelectionToolStripMenuItem
            // 
            this.circleSelectionToolStripMenuItem.Name = "circleSelectionToolStripMenuItem";
            this.circleSelectionToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
            this.circleSelectionToolStripMenuItem.Text = "Circle Selection";
            this.circleSelectionToolStripMenuItem.Click += new System.EventHandler(this.circleSelectionToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rectangleSelectionToolStripMenuItem,
            this.circleSelectionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 7;
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
            // button_assignmaterial
            // 
            this.button_assignmaterial.Location = new System.Drawing.Point(178, 218);
            this.button_assignmaterial.Name = "button_assignmaterial";
            this.button_assignmaterial.Size = new System.Drawing.Size(137, 28);
            this.button_assignmaterial.TabIndex = 3;
            this.button_assignmaterial.Text = "Assign Material";
            this.button_assignmaterial.UseVisualStyleBackColor = true;
            this.button_assignmaterial.Click += new System.EventHandler(this.button_assignmaterial_Click);
            // 
            // textBox_selectedelements
            // 
            this.textBox_selectedelements.Location = new System.Drawing.Point(9, 51);
            this.textBox_selectedelements.Multiline = true;
            this.textBox_selectedelements.Name = "textBox_selectedelements";
            this.textBox_selectedelements.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_selectedelements.Size = new System.Drawing.Size(467, 147);
            this.textBox_selectedelements.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(140, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Selected Elements: ";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_assignmaterial);
            this.groupBox2.Controls.Add(this.textBox_selectedelements);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(390, 248);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(482, 260);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Assign Material:";
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(236, 219);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(93, 28);
            this.button_delete.TabIndex = 10;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // button_update
            // 
            this.button_update.Location = new System.Drawing.Point(137, 219);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(93, 28);
            this.button_update.TabIndex = 9;
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            this.button_update.Click += new System.EventHandler(this.button_update_Click);
            // 
            // button_create
            // 
            this.button_create.Location = new System.Drawing.Point(38, 219);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(93, 28);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "Create";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // textBox_poissonsratio
            // 
            this.textBox_poissonsratio.Location = new System.Drawing.Point(176, 118);
            this.textBox_poissonsratio.Name = "textBox_poissonsratio";
            this.textBox_poissonsratio.Size = new System.Drawing.Size(130, 23);
            this.textBox_poissonsratio.TabIndex = 7;
            // 
            // textBox_density
            // 
            this.textBox_density.Location = new System.Drawing.Point(176, 89);
            this.textBox_density.Name = "textBox_density";
            this.textBox_density.Size = new System.Drawing.Size(130, 23);
            this.textBox_density.TabIndex = 6;
            // 
            // textBox_youngsmodulus
            // 
            this.textBox_youngsmodulus.Location = new System.Drawing.Point(176, 60);
            this.textBox_youngsmodulus.Name = "textBox_youngsmodulus";
            this.textBox_youngsmodulus.Size = new System.Drawing.Size(130, 23);
            this.textBox_youngsmodulus.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Poissons Ratio (ν): ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(78, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Density (μ): ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Youngs Modulus (E): ";
            // 
            // textBox_materialname
            // 
            this.textBox_materialname.Location = new System.Drawing.Point(176, 31);
            this.textBox_materialname.Name = "textBox_materialname";
            this.textBox_materialname.Size = new System.Drawing.Size(130, 23);
            this.textBox_materialname.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Material Name: ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_thickness);
            this.groupBox1.Controls.Add(this.textBox_yieldpoint);
            this.groupBox1.Controls.Add(this.button_delete);
            this.groupBox1.Controls.Add(this.button_update);
            this.groupBox1.Controls.Add(this.button_create);
            this.groupBox1.Controls.Add(this.textBox_poissonsratio);
            this.groupBox1.Controls.Add(this.textBox_density);
            this.groupBox1.Controls.Add(this.textBox_youngsmodulus);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_materialname);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 247);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(372, 261);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Material Data: ";
            // 
            // dataGridView_MaterialList
            // 
            this.dataGridView_MaterialList.AllowUserToAddRows = false;
            this.dataGridView_MaterialList.AllowUserToDeleteRows = false;
            this.dataGridView_MaterialList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_MaterialList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_MaterialList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1_materialid,
            this.Column2_materialname,
            this.Column3_youngsmodulus,
            this.Column4_density,
            this.Column5_poissonsratio,
            this.Column6_yieldpoint,
            this.Column7_thickness,
            this.Column8_materialcolor});
            this.dataGridView_MaterialList.Location = new System.Drawing.Point(12, 42);
            this.dataGridView_MaterialList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridView_MaterialList.MultiSelect = false;
            this.dataGridView_MaterialList.Name = "dataGridView_MaterialList";
            this.dataGridView_MaterialList.ReadOnly = true;
            this.dataGridView_MaterialList.RowHeadersWidth = 62;
            this.dataGridView_MaterialList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_MaterialList.Size = new System.Drawing.Size(860, 199);
            this.dataGridView_MaterialList.TabIndex = 4;
            this.dataGridView_MaterialList.SelectionChanged += new System.EventHandler(this.dataGridView_MaterialList_SelectionChanged);
            // 
            // textBox_yieldpoint
            // 
            this.textBox_yieldpoint.Location = new System.Drawing.Point(176, 147);
            this.textBox_yieldpoint.Name = "textBox_yieldpoint";
            this.textBox_yieldpoint.Size = new System.Drawing.Size(130, 23);
            this.textBox_yieldpoint.TabIndex = 11;
            // 
            // textBox_thickness
            // 
            this.textBox_thickness.Location = new System.Drawing.Point(176, 176);
            this.textBox_thickness.Name = "textBox_thickness";
            this.textBox_thickness.Size = new System.Drawing.Size(130, 23);
            this.textBox_thickness.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(59, 150);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 16);
            this.label6.TabIndex = 13;
            this.label6.Text = "Yield Point (Y): ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(66, 179);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 16);
            this.label7.TabIndex = 14;
            this.label7.Text = "Thickness (t): ";
            // 
            // Column1_materialid
            // 
            this.Column1_materialid.FillWeight = 40F;
            this.Column1_materialid.HeaderText = "ID";
            this.Column1_materialid.MinimumWidth = 8;
            this.Column1_materialid.Name = "Column1_materialid";
            this.Column1_materialid.ReadOnly = true;
            this.Column1_materialid.Width = 40;
            // 
            // Column2_materialname
            // 
            this.Column2_materialname.FillWeight = 110F;
            this.Column2_materialname.HeaderText = "Material Name";
            this.Column2_materialname.MinimumWidth = 8;
            this.Column2_materialname.Name = "Column2_materialname";
            this.Column2_materialname.ReadOnly = true;
            this.Column2_materialname.Width = 110;
            // 
            // Column3_youngsmodulus
            // 
            this.Column3_youngsmodulus.FillWeight = 130F;
            this.Column3_youngsmodulus.HeaderText = "Youngs Modulus (E) ";
            this.Column3_youngsmodulus.MinimumWidth = 8;
            this.Column3_youngsmodulus.Name = "Column3_youngsmodulus";
            this.Column3_youngsmodulus.ReadOnly = true;
            this.Column3_youngsmodulus.Width = 130;
            // 
            // Column4_density
            // 
            this.Column4_density.FillWeight = 130F;
            this.Column4_density.HeaderText = "Density (μ) ";
            this.Column4_density.MinimumWidth = 8;
            this.Column4_density.Name = "Column4_density";
            this.Column4_density.ReadOnly = true;
            this.Column4_density.Width = 130;
            // 
            // Column5_poissonsratio
            // 
            this.Column5_poissonsratio.HeaderText = "Poissons Ratio (ν) ";
            this.Column5_poissonsratio.MinimumWidth = 8;
            this.Column5_poissonsratio.Name = "Column5_poissonsratio";
            this.Column5_poissonsratio.ReadOnly = true;
            // 
            // Column6_yieldpoint
            // 
            this.Column6_yieldpoint.FillWeight = 120F;
            this.Column6_yieldpoint.HeaderText = "Yield Point (Y)";
            this.Column6_yieldpoint.Name = "Column6_yieldpoint";
            this.Column6_yieldpoint.ReadOnly = true;
            this.Column6_yieldpoint.Width = 120;
            // 
            // Column7_thickness
            // 
            this.Column7_thickness.FillWeight = 80F;
            this.Column7_thickness.HeaderText = "Thickness (t)";
            this.Column7_thickness.Name = "Column7_thickness";
            this.Column7_thickness.ReadOnly = true;
            this.Column7_thickness.Width = 80;
            // 
            // Column8_materialcolor
            // 
            this.Column8_materialcolor.FillWeight = 70F;
            this.Column8_materialcolor.HeaderText = "Color";
            this.Column8_materialcolor.MinimumWidth = 6;
            this.Column8_materialcolor.Name = "Column8_materialcolor";
            this.Column8_materialcolor.ReadOnly = true;
            this.Column8_materialcolor.Width = 70;
            // 
            // matprop_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 517);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView_MaterialList);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(900, 556);
            this.Name = "matprop_frm";
            this.Opacity = 0.85D;
            this.Text = "Material Properties";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.matprop_frm_FormClosing);
            this.Load += new System.EventHandler(this.matprop_frm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_MaterialList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem circleSelectionToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rectangleSelectionToolStripMenuItem;
        private System.Windows.Forms.Button button_assignmaterial;
        private System.Windows.Forms.TextBox textBox_selectedelements;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.TextBox textBox_poissonsratio;
        private System.Windows.Forms.TextBox textBox_density;
        private System.Windows.Forms.TextBox textBox_youngsmodulus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_materialname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dataGridView_MaterialList;
        private System.Windows.Forms.TextBox textBox_thickness;
        private System.Windows.Forms.TextBox textBox_yieldpoint;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1_materialid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2_materialname;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3_youngsmodulus;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4_density;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5_poissonsratio;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6_yieldpoint;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7_thickness;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8_materialcolor;
    }
}