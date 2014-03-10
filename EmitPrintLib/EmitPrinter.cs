using EmitPrintLib.PrinterBuilders;
//#define foo 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EmitPrintLib
{
    delegate string ObjPrinter(object o);
    delegate void   TypeOverride(StringBuilder sb);
    public class EmitPrinter
    {
        readonly AsseblyBuilderUtil _asmUtil = null;
        readonly IEmitTypeCache _typeCache = null; 
        public EmitPrinter(string AssemblyName):
            this(AssemblyName,new EmitTypeCache())
        {
            
        }

        public EmitPrinter(string AssemblyName,IEmitTypeCache cache)
            
        {
            _typeCache = cache;
            _asmUtil = new AsseblyBuilderUtil("PrintTest");
        }
        
        public string EmitPrintMessage<T>(T obj)
        {
            var theType = typeof(T); 
            if (!_typeCache.TypeEmited(theType))
            {
                var emiter = TypeEmiter.Emit<T>(_asmUtil.MainModlue, _typeCache);
                _typeCache.AddPrinter(emiter); 
                _asmUtil.Save(); 
            }
            var tPrinter = _typeCache.GetPrinter<T>();             
            if (tPrinter != null)
            {
                return tPrinter.FormatObject(obj, _typeCache);
            }
            else
            {
                return obj.ToString();
            }
        }   
        
    }
}
