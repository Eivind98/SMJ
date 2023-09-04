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

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class CopyRoomsFromLinkedFile : IExternalCommand
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

            var sel = uidoc.Selection;

            if (sel != null)
            {
                var ele = doc.GetElement(sel.GetElementIds().First());
                if (ele != null && ele is RevitLinkInstance)
                {
                    RevitLinkInstance linkedDoc = ele as RevitLinkInstance;
                    FilteredElementCollector roomsCollector = new FilteredElementCollector(linkedDoc.GetLinkDocument()).OfCategory(BuiltInCategory.OST_Rooms);

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Creating Rooms");

                        foreach (Room ro in roomsCollector)
                        {
                            if (ro != null && ro.Area != 0)
                            {
                                double elevationInLinkedDocument = ro.Level.Elevation;
                                Level theChosenLevel = null;

                                FilteredElementCollector LevelsInCurrentDocument = new FilteredElementCollector(doc).OfClass(typeof(Level));

                                foreach (Level level in LevelsInCurrentDocument.ToList())
                                {
                                    if (level.Elevation == elevationInLinkedDocument)
                                    {
                                        theChosenLevel = level;
                                        break;
                                    }
                                }

                                if (theChosenLevel != null)
                                {
                                    XYZ xyzPoint = (ro.Location as LocationPoint).Point;
                                    Room createdRoom = doc.Create.NewRoom(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));
                                    
                                    string name = ro.LookupParameter("Name").AsValueString();
                                    createdRoom.Name = name;

                                    string number = ro.LookupParameter("Number").AsValueString();
                                    createdRoom.Number = number;

                                    Level upperLimitInLinked = ro.UpperLimit;

                                    double limitOffsetInLinked = ro.LimitOffset;
                                    double baseOffset = ro.BaseOffset;

                                    createdRoom.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(baseOffset);

                                    if (upperLimitInLinked != null)
                                    {
                                        Level upperLevelInCurrent = null;

                                        foreach (Level level in LevelsInCurrentDocument.ToList())
                                        {
                                            if (level.Elevation == upperLimitInLinked.Elevation)
                                            {
                                                upperLevelInCurrent = level;
                                                break;
                                            }
                                        }
                                        if (upperLevelInCurrent != null)
                                        {
                                            Parameter limitOffset = createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                                            limitOffset.Set(limitOffsetInLinked);
                                            createdRoom.UpperLimit = upperLevelInCurrent;
                                        }
                                        else
                                        {
                                            createdRoom.LimitOffset = limitOffsetInLinked + (upperLimitInLinked.Elevation - theChosenLevel.Elevation);
                                        }
                                    }
                                    else
                                    {
                                        createdRoom.get_Parameter(BuiltInParameter.OFFSET_FROM_REFERENCE_BASE).Set(limitOffsetInLinked);
                                    }
                                }
                            }
                        }

                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
