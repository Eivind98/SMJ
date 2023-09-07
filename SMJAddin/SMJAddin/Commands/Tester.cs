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

            // Access current selection

            using (var tx = new Transaction(doc))
            {
                tx.Start("Moving Rooms");

                Room room = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).First(i => i.Id.IntegerValue == 857279) as Room;

                //RoomMethods.MoveRoomLocationToCentroid(doc, room);

                RoomMethods.AlignRoomXY(room, ViewMethods.CreateViewForRay(doc));

                //RoomMethods.TryMoveRoomLocationToCenter(room);


                tx.Commit();
            }


            return Result.Succeeded;
        }
    }
}
