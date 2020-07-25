using Relic_IC_Image_Parser.cSharp.imaging.relic;
using Relic_IC_Image_Parser.cSharp.ui;
using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.IO;
using System.Net.Cache;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Relic_IC_Image_Parser.cSharp.imaging
{
    /// <summary>
    /// Class that wraps all image related operations
    /// </summary>
    public class ImageManager
    {
        /// <summary>
        /// The file type containing the image
        /// </summary>
        public enum FileType { Relic, Standard, Unknown }

        /// <summary>
        /// The export type of the file
        /// </summary>
        public enum ExportType { BMP, GIF, JPG, PNG, TIFF, SPT, TXR }

        /// <summary>
        /// Simple method that receives file path to load the image from.
        /// <para>Also it determines what type of file is it.</para>
        /// </summary>
        /// <param name="fileType">The ref to the file type variable to set.</param>
        /// <param name="fileName">The file path to load.</param>
        /// <returns></returns>
        public static object GetImage(ref FileType fileType, string fileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Get image...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File path: " + fileName);

            // test if Relic type of image
            RelicImage rImage = RelicImage.GetRelicImage(fileName);
            if (rImage != null)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Relic image not null");
                fileType = FileType.Relic;
                return rImage;
            }
            else
            {
                // test if standart type of image
                BitmapSource bImage = GetBitmapImage(fileName);

                if (bImage != null)
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Standard image not null");
                    fileType = FileType.Standard;
                    return bImage;
                }

                // otherwise, unknown
                else
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Unable to parse file");
                    fileType = FileType.Unknown;
                    return null;
                }
            }
        }

        /// <summary>
        /// Simple method to open standard image file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static BitmapSource GetBitmapImage(string fileName)
        {
            BitmapSource source = null;

            try
            {
                // try to open the file
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fileName);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.EndInit();

                FormatConvertedBitmap bgraImage = new FormatConvertedBitmap();
                bgraImage.BeginInit();
                bgraImage.Source = image;
                bgraImage.DestinationFormat = PixelFormats.Bgra32;
                bgraImage.DestinationPalette = null;
                bgraImage.AlphaThreshold = 0;
                bgraImage.EndInit();

                source = bgraImage.Source;
            }
            catch
            {
                // Do nothing...
            }

            return source;
        }

        /// <summary>
        /// Method that invokes the export of an image to a file according to the export type provided.
        /// </summary>
        /// <param name="exportType">The export type.</param>
        /// <param name="bitmapSource">The image to export.</param>
        /// <param name="fileInfo">The file info to save to.</param>
        public static void ExportImage(Window owner, ExportType exportType, BitmapSource bitmapSource, FileInfo fileInfo)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Exporting image...");
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Type: " + exportType.ToString());
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File: " + fileInfo.ToString());

            // we have nothing to do if the file info is null
            if (fileInfo == null)
            {
                return;
            }

            // if we are exporting TXR and the image is not properly sized, again, return
            if (exportType == ExportType.TXR && !IsValidTxrSize(bitmapSource.PixelWidth, bitmapSource.PixelHeight))
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "TXR not properly sized!");

                string msg = "The image you are trying to export is not a valid TXR file!\n\n" +
                    "A valid TXR width and height are of powers of 2. (..., 32, 64, 128, 256, ...)";
                string title = "Invalid TXR File";
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            // set the txtr data file path
            string txtrDataPath = null;
            if (exportType == ExportType.TXR)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Constructing TXR inner path id...");

                // remove extention from file name
                string[] fileNameSplit = fileInfo.Name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string filenameNoExt = string.Join(".", fileNameSplit, 0, fileNameSplit.Length - 1);
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name without extension: " + filenameNoExt);

                // open dialog to get the txtr data path
                TxtrExportWindow txtrExportWindow = new TxtrExportWindow(filenameNoExt);
                txtrExportWindow.Owner = owner;

                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Opening dialog...");
                // if return false, exit
                if (!txtrExportWindow.ShowDialog().Value)
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Cancel.");
                    return;
                }

                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Dialog return OK");
                // set the txtr data path
                txtrDataPath = txtrExportWindow.txtrFilePath;
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "TXR inner path id: " + txtrDataPath);
            }

            // opening a file stream to the disk
            using (FileStream fileStream = File.OpenWrite(fileInfo.FullName))
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File stream opened");
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Export type: " + exportType.ToString());

                // if we are dealing with relic type of image
                if (exportType == ExportType.SPT || exportType == ExportType.TXR)
                {
                    switch(exportType)
                    {
                        case ExportType.TXR:
                            RelicEncoder.EncodeTxr(txtrDataPath + "\0", bitmapSource, fileStream);
                            break;
                        default: //ExportType.SPT
                            RelicEncoder.EncodeSpt(bitmapSource, fileStream);
                            break;
                    }
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Success");
                }

                // if we are dealing with standard type of image
                else // BMP GIF JPG PNG TIFF
                {
                    // create the encoder and its settings according to the export type
                    BitmapEncoder encoder = null;
                    switch (exportType)
                    {
                        case ExportType.GIF:
                            encoder = new GifBitmapEncoder();
                            break;
                        case ExportType.JPG:
                            encoder = new JpegBitmapEncoder();
                            ((JpegBitmapEncoder)encoder).QualityLevel = 100;
                            break;
                        case ExportType.PNG:
                            encoder = new PngBitmapEncoder();
                            ((PngBitmapEncoder)encoder).Interlace = PngInterlaceOption.Default;
                            break;
                        case ExportType.TIFF:
                            encoder = new TiffBitmapEncoder();
                            ((TiffBitmapEncoder)encoder).Compression = TiffCompressOption.None;
                            break;
                        default: //ExportType.BMP
                            encoder = new BmpBitmapEncoder();
                            break;
                    }

                    // write the image to the disk
                    BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapSource);
                    encoder.Frames.Add(bitmapFrame);
                    encoder.Save(fileStream);
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Success");
                }
            }
        }

        /// <summary>
        /// Tests if the provided size is a valid TXR size.
        /// <para>A valid TXR image is consisted of width and height that are powers of 2, or the number 1.</para>
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>If it is a valid TXR size.</returns>
        private static bool IsValidTxrSize(int width, int height)
        {
            return IsPowerOfTwoOrTheNumberOne(width) && IsPowerOfTwoOrTheNumberOne(height);
        }

        /// <summary>
        /// Tests if the provided number is a power of 2 or the number 1.
        /// </summary>
        /// <param name="num">The number to test.</param>
        /// <returns>If the number is a power of 2.</returns>
        private static bool IsPowerOfTwoOrTheNumberOne(int num)
        {
            return (num == 1) || ((num != 0) && ((num & (num - 1)) == 0));
        }
    }
}
