﻿using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;

namespace SMJAddin
{
    public static class SpaceMethods
    {
        public static Space TryMoveSpaceLocationToCenter(Space space)
        {
            return TryMoveSpaceLocationToCenter(space, ViewMethods.CreateViewForRay(space.Document));
        }

        public static Space TryMoveSpaceLocationToCenter(Space space, View3D view)
        {
            MoveSpaceLocationToCentroid(space.Document, space);
            Space output = AlignSpaceXY(space, view);

            return output;
        }


        public static Space AlignSpaceXY(Space space, View3D view)
        {
            var viewForRay = new ReferenceIntersector(view);
            viewForRay.FindReferencesInRevitLinks = true;

            XYZ spaceLocation = (space.Location as LocationPoint).Point;

            var Right = viewForRay.FindNearest(spaceLocation, new XYZ(1, 0, 0)).Proximity;
            var Left = viewForRay.FindNearest(spaceLocation, new XYZ(-1, 0, 0)).Proximity;

            var up = viewForRay.FindNearest(spaceLocation, new XYZ(0, 1, 0)).Proximity;
            var down = viewForRay.FindNearest(spaceLocation, new XYZ(0, -1, 0)).Proximity;

            double halfOfTotalX = Left - ((Right + Left) / 2);
            double halfOfTotalY = down - ((up + down) / 2);

            XYZ newPoint = new XYZ(spaceLocation.X - halfOfTotalX, spaceLocation.Y - halfOfTotalY, spaceLocation.Z + (space.UnboundedHeight / 2));

            if (space.IsPointInSpace(newPoint))
            {
                XYZ translation = newPoint.Subtract(spaceLocation);
                space.Location.Move(translation);
            }

            return space;
        }


        public static Space AlignSpaceX(Space space, View3D view)
        {
            var viewForRay = new ReferenceIntersector(view);
            viewForRay.FindReferencesInRevitLinks = true;

            XYZ spaceLocation = (space.Location as LocationPoint).Point;

            var Right = viewForRay.FindNearest(spaceLocation, new XYZ(1, 0, 0)).Proximity;
            var Left = viewForRay.FindNearest(spaceLocation, new XYZ(-1, 0, 0)).Proximity;

            double halfOfTotal = Left - ((Right + Left) / 2);

            XYZ newPoint = new XYZ(spaceLocation.X - halfOfTotal, spaceLocation.Y, spaceLocation.Z + (space.UnboundedHeight / 2));

            if (space.IsPointInSpace(newPoint))
            {
                XYZ translation = newPoint.Subtract(spaceLocation);
                space.Location.Move(translation);
            }


            return space;
        }

        public static Space AlignSpaceY(Space space, View3D view)
        {
            var viewForRay = new ReferenceIntersector(view);
            viewForRay.FindReferencesInRevitLinks = true;

            XYZ roomLocation = (space.Location as LocationPoint).Point;

            var up = viewForRay.FindNearest(roomLocation, new XYZ(0, 1, 0)).Proximity;
            var down = viewForRay.FindNearest(roomLocation, new XYZ(0, -1, 0)).Proximity;

            double halfOfTotal = down - ((up + down) / 2);

            XYZ newPoint = new XYZ(roomLocation.X, roomLocation.Y - halfOfTotal, roomLocation.Z + (space.UnboundedHeight / 2));

            if (space.IsPointInSpace(newPoint))
            {
                XYZ translation = newPoint.Subtract(roomLocation);
                space.Location.Move(translation);
            }


            return space;
        }


        public static Space MoveSpaceLocationToCentroid(Document doc, Space space)
        {
            if (space == null || space.Area == 0)
            {
                return null;
            }

            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(space);

            Solid roomSolid = results.GetGeometry();
            PlanarFace planar = null;

            foreach (Face face in roomSolid.Faces)
            {
                PlanarFace pf = face as PlanarFace;
                if (pf != null)
                {
                    if (pf.FaceNormal.Z == -1)
                    {
                        planar = pf;
                        break;
                    }
                }
            }

            if (planar == null)
            {
                return null;
            }

            EdgeArray edges = planar.EdgeLoops.get_Item(0);
            List<XYZ> test = new List<XYZ>();

            foreach (Edge edge in edges)
            {
                test.Add(edge.AsCurve().GetEndPoint(0));
                test.Add(edge.AsCurve().GetEndPoint(1));
            }

            var maxX = test.Max(i => i.X);
            var maxY = test.Max(i => i.Y);
            var maxZ = test.Max(i => i.Z);

            var minX = test.Min(i => i.X);
            var minY = test.Min(i => i.Y);
            var minZ = test.Min(i => i.Z);

            var x = (maxX + minX) / 2;
            var y = (maxY + minY) / 2;
            var z = (maxZ + minZ) / 2;

            XYZ roomLocation = (space.Location as LocationPoint).Point;
            XYZ centeroid = new XYZ(x, y, z);

            if (space.IsPointInSpace(centeroid))
            {
                XYZ translation = centeroid.Subtract(roomLocation);
                space.Location.Move(translation);
            }

            return space;
        }


        public static Space CreateOrUpdateSpaceFromRoomInLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            string name = roomFromLinkedFile.LookupParameter("Name").AsValueString();
            string number = roomFromLinkedFile.LookupParameter("Number").AsValueString();
            Space spaceAtPoint = WhatSpaceHasPoint(currentDoc, roomFromLinkedFile);

            if (spaceAtPoint != null)
            {
                spaceAtPoint.Name = name;
                spaceAtPoint.Number = number;
            }
            else
            {
                CreateSpaceFromRoomInLinkedFile(currentDoc, roomFromLinkedFile);
            }


            return null;
        }


        public static Space CreateSpaceFromRoomInLinkedFile(Document currentDoc, Room roomFromLinkedFile)
        {
            Level levelInLinkedModel = roomFromLinkedFile.Level;
            Level theChosenLevel = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, levelInLinkedModel, roomFromLinkedFile.BaseOffset);

            double theDifference = levelInLinkedModel.Elevation - theChosenLevel.Elevation;

            if (theChosenLevel != null)
            {
                XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
                Space createdSpace = currentDoc.Create.NewSpace(theChosenLevel, new UV(xyzPoint.X, xyzPoint.Y));

                string name = roomFromLinkedFile.LookupParameter("Name").AsValueString();
                string number = roomFromLinkedFile.LookupParameter("Number").AsValueString();

                createdSpace.Name = name;
                createdSpace.Number = number;

                Level upperLevelLimitInLinked = roomFromLinkedFile.UpperLimit;
                double limitOffsetInLinked = roomFromLinkedFile.LimitOffset;
                double baseOffset = roomFromLinkedFile.BaseOffset + theDifference;

                createdSpace.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(baseOffset);

                if (upperLevelLimitInLinked != null)
                {
                    Level upperLevelInCurrent = LevelMethods.GetLevelInCurrentThatMatchesLinkedLevel(currentDoc, upperLevelLimitInLinked, limitOffsetInLinked);

                    if (upperLevelInCurrent != null)
                    {
                        limitOffsetInLinked += upperLevelLimitInLinked.Elevation - upperLevelInCurrent.Elevation;
                        Parameter limitOffset = createdSpace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                        limitOffset.Set(limitOffsetInLinked);
                        createdSpace.UpperLimit = upperLevelInCurrent;
                    }
                    else
                    {
                        double limtThingy = limitOffsetInLinked + (upperLevelLimitInLinked.Elevation - theChosenLevel.Elevation);
                        createdSpace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(limtThingy);
                    }
                }
                else
                {
                    createdSpace.get_Parameter(BuiltInParameter.OFFSET_FROM_REFERENCE_BASE).Set(limitOffsetInLinked + theDifference);
                }

                return createdSpace;
            }
            return null;
        }


        public static Space WhatSpaceHasPoint(Document doc, Room roomFromLinkedFile)
        {
            XYZ xyzPoint = (roomFromLinkedFile.Location as LocationPoint).Point;
            double halfHeight = roomFromLinkedFile.UnboundedHeight / 2;
            List<XYZ> points = new List<XYZ>() { xyzPoint, new XYZ(xyzPoint.X, xyzPoint.Y, xyzPoint.Z + halfHeight) };
            Space spaceAtPoint = WhatSpaceHasPoint(doc, points);

            return spaceAtPoint;
        }

        public static Space WhatSpaceHasPoint(Document doc, List<XYZ> points)
        {
            Space test = null;
            foreach (XYZ point in points)
            {
                test = WhatSpaceHasPoint(doc, point);
                if (test != null)
                {
                    return test;
                }
            }

            return test;
        }

        public static Space WhatSpaceHasPoint(Document doc, XYZ point)
        {
            var spaces = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces);
            Space output = null;

            foreach (Space space in spaces)
            {
                if (space.IsPointInSpace(point))
                {
                    output = space;
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
