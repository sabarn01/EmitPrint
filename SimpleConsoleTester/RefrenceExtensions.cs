using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleTester
{
    public static class RefrenceExtensions
    {
        private static void AppendProperty(this IList list, StringBuilder sb, bool recurse)
        {
            if (list.Count < 5)
            {

            }
        }
        private static void AppendProperty(this IDictionary dict, StringBuilder sb, bool recurse)
        {
            if (dict.Count < 5)
            {

            }
        }
        /// <summary>
        /// this function checks to see if the object has a to string implemntation
        /// other wise it calls appendproperties 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="sb"></param>
        private static void AppendProperty(this object o, StringBuilder sb, bool recurse = true)
        {
            if (HasToString(o.GetType()))
            {
                sb.Append(o);
            }
            else
            {
                AppendProperties(o, sb, recurse);
            }

        }


        private static void AppendProperties(this object o, StringBuilder sb, bool recurse = true)
        {
            bool firstPass = true;
            sb.Append("{");
            foreach (var prop in o.GetType().GetProperties())
            {
                if (!firstPass)
                {
                    sb.Append(";");
                }
                else
                {
                    firstPass = false;
                }
                var val = prop.GetValue(o);
                sb.Append(prop.Name).Append(" = ");
                if (val == null)
                {
                    sb.Append("{null}");
                    continue;
                }

                var dict = val as IDictionary;

                if (dict != null)
                {
                    AppendProperty(dict, sb, recurse);
                    continue;
                }
                var list = val as IList;
                if (list != null)
                {
                    AppendProperty(list, sb, recurse);
                    continue;
                }
                AppendProperty(val, sb, recurse);
            }
            sb.Append("}");
        }

        private static bool HasToString(Type type)
        {
            var bflags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            return type.GetMethod("ToString", bflags, null, Type.EmptyTypes, null)
                != null;
        }
        public static string ExToString(this object o, string Name = null, bool recurse = true)
        {
            var sb = new StringBuilder();
            AppendProperty(o, sb);
            return sb.ToString();

        }
    }
}
