using Relic_IC_Image_Parser.cSharp.imaging.relic;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Relic_IC_Image_Parser.cSharp.imaging
{
    public class ImageManager
    {
        public enum FileType { Relic, Standard, Unknown }

        public enum ExportType { BMP, GIF, JPG, PNG, TIFF, SPT, TXR }

        public static object GetImage(ref FileType fileType, string fileName)
        {
            RelicImage rImage = RelicImage.GetRelicImage(fileName);
            if (rImage != null)
            {
                // define self with relic image
                fileType = FileType.Relic;
                return rImage;
            }
            else
            {
                BitmapImage bImage = GetBitmapImage(fileName);
                if (bImage != null)
                {
                    // define self with normal image
                    fileType = FileType.Standard;
                    return bImage;
                }
                else
                {
                    // throw error message for unknown format
                    fileType = FileType.Unknown;
                    return null;
                }
            }
        }

        public static BitmapImage GetBitmapImage(string fileName)
        {
            BitmapImage image = null;

            try
            {
                image = new BitmapImage(new Uri(fileName));
            }
            catch
            {
                // Do nothing...
            }

            return image;
        }

        public static void ExportImage(ExportType exportType, BitmapSource bitmapSource, string fileName)
        {
            if (fileName == null)
            {
                return;
            }

            if (exportType == ExportType.TXR && !IsValidTxrSize(bitmapSource.PixelWidth, bitmapSource.PixelHeight))
            {
                // TODO throw an error
                return;
            }
            
            using (FileStream fileStream = File.OpenWrite(fileName))
            {
                if (exportType == ExportType.SPT || exportType == ExportType.TXR)
                {
                    switch(exportType)
                    {
                        case ExportType.TXR:
                            RelicTxrEncoder.EncodeTxr(bitmapSource, fileStream);
                            break;
                        default: //ExportType.SPT
                            break;
                    }

                    fileStream.Close();
                }
                else // BMP GIF JPG PNG TIFF
                {
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

                    BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapSource);
                    encoder.Frames.Add(bitmapFrame);
                    encoder.Save(fileStream);
                    fileStream.Close();
                }
            }
        }

        private static bool IsValidTxrSize(int width, int height)
        {
            return IsPowerOfTwoOrTheNumberOne(width) && IsPowerOfTwoOrTheNumberOne(height);
        }

        private static bool IsPowerOfTwoOrTheNumberOne(int num)
        {
            return (num == 1) || ((num != 0) && ((num & (num - 1)) == 0));
        }

        /*using (FileStream fileStream = File.OpenWrite("G:\\Steam\\steamapps\\common\\Impossible Creatures\\Data\\ui\\screens\\textures\\test.png"))
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            BitmapFrame bitmapFrame = BitmapFrame.Create(image);
            encoder.Frames.Add(bitmapFrame);
            encoder.Interlace = PngInterlaceOption.Off;
            encoder.Save(fileStream);
            //stream.Close();
        }*/

        /*private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat)
        {
            switch (sourceFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;

                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                    // .. as many as you need...
            }
            return new System.Windows.Media.PixelFormat();
        }*/
    }
}
