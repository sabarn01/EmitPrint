using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleTester
{
    struct TestResult
    {
        public TestResult(string description,int batchSize,int batchCount, List<double> runs)
        {
            Description = description;
            runs.Sort();
            High = runs.Last();
            Low = runs.First();
            Mean = runs[runs.Count / 2];
            Average = runs.Average();
            BatchCount = batchCount;
            BatchSize = batchSize; 
        }

        public override string ToString()
        {
            string fmtStr = "Description = {0};BatchSize ={5},BatchCount = {6}; High = {1}, Mean = {2}, Low = {3}, Average = {4}";
            return string.Format(fmtStr, Description, High, Mean, Low, Average,BatchSize,BatchCount);
        }

        public int BatchSize;
        public int BatchCount; 
        public string Description;
        public double High;
        public double Mean;
        public double Low;
        public double Average;
    }
}
