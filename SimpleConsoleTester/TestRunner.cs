using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleTester
{
    static class TestRunner
    {
        static void RunBatch(int BatchSize, Action<int> methodUnderTest)
        {
            for (int x = 0; x < BatchSize; x++)
            {
                methodUnderTest(x);
            }
        }

        public static TestResult RunTest(string description, int BatchSize, int NumberOfBatches, Action<int> MethodUnderTest)
        {
            Console.WriteLine("Running Test " + description + " in batches of " + BatchSize + " " + NumberOfBatches + " Times"  );
            var consoleY = Console.CursorTop;
            var width = Console.BufferWidth;
            List<double> Times = new List<double>();

            for (int x = 0; x < NumberOfBatches; x++)
            {
                PrintStatus(consoleY, x, NumberOfBatches, width);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                RunBatch(BatchSize, MethodUnderTest);
                sw.Stop();
                Times.Add(sw.Elapsed.TotalMilliseconds);
            }
            GC.Collect(2);
            return new TestResult(description,BatchSize,NumberOfBatches, Times);

        }

        private static void PrintStatus(int ConsleLine, int x, int NumberOfBatches, int width)
        {
            Console.SetCursorPosition(0, ConsleLine);
            var len = width * (x / (double)NumberOfBatches);
            string Status = new string('*', (int)len);
            Console.WriteLine(Status);
        }
        
    }
}
