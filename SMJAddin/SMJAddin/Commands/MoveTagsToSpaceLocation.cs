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
using System.Collections;
using Autodesk.Revit.DB.Mechanical;

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class MoveTagsToSpaceLocation : IExternalCommand
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

            FilteredElementCollector allSpaceTagsInView = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_MEPSpaceTags);
            if (allSpaceTagsInView != null || allSpaceTagsInView.Count() == 0)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("Moving Tags");
                    foreach (SpaceTag tag in allSpaceTagsInView)
                    {
                        XYZ tagLocation = (tag.Location as LocationPoint).Point;
                        XYZ roomLocation = (tag.Space.Location as LocationPoint).Point;

                        XYZ translation = roomLocation.Subtract(tagLocation);

                        tag.Location.Move(translation);
                    }
                    tx.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
