using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Diagnostics;

namespace SMJAddin
{
    public static class SpatialTagMethods
    {


        public static void SpaceTagsEvenly(List<SpatialElementTag> tags)
        {
            tags = tags.OrderBy(p => p.TagHeadPosition.X).ThenBy(p => p.TagHeadPosition.Y).ToList();


            SpatialElementTag firstTag = tags[0];
            SpatialElementTag lastTag = tags[tags.Count - 1];

            XYZ firstTagHeadPoint = firstTag.TagHeadPosition;

            XYZ distanceAsPoint = lastTag.TagHeadPosition.Subtract(firstTagHeadPoint);

            XYZ Segment = distanceAsPoint.Divide(tags.Count - 1);




            for (int i = 0; i < tags.Count; i++)
            {
                SpatialElementTag tag = tags[i];

                var newPoint = firstTagHeadPoint.Add(Segment.Multiply(i));

                //var end = tag.LeaderEnd;

                //XYZ elbow = null;

                //if (tag.HasElbow)
                //{
                //    elbow = tag.LeaderElbow;
                //}

                if (!tag.HasLeader)
                {
                    tag.HasLeader = true;
                }


                MoveTagheadToPoint(tag, newPoint);

                //tag.LeaderEnd = end;

                //if (tag.HasElbow)
                //{
                //    tag.LeaderElbow = elbow;
                //}
            }
        }

        public static void MoveTagLocationToPoint(SpatialElementTag tag, XYZ Point)
        {
            XYZ tagLocation = ((LocationPoint)tag.Location).Point;
            XYZ translation = Point.Subtract(tagLocation);

            tag.Location.Move(translation);
        }

        public static void MoveTagheadToPoint(SpatialElementTag tag, XYZ Point)
        {
            tag.TagHeadPosition = Point;

            //Wiggle to make sure Revit registers the change. Otherwise it doesn't for some reason
            tag.Location.Move(new XYZ(2, 0, 0));
            tag.Location.Move(new XYZ(-2, 0, 0));
        }



    }
}
