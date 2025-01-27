namespace LoggerHost
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            darkListView1 = new DarkUI.Controls.DarkListView();
            darkDockPanel1 = new DarkUI.Docking.DarkDockPanel();
            darkCheckBox1 = new DarkUI.Controls.DarkCheckBox();
            darkSeparator1 = new DarkUI.Controls.DarkSeparator();
            SuspendLayout();
            // 
            // darkListView1
            // 
            darkListView1.Dock = DockStyle.Fill;
            darkListView1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
            darkListView1.Location = new Point(0, 0);
            darkListView1.Name = "darkListView1";
            darkListView1.Size = new Size(800, 424);
            darkListView1.TabIndex = 0;
            darkListView1.Text = "darkListView1";
            darkListView1.Click += darkListView1_Click;
            darkListView1.DoubleClick += darkListView1_DoubleClick;
            // 
            // darkDockPanel1
            // 
            darkDockPanel1.BackColor = Color.FromArgb(60, 63, 65);
            darkDockPanel1.Dock = DockStyle.Bottom;
            darkDockPanel1.Location = new Point(0, 424);
            darkDockPanel1.Name = "darkDockPanel1";
            darkDockPanel1.Size = new Size(800, 26);
            darkDockPanel1.TabIndex = 1;
            // 
            // darkCheckBox1
            // 
            darkCheckBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            darkCheckBox1.AutoSize = true;
            darkCheckBox1.CheckAlign = ContentAlignment.TopRight;
            darkCheckBox1.Location = new Point(715, 428);
            darkCheckBox1.Name = "darkCheckBox1";
            darkCheckBox1.Size = new Size(81, 19);
            darkCheckBox1.TabIndex = 2;
            darkCheckBox1.Text = "auto scroll";
            darkCheckBox1.CheckedChanged += darkCheckBox1_CheckedChanged_1;
            // 
            // darkSeparator1
            // 
            darkSeparator1.Dock = DockStyle.Top;
            darkSeparator1.Location = new Point(0, 0);
            darkSeparator1.Name = "darkSeparator1";
            darkSeparator1.Size = new Size(800, 2);
            darkSeparator1.TabIndex = 3;
            darkSeparator1.Text = "darkSeparator1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(darkSeparator1);
            Controls.Add(darkCheckBox1);
            Controls.Add(darkListView1);
            Controls.Add(darkDockPanel1);
            Name = "Form1";
            Text = "RemoteLogger";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DarkUI.Controls.DarkListView darkListView1;
        private DarkUI.Docking.DarkDockPanel darkDockPanel1;
        private DarkUI.Controls.DarkCheckBox darkCheckBox1;
        private DarkUI.Controls.DarkSeparator darkSeparator1;
    }
}
