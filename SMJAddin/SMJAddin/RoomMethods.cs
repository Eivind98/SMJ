using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace SMJAddin
{
    public static class RoomMethods
    {
        public static Room CreateOrUpdateRoomFromLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            string name = roomFromLinkedFile.LookupParameter("Name").AsValueString();
            string number = roomFromLinkedFile.LookupParameter("Number").AsValueString();
            Room roomAtPoint = WhatRoomHasPoint(currentDoc, roomFromLinkedFile);

            if (roomAtPoint != null)
            {
                roomAtPoint.Name = name;
                roomAtPoint.Number = number;
            }
            else
            {
                CreateRoomFromLinkedFile(currentDoc, roomFromLinkedFile);
            }


            return null;
        }


        public static Room CreateRoomFromLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            Level levelInLinkedModel = roomFromLinkedFile.Level;
            Level theChosenLevel = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, levelInLinkedModel, roomFromLinkedFile.BaseOffset);

            double theDifference = levelInLinkedModel.Elevation - theChosenLevel.Elevation;

            if (theChosenLevel != null)
            {
                XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
                Room createdRoom = currentDoc.Create.NewRoom(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));

                createdRoom.Name = roomFromLinkedFile.Name;
                createdRoom.Number = roomFromLinkedFile.Number;

                Level upperLevelLimitInLinked = roomFromLinkedFile.UpperLimit;
                double limitOffsetInLinked = roomFromLinkedFile.LimitOffset;
                double baseOffset = roomFromLinkedFile.BaseOffset + theDifference;

                createdRoom.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(baseOffset);

                if (upperLevelLimitInLinked != null)
                {
                    Level upperLevelInCurrent = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, upperLevelLimitInLinked, limitOffsetInLinked);

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

            return null;
        }


        public static Room WhatRoomHasPoint(Document doc, Room roomFromLinkedFile)
        {
            XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
            double halfHeight = roomFromLinkedFile.UnboundedHeight / 2;
            List<XYZ> points = new List<XYZ>() { xyzPoint, new XYZ(xyzPoint.X, xyzPoint.Y, xyzPoint.Z + halfHeight) };
            Room roomAtPoint = WhatRoomHasPoint(doc, points);

            return roomAtPoint;
        }

        public static Room WhatRoomHasPoint(Document doc, List<XYZ> points)
        {
            Room test = null;
            foreach (XYZ point in points)
            {
                test = WhatRoomHasPoint(doc, point);
                if (test != null)
                {
                    return test;
                }
            }

            return test;
        }

        public static Room WhatRoomHasPoint(Document doc, XYZ point)
        {
            var rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            Room output = null;

            foreach (Room room in rooms)
            {
                if (room.IsPointInRoom(point))
                {
                    output = room;
                }
            }

            return output;
        }

        public static bool IsPointWithinARoom(Document doc, XYZ point)
        {
            var rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Room room in rooms)
            {
                if (room.IsPointInRoom(point))
                {
                    return true;
                }
            }
            return false;
        }



        public static Room WhatRoomIsAlmostTheSame(Document currentDoc, Room roomFromLinkedFile)
        {
            var rooms = new FilteredElementCollector(currentDoc).OfCategory(BuiltInCategory.OST_Rooms);
            double variance = 0.10;
            double LinkedRoomArea = roomFromLinkedFile.Area;
            double curremtRoomArea;
            XYZ linkedRoomPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
            XYZ currentRoomPoint;

            Room output = WhatRoomHasPoint(currentDoc, linkedRoomPoint);

            if (output == null)
            {
                foreach (Room room in rooms.Cast<Room>())
                {
                    curremtRoomArea = room.Area;
                    currentRoomPoint = (room.Location as LocationPoint).Point;

                    bool test1 = Math.Abs(curremtRoomArea - LinkedRoomArea) <= (variance * LinkedRoomArea);
                    bool test2 = linkedRoomPoint.IsAlmostEqualTo(currentRoomPoint, variance);
                    bool test3 = true;

                    View that = currentDoc.ActiveView;

                    if (that is View3D)
                    {

                        var test = new ReferenceIntersector(that as View3D);
                        test.FindReferencesInRevitLinks = true;

                        var objStuff = test.Find(linkedRoomPoint, currentRoomPoint);

                        foreach (var obj in objStuff)
                        {
                            var eleId = obj.GetReference().ElementId;
                            var ele = currentDoc.GetElement(eleId);
                            if (ele is RevitLinkInstance instance)
                            {
                                eleId = obj.GetReference().LinkedElementId;
                                ele = instance.GetLinkDocument().GetElement(eleId);
                            }

                            double number = linkedRoomPoint.DistanceTo(obj.GetReference().GlobalPoint);
                            double number2 = linkedRoomPoint.DistanceTo(currentRoomPoint);

                            if (number < number2 && number != 0)
                            {
                                TaskDialog task = new TaskDialog("Yo");
                                task.MainContent = (currentDoc.GetElement(obj.GetReference().ElementId) as RevitLinkInstance).GetLinkDocument().GetElement(eleId).Name;
                                task.Show();

                                test3 = false;
                                break;
                            }

                        }

                    }

                    //using (Transaction tx = new Transaction(currentDoc, "Create 3D View"))
                    //{
                    //    tx.Start();

                    //    new3DView = View3D.CreateIsometric(currentDoc, (new FilteredElementCollector(currentDoc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(v => v.ViewFamily == ViewFamily.ThreeDimensional)).Id);

                    //    tx.Commit();
                    //}








                    if (test1 && test2 && test3)
                    {
                        output = room;
                    }
                }
            }

            return output;
        }


    }
}
