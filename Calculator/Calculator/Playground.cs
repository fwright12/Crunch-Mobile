using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;

namespace Playground
{
    public class Symbol { }

        public class Expression : Symbol { }

        public class Operation : Symbol { }

            public class Fraction : Operation { }
            public class Exponent : Operation { }

        public class Constant : Symbol { }

        public abstract class Variable : Symbol { }

            public class Unknown : Variable { }

            public class Known : Variable { }

        public class Function : Symbol { }
}
