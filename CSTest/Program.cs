using EmitPrintLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSTest
{
    public class foo
    {
        public int x { get; set; }
        public int y { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            foo f = new foo{x=4,y = 19 };
            BaseEmitPrinter Printer = new BaseEmitPrinter();
            var msg = Printer.EmitPrintMessage(f);
            Console.WriteLine(msg);
            var val = Console.ReadLine();
        }
    }
}
