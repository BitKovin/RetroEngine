using System.Diagnostics;

namespace Launcher
{


    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Program.StartSelectedGame();

            Thread.Sleep(1000);
            this.Close();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.executable = (GameExecutable)comboBox1.SelectedIndex;
            label1.Text = Program.executables[Program.executable].ToString();
        }
    }
}
