using Relic_IC_Image_Parser.cSharp.data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Relic_IC_Image_Parser.Coordinates;

namespace Relic_IC_Image_Parser
{
    partial class RelicImage
    {
        public enum ImageType { SPT, TXR }

        private readonly ImageType imageType;
        private readonly FileInfo imgFileInfo;
        private readonly List<RelicSubImage> subImages;
        private readonly Size totalSize;
        private readonly byte[] imageData;

        private BitmapSource bitmap;
        private List<BitmapSource> subBitmaps;

        public RelicImage(ImageType imageType, FileInfo imgFileInfo, List<RelicSubImage> subImages)
        {
            this.imageType = imageType;
            this.imgFileInfo = imgFileInfo;
            this.subImages = subImages;
            this.totalSize = CalcSize();
            this.imageData = CalcImageData();
        }

        public static RelicImage GetRelicImage(string fullFileName)
        {
            FileStream stream = RelicParser.OpenStream(fullFileName);
            if (RelicParser.IsRelicFile(stream))
            {
                List<RelicTag> relicTags = RelicParser.ReadAllTags(stream);
                if (RelicParser.HasNullTags(relicTags))
                {
                    return null;
                }

                ImageType imageType = RelicParser.AnalyzeTags(relicTags);
                List<RelicSubImage> subImages = RelicParser.ParseImage(relicTags);

                return new RelicImage(imageType, new FileInfo(fullFileName), subImages);
            }

            return null;
        }

        private Size CalcSize()
        {
            if (imageType == ImageType.SPT)
            {
                int width = 0;
                int height = 0;
                foreach (RelicSubImage sub in subImages)
                {
                    Rect position = sub.GetPosPercent();
                    Size size = sub.GetActualSize();

                    if (position.topLeft.x == 0)
                    {
                        height += size.height;
                    }
                    if (position.topLeft.y == 0)
                    {
                        width += size.width;
                    }
                }

                return new Size(width, height);
            }

            // else ImageType.TXR
            return subImages[0].GetCanvasSize();
        }

        private byte[] CalcImageData()
        {
            byte[] data;
            
            if (imageType == ImageType.SPT)
            {
                data = new byte[totalSize.width * totalSize.height * 4];

                foreach (RelicSubImage subImage in subImages)
                {
                    int xStart = (int)Math.Round(totalSize.width * subImage.GetPosPercent().topLeft.x);
                    int xEnd = (int)Math.Round(totalSize.width * subImage.GetPosPercent().bottomRight.x);
                    int yStart = (int)Math.Round(totalSize.height * subImage.GetPosPercent().topLeft.y);
                    int yEnd = (int)Math.Round(totalSize.height * subImage.GetPosPercent().bottomRight.y);

                    subImage.top = yStart / 255;
                    subImage.left = xStart / 255;

                    int i = 0;
                    byte[] subData = subImage.GetActualData();
                    for (int y = yStart; y < yEnd; y++)
                    {
                        for (int x = xStart; x < xEnd; x++)
                        {
                            byte b = subData[i];
                            byte g = subData[i + 1];
                            byte r = subData[i + 2];
                            byte a = subData[i + 3];

                            int pos = (y * totalSize.width + x) * 4;
                            data[pos] = b;
                            data[pos + 1] = g;
                            data[pos + 2] = r;
                            data[pos + 3] = a;

                            i += 4;
                        }
                    }
                }
            }
            else //if (imageType == ImageType.TXR)
            {
                data = subImages[0].GetCanvasData();
                subImages.RemoveAt(0);
            }

            return data;
        }

        public ImageType GetImageType()
        {
            return imageType;
        }

        public List<RelicSubImage> GetSubImages()
        {
            return subImages;
        }

        private BitmapSource ConstructBitmap(Size size, byte[] data)
        {
            PixelFormat pixelFormat = PixelFormats.Bgra32;
            int stride = pixelFormat.BitsPerPixel * size.width / 8;
            return BitmapSource.Create(size.width, size.height, DataManager.relicImageDpi, DataManager.relicImageDpi, pixelFormat, null, data, stride);
        }

        public BitmapSource GetBitmap()
        {
            if (bitmap == null)
            {
                bitmap = ConstructBitmap(totalSize, imageData);
            }

            return bitmap;
        }

        public List<BitmapSource> GetSubBitmaps()
        {
            if (subBitmaps == null)
            {
                subBitmaps = new List<BitmapSource>();
                foreach (RelicSubImage subImage in subImages)
                {
                    if (imageType == ImageType.SPT)
                    {
                        subBitmaps.Add(ConstructBitmap(subImage.GetActualSize(), subImage.GetActualData()));
                    }
                    else //if (imageType == ImageType.TXR)
                    {
                        subBitmaps.Add(ConstructBitmap(subImage.GetCanvasSize(), subImage.GetCanvasData()));
                    }
                }
            }
            
            return subBitmaps;
        }
    }
}