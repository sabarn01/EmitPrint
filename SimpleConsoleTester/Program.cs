using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleTester
{
    public class foo
    {
        public int A { get; set; }
        public string B { get; set; }
    }

    public class Comp
    {
        public int X { get; set; }

        public foo myFoo { get; set;  }
    }
    class Program
    {
        static void Main(string[] args)
        {
            EmitPrintLib.EmitPrinter ep = new EmitPrintLib.EmitPrinter("Test.dll");
            var cmp = new Comp() { X = 11, myFoo = new foo { A = 3, B = "omygad" } };
            var str = ep.EmitPrintMessage(cmp);
            Console.WriteLine(str);
            str = Console.ReadLine(); 

        }
    }
}
