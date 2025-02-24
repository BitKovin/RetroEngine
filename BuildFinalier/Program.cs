﻿using System.Diagnostics;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {

        Start();

    }

    static void Start()
    {

        string solutionPath = "../";

        if(File.Exists(solutionPath + ".gitignore") == false)
        {
            Console.WriteLine("solution path not found");
            Console.WriteLine("switching to alternative solution path");
            solutionPath = "../../../../";
        }

        if(File.Exists(solutionPath + ".gitignore") == false)
        {
            Console.WriteLine("solution path not found");
        }

        string buildPath = solutionPath + "Build/";


        if(Path.Exists(buildPath) == false)
        {
            Console.WriteLine("Invalid Path");
            Start();
            return;
        }

        string winEXE = buildPath + "/Engine/bin/Release/DirectX/RetroEngine.Windows.exe";
        string glEXE = buildPath + "/Engine/bin/Release/OpenGL/RetroEngine.Desktop.exe";
        string vulkanEXE = buildPath + "/Engine/bin/Release/Vulkan/RetroEngine.WindowsVK.exe";

        PublishPlatforms(solutionPath, buildPath);

        CopyGameData(solutionPath, buildPath);

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
        Console.WriteLine("patching "+ Path.GetFileName(path));
        string command = $"nvpatch --enable \"{path}\"";
        ExecutePowerShellCommand(command);
        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine();
        Console.WriteLine();
    }

    static void PublishPlatforms(string solutionPath, string buildPath)
    {

        PublishProject(solutionPath + "RetroEngine.Windows/RetroEngine.Windows.csproj", buildPath + "Engine/bin/Release/DirectX");
        //PublishProject(solutionPath + "RetroEngine.Desktop/RetroEngine.Desktop.csproj", buildPath + "Engine/bin/Release/OpenGL");
        //PublishProject(solutionPath + "RetroEngine.WindowsVK/RetroEngine.WindowsVK.csproj", buildPath + "Engine/bin/Release/Vulkan");
    }

    static void CopyGameData(string solutionPath, string buildPath)
    {

        if (Directory.Exists(buildPath + "GameData"))
            Directory.Delete(buildPath + "GameData",true);

        Directory.CreateDirectory(buildPath + "GameData");

        CopyFilesRecursively(solutionPath + "GameData", buildPath + "GameData");
    }

    static void PublishProject(string projectPath, string outPath)
    {

        if(Directory.Exists(outPath))
            Directory.Delete(outPath,true);

        projectPath = Path.GetFullPath(projectPath);
        outPath = Path.GetFullPath(outPath);

        Directory.CreateDirectory(outPath);

        ExecutePowerShellCommand($"dotnet restore {projectPath}");

        ExecutePowerShellCommand($"dotnet publish {projectPath}" +
            " -c Release" +
            $" -o {outPath} " +
            "--no-self-contained " +
            "--no-restore");

        var runtimes = Directory.GetDirectories(outPath + "\\runtimes\\");

        foreach(var runtime in runtimes)
        {
            if(runtime.EndsWith("win-x64") == false)
            {
                Directory.Delete(runtime,true);
            }    
        }

        File.Delete(outPath + "\\runtimes\\win-x64\\native\\" + "ffmpeg.exe");
        File.Delete(outPath + "\\runtimes\\win-x64\\native\\" + "ffprobe.exe");

    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {

        List<string> ignoreDirs = new List<string>();

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {

            string dir = Path.GetFullPath(Path.GetDirectoryName(newPath));



            if (File.Exists(dir + "\\.dev"))
            {
                ignoreDirs.Add(dir);
            }
        }


        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {

            bool copy = true;

            foreach (var dir in ignoreDirs)
            {

                string path = Path.GetFullPath(dirPath);

                if (path.Contains(dir))
                {

                    copy = false;

                    continue;
                }
            }

            if (copy)
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {

            bool copy = true;

            foreach(var dir in ignoreDirs)
            {

                string path = Path.GetFullPath(newPath);

                if (path.Contains(dir))
                {

                    copy = false;

                    continue;
                }
            }

            if(copy)
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
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