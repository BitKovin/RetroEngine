using System.Diagnostics;

namespace Launcher
{
    internal static class Program
    {



        public static GameExecutable executable;

        public static Dictionary<GameExecutable, string> executables = new Dictionary<GameExecutable, string>();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            InitGames();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-game" && i+1< args.Length)
                {
                    switch(args[i+1].ToLower())
                    {
                        case "directx":

                            executable = GameExecutable.DirectX;

                            StartSelectedGame();
                            return;

                        case "opengl":
                            executable = GameExecutable.OpenGL;

                            StartSelectedGame();
                            return;

                        case "vulkan":
                            executable = GameExecutable.Vulkan;

                            StartSelectedGame();
                            return;
                    }
                }
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        public static void StartSelectedGame()
        {
            string executablePath = Program.executables[Program.executable];
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executablePath;
            startInfo.WorkingDirectory = Path.GetDirectoryName(executablePath); // Set working directory to the game's directory
            Process.Start(startInfo);
        }

        static void InitGames()
        {


#if RELEASE

            executables.Add(GameExecutable.DirectX, "Engine/bin/Release/DirectX/RetroEngine.Windows.exe");
            executables.Add(GameExecutable.OpenGL, "Engine/bin/Release/OpenGL/RetroEngine.Desktop.exe");
            executables.Add(GameExecutable.Vulkan, "Engine/bin/Release/Vulkan/RetroEngine.WindowsVK.exe");
#else
            executables.Add(GameExecutable.DirectX,ROOT_PATH + "RetroEngine.Windows/bin/Release/net7.0-windows/RetroEngine.Windows.exe");
            executables.Add(GameExecutable.OpenGL, ROOT_PATH + "RetroEngine.Desktop/bin/Release/net6.0/RetroEngine.Desktop.exe");
            executables.Add(GameExecutable.Vulkan, ROOT_PATH + "RetroEngine.WindowsVK/bin/Release/net7.0-windows/RetroEngine.WindowsVK.exe");
#endif

        }

    }




    enum GameExecutable
    {
        DirectX = 0,
        Vulkan,
        OpenGL
    }

}