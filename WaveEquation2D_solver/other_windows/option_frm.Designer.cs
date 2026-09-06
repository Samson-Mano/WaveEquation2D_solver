namespace WaveEquation2D_solver.other_windows
{
    partial class option_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(option_frm));
            this.button_ok = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_paintmeshpoints = new System.Windows.Forms.CheckBox();
            this.checkBox_paintconstraints = new System.Windows.Forms.CheckBox();
            this.checkBox_paintloads = new System.Windows.Forms.CheckBox();
            this.checkBox_paintmeshboundaries = new System.Windows.Forms.CheckBox();
            this.checkBox_paintmesh = new System.Windows.Forms.CheckBox();
            this.checkBox_paintshrinkmesh = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(92, 359);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(92, 35);
            this.button_ok.TabIndex = 3;
            this.button_ok.Text = "Ok";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_paintmeshpoints);
            this.groupBox1.Controls.Add(this.checkBox_paintconstraints);
            this.groupBox1.Controls.Add(this.checkBox_paintloads);
            this.groupBox1.Controls.Add(this.checkBox_paintmeshboundaries);
            this.groupBox1.Controls.Add(this.checkBox_paintmesh);
            this.groupBox1.Controls.Add(this.checkBox_paintshrinkmesh);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 309);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Drawing Option";
            // 
            // checkBox_paintmeshpoints
            // 
            this.checkBox_paintmeshpoints.AutoSize = true;
            this.checkBox_paintmeshpoints.Location = new System.Drawing.Point(31, 52);
            this.checkBox_paintmeshpoints.Name = "checkBox_paintmeshpoints";
            this.checkBox_paintmeshpoints.Size = new System.Drawing.Size(143, 20);
            this.checkBox_paintmeshpoints.TabIndex = 5;
            this.checkBox_paintmeshpoints.Text = "Paint Mesh Points";
            this.checkBox_paintmeshpoints.UseVisualStyleBackColor = true;
            this.checkBox_paintmeshpoints.CheckedChanged += new System.EventHandler(this.checkBox_paintmeshpoints_CheckedChanged);
            // 
            // checkBox_paintconstraints
            // 
            this.checkBox_paintconstraints.AutoSize = true;
            this.checkBox_paintconstraints.Location = new System.Drawing.Point(31, 245);
            this.checkBox_paintconstraints.Name = "checkBox_paintconstraints";
            this.checkBox_paintconstraints.Size = new System.Drawing.Size(139, 20);
            this.checkBox_paintconstraints.TabIndex = 4;
            this.checkBox_paintconstraints.Text = "Paint Constraints";
            this.checkBox_paintconstraints.UseVisualStyleBackColor = true;
            this.checkBox_paintconstraints.CheckedChanged += new System.EventHandler(this.checkBox_paintconstraints_CheckedChanged);
            // 
            // checkBox_paintloads
            // 
            this.checkBox_paintloads.AutoSize = true;
            this.checkBox_paintloads.Location = new System.Drawing.Point(31, 205);
            this.checkBox_paintloads.Name = "checkBox_paintloads";
            this.checkBox_paintloads.Size = new System.Drawing.Size(102, 20);
            this.checkBox_paintloads.TabIndex = 3;
            this.checkBox_paintloads.Text = "Paint Loads";
            this.checkBox_paintloads.UseVisualStyleBackColor = true;
            this.checkBox_paintloads.CheckedChanged += new System.EventHandler(this.checkBox_paintloads_CheckedChanged);
            // 
            // checkBox_paintmeshboundaries
            // 
            this.checkBox_paintmeshboundaries.AutoSize = true;
            this.checkBox_paintmeshboundaries.Location = new System.Drawing.Point(31, 128);
            this.checkBox_paintmeshboundaries.Name = "checkBox_paintmeshboundaries";
            this.checkBox_paintmeshboundaries.Size = new System.Drawing.Size(174, 20);
            this.checkBox_paintmeshboundaries.TabIndex = 2;
            this.checkBox_paintmeshboundaries.Text = "Paint Mesh Boundaries";
            this.checkBox_paintmeshboundaries.UseVisualStyleBackColor = true;
            this.checkBox_paintmeshboundaries.CheckedChanged += new System.EventHandler(this.checkBox_paintmeshboundaries_CheckedChanged);
            // 
            // checkBox_paintmesh
            // 
            this.checkBox_paintmesh.AutoSize = true;
            this.checkBox_paintmesh.Location = new System.Drawing.Point(31, 91);
            this.checkBox_paintmesh.Name = "checkBox_paintmesh";
            this.checkBox_paintmesh.Size = new System.Drawing.Size(98, 20);
            this.checkBox_paintmesh.TabIndex = 1;
            this.checkBox_paintmesh.Text = "Paint Mesh";
            this.checkBox_paintmesh.UseVisualStyleBackColor = true;
            this.checkBox_paintmesh.CheckedChanged += new System.EventHandler(this.checkBox_paintmesh_CheckedChanged);
            // 
            // checkBox_paintshrinkmesh
            // 
            this.checkBox_paintshrinkmesh.AutoSize = true;
            this.checkBox_paintshrinkmesh.Location = new System.Drawing.Point(31, 165);
            this.checkBox_paintshrinkmesh.Name = "checkBox_paintshrinkmesh";
            this.checkBox_paintshrinkmesh.Size = new System.Drawing.Size(143, 20);
            this.checkBox_paintshrinkmesh.TabIndex = 0;
            this.checkBox_paintshrinkmesh.Text = "Paint Shrink Mesh";
            this.checkBox_paintshrinkmesh.UseVisualStyleBackColor = true;
            this.checkBox_paintshrinkmesh.CheckedChanged += new System.EventHandler(this.checkBox_paintshrinkmesh_CheckedChanged);
            // 
            // option_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 406);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "option_frm";
            this.Opacity = 0.85D;
            this.Text = "Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_paintconstraints;
        private System.Windows.Forms.CheckBox checkBox_paintloads;
        private System.Windows.Forms.CheckBox checkBox_paintmeshboundaries;
        private System.Windows.Forms.CheckBox checkBox_paintmesh;
        private System.Windows.Forms.CheckBox checkBox_paintshrinkmesh;
        private System.Windows.Forms.CheckBox checkBox_paintmeshpoints;
    }
}