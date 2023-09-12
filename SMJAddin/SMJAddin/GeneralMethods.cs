using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;

namespace SMJAddin
{
    public static class GeneralMethods
    {

        public static ICollection<Element> GetSimilarInstances(Element instance)
        {
            Document doc = instance.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector = collector.OfClass(typeof(FamilySymbol));

            var query = from element in collector where element.Name == instance.Name select element;
            List<Element> famSyms = query.ToList<Element>();
            ElementId symbolId = famSyms[0].Id;

            FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, symbolId);

            collector = new FilteredElementCollector(doc);
            return collector.WherePasses(filter).ToElements();
        }


    }
}
