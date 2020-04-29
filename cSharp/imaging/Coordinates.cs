namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// A simple class consisted of objects that used to hold data for image parsing
    /// </summary>
    partial class Coordinates
    {
        /// <summary>
        /// Basic class that holds the size of the image
        /// </summary>
        public class Size
        {
            public readonly int width;
            public readonly int height;

            public Size(int width, int height)
            {
                this.width = width;
                this.height = height;
            }

            public Size Clone()
            {
                return new Size(width, height);
            }
        }

        /// <summary>
        /// Basic class that holds the TopLeft and BottomRight corners of the rectangle clip of the image
        /// </summary>
        public class Rect
        {
            public readonly Point topLeft;
            public readonly Point bottomRight;

            public Rect(double[] relicFormat)
            {
                this.topLeft = new Point(relicFormat[0], relicFormat[2]);
                this.bottomRight = new Point(relicFormat[1], relicFormat[3]);
            }

            public Rect(Point topLeft, Point bottomRight)
            {
                this.topLeft = topLeft.Clone();
                this.bottomRight = bottomRight.Clone();
            }

            public Rect Clone()
            {
                return new Rect(topLeft, bottomRight);
            }

            /// <summary>
            /// Basic class that holds a position on the image canvas grid
            /// </summary>
            public class Point
            {
                public readonly double x;
                public readonly double y;

                public Point(double x, double y)
                {
                    this.x = x;
                    this.y = y;
                }

                public Point Clone()
                {
                    return new Point(x, y);
                }
            }
        }
    }
}