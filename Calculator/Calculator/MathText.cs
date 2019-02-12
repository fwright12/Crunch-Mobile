using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class MathText
    {
        public static MathText selected;

        public List<Symbol> text = new List<Symbol>() { Symbol.Cursor };
        public MathView mathView = new MathView();

        public int pos;

        public MathText()
        {

        }

        //Insert symbol where cursor is, indicate that it was the last added symbol
        public void Insert(Symbol sender)
        {
            Insert(pos, sender);
        }

        public void Insert(int index, Symbol sender)
        {
            text.Insert(index, sender);
            Input.adding.Add(sender);
            pos++;

            print.log("ewruoiuweroiuweoiruowier");
            foreach (Symbol s in Input.Parse(text))
                print.log(s.text);
        }
    }
}
