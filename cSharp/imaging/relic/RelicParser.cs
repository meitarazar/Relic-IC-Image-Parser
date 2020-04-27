using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Relic_IC_Image_Parser.Coordinates;
using static Relic_IC_Image_Parser.RelicImage;

namespace Relic_IC_Image_Parser
{
    partial class RelicParser
    {
        public static FileStream OpenStream(string fullFileName)
        {
            //return new StreamReader(File.OpenRead(fullFileName));
            return File.OpenRead(fullFileName);
        }

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

        public static List<RelicTag> ReadAllTags(FileStream stream)
        {
            List<RelicTag> relicTags = new List<RelicTag>();
            while (stream.Position != stream.Length)
            {
                relicTags.Add(ReadTag(stream, false));
            }
            return relicTags;
        }

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

        public static List<RelicSubImage> ParseImage(List<RelicTag> tags)
        {
            bool consumeSubImageData = false;
            List<RelicSubImage> subImages = new List<RelicSubImage>();

            foreach (RelicTag tag in tags)
            {
                if ("IMAGNAME".Equals(tag.name))
                {
                    string imageFullName = Encoding.ASCII.GetString(tag.data).Replace("\0", "");
                    subImages.Add(new RelicSubImage(imageFullName));

                    consumeSubImageData = true;
                }
                else if (consumeSubImageData && "ATTR".Equals(tag.name))
                {
                    int width = BitConverter.ToInt32(tag.data, 4);
                    int height = BitConverter.ToInt32(tag.data, 8);

                    subImages.ElementAt(subImages.Count - 1).SetCanvasSize(width, height);
                }
                else if (consumeSubImageData && "DATA".Equals(tag.name))
                {
                    subImages.ElementAt(subImages.Count - 1).SetCanvasData(tag.data);

                    consumeSubImageData = false;
                }
                else if ("RECT".Equals(tag.name))
                {
                    byte[] rectData = tag.data;

                    int nameLength = BitConverter.ToInt32(rectData, 0);
                    string name = Encoding.ASCII.GetString(rectData, 4, nameLength);

                    for (int i = 0; i < subImages.Count; i++)
                    {
                        RelicSubImage image = subImages[i];
                        if (image.GetImageName().Equals(name))
                        {
                            double[] doubleData = new double[4];
                            for (int j = nameLength + 4; j < rectData.Length; j += 4)
                            {
                                int index = (j - nameLength - 4) / 4;
                                byte[] arr = new byte[4];
                                Array.Copy(rectData, j, arr, 0, 4);
                                doubleData[index % 4] = BitConverter.ToSingle(rectData, j);
                                if (index % 4 == 3)
                                {
                                    if (index < 4)
                                    {
                                        image.SetPosPercent(new Rect(doubleData));
                                        //Array.Copy(floatData, image.posData, 4);
                                    }
                                    else
                                    {
                                        image.SetClipPercent(new Rect(doubleData));
                                        //Array.Copy(floatData, image.clipData, 4);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return subImages;
        }

        private static RelicTag ReadTag(FileStream stream, bool forceDataRead)
        {
            string tagName = ReadTagName(stream);
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            int tagLength = ReadTagLength(stream);

            byte[] tagData = null;
            if (forceDataRead || (!"FORM".Equals(tagName) && !"PICTFORM".Equals(tagName)))
            {
                tagData = ReadTagData(stream, tagLength);
            }

            return new RelicTag(tagName, tagLength, tagData);
        }

        private static string ReadTagName(FileStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            string tag = Encoding.ASCII.GetString(buffer);

            if ((new string[] { "PICT", "TXTR", "IMAG" }).Contains(tag))
            {
                tag += ReadTagName(stream);
            }

            if ((new string[] { "FORM", "PICTFORM", "NAME", "TXTRNAME", "IMAGNAME", "VERS", "DATA", "ATTR", "RECT" }).Contains(tag))
            {
                return tag;
            }

            return "";
        }

        private static int ReadTagLength(FileStream stream)
        {
            byte[] buffer = ReadTagData(stream, 4);
            Array.Reverse(buffer);

            return BitConverter.ToInt32(buffer, 0);
        }

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
