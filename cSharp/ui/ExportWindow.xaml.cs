using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.imaging;
using Relic_IC_Image_Parser.cSharp.util;
using System.IO;
using System.Reflection;
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
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            InitializeComponent();

            if (fileType == FileType.Relic)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File type: Relic");

                ExportBmp.IsEnabled = true;
                ExportGif.IsEnabled = true;
                ExportJpg.IsEnabled = true;
                ExportPng.IsEnabled = true;
                ExportTiff.IsEnabled = true;
            }
            else if (fileType == FileType.Standard)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File type: Standard");

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
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Exporting as " + exportType + "...");

            string filePath = DataManager.SaveFile(exportType, fileName);
            if (filePath == null) {
                return;
            }

            ImageManager.ExportImage(this, exportType, bitmapSource, new FileInfo(filePath));
            Close();
        }
    }
}
