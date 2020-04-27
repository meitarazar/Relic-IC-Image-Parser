namespace Relic_IC_Image_Parser
{
    partial class Coordinates
    {
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