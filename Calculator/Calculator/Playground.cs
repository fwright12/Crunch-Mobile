using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculator;

namespace Playground
{
    public class Playground
    {
        private Action test;

        public Playground()
        {
            test = delegate
            {
                print.log("H");
            };
        }
    }
}
