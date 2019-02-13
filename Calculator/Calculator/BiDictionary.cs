using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class BiDictionary<TValue1, TValue2>
    {
        public List<TValue1> value1List = new List<TValue1>();
        public List<TValue2> value2List = new List<TValue2>();

        public TValue1 this[TValue2 Value2]
        {
            get
            {
                if (value2List.Contains(Value2))
                {
                    return value1List[value2List.IndexOf(Value2)];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                value1List[value2List.IndexOf(Value2)] = value;
            }
        }

        public TValue2 this[TValue1 Value1]
        {
            get
            {
                if (value1List.Contains(Value1))
                {
                    return value2List[value1List.IndexOf(Value1)];
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                value2List[value1List.IndexOf(Value1)] = value;
            }
        }

        public void Add(TValue1 value1, TValue2 value2)
        {
            value1List.Add(value1);
            value2List.Add(value2);
        }

        public void Remove(TValue1 value1)
        {
            value2List.RemoveAt(value1List.IndexOf(value1));
            value1List.Remove(value1);
        }

        public void Remove(TValue2 value2)
        {
            value1List.RemoveAt(value2List.IndexOf(value2));
            value2List.Remove(value2);
        }
        
        public bool Contains(TValue1 value1)
        {
            return value1List.Contains(value1);
        }

        public bool Contains(TValue2 value2)
        {
            return value2List.Contains(value2);
        }
    }
}
