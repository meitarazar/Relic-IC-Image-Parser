using System;
using System.IO;
using System.Text;
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
        private const string VAL_NOBS = "NOBS";
        
        public static void EncodeTxr(string txtrDataName, BitmapSource bitmapSource, FileStream fileStream)
        {
            SubImagForm[] subImags = CreateSubImagForms(bitmapSource);

            SubTxtrForm subTxtrForm = new SubTxtrForm(
                txtrDataName, 
                3, 
                new int[] { 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight, subImags.Length },
                subImags
                );

            byte[] form = new byte[0];

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, 4);
            AddString(ref form, VAL_NOBS);

            form = MergeArrays(form, subTxtrForm.GetFormAsBytes());

            fileStream.Write(form, 0, form.Length);
        }

        private static SubImagForm[] CreateSubImagForms(BitmapSource bitmapSource)
        {
            int minSize = Math.Min(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            int subCount = 1;
            while (minSize != 1)
            {
                minSize /= 2;
                subCount++;
            }

            TransformedBitmap[] transformedBitmaps = CreateSubScaledTxtrs(subCount, bitmapSource);
            byte[][] subImagFormData = ExtractSubScaledTxtrsData(transformedBitmaps);

            SubImagForm[] subImags = new SubImagForm[subCount];
            for (int i = 0; i < subCount; i++)
            {
                subImags[i] = new SubImagForm(
                    (i == 0 ? "S:\\DataSrc\\Art\\Structures\\Lab\\MODEL\\LabMain_11.bmp\0" : "\0"),
                    1,
                    new int[] { 0, transformedBitmaps[i].PixelWidth, transformedBitmaps[i].PixelHeight, subImagFormData[i].Length},
                    subImagFormData[i]
                    );
            }

            return subImags;
        }

        private static TransformedBitmap[] CreateSubScaledTxtrs(int count, BitmapSource bitmapSource)
        {
            TransformedBitmap[] transformedBitmaps = new TransformedBitmap[count];

            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    transformedBitmaps[i] = new TransformedBitmap(bitmapSource, new ScaleTransform());
                }
                else
                {
                    transformedBitmaps[i] = new TransformedBitmap(transformedBitmaps[i - 1], new ScaleTransform(0.5, 0.5));
                }
            }

            return transformedBitmaps;
        }

        private static byte[][] ExtractSubScaledTxtrsData(TransformedBitmap[] transformedBitmaps)
        {
            byte[][] subTxtrs = new byte[transformedBitmaps.Length][];

            PixelFormat pixelFormat = transformedBitmaps[0].Format;
            for (int i = 0; i < transformedBitmaps.Length; i++)
            {
                int stride = pixelFormat.BitsPerPixel * transformedBitmaps[i].PixelWidth / 8;
                byte[] pixels = new byte[transformedBitmaps[i].PixelHeight * stride];
                transformedBitmaps[i].CopyPixels(pixels, stride, 0);

                subTxtrs[i] = pixels;
            }

            return subTxtrs;
        }

        private static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] mergeArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, mergeArray, array1.Length);
            Array.Copy(array2, 0, mergeArray, array1.Length, array2.Length);

            return mergeArray;
        }

        private static void AddTag(ref byte[] form, string tag)
        {
            AddString(ref form, tag);
        }

        private static void AddTagLength(ref byte[] form, int length)
        {
            byte[] formSizeBytes = BitConverter.GetBytes(length);
            Array.Reverse(formSizeBytes);
            form = MergeArrays(form, formSizeBytes);
        }

        private static void AddString(ref byte[] form, string str)
        {
            form = MergeArrays(form, Encoding.ASCII.GetBytes(str));
        }

        private class SubTxtrForm
        {
            int formSize = -1;
            string txtrName = null;
            byte[] vers = null;
            byte[] data = null;
            SubImagForm[] subImags = null;

            public SubTxtrForm(string txtrName, int versIntValue, int[] dataIntValues, SubImagForm[] subImags)
            {
                this.txtrName = txtrName;

                this.vers = BitConverter.GetBytes(versIntValue);

                this.data = new byte[dataIntValues.Length * 4];
                for (int i = 0; i < data.Length - 3; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(dataIntValues[i / 4]);
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
                byte[] form = new byte[0];

                // FORM
                AddTag(ref form, TAG_FORM);
                AddTagLength(ref form, formSize);

                // TXTRNAME
                AddTag(ref form, TAG_TXTRNAME);
                AddTagLength(ref form, txtrName.Length);
                AddString(ref form, txtrName);

                // VERS
                AddTag(ref form, TAG_VERS);
                AddTagLength(ref form, vers.Length);
                form = MergeArrays(form, vers);

                // DATA
                AddTag(ref form, TAG_DATA);
                AddTagLength(ref form, data.Length);
                form = MergeArrays(form, data);

                foreach (SubImagForm subImag in subImags)
                {
                    form = MergeArrays(form, subImag.GetFormAsBytes());
                }

                return form;
            }
        }

        private class SubImagForm
        {
            public readonly int formSize = -1;
            public readonly string imagName = null;
            public readonly byte[] vers = null;
            public readonly byte[] attr = null;
            public readonly byte[] data = null;
            
            public SubImagForm(string imagName, int versIntValue, int[] attrIntValues, byte[] pixels)
            {
                this.imagName = imagName;

                this.vers = BitConverter.GetBytes(versIntValue);

                this.attr = new byte[attrIntValues.Length * 4];
                for (int i = 0; i < attr.Length - 3; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(attrIntValues[i / 4]);
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
                byte[] form = new byte[0];

                // FORM
                AddTag(ref form, TAG_FORM);
                AddTagLength(ref form, formSize);

                // IMAGNAME
                AddTag(ref form, TAG_IMAGNAME);
                AddTagLength(ref form, imagName.Length);
                AddString(ref form, imagName);

                // VERS
                AddTag(ref form, TAG_VERS);
                AddTagLength(ref form, vers.Length);
                form = MergeArrays(form, vers);

                // ATTR
                AddTag(ref form, TAG_ATTR);
                AddTagLength(ref form, attr.Length);
                form = MergeArrays(form, attr);

                // DATA
                AddTag(ref form, TAG_DATA);
                AddTagLength(ref form, data.Length);
                form = MergeArrays(form, data);

                return form;
            }
        }
    }
}
