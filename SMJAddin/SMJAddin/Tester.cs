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

                    Room roomInLinkedModel = roomsCollector.First(i => ((Room)i).Number == "335") as Room;

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Creating Rooms");

                        if (roomInLinkedModel != null && roomInLinkedModel.Area != 0)
                        {
                            XYZ xyzPoint = (roomInLinkedModel.Location as LocationPoint).Point;
                            string name = roomInLinkedModel.LookupParameter("Name").AsValueString();
                            string number = roomInLinkedModel.LookupParameter("Number").AsValueString();

                            TaskDialog task = new TaskDialog("yo");
                            task.MainContent = roomInLinkedModel.UnboundedHeight.ToString() + " - " + xyzPoint.X.ToString();
                            task.Show();


                            Room roomThatIsSimilar = Helper.WhatRoomIsAlmostTheSame(doc, roomInLinkedModel);

                            if (roomThatIsSimilar != null)
                            {
                                roomThatIsSimilar.Name = name;
                                roomThatIsSimilar.Number = number;
                            }
                            else
                            {
                                Level levelInLinkedModel = roomInLinkedModel.Level;
                                Level theChosenLevel = Helper.GetLevelInCurrentThatMatchesLinkedLevel(doc, levelInLinkedModel, roomInLinkedModel.BaseOffset);

                                double theDifference = levelInLinkedModel.Elevation - theChosenLevel.Elevation;

                                if (theChosenLevel != null)
                                {
                                    Room createdRoom = doc.Create.NewRoom(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));

                                    createdRoom.Name = name;
                                    createdRoom.Number = number;

                                    Level upperLevelLimitInLinked = roomInLinkedModel.UpperLimit;
                                    double limitOffsetInLinked = roomInLinkedModel.LimitOffset;
                                    double baseOffset = roomInLinkedModel.BaseOffset + theDifference;

                                    createdRoom.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(baseOffset);

                                    if (upperLevelLimitInLinked != null)
                                    {
                                        Level upperLevelInCurrent = Helper.GetLevelInCurrentThatMatchesLinkedLevel(doc, upperLevelLimitInLinked, limitOffsetInLinked);

                                        if (upperLevelInCurrent != null)
                                        {
                                            limitOffsetInLinked += upperLevelLimitInLinked.Elevation - upperLevelInCurrent.Elevation;
                                            Parameter limitOffset = createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                                            limitOffset.Set(limitOffsetInLinked);
                                            createdRoom.UpperLimit = upperLevelInCurrent;
                                        }
                                        else
                                        {
                                            double limtThingy = limitOffsetInLinked + (upperLevelLimitInLinked.Elevation - theChosenLevel.Elevation);
                                            createdRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(limtThingy);
                                        }
                                    }
                                    else
                                    {
                                        createdRoom.get_Parameter(BuiltInParameter.OFFSET_FROM_REFERENCE_BASE).Set(limitOffsetInLinked + theDifference);
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
