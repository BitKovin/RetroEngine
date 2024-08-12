using System;
using System.IO;
using System.Text;
using Assassin;

namespace Test;

internal static class Program
{

    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Test.exe <ASS path> <time in seconds> [<output PPM path>]");

            return;
        }

        var inputAssPath = args[0];
        var assTime = double.Parse(args[1]);

        string outputPpmPath;

        if (args.Length < 3)
        {
            var fileInfo = new FileInfo(inputAssPath);
            string baseName;

            if (string.IsNullOrEmpty(fileInfo.Extension))
            {
                baseName = fileInfo.Name;
            }
            else
            {
                baseName = fileInfo.Name[..^fileInfo.Extension.Length];
            }

            var inputAssDir = fileInfo.DirectoryName!;
            outputPpmPath = Path.Combine(inputAssDir, baseName + ".ppm");
        }
        else
        {
            outputPpmPath = args[2];
        }

        using var lib = AssLibrary.Create();
        using var renderer = lib.CreateRenderer();

        renderer.SetFonts("Arial");
        renderer.SetFrameSize(1280, 720);

        var text = File.ReadAllText(inputAssPath);
        var source = new StringAssSource(text);

        using var track = lib.CreateTrack(source);

        var ts = (long)TimeSpan.FromSeconds(assTime).TotalMilliseconds;
        var image = renderer.RenderFrame(track, ts);

        var buf = image.Blend();

        SaveImageAsPpm(buf, outputPpmPath);
    }

    private static void SaveImageAsPpm(RgbaImage img, string path)
    {
        var utf8 = new UTF8Encoding(false);

        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Write);
        using var writer = new StreamWriter(fs, utf8);

        writer.NewLine = "\n";

        var w = img.Width;
        var h = img.Height;

        writer.WriteLine("P3");
        writer.WriteLine($"{w} {h}");
        writer.WriteLine("255");

        for (var y = 0; y < h; y += 1)
        {
            for (var x = 0; x < w; x += 1)
            {
                var pixel = img.ReadPixel(x, y);

                var a = pixel.A / 255.0f;
                var r = (int)(pixel.R * a);
                var g = (int)(pixel.G * a);
                var b = (int)(pixel.B * a);

                writer.Write($"{r} {g} {b}");

                writer.Write("\t");
            }

            writer.WriteLine();
        }
    }

}
