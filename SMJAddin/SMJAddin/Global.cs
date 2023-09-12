using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMJAddin
{
    public static class Global
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }


        public static string ApplicationPath = "C:\\Program Files\\Vormadal Brothers\\SMJAddin";
        public static string Modifiable = "";


        public static T MostCommon<T>(this IEnumerable<T> list)
        {
            var most = (from i in list
                        group i by i into grp
                        orderby grp.Count() descending
                        select grp.Key).First();
            return most;
        }
    }
}
