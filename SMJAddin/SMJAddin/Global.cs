using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMJAddin
{

    public enum XOrY
    {
        XThenY = 0,
        YThenX = 1,
        SumOfXandY = 2,
    }

    public enum Alignment
    {
        Left = 0,
        Right = 1,
        Center = 2,
    }

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
        public static Guid GUIDTriggerSheetName = new Guid("fafbf6b2-4c06-42d4-97c1-d1b4eb593eff");
        public static Guid GUIDTriggerSheetNumber = new Guid("dea3bf3d-74eb-42a1-a757-1539271fc0d0");
        public static Guid GUIDTriggerTitleBlockChanged = new Guid("124d2dea-8ec6-40f5-9bcb-f84d23ee3309");

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
