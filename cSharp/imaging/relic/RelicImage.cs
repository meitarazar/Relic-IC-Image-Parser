using Relic_IC_Image_Parser.cSharp.data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Relic_IC_Image_Parser.Coordinates;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Class that holds the Relic's grand image
    /// </summary>
    partial class RelicImage
    {
        /// <summary>
        /// The Relic image type
        /// </summary>
        public enum ImageType { SPT, TXR }

        private readonly ImageType imageType;
        private readonly FileInfo imgFileInfo;
        private readonly List<RelicSubImage> subImages;
        private readonly Size totalSize;
        private readonly byte[] imageData;

        // the grand image and its sub images
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

        /// <summary>
        /// T-h-e method that starts it all!
        /// <para>(Just kidding, just kidding, forgive me... too much home for me, plus it's late now so come on, give me a break will ya?)</para>
        /// <para>This method is made to start the ball rolling in parsing Relic's images. Just taking the file path and starts working.</para>
        /// </summary>
        /// <param name="fullFileName">The file path to open.</param>
        /// <returns></returns>
        public static RelicImage GetRelicImage(string fullFileName)
        {
            FileStream stream = RelicParser.OpenStream(fullFileName);
            if (RelicParser.IsRelicFile(stream))
            {
                List<RelicTag> relicTags = RelicParser.ReadAllTags(stream);
                
                // if we have null tags that must mean that the format is unknown to us
                //   (until someone make an upgrade... *wink* *wink*)
                if (RelicParser.HasNullTags(relicTags))
                {
                    return null;
                }

                // based on super sophisticated shit, determining the Relic image type
                //   (I dare you to look inside this function!)
                ImageType imageType = RelicParser.AnalyzeTags(relicTags);

                // the actual sub image extraction
                List<RelicSubImage> subImages = RelicParser.ParseImage(relicTags);

                // done, it wasn't easy, but we are done
                return new RelicImage(imageType, new FileInfo(fullFileName), subImages);
            }

            // yes, we might get some hiccups along the way so, we just give up.
            return null;
        }

        /// <summary>
        /// It says in the name, calculating size, what did you expect?
        /// </summary>
        /// <returns>The size of course...</returns>
        private Size CalcSize()
        {
            // ohh goody, we have an SPT on our hands...
            if (imageType == ImageType.SPT)
            {
                int width = 0;
                int height = 0;
                foreach (RelicSubImage sub in subImages)
                {
                    Rect position = sub.GetPosPercent();
                    Size size = sub.GetActualSize();

                    // if the sub image in touching the y axis
                    //   it meand that we append images in a column
                    //   which means, we measure the height
                    if (position.topLeft.x == 0)
                    {
                        height += size.height;
                    }

                    // if the sub image in touching the x axis
                    //   it meand that we append images in a row
                    //   which means, we measure the width
                    if (position.topLeft.y == 0)
                    {
                        width += size.width;
                    }
                }

                // boom! gotcha! calculation succeeded!
                return new Size(width, height);
            }

            // else ImageType.TXR
            // by the way the TXR is built, the first image is the largest, so we take it as the grand one
            return subImages[0].GetCanvasSize();
        }

        /// <summary>
        /// Why so complicated? Huh Relic? Why?
        /// <para>Yes boys and girls, this is where the shit goes down.</para>
        /// <para>We take everything we know about the sub images and construct the grand image.</para>
        /// <para>By taking each sub image at a time, and just copy it's raw pixels data on to it's corresponding position on the grand image, we can basically... make the image!</para>
        /// <para>Wow! amazing stuff indeed.</para>
        /// </summary>
        /// <returns></returns>
        private byte[] CalcImageData()
        {
            // yes... a placeholder, we prepare for battle.
            byte[] data;
            
            // ok, our enemy is SPT!
            if (imageType == ImageType.SPT)
            {
                // ok, we have the size of the image times four,
                //   for each pixel is four bytes, ARGB
                data = new byte[totalSize.width * totalSize.height * 4];

                foreach (RelicSubImage subImage in subImages)
                {
                    // where does it begin and where does it end?
                    //   the position placment on the grand image
                    int xStart = (int)Math.Round(totalSize.width * subImage.GetPosPercent().topLeft.x);
                    int xEnd = (int)Math.Round(totalSize.width * subImage.GetPosPercent().bottomRight.x);
                    int yStart = (int)Math.Round(totalSize.height * subImage.GetPosPercent().topLeft.y);
                    int yEnd = (int)Math.Round(totalSize.height * subImage.GetPosPercent().bottomRight.y);

                    // we take the column and row positions of the sub image,
                    //   top for row, and left for column
                    subImage.top = yStart / 255;
                    subImage.left = xStart / 255;

                    // just copy, pixel by pixel, to the corresponding place on the grand canvas,
                    //   we already did the heavy lifting inside the RelicSubImage class
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

            // really? a TXR...? don't wast my time, will ya?
            else //if (imageType == ImageType.TXR)
            {
                // by the way the TXR is built, the first sub image is the largest, so we just take it as is
                data = subImages[0].GetCanvasData();
                // and of course remove it from the sub image list
                subImages.RemoveAt(0);
            }

            // return the magnificent pixel data!
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

        /// <summary>
        /// Simple method to convert the raw pixels data to a workable, presentable image image
        /// </summary>
        /// <param name="size">The reported size of the image.</param>
        /// <param name="data">The raw pixels data.</param>
        /// <returns>The workable, presentable image.</returns>
        private BitmapSource ConstructBitmap(Size size, byte[] data)
        {
            PixelFormat pixelFormat = PixelFormats.Bgra32;
            int stride = pixelFormat.BitsPerPixel * size.width / 8;
            return BitmapSource.Create(size.width, size.height, DataManager.relicImageDpi, DataManager.relicImageDpi, pixelFormat, null, data, stride);
        }

        /// <summary>
        /// Gets the bitmap image, and creates it beforehand if it wasn't constructed yest
        /// </summary>
        /// <returns>The bitmap! horray!</returns>
        public BitmapSource GetBitmap()
        {
            if (bitmap == null)
            {
                bitmap = ConstructBitmap(totalSize, imageData);
            }

            return bitmap;
        }

        /// <summary>
        /// Nobody wants to ba alone, so we acompany our grand image with it's sub images
        /// </summary>
        /// <returns>A list containing the sub bitmaps.</returns>
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