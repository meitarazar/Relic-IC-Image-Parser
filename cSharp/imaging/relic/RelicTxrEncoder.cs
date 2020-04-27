using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Relic_IC_Image_Parser.cSharp.imaging.relic
{
    class RelicTxrEncoder
    {
        private const string TAG_FORM = "FORM";
        private const string TAG_TXTRNAME = "TXTRNAME";
        private const string TAG_IMAGNAME = "IMAGNAME";
        private const string TAG_VERS = "VERS";
        private const string TAG_ATTR = "ATTR";
        private const string TAG_DATA = "DATA";
        
        public static void EncodeTxr(BitmapSource bitmapSource, FileStream fileStream)
        {
            PixelFormat pixelFormat = bitmapSource.Format;
            int stride = pixelFormat.BitsPerPixel * bitmapSource.PixelWidth / 8;
            byte[] pixels = new byte[bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);

            TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(0.5, 0.5));

            int strideScale = pixelFormat.BitsPerPixel * transformedBitmap.PixelWidth / 8;
            byte[] pixelsScale = new byte[transformedBitmap.PixelHeight * stride];
            transformedBitmap.CopyPixels(pixelsScale, strideScale, 0);
        }

        private static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] mergeArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, mergeArray, array1.Length);
            Array.Copy(array2, 0, mergeArray, array1.Length, array2.Length);

            return mergeArray;
        }

        private class SubTxtrForm
        {
            int formSize = -1;
            string txtrName = null;
            byte[] vers = null;
            byte[] data = null;
            SubImagForm[] subImags = null;

            SubTxtrForm(string txtrName, int versIntValue, int[] dataIntValues, SubImagForm[] subImags)
            {
                this.txtrName = txtrName;

                this.vers = BitConverter.GetBytes(versIntValue);

                this.data = new byte[dataIntValues.Length * 4];
                for (int i = 0; i < dataIntValues.Length; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(dataIntValues[i]);
                    data[i] = curBytes[i % 4];
                    data[i + 1] = curBytes[(i + 1) % 4];
                    data[i + 2] = curBytes[(i + 2) % 4];
                    data[i + 3] = curBytes[(i + 3) % 4];
                }

                this.subImags = subImags;

                this.formSize = CalcFormSize();
            }

            private int CalcFormSize()
            {
                int formSize = 0;

                // TXTRNAME
                formSize += TAG_TXTRNAME.Length; // tag length
                formSize += 4; // tag info length
                formSize += txtrName.Length; // tag data length

                // VERS
                formSize += TAG_VERS.Length; // tag length
                formSize += 4; // tag info length
                formSize += vers.Length; // tag data length

                // DATA
                formSize += TAG_DATA.Length; // tag length
                formSize += 4; // tag info length
                formSize += data.Length; // tag data length

                // SubImags
                formSize += TAG_FORM.Length * subImags.Length;
                formSize += 4 * subImags.Length;
                foreach (SubImagForm subImag in subImags)
                {
                    formSize += subImag.formSize;
                }

                return formSize;
            }

            public byte[] GetFormAsBytes()
            {
                return null;
            }
        }

        private class SubImagForm
        {
            public readonly int formSize = -1;
            public readonly string imagName = null;
            public readonly byte[] vers = null;
            public readonly byte[] attr = null;
            public readonly byte[] data = null;
            
            SubImagForm(string imagName, int versIntValue, int[] attrIntValues, byte[] pixels)
            {
                this.imagName = imagName;

                this.vers = BitConverter.GetBytes(versIntValue);

                this.attr = new byte[attrIntValues.Length * 4];
                for (int i = 0; i < attrIntValues.Length; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(attrIntValues[i]);
                    attr[i] = curBytes[i % 4];
                    attr[i + 1] = curBytes[(i + 1) % 4];
                    attr[i + 2] = curBytes[(i + 2) % 4];
                    attr[i + 3] = curBytes[(i + 3) % 4];
                }

                this.data = (byte[])pixels.Clone();

                this.formSize = CalcFormSize();
            }

            private int CalcFormSize()
            {
                int formSize = 0;

                // IMAGENAME
                formSize += TAG_IMAGNAME.Length; // tag length
                formSize += 4; // tag info length
                formSize += imagName.Length; // tag data length

                // VERS
                formSize += TAG_VERS.Length; // tag length
                formSize += 4; // tag info length
                formSize += vers.Length; // tag data length

                // ATTR
                formSize += TAG_ATTR.Length; // tag length
                formSize += 4; // tag info length
                formSize += attr.Length; // tag data length

                // DATA
                formSize += TAG_DATA.Length; // tag length
                formSize += 4; // tag info length
                formSize += data.Length; // tag data length

                return formSize;
            }

            public byte[] GetFormAsBytes()
            {
                byte[] form = new byte[formSize + 4 + TAG_FORM.Length];
                return null;
            }
        }
    }
}
