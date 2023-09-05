using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;

namespace SMJAddin
{
    public static class Helper
    {

        public static Level GetLevelInCurrentThatMatchesLinkedLevel(Document currentDoc, Level levelInLinkedDocument)
        {
            return GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, levelInLinkedDocument, 0);
        }

        public static Level GetLevelInCurrentThatMatchesLinkedLevel(Document currentDoc, Level levelInLinkedDocument, double offset)
        {
            Level theChosenLevel = null;
            double elevationInLinkedDocument = levelInLinkedDocument.Elevation + offset;
            List<Level> levelsInCurrentDocument = new FilteredElementCollector(currentDoc).OfClass(typeof(Level)).Cast<Level>().ToList();

            double previousDifference = 200000;
            int chosenIndex = 0;

            for (int i = 0; i < levelsInCurrentDocument.Count(); i++)
            {
                Level level = levelsInCurrentDocument[i];
                double currentDifference = Math.Abs(elevationInLinkedDocument - level.Elevation);

                if (currentDifference < previousDifference)
                {
                    previousDifference = currentDifference;
                    chosenIndex = i;

                    if (previousDifference == 0)
                    {
                        theChosenLevel = level;
                        return theChosenLevel;
                    }
                }
            }

            if (chosenIndex != -1)
            {
                theChosenLevel = levelsInCurrentDocument[chosenIndex];
            }

            return theChosenLevel;
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
                    
                    if(that is View3D)
                    {
                        
                        var test = new ReferenceIntersector(that as View3D);
                        test.FindReferencesInRevitLinks = true;

                        var objStuff = test.Find(linkedRoomPoint, currentRoomPoint);

                        foreach(var obj in objStuff)
                        {
                            var eleId = obj.GetReference().ElementId;
                            var ele = currentDoc.GetElement(eleId);
                            if(ele is RevitLinkInstance instance)
                            {
                                eleId = obj.GetReference().LinkedElementId;
                                ele = instance.GetLinkDocument().GetElement(eleId);
                            }

                            double number = linkedRoomPoint.DistanceTo(obj.GetReference().GlobalPoint);
                            double number2 = linkedRoomPoint.DistanceTo(currentRoomPoint);

                            if(number < number2 && number != 0)
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


        public static Room WhatRoomHasPoint(Document doc, List<XYZ> points)
        {
            Room test = null;
            foreach(XYZ point in points)
            {
                test = WhatRoomHasPoint(doc, point);
                if(test != null)
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



    }
}
