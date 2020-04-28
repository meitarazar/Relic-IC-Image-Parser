using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Relic_IC_Image_Parser.cSharp.imaging.ImageManager;

namespace Relic_IC_Image_Parser.cSharp.ui
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly BitmapSource bitmapSource;
        
        public ExportWindow(FileType fileType, BitmapSource bitmapSource)
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
            ImageManager.ExportImage(exportType, bitmapSource, DataManager.SaveFile(exportType));
            Close();
        }
    }
}
