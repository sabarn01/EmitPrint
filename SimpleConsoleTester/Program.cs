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
    class Program
    {
        static void Main(string[] args)
        {
            EmitPrintLib.EmitPrinter ep = new EmitPrintLib.EmitPrinter("Test.dll");
            var str = ep.EmitPrintMessage(new foo{A=3,B= "omygad"});
            Console.WriteLine(str);
            str = Console.ReadLine(); 

        }
    }
}
