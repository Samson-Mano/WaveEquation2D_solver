using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveEquation2D_solver.other_windows
{
    public partial class about_frm : Form
    {

        private PictureBox pictureBox_icon;
        private Label label_version;
        private Label label_build;
        private LinkLabel linkLabel_email;
        private LinkLabel linkLabel_repo;
        private Button button_ok;


        public about_frm()
        {
            InitializeComponent();


            this.ClientSize = new Size(480, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;   // ignored when AutoSize = false
            this.Text = "About";

            BuildLayout();

            this.Load += about_frm_Load;
        }

        // ---------------------------------------------------------------
        // Build the entire form layout in code
        // ---------------------------------------------------------------
        private void BuildLayout()
        {
            int W = 480;
            int y = 20;

            // ----- Icon (left side) -----
            pictureBox_icon = new PictureBox
            {
                Size = new Size(96, 96),
                Location = new Point(30, y),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            this.Controls.Add(pictureBox_icon);

            // ----- Title (right side) -----
            int xRight = 150;
            int wRight = W - xRight - 30;

            var lblName = new Label
            {
                Text = "2D Wave Equation Solver",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(wRight, 30),
                Location = new Point(xRight, y)
            };
            this.Controls.Add(lblName);

            // ----- Version (right side, below title) -----
            label_version = new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(wRight, 20),
                Location = new Point(xRight, y + 32)
            };
            this.Controls.Add(label_version);

            // ----- Description (right side, below version) -----
            var lblDesc = new Label
            {
                Text = "Spectral element solver for the 2D Wave Equation\n" +
                       "and modal analysis problems.",
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft,
                Size = new Size(wRight, 46),
                Location = new Point(xRight, y + 56)
            };
            this.Controls.Add(lblDesc);

            // Move past the icon block
            y = 140;

            // ----- Separator -----
            var sep = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Width = W - 60,
                Location = new Point(30, y)
            };
            this.Controls.Add(sep);
            y += 18;

            // ----- Created by -----
            var lblBy = new Label
            {
                Text = "Created by",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 20),
                Location = new Point(30, y)
            };
            this.Controls.Add(lblBy);
            y += 22;

            var lblAuthor = new Label
            {
                Text = "Samson Mano",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 24),
                Location = new Point(30, y)
            };
            this.Controls.Add(lblAuthor);
            y += 30;

            // ----- Email link -----
            linkLabel_email = new LinkLabel
            {
                Text = "saminnx@gmail.com",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 20),
                Location = new Point(30, y)
            };
            linkLabel_email.LinkClicked += linkLabel_email_LinkClicked;
            this.Controls.Add(linkLabel_email);
            y += 24;

            // ----- Repository link -----
            linkLabel_repo = new LinkLabel
            {
                Text = "https://github.com/Samson-Mano/2DHelmholtz_solver",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 20),
                Location = new Point(30, y)
            };
            linkLabel_repo.LinkClicked += linkLabel_repo_LinkClicked;
            this.Controls.Add(linkLabel_repo);
            y += 28;

            // ----- Build date -----
            label_build = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 18),
                Location = new Point(30, y)
            };
            this.Controls.Add(label_build);
            y += 20;

            // ----- Copyright -----
            var lblCopy = new Label
            {
                Text = "© 2026 Nova Propulsion. All rights reserved.",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(W - 60, 18),
                Location = new Point(30, y)
            };
            this.Controls.Add(lblCopy);

            // ----- OK button (bottom center) -----
            button_ok = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(90, 30),
                Location = new Point((W - 90) / 2, this.ClientSize.Height - 44),
                Anchor = AnchorStyles.Bottom
            };
            button_ok.Click += (s, e) => this.Close();
            this.Controls.Add(button_ok);
        }

        // ---------------------------------------------------------------
        // Fill in dynamic info on load
        // ---------------------------------------------------------------
        private void about_frm_Load(object sender, EventArgs e)
        {
            // Version from the assembly
            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            if (ver != null)
                label_version.Text = $"Version {ver.Major}.{ver.Minor}.{ver.Build}";

            // Build date from the executable
            try
            {
                var exePath = Application.ExecutablePath;
                var buildTime = System.IO.File.GetLastWriteTime(exePath);
                label_build.Text = $"Build date: {buildTime:yyyy-MM-dd}";
            }
            catch
            {
                label_build.Text = string.Empty;
            }

            // Icon from the executable
            try
            {
                //var ico = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                //if (ico != null)
                //{
                //    this.Icon = ico;
                pictureBox_icon.Image = Resources.Resource_pics.Nova_Propulsion_logo_main2;
                // }
            }
            catch { /* ignore */ }
        }

        // ---------------------------------------------------------------
        // Click handlers
        // ---------------------------------------------------------------
        private void linkLabel_email_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mailto:saminnx@gmail.com",
                    UseShellExecute = true
                });
            }
            catch { /* ignore */ }
        }

        private void linkLabel_repo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Samson-Mano/2DHelmholtz_solver",
                    UseShellExecute = true
                });
            }
            catch { /* ignore */ }
        }




        // ---------------------------------------------------------------
    }
}
