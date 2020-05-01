using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Relic_IC_Image_Parser.cSharp.imaging.relic
{
    /// <summary>
    /// Tam Tam Tam!
    /// <para>When reconstructing this silly image format, there is a lot to do.</para>
    /// </summary>
    class RelicEncoder
    {
        // static strings of the tags
        private const string TAG_FORM = "FORM";
        private const string TAG_PART_PICT = "PICT";
        private const string TAG_TXTRNAME = "TXTRNAME";
        private const string TAG_IMAGNAME = "IMAGNAME";
        private const string TAG_VERS = "VERS";
        private const string TAG_ATTR = "ATTR";
        private const string TAG_DATA = "DATA";
        private const string TAG_RECT = "RECT";
        private const string VAL_NOBS = "NOBS";

        // TODO doc
        public static void EncodeSpt(BitmapSource bitmapSource, FileStream fileStream)
        {
            string txtrName = "C:\\SomeImage00.tga";

            int fixedWidth = 1;
            int fixedHeight = 1;
            while (fixedWidth < bitmapSource.PixelWidth)
            {
                fixedWidth *= 2;
            }
            while (fixedHeight < bitmapSource.PixelHeight)
            {
                fixedHeight *= 2;
            }
            int fixedDataLength = fixedWidth * fixedHeight * 4;

            PixelFormat pixelFormat = bitmapSource.Format;
            int stride = pixelFormat.BitsPerPixel * bitmapSource.PixelWidth / 8;
            byte[] pixels = new byte[bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);

            SubImagForm[] subImags = new SubImagForm[] { new SubImagForm(
                txtrName + "\0",
                1,
                new int[] { 0, fixedWidth, fixedHeight, fixedDataLength},
                stride,
                pixels
                ) 
            };

            SubTxtrForm subTxtrForm = new SubTxtrForm(
                txtrName + "\0",
                3,
                new int[] { 1, fixedWidth, fixedHeight, subImags.Length },
                subImags
                );

            RectForm rectForm = new RectForm(
                txtrName,
                new float[] { 0, 1, 0, 1},
                new float[] { 0, ((float) bitmapSource.PixelWidth) / fixedWidth, 1, 1 - ((float)bitmapSource.PixelHeight) / fixedHeight }
                );

            // prepare form data
            byte[] subTxtrFormData = subTxtrForm.GetFormAsBytes();
            byte[] rectFormData = rectForm.GetFormAsBytes();

            // create the full file 'FORM'
            byte[] form = new byte[0];

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, 4);
            AddString(ref form, VAL_NOBS);

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, subTxtrFormData.Length + rectFormData.Length + 4);

            AddString(ref form, TAG_PART_PICT);

            form = MergeArrays(form, subTxtrFormData);

            form = MergeArrays(form, rectFormData);

            fileStream.Write(form, 0, form.Length);
        }
        
        /// <summary>
        /// Encode TXR!
        /// <para>We have the TXR data path, we have the bitmap and the file to write to! Lets' begin!</para>
        /// </summary>
        /// <param name="txtrDataPath">The TXR data path.</param>
        /// <param name="bitmapSource">The bitmap to encode.</param>
        /// <param name="fileStream">The file to write to.</param>
        public static void EncodeTxr(string txtrDataPath, BitmapSource bitmapSource, FileStream fileStream)
        {
            // Gets all the sub image 'FORM's
            SubImagForm[] subImags = CreateSubImagForms(bitmapSource);

            // Creates the 'FORM' that encapsulates all the sub images
            SubTxtrForm subTxtrForm = new SubTxtrForm(
                txtrDataPath, 
                3, 
                new int[] { 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight, subImags.Length },
                subImags
                );

            // Create the full file 'FORM'
            byte[] form = new byte[0];

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, 4);
            AddString(ref form, VAL_NOBS);

            form = MergeArrays(form, subTxtrForm.GetFormAsBytes());

            fileStream.Write(form, 0, form.Length);
        }

        /// <summary>
        /// Constructs the sub image 'FORM's.
        /// </summary>
        /// <param name="bitmapSource">The largest bitmap.</param>
        /// <returns></returns>
        private static SubImagForm[] CreateSubImagForms(BitmapSource bitmapSource)
        {
            // calc how many to create
            int minSize = Math.Min(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            int subCount = 1;
            while (minSize != 1)
            {
                minSize /= 2;
                subCount++;
            }

            // create all scaled bitmaps and extract the pixels
            TransformedBitmap[] transformedBitmaps = CreateSubScaledTxtrs(subCount, bitmapSource);
            byte[][] subImagPixelData = ExtractSubScaledTxtrsData(transformedBitmaps);

            // buld each 'FORM' with the image and pixels data
            SubImagForm[] subImags = new SubImagForm[subCount];
            for (int i = 0; i < subCount; i++)
            {
                subImags[i] = new SubImagForm(
                    (i == 0 ? "S:\\DataSrc\\Art\\Structures\\Lab\\MODEL\\LabMain_11.bmp\0" : "\0"),
                    1,
                    new int[] { 0, transformedBitmaps[i].PixelWidth, transformedBitmaps[i].PixelHeight, subImagPixelData[i].Length},
                    0,
                    subImagPixelData[i]
                    );
            }

            return subImags;
        }

        /// <summary>
        /// Creates all the smaller bitmaps from the largest bitmap.
        /// </summary>
        /// <param name="count">The count of bitmaps to produce.</param>
        /// <param name="bitmapSource">The largest bitmap.</param>
        /// <returns>A list of all the smaller scaled bitmaps including the largest.</returns>
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

        /// <summary>
        /// Takes all the scaled bitmaps and the largest one, and extract the pixels from them.
        /// </summary>
        /// <param name="transformedBitmaps">The list of all the bitmaps.</param>
        /// <returns>Two dimentional array that represents the raw pixels for each bitmap.</returns>
        private static byte[][] ExtractSubScaledTxtrsData(TransformedBitmap[] transformedBitmaps)
        {
            byte[][] subTxtrs = new byte[transformedBitmaps.Length][];

            PixelFormat pixelFormat = transformedBitmaps[0].Format;
            for (int i = 0; i < transformedBitmaps.Length; i++)
            {
                int width = transformedBitmaps[i].PixelWidth;
                int stride = pixelFormat.BitsPerPixel * width / 8;
                byte[] pixels = new byte[transformedBitmaps[i].PixelHeight * stride];
                transformedBitmaps[i].CopyPixels(pixels, stride, 0);

                // remember that TXR is vertically flipped? so yeah...
                //   we flipped it while importing, and we need to flip it back
                subTxtrs[i] = RelicImage.ReverseTxrData(width, pixels);
            }

            return subTxtrs;
        }

        /// <summary>
        /// Takes two arrays and add them together.
        /// </summary>
        /// <param name="array1">Array 1.</param>
        /// <param name="array2">Array 2.</param>
        /// <returns>The combined array.</returns>
        private static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] mergeArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, mergeArray, array1.Length);
            Array.Copy(array2, 0, mergeArray, array1.Length, array2.Length);

            return mergeArray;
        }

        /// <summary>
        /// Add the tage name to the 'FORM'.
        /// </summary>
        /// <param name="form">The 'FORM' to add to.</param>
        /// <param name="tag">The tag name.</param>
        private static void AddTag(ref byte[] form, string tag)
        {
            AddString(ref form, tag);
        }

        /// <summary>
        /// Add the length of the tag as bytes.
        /// </summary>
        /// <param name="form">The 'FORM' to add to.</param>
        /// <param name="length">The length as int 32 bit / 4 bytes</param>
        private static void AddTagLength(ref byte[] form, int length)
        {
            byte[] tagSizeBytes = BitConverter.GetBytes(length);

            // relic reverse the tag length
            Array.Reverse(tagSizeBytes);

            AddInt32(ref form, BitConverter.ToInt32(tagSizeBytes, 0));
        }

        private static void AddInt32(ref byte[] form, int integer)
        {
            form = MergeArrays(form, BitConverter.GetBytes(integer));
        }

        /// <summary>
        /// Adds the string as bytes to the 'FORM'.
        /// </summary>
        /// <param name="form">The 'FORM' to add to.</param>
        /// <param name="str">The string to add.</param>
        private static void AddString(ref byte[] form, string str)
        {
            form = MergeArrays(form, Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// The 'FORM' that conatins all of the sub images of the Relic image file.
        /// </summary>
        private class SubTxtrForm
        {
            int formSize = -1;
            string txtrPath = null;
            byte[] vers = null;
            byte[] data = null;
            SubImagForm[] subImags = null;

            public SubTxtrForm(string txtrPath, int versIntValue, int[] dataIntValues, SubImagForm[] subImags)
            {
                this.txtrPath = txtrPath;

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

            /// <summary>
            /// Calculate the total 'FORM' bytes size based on the current tags data
            /// </summary>
            /// <returns>The 'FORM' bytes size.</returns>
            private int CalcFormSize()
            {
                int formSize = 0;

                // TXTRNAME
                formSize += TAG_TXTRNAME.Length; // tag length
                formSize += 4; // tag info length
                formSize += txtrPath.Length; // tag data length

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

            /// <summary>
            /// Convert the data we have to bytes representation.
            /// </summary>
            /// <returns>The array of bytes containing the 'FORM' data.</returns>
            public byte[] GetFormAsBytes()
            {
                byte[] form = new byte[0];

                // FORM
                AddTag(ref form, TAG_FORM);
                AddTagLength(ref form, formSize);

                // TXTRNAME
                AddTag(ref form, TAG_TXTRNAME);
                AddTagLength(ref form, txtrPath.Length);
                AddString(ref form, txtrPath);

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

        /// <summary>
        /// The most basic 'FORM' in Relic image file.
        /// </summary>
        private class SubImagForm
        {
            public readonly int formSize = -1;
            public readonly string imagName = null;
            public readonly byte[] vers = null;
            public readonly byte[] attr = null;
            public readonly byte[] data = null;

            public SubImagForm(string imagName, int versIntValue, int[] attrIntValues, int stride, byte[] pixels)
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

                if (attrIntValues[3] > pixels.Length)
                {
                    this.data = new byte[attrIntValues[3]];

                    int dataStride = attrIntValues[1] * 4;
                    int pixelsHeight = pixels.Length / stride;
                    for (int i = 0; i < pixelsHeight; i++)
                    {
                        Array.Copy(pixels, i * stride, data, data.Length - (i + 1) * dataStride, stride);
                    }
                }
                else
                {
                    this.data = (byte[])pixels.Clone();
                }

                this.formSize = CalcFormSize();
            }

            /// <summary>
            /// Calculate the total 'FORM' bytes size based on the current tags data
            /// </summary>
            /// <returns>The 'FORM' bytes size.</returns>
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

            /// <summary>
            /// Convert the data we have to bytes representation.
            /// </summary>
            /// <returns>The array of bytes containing the 'FORM' data.</returns>
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

        // TODO doc
        private class RectForm
        {
            public readonly int formSize = -1;
            public readonly string txtrName = null;
            public readonly byte[] pos = null;
            public readonly byte[] clip = null;

            public RectForm(string txtrName, float[] posFloatVals, float[] clipFloatVals)
            {
                this.txtrName = txtrName;

                this.pos = new byte[posFloatVals.Length * 4];
                for (int i = 0; i < pos.Length - 3; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(posFloatVals[i / 4]);
                    pos[i] = curBytes[i % 4];
                    pos[i + 1] = curBytes[(i + 1) % 4];
                    pos[i + 2] = curBytes[(i + 2) % 4];
                    pos[i + 3] = curBytes[(i + 3) % 4];
                }

                this.clip = new byte[clipFloatVals.Length * 4];
                for (int i = 0; i < clip.Length - 3; i += 4)
                {
                    byte[] curBytes = BitConverter.GetBytes(clipFloatVals[i / 4]);
                    clip[i] = curBytes[i % 4];
                    clip[i + 1] = curBytes[(i + 1) % 4];
                    clip[i + 2] = curBytes[(i + 2) % 4];
                    clip[i + 3] = curBytes[(i + 3) % 4];
                }

                this.formSize = CalcFormSize();
            }

            /// <summary>
            /// Calculate the total 'FORM' bytes size based on the current tags data
            /// </summary>
            /// <returns>The 'FORM' bytes size.</returns>
            private int CalcFormSize()
            {
                int formSize = 0;

                // TXTRNAME
                formSize += 4; // name info length
                formSize += txtrName.Length; // name data length

                // POS
                formSize += pos.Length; // pos data length

                // CLIP
                formSize += clip.Length; // clip data length

                return formSize;
            }

            internal byte[] GetFormAsBytes()
            {
                byte[] form = new byte[0];

                // RECT
                AddTag(ref form, TAG_RECT);
                AddTagLength(ref form, formSize);

                // TXTRNAME (in 'RECT' form the length is not reversed)
                AddInt32(ref form, txtrName.Length);
                AddString(ref form, txtrName);

                // POS
                form = MergeArrays(form, pos);

                // CLIP
                form = MergeArrays(form, clip);

                return form;
            }
        }
    }
}
