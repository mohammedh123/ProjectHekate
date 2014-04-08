using System;

namespace ProjectHekate.Core.MathExtras
{
    public struct Vector<TNumericType>
    {
        public TNumericType X;
        public TNumericType Y;

        public Vector(TNumericType x, TNumericType y) : this()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }
    }
}
