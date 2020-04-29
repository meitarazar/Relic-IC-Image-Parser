using System;
using static Relic_IC_Image_Parser.Coordinates;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Class used to hold a single sub image of the grand Relic image
    /// </summary>
    class RelicSubImage
    {
        private readonly string imageName;
        private Size canvasSize = null;
        private byte[] canvasData = null;

        private Size actualSize = null;
        private byte[] actualData = null;

        // the position and clip are saved as floating points
        //   representing the precentage of the distance on the canvases
        //   for example, the position 100 on canvas with width of 400 will be 0.25 for the x
        private Rect posPercent = null;
        private Rect clipPercent = null;

        // the row and column of the image inside the grand image
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

            //  taking the precentage values and converting them to real positions
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

            // creating data size based on the image size and 4 bytes per pixel (ARGB)
            actualData = new byte[actualSize.width * actualSize.height * 4];

            // taking the percentage values and converting them to real positions
            int iClipWStart = (int)Math.Round((canvasSize.width - 1) * clipPercent.topLeft.x * 4);
            int iClipWEnd = (int)Math.Round((canvasSize.width - 1) * clipPercent.bottomRight.x * 4);
            int iClipWAdvance = iClipWStart < iClipWEnd ? 4 : -4;
            int iClipWSkip = (canvasSize.width - actualSize.width) * iClipWAdvance;
            int iClipHStart = (int)Math.Round((canvasSize.height - 1) * clipPercent.topLeft.y * 4 * canvasSize.width);
            int iClipHEnd = (int)Math.Round((canvasSize.height - 1) * clipPercent.bottomRight.y * 4);
            int iClipHAdvance = (iClipHStart < iClipHEnd ? 4 : -4) * canvasSize.width;

            // clipping the data provided to a normalized data using the clipping positions
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
        }
    }
}
