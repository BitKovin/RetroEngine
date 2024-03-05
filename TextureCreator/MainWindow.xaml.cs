using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using Microsoft.Win32;

namespace TextureCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ComboBox1.SelectedIndex = 0;
            ComboBox2.SelectedIndex = 1;
            ComboBox3.SelectedIndex = 2;

            image1 = GetImageChannel(image1Path, color1);
            occlusion.Source = image1;

            image2 = GetImageChannel(image2Path, color2);
            roughness.Source = image2;

            image3 = GetImageChannel(image3Path, color3);
            metalic.Source = image3;

            resultImage = ComposeImages(image1, image2, image3);
            result.Source = resultImage;

        }

        int color1 = 0;
        int color2 = 1;
        int color3 = 2;

        BitmapImage image1;
        BitmapImage image2;
        BitmapImage image3;

        BitmapImage resultImage;

        string image1Path = "pack://application:,,,/none.png";
        string image2Path = "pack://application:,,,/none.png";
        string image3Path = "pack://application:,,,/none.png";

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            color1 = ComboBox1.SelectedIndex;

            image1 = GetImageChannel(image1Path, color1);
            occlusion.Source = image1;

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            color2 = ComboBox2.SelectedIndex;

            image2 = GetImageChannel(image2Path, color2);
            roughness.Source = image2;
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            color3 = ComboBox3.SelectedIndex;

            image3 = GetImageChannel(image3Path, color3);
            metalic.Source = image3;
        }

        private void occlusion_MouseDown(object sender, MouseButtonEventArgs e)
        {

            image1Path = GetImagePath(image1Path);

            image1 = GetImageChannel(image1Path, color1);
            occlusion.Source = image1;
        }

        private void roughness_MouseDown(object sender, MouseButtonEventArgs e)
        {
            image2Path = GetImagePath(image2Path);

            image2 = GetImageChannel(image2Path, color2);
            roughness.Source = image2;
        }

        private void metalic_MouseDown(object sender, MouseButtonEventArgs e)
        {

            image3Path = GetImagePath(image3Path);

            image3 = GetImageChannel(image3Path, color3);
            metalic.Source = image3;
        }

        BitmapImage GetImageChannel(string imagePath, int color)
        {

            try
            {

                BitmapImage originalBitmap = new BitmapImage(new Uri(imagePath));

                return GetImageChannel(originalBitmap, color);
            }catch (Exception ex) { return new BitmapImage(new Uri("pack://application:,,,/none.png")); }

        }

        string GetImagePath(string def)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                return imagePath;

            }

            return def;

        }

        private BitmapImage GetImageChannel(BitmapImage originalBitmap, int colorChannel)
        {

            colorChannel = 2 - colorChannel;

            if (colorChannel < 0 || colorChannel > 2)
            {
                throw new ArgumentOutOfRangeException("colorChannel", "Color channel must be 0 (red), 1 (green), or 2 (blue).");
            }

            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(originalBitmap, PixelFormats.Bgra32, null, 0);

            int bytesPerPixel = 4; // BGRA format
            int stride = convertedBitmap.PixelWidth * bytesPerPixel;
            byte[] pixels = new byte[convertedBitmap.PixelHeight * stride];
            convertedBitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += bytesPerPixel)
            {
                byte selectedChannelValue = pixels[i + colorChannel]; // Get the value of the selected channel

                // Set all RGB channels to the selected channel value
                for (int j = 0; j < 3; j++)
                {
                    pixels[i + j] = selectedChannelValue;
                }
            }

            BitmapSource bitmap = BitmapSource.Create(convertedBitmap.PixelWidth, convertedBitmap.PixelHeight,
                convertedBitmap.DpiX, convertedBitmap.DpiY, PixelFormats.Bgra32, null, pixels, stride);

            return ConvertBitmapSourceToBitmapImage(bitmap);
        }

        private BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private BitmapImage ComposeImages(BitmapImage redImage, BitmapImage greenImage, BitmapImage blueImage)
        {
            int maxWidth = Math.Max(Math.Max(redImage.PixelWidth, greenImage.PixelWidth), blueImage.PixelWidth);
            int minHeight = Math.Max(Math.Max(redImage.PixelHeight, greenImage.PixelHeight), blueImage.PixelHeight);

            redImage = ResizeImage(redImage, maxWidth, minHeight);
            greenImage = ResizeImage(greenImage, maxWidth, minHeight);
            blueImage = ResizeImage(blueImage, maxWidth, minHeight);

            FormatConvertedBitmap redBitmap = new FormatConvertedBitmap(redImage, PixelFormats.Bgra32, null, 0);
            FormatConvertedBitmap greenBitmap = new FormatConvertedBitmap(greenImage, PixelFormats.Bgra32, null, 0);
            FormatConvertedBitmap blueBitmap = new FormatConvertedBitmap(blueImage, PixelFormats.Bgra32, null, 0);

            int bytesPerPixel = 4; // BGRA format
            int stride = redBitmap.PixelWidth * bytesPerPixel;
            byte[] redPixels = new byte[redBitmap.PixelHeight * stride];
            byte[] greenPixels = new byte[greenBitmap.PixelHeight * stride];
            byte[] bluePixels = new byte[blueBitmap.PixelHeight * stride];

            redBitmap.CopyPixels(redPixels, stride, 0);
            greenBitmap.CopyPixels(greenPixels, stride, 0);
            blueBitmap.CopyPixels(bluePixels, stride, 0);

            byte[] composedPixels = new byte[redPixels.Length];

            for (int i = 0; i < redPixels.Length; i += bytesPerPixel)
            {
                composedPixels[i] = bluePixels[i];      // Blue channel
                composedPixels[i + 1] = greenPixels[i + 1]; // Green channel
                composedPixels[i + 2] = redPixels[i + 2]; // Red channel
                composedPixels[i + 3] = 255; // Alpha channel (fully opaque)
            }

            BitmapSource composedBitmap = BitmapSource.Create(redBitmap.PixelWidth, redBitmap.PixelHeight,
                redBitmap.DpiX, redBitmap.DpiY, PixelFormats.Bgra32, null, composedPixels, stride);

            return ConvertBitmapSourceToBitmapImage(composedBitmap);
        }

        private BitmapImage ResizeImage(BitmapImage image, int width, int height)
        {
            var resizedImage = new TransformedBitmap(image, new ScaleTransform(width / (double)image.PixelWidth, height / (double)image.PixelHeight));
            var encoder = new PngBitmapEncoder();
            var memoryStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(resizedImage));
            encoder.Save(memoryStream);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            resultImage = ComposeImages(image1, image2, image3);
            result.Source = resultImage;
        }

        private void SaveImageToFile(BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
            {
                MessageBox.Show("ResultImageIsEmpty");
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                BitmapEncoder encoder = null;

                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 2:
                        encoder = new JpegBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder(); // Default to PNG
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            SaveImageToFile(resultImage);
        }
    }
}