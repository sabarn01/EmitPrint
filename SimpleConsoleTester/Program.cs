using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleTester
{
 
    class Program
    {
        static string PrintTestResults(IEnumerable<TestResult> trList)
        {
            StringBuilder sb = new StringBuilder();
            var longestDescription = trList.Max((x) => x.Description.Count()) + 1;
            string fmtStr = String.Format("{{0,-{0}}}|{{1,-6}}|{{2,-6}}|{{3,-6}}|{{4,-7}}|{{5,-6}}", longestDescription);
            sb.AppendFormat(fmtStr,"Description","High","Mean","Low","Average","BatchSize");
            sb.AppendLine(); 
            foreach(var itm in trList)
            {
                
                sb.AppendFormat(fmtStr,
                    itm.Description, itm.High.ToString("#.##"), itm.Mean.ToString("#.##"), itm.Low.ToString("#.##"), itm.Average.ToString("#.##"),itm.BatchSize);
                sb.AppendLine(); 
            }
            return sb.ToString(); 
        }


        static dynamic  BuildComps(dynamic d,int x )
        {
            d.myFoo.A = -x;
            d.myFoo.B = x.ToString();
            d.X = x; 
            return d; 
        }

        static int AccessAMember (dynamic c )
        {
            return c.X + c.myFoo.A;
        }

        static int AccessAMember(Expression<Func<int>> ex )
        {
            return ex.Compile()(); 
        }

        static int AccessMember(Comp c)
        {
            return c.X + c.myFoo.A;
        }
        static int AccessMember(Comp2 c)        
        {
            return c.X + c.myFoo.A;
        }

        


        static void Main(string[] args)
        {
            const int BatchSize = 100;
            const int RunCount  = 10000;
            var comps2 = new List<Comp2>();
            for (int x = 0 ; x < 100 ; x++)
            {
                comps2.Add(BuildComps(new Comp2(),x));
            }
            var comps = new List<Comp>();
            for (int x = 0 ; x < 100 ; x++)
            {
                comps.Add(BuildComps(new Comp(),x));

            }
            
            var compwithoverloadAct = new Action<int>((x)=>
                {
                    var s = comps2[x % comps2.Count];
                    var y = x.ToString(); 
                }            
            );

            var compsReflection = new Action<int>((x)=>
                {
                    var s = comps[x % comps.Count];
                    var y = s.ExToString(); 
                }
            );

            var AccessThoughAnEx = new Action<int>((x)=>
                {
                    var s = comps[x % comps.Count];
                    AccessAMember(() => s.X + s.myFoo.A);
                }
            );


            var ep = new EmitPrintLib.EmitPrinter("test");
            var z = ep.EmitPrintMessage(comps[0]);
            Console.WriteLine(z); 
            var emitprint = new Action<int>((x)=>
                {
                   var s = comps[x % comps.Count];
                   ep.EmitPrintMessage(s);
                }
            );

            var dynAcMember = new Action<int>((x) =>
                {
                    object s = x % 2 == 0 ? (object)comps[x % comps.Count] : (object)comps2[x % comps2.Count];
                    AccessAMember(s);
                }
            );

            var staticAcMember = new Action<int>((x) =>
            {
                if(x % 2 == 0 )
                {
                    AccessMember(comps[x % comps.Count]);
                }
                else
                {
                    AccessMember(comps2[x % comps2.Count]);
                }                
            }
            );

            



            Console.Clear(); 
            List<TestResult> Results = new List<TestResult>();
            
            Results.Add(TestRunner.RunTest("Composit object with overload ", RunCount, BatchSize, compwithoverloadAct));
            Results.Add(TestRunner.RunTest("Composit object with overload ", RunCount * 10, BatchSize, compwithoverloadAct));
            Results.Add(TestRunner.RunTest("Comps without overload ", RunCount, BatchSize, compsReflection));
            Results.Add(TestRunner.RunTest("Comps without overload ", RunCount * 10, BatchSize, compsReflection));
            Results.Add(TestRunner.RunTest("Emit Print ", RunCount, BatchSize, emitprint));
            Results.Add(TestRunner.RunTest("Emit Print ", RunCount * 10, BatchSize, emitprint));
            Results.Add(TestRunner.RunTest("Access through an expression", RunCount, BatchSize, AccessThoughAnEx));
            Results.Add(TestRunner.RunTest("Access a dynamic method", RunCount, BatchSize, dynAcMember));
            Results.Add(TestRunner.RunTest("access a static method", RunCount, BatchSize, staticAcMember));

            var reslutsTable = PrintTestResults(Results);
            Console.WriteLine(reslutsTable); 

            if(args.Count() > 0 )
            {
                File.WriteAllText(args[0], reslutsTable); 
            }

            if(System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Done");
                string str; 
                str = Console.ReadLine(); 
            }

        }
    }
}
