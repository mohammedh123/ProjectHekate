using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Grammar.Implementation
{
    class Value
    {
        public enum Type
        {
            Integer,
            Float
        }

        private object _value;

        public int AsInteger()
        {
            return (int)_value;
        }

        public float AsFloat()
        {
            return (float) _value;
        }
    }
}
