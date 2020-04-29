using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Relic_IC_Image_Parser.Coordinates;
using static Relic_IC_Image_Parser.RelicImage;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// A bit complex, but does the job of making sense of the relic image files.
    /// </summary>
    partial class RelicParser
    {
        /// <summary>
        /// Open the file to a readable stream.
        /// </summary>
        /// <param name="fullFileName">The file to open.</param>
        /// <returns>The opened stream of the file.</returns>
        public static FileStream OpenStream(string fullFileName)
        {
            //return new StreamReader(File.OpenRead(fullFileName));
            return File.OpenRead(fullFileName);
        }

        /// <summary>
        /// Determining if it is a relic file by checking the top 'FORM' tag of the file,
        /// plus, if we don't get an unknown tag that is seen as null.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <returns>True for valid image, and False otherwise.</returns>
        public static bool IsRelicFile(FileStream stream)
        {
            RelicTag tag = ReadTag(stream, true);
            if (tag == null)
            {
                return false;
            }

            if ("FORM".Equals(tag.name) && "NOBS".Equals(Encoding.ASCII.GetString(tag.data)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Run through the whole file and convert it to manageable tags.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <returns>A list of all the file's tags.</returns>
        public static List<RelicTag> ReadAllTags(FileStream stream)
        {
            List<RelicTag> relicTags = new List<RelicTag>();
            while (stream.Position != stream.Length)
            {
                relicTags.Add(ReadTag(stream, false));
            }
            return relicTags;
        }

        /// <summary>
        /// Check if there is at leat one null tag in the list.
        /// </summary>
        /// <param name="tags">The tags list to check.</param>
        /// <returns>True if there is at leat one null tag, False otherwise.</returns>
        public static bool HasNullTags(List<RelicTag> tags)
        {
            foreach (RelicTag tag in tags)
            {
                if (tag == null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determine the difference between SPT and TXR by the 'RECT' tag that appears only in SPTs.
        /// </summary>
        /// <param name="tags">The tags list to check.</param>
        /// <returns>The type of this represented relic image.</returns>
        public static ImageType AnalyzeTags(List<RelicTag> tags)
        {
            foreach (RelicTag tag in tags)
            {
                if ("RECT".Equals(tag.name))
                {
                    return ImageType.SPT;
                }
            }
            return ImageType.TXR;
        }

        /// <summary>
        /// This is where the magic happens.
        /// <para>We look for specific tags along the way while reading the stream, and inserting them to the
        /// <see cref="RelicSubImage"/> object that holds the current sub image.</para>
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<RelicSubImage> ParseImage(List<RelicTag> tags)
        {
            bool consumeSubImageData = false;
            List<RelicSubImage> subImages = new List<RelicSubImage>();

            foreach (RelicTag tag in tags)
            {
                // in case if 'IMAGNAME' tag
                if ("IMAGNAME".Equals(tag.name))
                {
                    // convert the data to a string, because its a name
                    string imageFullName = Encoding.ASCII.GetString(tag.data).Replace("\0", "");
                    subImages.Add(new RelicSubImage(imageFullName));

                    // mark that we are at the start of a new sub image
                    consumeSubImageData = true;
                }

                // in case of 'ATTR' tag and we are inside sub image 'FORM'
                else if (consumeSubImageData && "ATTR".Equals(tag.name))
                {
                    // we take only the size of the image from this tag
                    int width = BitConverter.ToInt32(tag.data, 4);
                    int height = BitConverter.ToInt32(tag.data, 8);

                    subImages.ElementAt(subImages.Count - 1).SetCanvasSize(width, height);
                }

                // in case of 'DATA' tag and we are inside sub image 'FORM'
                else if (consumeSubImageData && "DATA".Equals(tag.name))
                {
                    // just take the data as is
                    subImages.ElementAt(subImages.Count - 1).SetCanvasData(tag.data);

                    consumeSubImageData = false;
                }

                // in case of 'RECT' tag
                else if ("RECT".Equals(tag.name))
                {
                    byte[] rectData = tag.data;

                    // in the 'RECT' tag first comes the name
                    int nameLength = BitConverter.ToInt32(rectData, 0);
                    string name = Encoding.ASCII.GetString(rectData, 4, nameLength);

                    // then comes the position and clipping data
                    for (int i = 0; i < subImages.Count; i++)
                    {
                        // find the matching sub image by name
                        RelicSubImage image = subImages[i];
                        if (image.GetImageName().Equals(name))
                        {
                            double[] doubleData = new double[4];
                            for (int j = nameLength + 4; j < rectData.Length; j += 4)
                            {
                                // we are taking the floats one by one and adding them to the matching place
                                int index = (j - nameLength - 4) / 4;
                                byte[] arr = new byte[4];

                                // copy next 4 bytes of float
                                Array.Copy(rectData, j, arr, 0, 4);

                                // put it inside the data array in the matching place
                                doubleData[index % 4] = BitConverter.ToSingle(rectData, j);

                                // if we are at the end of the data array, according to the index we are in
                                //   indert the data to the matching place
                                if (index % 4 == 3)
                                {
                                    if (index < 4)
                                    {
                                        image.SetPosPercent(new Rect(doubleData));
                                    }
                                    else
                                    {
                                        image.SetClipPercent(new Rect(doubleData));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return subImages;
        }

        /// <summary>
        /// Convert the next bytes in the stream to a relic's tag.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="forceDataRead">If we must read the data from the tag.</param>
        /// <returns>The relic tag from the stream.</returns>
        private static RelicTag ReadTag(FileStream stream, bool forceDataRead)
        {
            // checking we have a valid tage name
            string tagName = ReadTagName(stream);
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            // reading tag length
            int tagLength = ReadTagLength(stream);

            // taking tag data if we must or if we are not dealing with
            //   a 'FORM' tag or a 'PICTFORM' tag
            byte[] tagData = null;
            if (forceDataRead || (!"FORM".Equals(tagName) && !"PICTFORM".Equals(tagName)))
            {
                tagData = ReadTagData(stream, tagLength);
            }

            return new RelicTag(tagName, tagLength, tagData);
        }

        /// <summary>
        /// Reads the next bytes in the stream and converting them to a tag name.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The tag name.</returns>
        private static string ReadTagName(FileStream stream)
        {
            // reading the next four bytes
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            // converting to string
            string tag = Encoding.ASCII.GetString(buffer);

            // if we know that this tag is 8 chars long we are appending another four chars
            if ((new string[] { "PICT", "TXTR", "IMAG" }).Contains(tag))
            {
                tag += ReadTagName(stream);
            }

            // return the tag if we know it
            if ((new string[] { "FORM", "PICTFORM", "NAME", "TXTRNAME", "IMAGNAME", "VERS", "DATA", "ATTR", "RECT" }).Contains(tag))
            {
                return tag;
            }

            return "";
        }

        /// <summary>
        /// Reads the next bytes for the tag data length.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <returns>The length of the tag.</returns>
        private static int ReadTagLength(FileStream stream)
        {
            // takes the next four bytes and reverse them
            //   (relic reverse the length)
            byte[] buffer = ReadTagData(stream, 4);
            Array.Reverse(buffer);

            // converts to int 32 bit / 4 bytes
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// Reads the requested length of bytes from the stream.
        /// </summary>
        /// <param name="stream">The file stream to read from.</param>
        /// <param name="length">The amount of bytes to read.</param>
        /// <returns>The requested array of bytes from the stream.</returns>
        private static byte[] ReadTagData(FileStream stream, int length)
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Convert.ToByte(stream.ReadByte());
            }
            return buffer;
        }
    }
}
