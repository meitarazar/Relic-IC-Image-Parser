using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        // static strings for tags
        private const string TAG_ATTR      = "ATTR";
        private const string TAG_DATA      = "DATA";
        private const string TAG_FORM      = "FORM";
        private const string TAG_IMAGNAME  = "IMAGNAME";
        private const string TAG_RECT      = "RECT";
        private const string TAG_TXTRNAME  = "TXTRNAME";
        private const string TAG_VERS      = "VERS";

        // static strings for partial tags
        private const string TAG_PART_PICT = "PICT";

        // static strings for values
        private const string VAL_NOBS      = "NOBS";

        /// <summary>
        /// Encode SPT.
        /// <para>Only the source image and the file to write to is needed, the image will be
        /// plistted into sub images based on the best <i><b>current</b></i> optimization algorithm</para>
        /// </summary>
        /// <param name="bitmapSource">The bitmap to encode.</param>
        /// <param name="fileStream">The file to write to.</param>
        public static void EncodeSpt(BitmapSource bitmapSource, FileStream fileStream)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Encoding SPT...");

            string txtrName = "C:\\Exported\\From\\Relic_IC_Image_Parser\\By\\MightySarion";
            string txtrExtension = ".tga";

            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;

            int[] widthCuts = CalcCuts(width);
            int[] heightCuts = CalcCuts(height);

            float posStartX = 0;
            RectForm[] rectForms = new RectForm[widthCuts.Length * heightCuts.Length];
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Number of sub images: " + rectForms.Length);

            for (int w = 0; w < widthCuts.Length; w++)
            {
                float posStartY = 0;
                for (int h = 0; h < heightCuts.Length; h++)
                {
                    float posEndX = posStartX + widthCuts[w];
                    float posEndY = posStartY + heightCuts[h];

                    int arrPos = (w * heightCuts.Length) + h;
                    rectForms[arrPos] = new RectForm(
                        string.Format("{0}{1}{2}", txtrName, arrPos.ToString("D2"), txtrExtension),
                        new float[] { posStartX / width, posEndX / width, posStartY / height, posEndY / height },
                        new float[] { 0, 1, 1, 0 }
                        );
                    posStartY += heightCuts[h];
                }
                posStartX += widthCuts[w];
            }

            //Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating sub images...");
            SubImagForm[] subImags = CreateSubImageFormsFromRects(bitmapSource, rectForms);

            //Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating texture forms...");
            SubTxtrForm[] subTxtrs = CreateSubTxtrForms(subImags);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Preparing data...");
            // prepare txtrs data
            byte[] subTxtrFormsData = new byte[0];
            foreach (SubTxtrForm subTxtr in subTxtrs)
            {
                subTxtrFormsData = MergeArrays(subTxtrFormsData, subTxtr.GetFormAsBytes());
            }

            // prepare rects data
            byte[] rectFormsData = new byte[0];
            foreach (RectForm rectForm in rectForms)
            {
                rectFormsData = MergeArrays(rectFormsData, rectForm.GetFormAsBytes());
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Constructing data...");
            // create the full file 'FORM'
            byte[] form = new byte[0];

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, 4);
            AddString(ref form, VAL_NOBS);

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, subTxtrFormsData.Length + rectFormsData.Length + 4);

            // the full first form name is 'PICTFORM'
            //  so we just add the 'PICT' before the 'FORM'
            AddString(ref form, TAG_PART_PICT);

            form = MergeArrays(form, subTxtrFormsData);

            form = MergeArrays(form, rectFormsData);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Writing to file...");
            fileStream.Write(form, 0, form.Length);

            /*int fixedWidth = 1;
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
            AddTagLength(ref form, subTxtrFormsData.Length + rectFormData.Length + 4);

            // the full first form name is 'PICTFORM'
            //  so we just add the 'PICT' before the 'FORM'
            AddString(ref form, TAG_PART_PICT);

            form = MergeArrays(form, subTxtrFormsData);

            form = MergeArrays(form, rectFormData);

            fileStream.Write(form, 0, form.Length);*/

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Done.");
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int[] CalcCuts(int length)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Calculating cuts of length...");

            byte[] bytes = BitConverter.GetBytes(length);
            BitArray bits = new BitArray(bytes);

            List<int> cuts = new List<int>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    cuts.Add((int)Math.Pow(2, i));
                }
            }

            int[] cutsArr = cuts.ToArray();
            Array.Reverse(cutsArr);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Final cuts: " + string.Join(", ", cutsArr));
            return cutsArr;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <returns></returns>
        private static SubImagForm[] CreateSubImageFormsFromRects(BitmapSource bitmapSource, RectForm[] rectForms)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating sub image forms from RECTs...");

            int bitmapWidth = bitmapSource.PixelWidth;
            int bitmapHeight = bitmapSource.PixelHeight;

            PixelFormat pixelFormat = bitmapSource.Format;
            int bitmapStride = pixelFormat.BitsPerPixel * bitmapWidth / 8;
            byte[] flatPixels = new byte[bitmapHeight * bitmapStride];
            bitmapSource.CopyPixels(flatPixels, bitmapStride, 0);

            byte[][] bitmapPixels = new byte[bitmapHeight][];
            for (int y = 0; y < bitmapHeight; y++) {
                bitmapPixels[y] = new byte[bitmapStride];
                Array.Copy(flatPixels, y * bitmapStride, bitmapPixels[y], 0, bitmapStride);
            }

            SubImagForm[] subImags = new SubImagForm[rectForms.Length];
            for (int i = 0; i < rectForms.Length; i++)
            {
                RectForm rectForm = rectForms[i];

                int x = (int)Math.Round(bitmapWidth * BitConverter.ToSingle(rectForm.pos, 0));
                int y = (int)Math.Round(bitmapHeight * BitConverter.ToSingle(rectForm.pos, 8));

                int width = Math.Abs((int)Math.Round(bitmapWidth * BitConverter.ToSingle(rectForm.pos, 4)) - x);
                int height = Math.Abs((int)Math.Round(bitmapHeight * BitConverter.ToSingle(rectForm.pos, 12)) - y);

                int stride = pixelFormat.BitsPerPixel * width / 8;
                byte[] pixels = new byte[height * stride];

                for (int h = 0; h < height; h++)
                {
                    Array.Copy(bitmapPixels[y + h], x * 4, pixels, (height - h - 1) * stride, stride);
                }

                /*BitmapSource bitmapTest = BitmapSource.Create(width, height, 96, 96, pixelFormat, null, pixels, stride);
                BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapTest);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(bitmapFrame);
                FileStream fileStream = File.OpenWrite(rectForm.txtrName.Substring(3, 12) + "png");
                encoder.Save(fileStream);
                fileStream.Close();*/

                subImags[i] = new SubImagForm(
                    rectForm.txtrName + "\0",
                    1,
                    new int[] { 0, width, height, pixels.Length },
                    stride,
                    pixels
                    );
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Sub image form #" + subImags.Length);
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Total of " + subImags.Length + " sub image forms");
            return subImags;
        }

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="subImagForms"></param>
        /// <returns></returns>
        private static SubTxtrForm[] CreateSubTxtrForms(SubImagForm[] subImagForms)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating texture forms...");

            SubTxtrForm[] subTxtrs = new SubTxtrForm[subImagForms.Length];
            for (int i = 0; i < subImagForms.Length; i++)
            {
                SubImagForm subImag = subImagForms[i];
                subTxtrs[i] = new SubTxtrForm(
                    subImag.imagName,
                    3,
                    new int[] { 1, BitConverter.ToInt32(subImag.attr, 4), BitConverter.ToInt32(subImag.attr, 8), 1 },
                    new SubImagForm[] { subImag }
                    );
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Texture form #" + subTxtrs.Length);
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Total of " + subTxtrs.Length + " texture forms");
            return subTxtrs;
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
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Encoding TXR...");

            // Gets all the sub image 'FORM's
            SubImagForm[] subImags = CreateSubScaledImagForms(bitmapSource);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating texture form...");
            // Creates the 'FORM' that encapsulates all the sub images
            SubTxtrForm subTxtrForm = new SubTxtrForm(
                txtrDataPath, 
                3, 
                new int[] { 1, bitmapSource.PixelWidth, bitmapSource.PixelHeight, subImags.Length },
                subImags
                );

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Constructing data...");
            // Create the full file 'FORM'
            byte[] form = new byte[0];

            AddTag(ref form, TAG_FORM);
            AddTagLength(ref form, 4);
            AddString(ref form, VAL_NOBS);

            form = MergeArrays(form, subTxtrForm.GetFormAsBytes());

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Writing to file...");
            fileStream.Write(form, 0, form.Length);
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Done.");
        }

        /// <summary>
        /// Constructs the sub image 'FORM's of TXR format.
        /// </summary>
        /// <param name="bitmapSource">The largest bitmap.</param>
        /// <returns></returns>
        private static SubImagForm[] CreateSubScaledImagForms(BitmapSource bitmapSource)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating sub scaled textures...");

            // calc how many to create
            int minSize = Math.Min(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            int subCount = 1;
            while (minSize != 1)
            {
                minSize /= 2;
                subCount++;
            }
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, subCount + " sub scaled textures needed");

            // create all scaled bitmaps and extract the pixels
            TransformedBitmap[] transformedBitmaps = CreateSubScaledTxtrs(subCount, bitmapSource);
            byte[][] subImagPixelData = ExtractSubScaledTxtrsData(transformedBitmaps);

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Creating sub image forms...");
            // buld each 'FORM' with the image and pixels data
            SubImagForm[] subImags = new SubImagForm[subCount];
            for (int i = 0; i < subCount; i++)
            {
                subImags[i] = new SubImagForm(
                    (i == 0 ? "C:\\Exported\\From\\Relic_IC_Image_Parser\\By\\MightySarion.bmp\0" : "\0"),
                    1,
                    new int[] { 0, transformedBitmaps[i].PixelWidth, transformedBitmaps[i].PixelHeight, subImagPixelData[i].Length},
                    0,
                    subImagPixelData[i]
                    );
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Sub image form #" + (i + 1));
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Total of " + subImags.Length + " sub image forms");
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
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Sub scaling textures...");

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

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Total of " + transformedBitmaps.Length + " Sub scaled textures");
            return transformedBitmaps;
        }

        /// <summary>
        /// Takes all the scaled bitmaps and the largest one, and extract the pixels from them.
        /// </summary>
        /// <param name="transformedBitmaps">The list of all the bitmaps.</param>
        /// <returns>Two dimentional array that represents the raw pixels for each bitmap.</returns>
        private static byte[][] ExtractSubScaledTxtrsData(TransformedBitmap[] transformedBitmaps)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Extracting sub scaled texture's pixels...");

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
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Extraction #" + (i + 1));
            }

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Total of " + subTxtrs.Length + " sub scaled texture's pixel data were extracted");
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

        /// <summary>
        /// For SPT there is a need for rectangle clipping inside the canvas of the sub images.
        /// This class is meant to answer that need, by having the subImage name to link to,
        /// the positioning data in the complete image and the clipping data for its own canvas.
        /// </summary>
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
