using System.Diagnostics;

namespace Launcher
{

    enum GameExecutable
    {
        DirectX = 0,
        Vulkan,
        OpenGL
    }

    public partial class Form1 : Form
    {

        const string ROOT_PATH = "../../../../";

        public Form1()
        {
            InitializeComponent();

#if RELEASE

            executables.Add(GameExecutable.DirectX,ROOT_PATH + "Engine/bin/Release/DirectX/RetroEngine.Windows.exe");
            executables.Add(GameExecutable.Vulkan, ROOT_PATH + "Engine/bin/Release/OpenGL/RetroEngine.Desktop.exe");
            executables.Add(GameExecutable.OpenGL, ROOT_PATH + "Engine/bin/Release/WindowsVK/RetroEngine.WindowsVK.exe");
#else
            executables.Add(GameExecutable.DirectX,ROOT_PATH + "RetroEngine.Windows/bin/Release/net6.0-windows10.0.22621.0/RetroEngine.Windows.exe");
            executables.Add(GameExecutable.OpenGL, ROOT_PATH + "RetroEngine.Desktop/bin/Release/net6.0/RetroEngine.Desktop.exe");
            executables.Add(GameExecutable.Vulkan, ROOT_PATH + "RetroEngine.WindowsVK/bin/Release/net6.0-windows10.0.22621.0/RetroEngine.WindowsVK.exe");
#endif
        }

        GameExecutable executable;

        Dictionary<GameExecutable, string> executables = new Dictionary<GameExecutable, string>();

        private void button1_Click(object sender, EventArgs e)
        {
            Process process = Process.Start(executables[executable]);
            Thread.Sleep(1000);
            this.Close();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            executable = (GameExecutable)comboBox1.SelectedIndex;
            label1.Text = executables[executable].ToString();
        }
    }
}
