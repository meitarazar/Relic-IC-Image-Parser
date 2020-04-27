using System;
using System.Text;

namespace Relic_IC_Image_Parser
{
    partial class RelicTag
    {
        public readonly string name;
        public readonly int length;
        public readonly byte[] data;

        public RelicTag(string name, int length, byte[] data)
        {
            this.name = name;
            this.length = length;
            if (data != null)
            {
                this.data = (byte[])data.Clone();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}): {2}", name, length, Encoding.ASCII.GetString(data));
        }
    }
}
