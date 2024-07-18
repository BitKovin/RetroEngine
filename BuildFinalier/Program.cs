using System.Diagnostics;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {

        Start();

    }

    static void Start()
    {
        Console.WriteLine("Enter Build Path");

        string path = Console.ReadLine();


        if(Path.Exists(path) == false)
        {
            Console.WriteLine("Invalid Path");
            Start();
            return;
        }

        string winEXE = path + "/Engine/bin/Release/DirectX/RetroEngine.Windows.exe";
        string glEXE = path + "/Engine/bin/Release/OpenGL/RetroEngine.Desktop.exe";
        string vulkanEXE = path + "/Engine/bin/Release/Vulkan/RetroEngine.WindowsVK.exe";

        LoadPacks();

        PatchEXE(winEXE);
        PatchEXE(glEXE);
        PatchEXE(vulkanEXE);

        Console.Write("Finished");
        Console.ReadLine();

    }



    static void LoadPacks()
    {
        string command = $"dotnet tool install -g Topten.nvpatch";
        ExecutePowerShellCommand(command);
    }

    static void PatchEXE(string path)
    {
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine(Path.GetFileName(path));
        string command = $"nvpatch --enable \"{path}\"";
        ExecutePowerShellCommand(command);
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine();
        Console.WriteLine();
    }

    static void ExecutePowerShellCommand(string command)
    {
        try
        {
            // Start PowerShell process
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(psi))
            {
                if (p != null)
                {
                    // Read output and errors
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();

                    // Output to console
                    Console.WriteLine("Output:");
                    Console.WriteLine(output);
                    Console.WriteLine("Errors:");
                    Console.WriteLine(error);
                }
                else
                {
                    Console.WriteLine("Failed to start PowerShell process.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing PowerShell command: {ex.Message}");
        }
    }



}