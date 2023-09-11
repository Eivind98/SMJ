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
    public class SpaceTagsEvenlyXthenY : IExternalCommand
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

                List< SpatialElementTag > tags = new List<SpatialElementTag>();

                foreach ( var eleid in eleIds )
                {
                    Element ele = doc.GetElement(eleid);
                    if ( ele != null && ele is SpatialElementTag)
                    {
                        tags.Add(ele as SpatialElementTag);
                    }
                }

                if ( tags.Count > 1 )
                {
                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Spacing Tags Evenly");

                        SpatialTagMethods.SpaceTagsEvenly(tags, XOrY.XThenY);

                        tx.Commit();
                    }

                }
                else
                {
                    TaskDialog dia = new TaskDialog("No Selection");
                    dia.MainContent = "There is one or no Tags selected. Please select Multiple Tags";
                    dia.Show();
                }

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
