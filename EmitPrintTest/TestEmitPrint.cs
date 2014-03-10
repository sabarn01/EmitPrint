using EmitPrintLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace EmitPrintTest
{
    internal class NoToString
    {
        public int x { get; set; }
        public string y {get;set;}
    }
    
    internal class HasToString
    {
        public const string IHAS = "I HAS TO STRING";
        public int x { get; set; }
        public string y {get;set;}
        public override string ToString()
        {
 	         return IHAS; 
        }
    }

    [TestClass]
    public class TestEmitPrint
    {
        [TestMethod]
        public void WhenNullPassedInNullMessage()
        {
            NoToString TestObj = null;
            EmitPrinter Printer = new EmitPrinter("TestFoo");
            var res = Printer.EmitPrintMessage<NoToString>(null);
            Assert.AreEqual("{ NULL }", res); 
        }

        [TestMethod]
        public void WhenObjectHasToStrhing_CallsObjectsOverloadMethod()
        {
            var NTS = new HasToString();
            Assert.AreEqual(HasToString.IHAS, NTS.ToString()); 
        }

        [TestMethod]
        public void WhenObjectDoesNotHaveToString_PropertiesArePrinted()
        {
            const string Expected = "{x = {4} y = { test } }";
            var obj = new NoToString() { x = 4, y = "test" };
            var res = (new EmitPrinter("TestFoo")).EmitPrintMessage < NoToString>(obj); 
            Assert.AreEqual(Expected, obj); 
        }
    }
}
