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
        public Comp()
        {
            myFoo = new foo();
        }
        public int X { get; set; }

        public foo myFoo { get; set; }
    }


    public class fooWithOverride
    {
        public int A { get; set; }
        public string B { get; set; }
        public override string ToString()
        {
            return string.Format("A = {0} ; B = {1}", A, B);
        }
    }

    public class Comp2
    {
        public Comp2()
        {
            myFoo = new fooWithOverride();
        }
        public int X { get; set; }

        public fooWithOverride myFoo { get; set; }

        public override string ToString()
        {
            return string.Format("X = {0} ; myFoo = {1}", X, myFoo);
        }
    }
}
