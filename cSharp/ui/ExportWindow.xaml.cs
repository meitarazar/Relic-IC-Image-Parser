using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static Relic_IC_Image_Parser.cSharp.imaging.ImageManager;

namespace Relic_IC_Image_Parser.cSharp.ui
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly string fileName;
        private readonly BitmapSource bitmapSource;
        
        public ExportWindow(FileType fileType, string fileName, BitmapSource bitmapSource)
        {
            InitializeComponent();

            // TODO change
            /*if (true)
            {
                ExportBmp.IsEnabled = true;
                ExportGif.IsEnabled = true;
                ExportJpg.IsEnabled = true;
                ExportPng.IsEnabled = true;
                ExportTiff.IsEnabled = true;
                ExportSpt.IsEnabled = true;
                ExportTxr.IsEnabled = true;
            }
            else*/ if (fileType == FileType.Relic)
            {
                ExportBmp.IsEnabled = true;
                ExportGif.IsEnabled = true;
                ExportJpg.IsEnabled = true;
                ExportPng.IsEnabled = true;
                ExportTiff.IsEnabled = true;
            }
            else if (fileType == FileType.Standard)
            {
                ExportSpt.IsEnabled = true;
                ExportTxr.IsEnabled = true;
            }

            this.fileName = fileName;
            this.bitmapSource = bitmapSource;
        }

        private void ExportBmp_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.BMP);
        }

        private void ExportGif_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.GIF);
        }

        private void ExportJpg_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.JPG);
        }

        private void ExportPng_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.PNG);
        }

        private void ExportTiff_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.TIFF);
        }

        private void ExportSpt_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.SPT);
        }

        private void ExportTxr_Click(object sender, RoutedEventArgs e)
        {
            Export(ExportType.TXR);
        }

        private void Export(ExportType exportType)
        {
            ImageManager.ExportImage(this, exportType, bitmapSource, new FileInfo(DataManager.SaveFile(exportType, fileName)));
            Close();
        }
    }
}
