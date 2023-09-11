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
using Autodesk.Revit.DB.Mechanical;

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class MoveSpaceLocationToCenter : IExternalCommand
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

            using (var tx = new Transaction(doc))
            {
                tx.Start("Moving Rooms");

                var spaces = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces);
                
                View3D view = ViewMethods.CreateViewForRay(doc);

                foreach (Space room in spaces)
                {
                    if (room == null || room.Area == 0) continue;

                    SpaceMethods.TryMoveSpaceLocationToCenter(room, view);
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
