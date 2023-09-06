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

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class MoveTagsToRoomLocation : IExternalCommand
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

            FilteredElementCollector allRoomTagsInView = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_RoomTags);
            if (allRoomTagsInView != null || allRoomTagsInView.Count() == 0)
            {
                foreach ( RoomTag tag in allRoomTagsInView)
                {
                    XYZ tagLocation = (tag.Location as LocationPoint).Point;
                    XYZ roomLocation = (tag.Room.Location as LocationPoint).Point;

                    XYZ translation = roomLocation.Subtract(tagLocation);

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Moving Tags");
                        tag.Location.Move(translation);
                        tx.Commit();
                    }

                }
            }

            return Result.Succeeded;
        }
    }
}
