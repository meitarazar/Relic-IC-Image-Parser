using System;
using static Relic_IC_Image_Parser.Coordinates;

namespace Relic_IC_Image_Parser
{
    class RelicSubImage
    {
        private readonly string imageName;
        private Size canvasSize = null;
        private byte[] canvasData = null;

        private Size actualSize = null;
        private byte[] actualData = null;

        private Rect posPercent = null;
        private Rect clipPercent = null;

        public int top;
        public int left;

        public RelicSubImage(string imageName)
        {
            this.imageName = imageName;
        }

        public void SetCanvasSize(int canvasWidth, int canvasHeight)
        {
            SetCanvasSize(new Size(canvasWidth, canvasHeight));
        }

        public void SetCanvasSize(Size canvasSize)
        {
            this.canvasSize = canvasSize.Clone();

            CalcActualSize();
        }

        public void SetCanvasData(byte[] canvasData)
        {
            this.canvasData = (byte[])canvasData.Clone();

            CalcActualData();
        }

        public void SetPosPercent(Rect posPercent)
        {
            this.posPercent = posPercent.Clone();
        }

        public void SetClipPercent(Rect clipPercent)
        {
            this.clipPercent = clipPercent.Clone();

            CalcActualSize();

            CalcActualData();
        }

        public string GetImageName()
        {
            return imageName;
        }

        public Size GetCanvasSize()
        {
            return canvasSize.Clone();
        }

        public byte[] GetCanvasData()
        {
            return (byte[])canvasData.Clone();
        }

        public Size GetActualSize()
        {
            return actualSize.Clone();
        }

        public byte[] GetActualData()
        {
            return (byte[])actualData.Clone();
        }

        public Rect GetPosPercent()
        {
            return posPercent.Clone();
        }

        public Rect GetClipPercent()
        {
            return clipPercent.Clone();
        }

        private void CalcActualSize()
        {
            if (canvasSize == null || clipPercent == null)
            {
                return;
            }

            double clipWStart = canvasSize.width * clipPercent.topLeft.x;
            double clipWEnd = canvasSize.width * clipPercent.bottomRight.x;
            double clipHStart = canvasSize.height * clipPercent.topLeft.y;
            double clipHEnd = canvasSize.height * clipPercent.bottomRight.y;

            int actualWidth = (int)Math.Abs(clipWEnd - clipWStart);
            int actualHeight = (int)Math.Abs(clipHEnd - clipHStart);

            actualSize = new Size(actualWidth, actualHeight);
        }

        private void CalcActualData()
        {
            if (canvasData == null || clipPercent == null)
            {
                return;
            }

            actualData = new byte[actualSize.width * actualSize.height * 4];

            int iClipWStart = (int)Math.Round((canvasSize.width - 1) * clipPercent.topLeft.x * 4);
            int iClipWEnd = (int)Math.Round((canvasSize.width - 1) * clipPercent.bottomRight.x * 4);
            int iClipWAdvance = iClipWStart < iClipWEnd ? 4 : -4;
            int iClipWSkip = (canvasSize.width - actualSize.width) * iClipWAdvance;
            int iClipHStart = (int)Math.Round((canvasSize.height - 1) * clipPercent.topLeft.y * 4 * canvasSize.width);
            int iClipHEnd = (int)Math.Round((canvasSize.height - 1) * clipPercent.bottomRight.y * 4);
            int iClipHAdvance = (iClipHStart < iClipHEnd ? 4 : -4) * canvasSize.width;

            int i = iClipWStart + iClipHStart;
            for (int y = 0; y < actualSize.height; y++)
            {
                for (int x = 0; x < actualSize.width; x++)
                {
                    byte b = canvasData[i];
                    byte g = canvasData[i + 1];
                    byte r = canvasData[i + 2];
                    byte a = canvasData[i + 3];

                    int pos = (y * actualSize.width + x) * 4;
                    actualData[pos] = b;
                    actualData[pos + 1] = g;
                    actualData[pos + 2] = r;
                    actualData[pos + 3] = a;

                    i += iClipWAdvance;
                }
                i += iClipWSkip;
                i += canvasSize.width * (-iClipWAdvance);
                i += iClipHAdvance;
            }

            /*PixelFormat pixelFormat = PixelFormats.Bgra32;
            int stride = pixelFormat.BitsPerPixel * actualSize.width / 8;
            BitmapSource image = BitmapSource.Create(actualSize.width, actualSize.height, 96d, 96d, pixelFormat, null, actualData, stride);

            //new Bitmap(size.width, size.height, )
            using (FileStream fileStream = File.OpenWrite("G:\\Steam\\steamapps\\common\\Impossible Creatures\\Data\\ui\\screens\\textures\\test.png"))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                BitmapFrame bitmapFrame = BitmapFrame.Create(image);
                encoder.Frames.Add(bitmapFrame);
                encoder.Interlace = PngInterlaceOption.Off;
                encoder.Save(fileStream);
                fileStream.Close();
            }*/
        }
    }
}
