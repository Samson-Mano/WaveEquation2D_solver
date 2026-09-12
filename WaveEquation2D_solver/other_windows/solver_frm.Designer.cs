namespace WaveEquation2D_solver.other_windows
{
    partial class solver_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(solver_frm));
            this.richTextBox_AnalysisUpdate = new System.Windows.Forms.RichTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_solvertype = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_HRefinement = new System.Windows.Forms.ComboBox();
            this.button_solve = new System.Windows.Forms.Button();
            this.checkBox_extendconstraints = new System.Windows.Forms.CheckBox();
            this.checkBox_saveHrefinedmodel = new System.Windows.Forms.CheckBox();
            this.comboBox_spectralorderN = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_xyextent = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_totalsimulationtime = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_timeinterval = new System.Windows.Forms.TextBox();
            this.textBox_numberofmodes = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox_loadmodalanalysis = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // richTextBox_AnalysisUpdate
            // 
            this.richTextBox_AnalysisUpdate.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_AnalysisUpdate.Location = new System.Drawing.Point(12, 12);
            this.richTextBox_AnalysisUpdate.Name = "richTextBox_AnalysisUpdate";
            this.richTextBox_AnalysisUpdate.Size = new System.Drawing.Size(760, 267);
            this.richTextBox_AnalysisUpdate.TabIndex = 1;
            this.richTextBox_AnalysisUpdate.Text = "";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(234, 297);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 17);
            this.label9.TabIndex = 17;
            this.label9.Text = "Solver type: ";
            // 
            // comboBox_solvertype
            // 
            this.comboBox_solvertype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_solvertype.FormattingEnabled = true;
            this.comboBox_solvertype.Items.AddRange(new object[] {
            "Elimination method",
            "Lagrange Augmentation method"});
            this.comboBox_solvertype.Location = new System.Drawing.Point(341, 294);
            this.comboBox_solvertype.Name = "comboBox_solvertype";
            this.comboBox_solvertype.Size = new System.Drawing.Size(322, 25);
            this.comboBox_solvertype.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(210, 328);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 17);
            this.label1.TabIndex = 18;
            this.label1.Text = "H - Refinement: ";
            // 
            // comboBox_HRefinement
            // 
            this.comboBox_HRefinement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_HRefinement.FormattingEnabled = true;
            this.comboBox_HRefinement.Items.AddRange(new object[] {
            "1 (Orignal mesh)",
            "4 (Split each element into 4)",
            "16 (Split each element into 16)"});
            this.comboBox_HRefinement.Location = new System.Drawing.Point(341, 325);
            this.comboBox_HRefinement.Name = "comboBox_HRefinement";
            this.comboBox_HRefinement.Size = new System.Drawing.Size(273, 25);
            this.comboBox_HRefinement.TabIndex = 19;
            // 
            // button_solve
            // 
            this.button_solve.Location = new System.Drawing.Point(571, 489);
            this.button_solve.Name = "button_solve";
            this.button_solve.Size = new System.Drawing.Size(161, 71);
            this.button_solve.TabIndex = 22;
            this.button_solve.Text = "Solve";
            this.button_solve.UseVisualStyleBackColor = true;
            this.button_solve.Click += new System.EventHandler(this.button_solve_Click);
            // 
            // checkBox_extendconstraints
            // 
            this.checkBox_extendconstraints.AutoSize = true;
            this.checkBox_extendconstraints.Location = new System.Drawing.Point(194, 537);
            this.checkBox_extendconstraints.Name = "checkBox_extendconstraints";
            this.checkBox_extendconstraints.Size = new System.Drawing.Size(318, 21);
            this.checkBox_extendconstraints.TabIndex = 25;
            this.checkBox_extendconstraints.Text = "Extend Constraints to Refined Mid Nodes";
            this.checkBox_extendconstraints.UseVisualStyleBackColor = true;
            // 
            // checkBox_saveHrefinedmodel
            // 
            this.checkBox_saveHrefinedmodel.AutoSize = true;
            this.checkBox_saveHrefinedmodel.Location = new System.Drawing.Point(195, 591);
            this.checkBox_saveHrefinedmodel.Name = "checkBox_saveHrefinedmodel";
            this.checkBox_saveHrefinedmodel.Size = new System.Drawing.Size(179, 21);
            this.checkBox_saveHrefinedmodel.TabIndex = 27;
            this.checkBox_saveHrefinedmodel.Text = "Save h-Refined Model";
            this.checkBox_saveHrefinedmodel.UseVisualStyleBackColor = true;
            // 
            // comboBox_spectralorderN
            // 
            this.comboBox_spectralorderN.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_spectralorderN.FormattingEnabled = true;
            this.comboBox_spectralorderN.Items.AddRange(new object[] {
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comboBox_spectralorderN.Location = new System.Drawing.Point(341, 356);
            this.comboBox_spectralorderN.Name = "comboBox_spectralorderN";
            this.comboBox_spectralorderN.Size = new System.Drawing.Size(92, 25);
            this.comboBox_spectralorderN.TabIndex = 29;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(189, 359);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(146, 17);
            this.label10.TabIndex = 28;
            this.label10.Text = "Spectral order (N): ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(447, 501);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 17);
            this.label8.TabIndex = 35;
            this.label8.Text = "units";
            // 
            // textBox_xyextent
            // 
            this.textBox_xyextent.Enabled = false;
            this.textBox_xyextent.Location = new System.Drawing.Point(341, 498);
            this.textBox_xyextent.Name = "textBox_xyextent";
            this.textBox_xyextent.Size = new System.Drawing.Size(100, 24);
            this.textBox_xyextent.TabIndex = 33;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(191, 501);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(144, 17);
            this.label4.TabIndex = 32;
            this.label4.Text = "Model X, Y extent: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(172, 390);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 17);
            this.label3.TabIndex = 36;
            this.label3.Text = "Total simulation time: ";
            // 
            // textBox_totalsimulationtime
            // 
            this.textBox_totalsimulationtime.Location = new System.Drawing.Point(341, 387);
            this.textBox_totalsimulationtime.Name = "textBox_totalsimulationtime";
            this.textBox_totalsimulationtime.Size = new System.Drawing.Size(117, 24);
            this.textBox_totalsimulationtime.TabIndex = 37;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(227, 420);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 17);
            this.label5.TabIndex = 38;
            this.label5.Text = "Time interval: ";
            // 
            // textBox_timeinterval
            // 
            this.textBox_timeinterval.Location = new System.Drawing.Point(341, 417);
            this.textBox_timeinterval.Name = "textBox_timeinterval";
            this.textBox_timeinterval.Size = new System.Drawing.Size(117, 24);
            this.textBox_timeinterval.TabIndex = 39;
            // 
            // textBox_numberofmodes
            // 
            this.textBox_numberofmodes.Location = new System.Drawing.Point(341, 447);
            this.textBox_numberofmodes.Name = "textBox_numberofmodes";
            this.textBox_numberofmodes.Size = new System.Drawing.Size(117, 24);
            this.textBox_numberofmodes.TabIndex = 41;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(192, 450);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(143, 17);
            this.label6.TabIndex = 40;
            this.label6.Text = "Number of Modes: ";
            // 
            // checkBox_loadmodalanalysis
            // 
            this.checkBox_loadmodalanalysis.AutoSize = true;
            this.checkBox_loadmodalanalysis.Location = new System.Drawing.Point(195, 564);
            this.checkBox_loadmodalanalysis.Name = "checkBox_loadmodalanalysis";
            this.checkBox_loadmodalanalysis.Size = new System.Drawing.Size(295, 21);
            this.checkBox_loadmodalanalysis.TabIndex = 42;
            this.checkBox_loadmodalanalysis.Text = "Import existing modal analysis results";
            this.checkBox_loadmodalanalysis.UseVisualStyleBackColor = true;
            // 
            // solver_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 631);
            this.Controls.Add(this.checkBox_loadmodalanalysis);
            this.Controls.Add(this.textBox_numberofmodes);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_timeinterval);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_totalsimulationtime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox_xyextent);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_spectralorderN);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.checkBox_saveHrefinedmodel);
            this.Controls.Add(this.checkBox_extendconstraints);
            this.Controls.Add(this.button_solve);
            this.Controls.Add(this.comboBox_HRefinement);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.comboBox_solvertype);
            this.Controls.Add(this.richTextBox_AnalysisUpdate);
            this.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximumSize = new System.Drawing.Size(810, 680);
            this.MinimumSize = new System.Drawing.Size(800, 670);
            this.Name = "solver_frm";
            this.Opacity = 0.85D;
            this.Text = "Finite Element Solver";
            this.Load += new System.EventHandler(this.solver_frm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_AnalysisUpdate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBox_solvertype;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_HRefinement;
        private System.Windows.Forms.Button button_solve;
        private System.Windows.Forms.CheckBox checkBox_extendconstraints;
        private System.Windows.Forms.CheckBox checkBox_saveHrefinedmodel;
        private System.Windows.Forms.ComboBox comboBox_spectralorderN;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_xyextent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_totalsimulationtime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_timeinterval;
        private System.Windows.Forms.TextBox textBox_numberofmodes;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox_loadmodalanalysis;
    }
}