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

    public static class IndepententTagMethods
    {
        public static void TagFamily(FamilyInstance instance)
        {
            XYZ direction = instance.FacingOrientation.Normalize();
            double length = 1;
            Document doc = instance.Document;
            XYZ familyLocationPoint = (instance.Location as LocationPoint).Point;
            View view = doc.ActiveView;
            ElementId symbolId = GetMostCommonTagFromFamilyInstance(instance);
            TagOrientation orientation = TagOrientation.Vertical;

            if (Math.Round(direction.X, 2) != 0)
            {
                orientation = TagOrientation.Horizontal;
            }

            IndependentTag theTag = IndependentTag.Create(doc, symbolId, view.Id, new Reference(instance), false, orientation, familyLocationPoint);

            BoundingBoxXYZ tagBoundingBox = theTag.get_BoundingBox(view);

            switch (orientation)
            {
                case TagOrientation.Horizontal:
                    double lengthX = tagBoundingBox.Max.X - tagBoundingBox.Min.X;
                    length += lengthX/2;
                    break;
                case TagOrientation.Vertical:
                    double lengthY = tagBoundingBox.Max.Y - tagBoundingBox.Min.Y;
                    length += lengthY/2;
                    break;
            }

            XYZ endPoint = familyLocationPoint.Add(direction.Multiply(length));

            PlaceTagByMidpoint(theTag, endPoint);
        }

        public static void PlaceTagByMidpoint(IndependentTag tag, XYZ newMidpoint)
        {
            BoundingBoxXYZ box = tag.get_BoundingBox(tag.Document.ActiveView);
            TagOrientation orientation = tag.TagOrientation;
            double newX = newMidpoint.X;
            double newY = newMidpoint.Y;
            double newZ = box.Max.Z;

            if (orientation == TagOrientation.Horizontal)
            {
                double maxPointY = box.Max.Y;
                double minPointY = box.Min.Y;
                newY += (maxPointY - minPointY) / 2;
            }
            else if (orientation == TagOrientation.Vertical)
            {
                double maxPointX = box.Max.X;
                double minPointX = box.Min.X;
                newX -= (maxPointX - minPointX) / 2;
            }

            XYZ newHeadPoint = new XYZ(newX, newY, newZ);

            MoveTagHeadToPoint(tag, newHeadPoint);
        }

        public static XYZ PointFromFeetToMM(XYZ point)
        {
            double conversionactor = 304.8;
            return new XYZ(point.X * conversionactor, point.Y * conversionactor, point.Z * conversionactor);
        }

        public static ElementId GetMostCommonTagFromFamilyInstance(FamilyInstance instance)
        {
            Document doc = instance.Document;
            List<IndependentTag> independentTags = GetMostReleventTagsFromFamilyInstance(instance);

            IndependentTag mostCommon = independentTags.MostCommon();

            ElementId eleid = mostCommon.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId();
            FamilySymbol element = doc.GetElement(eleid) as FamilySymbol;

            return element.Id;
        }

        public static List<IndependentTag> GetMostReleventTagsFromFamilyInstance(FamilyInstance instance)
        {

            Document doc = instance.Document;
            string instanceCat = instance.Category.BuiltInCategory.ToString();
            if (instanceCat.EndsWith("s"))
            {
                instanceCat = instanceCat.Remove(instanceCat.Length - 1);
            }

            BuiltInCategory tagCategory = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), instanceCat + "Tags");
            var allDemTags = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(tagCategory);

            if (allDemTags.Count() == 0)
            {
                allDemTags = new FilteredElementCollector(doc).OfCategory(tagCategory);
            }

            List<IndependentTag> independentTags = new List<IndependentTag>();
            ICollection<Element> familyInstances = GeneralMethods.GetSimilarInstances(instance);
            List<ElementId> familyInstancesId = new List<ElementId>();

            foreach (Element familyInstance in familyInstances)
            {
                familyInstancesId.Add(familyInstance.Id);
            }

            foreach (var t in allDemTags)
            {
                if (t is IndependentTag tag)
                {
                    if (familyInstancesId.Contains(tag.GetTaggedElementIds().First().HostElementId))
                    {
                        independentTags.Add(tag);
                    }

                }
            }

            if (independentTags.Count() == 0)
            {
                allDemTags = new FilteredElementCollector(doc).OfCategory(tagCategory);
                foreach (var t in allDemTags)
                {
                    if (t is IndependentTag tag)
                    {
                        independentTags.Add(tag);
                    }
                }
            }

            if (independentTags.Count() == 0)
            {
                return null;
            }

            return independentTags;
        }



        public static void AlignTagsLeft(List<IndependentTag> tags)
        {
            if (tags.Count == 0)
            {
                return;
            }

            IndependentTag mostLeftTag = tags.OrderBy(tag => tag.TagHeadPosition.X).First();
            double leftX = mostLeftTag.TagHeadPosition.X;

            foreach (IndependentTag tag in tags)
            {

                MoveTagHeadToPoint(tag, new XYZ(leftX, tag.TagHeadPosition.Y, tag.TagHeadPosition.Z));
            }
        }

        public static void SpaceTagsEvenly(List<IndependentTag> tags, XOrY xOrY)
        {
            switch (xOrY)
            {
                case XOrY.XThenY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.X).ThenBy(p => p.TagHeadPosition.Y).ToList();
                    break;
                case XOrY.YThenX:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y).ThenBy(p => p.TagHeadPosition.X).ToList();
                    break;
                case XOrY.SumOfXandY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y + p.TagHeadPosition.X).ToList();
                    break;
            }



            IndependentTag firstTag = tags[0];
            IndependentTag lastTag = tags[tags.Count - 1];

            XYZ firstTagHeadPoint = firstTag.TagHeadPosition;

            XYZ distanceAsPoint = lastTag.TagHeadPosition.Subtract(firstTagHeadPoint);

            XYZ Segment = distanceAsPoint.Divide(tags.Count - 1);

            for (int i = 0; i < tags.Count; i++)
            {
                IndependentTag tag = tags[i];

                var newPoint = firstTagHeadPoint.Add(Segment.Multiply(i));

                MoveTagHeadToPoint(tag, newPoint);
            }
        }

        public static void MoveTagLocationToPoint(IndependentTag tag, XYZ Point)
        {
            XYZ tagLocation = ((LocationPoint)tag.Location).Point;
            XYZ translation = Point.Subtract(tagLocation);

            tag.Location.Move(translation);
        }

        public static void MoveTagHeadToPoint(IndependentTag tag, XYZ Point)
        {
            tag.TagHeadPosition = Point;

            //Wiggle to make sure Revit registers the change. Otherwise it doesn't for some reason
            tag.Location.Move(new XYZ(2, 0, 0));
            tag.Location.Move(new XYZ(-2, 0, 0));
        }



    }
}
