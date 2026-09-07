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
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_polynomialrefinement = new System.Windows.Forms.ComboBox();
            this.button_solve = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_formulation = new System.Windows.Forms.ComboBox();
            this.checkBox_extendconstraints = new System.Windows.Forms.CheckBox();
            this.checkBox_extendloads = new System.Windows.Forms.CheckBox();
            this.checkBox_saveHrefinedmodel = new System.Windows.Forms.CheckBox();
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
            this.label9.Location = new System.Drawing.Point(65, 288);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(101, 17);
            this.label9.TabIndex = 17;
            this.label9.Text = "Solver Type: ";
            // 
            // comboBox_solvertype
            // 
            this.comboBox_solvertype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_solvertype.FormattingEnabled = true;
            this.comboBox_solvertype.Items.AddRange(new object[] {
            "Elimination method",
            "Lagrange Augmentation method"});
            this.comboBox_solvertype.Location = new System.Drawing.Point(172, 285);
            this.comboBox_solvertype.Name = "comboBox_solvertype";
            this.comboBox_solvertype.Size = new System.Drawing.Size(322, 25);
            this.comboBox_solvertype.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 319);
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
            this.comboBox_HRefinement.Location = new System.Drawing.Point(172, 316);
            this.comboBox_HRefinement.Name = "comboBox_HRefinement";
            this.comboBox_HRefinement.Size = new System.Drawing.Size(273, 25);
            this.comboBox_HRefinement.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 350);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 17);
            this.label2.TabIndex = 20;
            this.label2.Text = "P - Refinement: ";
            // 
            // comboBox_polynomialrefinement
            // 
            this.comboBox_polynomialrefinement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_polynomialrefinement.FormattingEnabled = true;
            this.comboBox_polynomialrefinement.Items.AddRange(new object[] {
            "P = 1 (Linear T3, Bilinear Q4)",
            "P = 2 (Quadratic T6, Q9)",
            "P = 3 (Cubic T10, Q16)",
            "P = 4 (Quartic T15, Q25)"});
            this.comboBox_polynomialrefinement.Location = new System.Drawing.Point(172, 347);
            this.comboBox_polynomialrefinement.Name = "comboBox_polynomialrefinement";
            this.comboBox_polynomialrefinement.Size = new System.Drawing.Size(273, 25);
            this.comboBox_polynomialrefinement.TabIndex = 21;
            // 
            // button_solve
            // 
            this.button_solve.Location = new System.Drawing.Point(221, 504);
            this.button_solve.Name = "button_solve";
            this.button_solve.Size = new System.Drawing.Size(110, 55);
            this.button_solve.TabIndex = 22;
            this.button_solve.Text = "Solve";
            this.button_solve.UseVisualStyleBackColor = true;
            this.button_solve.Click += new System.EventHandler(this.button_solve_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 381);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(151, 17);
            this.label3.TabIndex = 23;
            this.label3.Text = "Solver Formulation: ";
            // 
            // comboBox_formulation
            // 
            this.comboBox_formulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_formulation.FormattingEnabled = true;
            this.comboBox_formulation.Items.AddRange(new object[] {
            "Plane Stress Formulation",
            "Plane Strain Formulation"});
            this.comboBox_formulation.Location = new System.Drawing.Point(172, 378);
            this.comboBox_formulation.Name = "comboBox_formulation";
            this.comboBox_formulation.Size = new System.Drawing.Size(273, 25);
            this.comboBox_formulation.TabIndex = 24;
            // 
            // checkBox_extendconstraints
            // 
            this.checkBox_extendconstraints.AutoSize = true;
            this.checkBox_extendconstraints.Location = new System.Drawing.Point(172, 409);
            this.checkBox_extendconstraints.Name = "checkBox_extendconstraints";
            this.checkBox_extendconstraints.Size = new System.Drawing.Size(318, 21);
            this.checkBox_extendconstraints.TabIndex = 25;
            this.checkBox_extendconstraints.Text = "Extend Constraints to Refined Mid Nodes";
            this.checkBox_extendconstraints.UseVisualStyleBackColor = true;
            // 
            // checkBox_extendloads
            // 
            this.checkBox_extendloads.AutoSize = true;
            this.checkBox_extendloads.Location = new System.Drawing.Point(172, 436);
            this.checkBox_extendloads.Name = "checkBox_extendloads";
            this.checkBox_extendloads.Size = new System.Drawing.Size(269, 21);
            this.checkBox_extendloads.TabIndex = 26;
            this.checkBox_extendloads.Text = "Exten Loads to Refined Mid Nodes";
            this.checkBox_extendloads.UseVisualStyleBackColor = true;
            // 
            // checkBox_saveHrefinedmodel
            // 
            this.checkBox_saveHrefinedmodel.AutoSize = true;
            this.checkBox_saveHrefinedmodel.Location = new System.Drawing.Point(172, 463);
            this.checkBox_saveHrefinedmodel.Name = "checkBox_saveHrefinedmodel";
            this.checkBox_saveHrefinedmodel.Size = new System.Drawing.Size(179, 21);
            this.checkBox_saveHrefinedmodel.TabIndex = 27;
            this.checkBox_saveHrefinedmodel.Text = "Save h-Refined Model";
            this.checkBox_saveHrefinedmodel.UseVisualStyleBackColor = true;
            // 
            // solver_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 571);
            this.Controls.Add(this.checkBox_saveHrefinedmodel);
            this.Controls.Add(this.checkBox_extendloads);
            this.Controls.Add(this.checkBox_extendconstraints);
            this.Controls.Add(this.comboBox_formulation);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_solve);
            this.Controls.Add(this.comboBox_polynomialrefinement);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_HRefinement);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.comboBox_solvertype);
            this.Controls.Add(this.richTextBox_AnalysisUpdate);
            this.Font = new System.Drawing.Font("Verdana", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(800, 610);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_polynomialrefinement;
        private System.Windows.Forms.Button button_solve;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_formulation;
        private System.Windows.Forms.CheckBox checkBox_extendconstraints;
        private System.Windows.Forms.CheckBox checkBox_extendloads;
        private System.Windows.Forms.CheckBox checkBox_saveHrefinedmodel;
    }
}