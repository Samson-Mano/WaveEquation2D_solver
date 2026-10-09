namespace WaveEquation2D_solver
{
    partial class main_frm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main_frm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTXTFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.constraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNodalConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addEdgeConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediumPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dWaveEquationSolveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.resultOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generalInstructionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_zoom_value = new System.Windows.Forms.ToolStripStatusLabel();
            this.glControl_main_panel = new OpenTK.GLControl();
            this.resultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fieldValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.firstDerivativeFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.secondDerivativeFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.hideResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.constraintsToolStripMenuItem,
            this.solverToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(803, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importTXTFileToolStripMenuItem,
            this.importModelToolStripMenuItem,
            this.exportModelToolStripMenuItem,
            this.optionToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importTXTFileToolStripMenuItem
            // 
            this.importTXTFileToolStripMenuItem.Name = "importTXTFileToolStripMenuItem";
            this.importTXTFileToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.importTXTFileToolStripMenuItem.Text = "Import TXT File";
            this.importTXTFileToolStripMenuItem.Click += new System.EventHandler(this.importTXTFileToolStripMenuItem_Click);
            // 
            // importModelToolStripMenuItem
            // 
            this.importModelToolStripMenuItem.Name = "importModelToolStripMenuItem";
            this.importModelToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.importModelToolStripMenuItem.Text = "Import Model";
            this.importModelToolStripMenuItem.Click += new System.EventHandler(this.importModelToolStripMenuItem_Click);
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.exportModelToolStripMenuItem_Click);
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.optionToolStripMenuItem.Text = "Option";
            this.optionToolStripMenuItem.Click += new System.EventHandler(this.optionToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(193, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // constraintsToolStripMenuItem
            // 
            this.constraintsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNodalConstraintsToolStripMenuItem,
            this.addEdgeConstraintsToolStripMenuItem,
            this.mediumPropertiesToolStripMenuItem});
            this.constraintsToolStripMenuItem.Name = "constraintsToolStripMenuItem";
            this.constraintsToolStripMenuItem.Size = new System.Drawing.Size(96, 24);
            this.constraintsToolStripMenuItem.Text = "Constraints";
            // 
            // addNodalConstraintsToolStripMenuItem
            // 
            this.addNodalConstraintsToolStripMenuItem.Name = "addNodalConstraintsToolStripMenuItem";
            this.addNodalConstraintsToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.addNodalConstraintsToolStripMenuItem.Text = "Add Nodal Constraints";
            this.addNodalConstraintsToolStripMenuItem.Click += new System.EventHandler(this.addNodalConstraintsToolStripMenuItem_Click);
            // 
            // addEdgeConstraintsToolStripMenuItem
            // 
            this.addEdgeConstraintsToolStripMenuItem.Name = "addEdgeConstraintsToolStripMenuItem";
            this.addEdgeConstraintsToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.addEdgeConstraintsToolStripMenuItem.Text = "Add Edge Constraints";
            this.addEdgeConstraintsToolStripMenuItem.Click += new System.EventHandler(this.addEdgeConstraintsToolStripMenuItem_Click);
            // 
            // mediumPropertiesToolStripMenuItem
            // 
            this.mediumPropertiesToolStripMenuItem.Name = "mediumPropertiesToolStripMenuItem";
            this.mediumPropertiesToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.mediumPropertiesToolStripMenuItem.Text = "Medium Properties";
            this.mediumPropertiesToolStripMenuItem.Click += new System.EventHandler(this.mediumPropertiesToolStripMenuItem_Click);
            // 
            // solverToolStripMenuItem
            // 
            this.solverToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dWaveEquationSolveToolStripMenuItem,
            this.toolStripSeparator2,
            this.resultsToolStripMenuItem,
            this.resultOptionsToolStripMenuItem});
            this.solverToolStripMenuItem.Name = "solverToolStripMenuItem";
            this.solverToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.solverToolStripMenuItem.Text = "Solver";
            // 
            // dWaveEquationSolveToolStripMenuItem
            // 
            this.dWaveEquationSolveToolStripMenuItem.Name = "dWaveEquationSolveToolStripMenuItem";
            this.dWaveEquationSolveToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.dWaveEquationSolveToolStripMenuItem.Text = "2D Wave Equation Solve";
            this.dWaveEquationSolveToolStripMenuItem.Click += new System.EventHandler(this.dWaveEquationSolveToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(251, 6);
            // 
            // resultOptionsToolStripMenuItem
            // 
            this.resultOptionsToolStripMenuItem.Name = "resultOptionsToolStripMenuItem";
            this.resultOptionsToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.resultOptionsToolStripMenuItem.Text = "Result Option";
            this.resultOptionsToolStripMenuItem.Click += new System.EventHandler(this.resultOptionsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generalInstructionToolStripMenuItem,
            this.toolStripSeparator1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // generalInstructionToolStripMenuItem
            // 
            this.generalInstructionToolStripMenuItem.Name = "generalInstructionToolStripMenuItem";
            this.generalInstructionToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.generalInstructionToolStripMenuItem.Text = "General Instruction";
            this.generalInstructionToolStripMenuItem.Click += new System.EventHandler(this.generalInstructionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(213, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(216, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_zoom_value});
            this.statusStrip1.Location = new System.Drawing.Point(0, 475);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.Size = new System.Drawing.Size(803, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_zoom_value
            // 
            this.toolStripStatusLabel_zoom_value.Name = "toolStripStatusLabel_zoom_value";
            this.toolStripStatusLabel_zoom_value.Size = new System.Drawing.Size(92, 20);
            this.toolStripStatusLabel_zoom_value.Text = "Zoom: 100%";
            // 
            // glControl_main_panel
            // 
            this.glControl_main_panel.BackColor = System.Drawing.Color.Black;
            this.glControl_main_panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.glControl_main_panel.Location = new System.Drawing.Point(100, 97);
            this.glControl_main_panel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.glControl_main_panel.Name = "glControl_main_panel";
            this.glControl_main_panel.Size = new System.Drawing.Size(279, 171);
            this.glControl_main_panel.TabIndex = 2;
            this.glControl_main_panel.VSync = false;
            this.glControl_main_panel.Load += new System.EventHandler(this.glControl_main_panel_Load);
            this.glControl_main_panel.SizeChanged += new System.EventHandler(this.glControl_main_panel_SizeChanged);
            this.glControl_main_panel.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_main_panel_Paint);
            this.glControl_main_panel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_main_panel_KeyDown);
            this.glControl_main_panel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_main_panel_KeyUp);
            this.glControl_main_panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseDown);
            this.glControl_main_panel.MouseEnter += new System.EventHandler(this.glControl_main_panel_MouseEnter);
            this.glControl_main_panel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseMove);
            this.glControl_main_panel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseUp);
            this.glControl_main_panel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl_main_panel_MouseWheel);
            // 
            // resultsToolStripMenuItem
            // 
            this.resultsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fieldValuesToolStripMenuItem,
            this.firstDerivativeFieldToolStripMenuItem,
            this.secondDerivativeFieldToolStripMenuItem,
            this.toolStripSeparator3,
            this.hideResultsToolStripMenuItem});
            this.resultsToolStripMenuItem.Name = "resultsToolStripMenuItem";
            this.resultsToolStripMenuItem.Size = new System.Drawing.Size(254, 26);
            this.resultsToolStripMenuItem.Text = "Results";
            // 
            // fieldValuesToolStripMenuItem
            // 
            this.fieldValuesToolStripMenuItem.Name = "fieldValuesToolStripMenuItem";
            this.fieldValuesToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.fieldValuesToolStripMenuItem.Text = "Field values";
            this.fieldValuesToolStripMenuItem.Click += new System.EventHandler(this.fieldValuesToolStripMenuItem_Click);
            // 
            // firstDerivativeFieldToolStripMenuItem
            // 
            this.firstDerivativeFieldToolStripMenuItem.Name = "firstDerivativeFieldToolStripMenuItem";
            this.firstDerivativeFieldToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.firstDerivativeFieldToolStripMenuItem.Text = "First Derivative Field";
            this.firstDerivativeFieldToolStripMenuItem.Click += new System.EventHandler(this.firstDerivativeFieldToolStripMenuItem_Click);
            // 
            // secondDerivativeFieldToolStripMenuItem
            // 
            this.secondDerivativeFieldToolStripMenuItem.Name = "secondDerivativeFieldToolStripMenuItem";
            this.secondDerivativeFieldToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.secondDerivativeFieldToolStripMenuItem.Text = "Second Derivative Field";
            this.secondDerivativeFieldToolStripMenuItem.Click += new System.EventHandler(this.secondDerivativeFieldToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(245, 6);
            // 
            // hideResultsToolStripMenuItem
            // 
            this.hideResultsToolStripMenuItem.Name = "hideResultsToolStripMenuItem";
            this.hideResultsToolStripMenuItem.Size = new System.Drawing.Size(248, 26);
            this.hideResultsToolStripMenuItem.Text = "Hide Results";
            this.hideResultsToolStripMenuItem.Click += new System.EventHandler(this.hideResultsToolStripMenuItem_Click);
            // 
            // main_frm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 501);
            this.Controls.Add(this.glControl_main_panel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "main_frm";
            this.Text = "2D Wave Equation Solver";
            this.Load += new System.EventHandler(this.main_frm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem constraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem solverToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_zoom_value;
        private System.Windows.Forms.ToolStripMenuItem importTXTFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNodalConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addEdgeConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mediumPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dWaveEquationSolveToolStripMenuItem;
        private OpenTK.GLControl glControl_main_panel;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalInstructionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem resultOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fieldValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem firstDerivativeFieldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem secondDerivativeFieldToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem hideResultsToolStripMenuItem;
    }
}

