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
    public class CopyRoomsFromLinkedFileAsSpaces : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;


            var sel = uidoc.Selection;
            var eleId = sel.GetElementIds();

            if (eleId.Count != 0)
            {
                var ele = doc.GetElement(eleId.First());
                if (ele != null && ele is RevitLinkInstance)
                {
                    RevitLinkInstance linkedDoc = ele as RevitLinkInstance;
                    FilteredElementCollector roomsCollector = new FilteredElementCollector(linkedDoc.GetLinkDocument()).OfCategory(BuiltInCategory.OST_Rooms);

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Creating Rooms");

                        foreach (Room roomInLinkedModel in roomsCollector)
                        {
                            if (roomInLinkedModel != null && roomInLinkedModel.Area != 0)
                            {
                                SpaceMethods.CreateOrUpdateSpaceFromRoomInLinkedFile(doc, roomInLinkedModel);
                            }
                        }

                        tx.Commit();
                    }
                }
                else
                {
                    TaskDialog dia = new TaskDialog("Not a linked file");
                    dia.MainContent = "Selected element was not a linked file. Please select a linked file and try again";
                    dia.Show();
                }
            }
            else
            {
                TaskDialog dia = new TaskDialog("No Selection");
                dia.MainContent = "Nothing is selected. Please select a linked file and try again";
                dia.Show();
            }

            return Result.Succeeded;
        }
    }
}
