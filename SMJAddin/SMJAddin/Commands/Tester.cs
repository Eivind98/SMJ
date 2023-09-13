#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using System.Reflection.Emit;

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class Tester : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;

            var sel = uidoc.Selection;
            var eleIds = sel.GetElementIds();

            if (eleIds.Count != 0)
            {
                List<string> tags = new List<string>();

                List<FamilyInstance> familyInstances = new List<FamilyInstance>();

                var element = doc.GetElement(eleIds.FirstOrDefault());
                string name = element.Name;

                using (var tx = new Transaction(doc))
                {
                    tx.Start("Aligning tags to the Left");

                    
                    ViewMethods.CreateSheetAndFilters(element);
                    tx.Commit();
                }
                //21591114

                //TaskDialog task = new TaskDialog("Tester");
                //task.MainContent = string.Join(Environment.NewLine, tags);
                //task.Show();

            }
            else
            {
                TaskDialog dia = new TaskDialog("No Selection");
                dia.MainContent = "Nothing is selected. Please select Multiple Tags";
                dia.Show();
            }


            return Result.Succeeded;
        }
    }
}
