using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace PngBrightnessAdjuster
{
    class Program
    {
        static void Main(string[] args)
        {
            // Prompt for folder path.
            Console.Write("Enter folder path (leave blank for current directory): ");
            string folderPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                folderPath = Directory.GetCurrentDirectory();
            }
            else if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("The specified folder does not exist.");
                return;
            }

            // Prompt for brightness multiplier.
            Console.Write("Enter brightness multiplier (e.g., 1.2 for a 20% increase): ");
            string inputMultiplier = Console.ReadLine();
            double brightnessMultiplier;
            if (!double.TryParse(inputMultiplier,CultureInfo.InvariantCulture, out brightnessMultiplier))
            {
                Console.WriteLine("Invalid brightness multiplier. Using default value 1.0 (no change).");
                brightnessMultiplier = 1.0;
            }

            // Get all PNG files.
            string[] pngFiles = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);
            Console.WriteLine($"Found {pngFiles.Length} PNG file(s) in {folderPath}.");

            foreach (string file in pngFiles)
            {
                try
                {
                    Console.WriteLine($"Processing: {file}");

                    // Load the image from a FileStream to release the file after loading.
                    Bitmap originalBitmap;
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        originalBitmap = new Bitmap(fs);
                    }

                    // Convert to 32bppArgb if necessary.
                    Bitmap bitmapToProcess;
                    if ((originalBitmap.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed ||
                        originalBitmap.PixelFormat != PixelFormat.Format32bppArgb)
                    {
                        bitmapToProcess = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(bitmapToProcess))
                        {
                            g.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height));
                        }
                    }
                    else
                    {
                        bitmapToProcess = (Bitmap)originalBitmap.Clone();
                    }
                    originalBitmap.Dispose();

                    // Lock bitmap bits for faster processing.
                    Rectangle rect = new Rectangle(0, 0, bitmapToProcess.Width, bitmapToProcess.Height);
                    BitmapData data = bitmapToProcess.LockBits(rect, ImageLockMode.ReadWrite, bitmapToProcess.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(bitmapToProcess.PixelFormat) / 8;
                    int byteCount = data.Stride * bitmapToProcess.Height;
                    byte[] pixels = new byte[byteCount];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                    // Process each pixel.
                    for (int i = 0; i < byteCount; i += bytesPerPixel)
                    {
                        // For Format32bppArgb, pixel order is Blue, Green, Red, Alpha.
                        // Convert channels to float (0 to 1).
                        float blue = pixels[i] / 255f;
                        float green = pixels[i + 1] / 255f;
                        float red = pixels[i + 2] / 255f;

                        // Multiply by brightness multiplier.
                        red = Math.Min(red * (float)brightnessMultiplier, 1f);
                        green = Math.Min(green * (float)brightnessMultiplier, 1f);
                        blue = Math.Min(blue * (float)brightnessMultiplier, 1f);

                        // Convert back to byte (0 to 255).
                        pixels[i] = (byte)(blue * 255);
                        pixels[i + 1] = (byte)(green * 255);
                        pixels[i + 2] = (byte)(red * 255);
                        // Alpha remains unchanged (pixels[i+3]).
                    }

                    // Copy modified pixels back.
                    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, byteCount);
                    bitmapToProcess.UnlockBits(data);

                    // Save the updated image (overwriting the original).
                    bitmapToProcess.Save(file, ImageFormat.Png);
                    bitmapToProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }
            }

            Console.WriteLine("Processing complete.");
        }
    }
}
