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

                        Room test = roomsCollector.First(i => ((Room)i).Number == "219") as Room;

                        var task = new TaskDialog("Bro");


                        double elevationInLinkedDocument = test.Level.Elevation;
                        Level theChosenLevel = null;

                        FilteredElementCollector LevelsInCurrentDocument = new FilteredElementCollector(doc).OfClass(typeof(Level));

                        double fyriYou = 200000;
                        int indx = 0;

                        for (int i = 0; i < LevelsInCurrentDocument.Count(); i++)
                        {
                            Level level = LevelsInCurrentDocument.ToList()[i] as Level;

                            double difference = Math.Abs(level.Elevation - elevationInLinkedDocument);

                            if (difference < fyriYou)
                            {
                                fyriYou = difference;
                                indx = i;
                            }

                            if (fyriYou == 0)
                            {
                                theChosenLevel = level;
                                break;
                            }
                        }

                        if (theChosenLevel == null && indx != -1)
                        {
                            theChosenLevel = LevelsInCurrentDocument.ToList()[indx] as Level;
                        }



                        if (theChosenLevel != null)
                        {
                            XYZ xyzPoint = (test.Location as LocationPoint).Point;
                            Room createdRoom = doc.Create.NewRoom(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));

                            string name = test.LookupParameter("Name").AsValueString();
                            createdRoom.Name = name;

                            string number = test.LookupParameter("Number").AsValueString();
                            createdRoom.Number = number;

                            Level upperLimitInLinked = test.UpperLimit;

                            double limitOffsetInLinked = test.LimitOffset;
                            double baseOffset = test.BaseOffset;

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
                                    double limtThingy = limitOffsetInLinked + (upperLimitInLinked.Elevation - theChosenLevel.Elevation);
                                    createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(limtThingy);
                                }
                            }
                            else
                            {
                                createdRoom.get_Parameter(BuiltInParameter.OFFSET_FROM_REFERENCE_BASE).Set(limitOffsetInLinked);
                            }
                        }


                        task.Show();


                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
