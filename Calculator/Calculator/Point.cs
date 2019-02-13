using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class Point
    {
        public float x;
        public float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point pos)
        {
            x = pos.x;
            y = pos.y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
