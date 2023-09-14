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
using Autodesk.Revit.DB.Events;

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class StuffTesterAlso : IExternalCommand
    {

        /// <summary>
        /// Updater notifying user if an 
        /// elevation view was added.
        /// </summary>
        public class ElevationWatcherUpdater : IUpdater
        {
            static AddInId _appId;
            static UpdaterId _updaterId;

            public ElevationWatcherUpdater(AddInId id)
            {
                _appId = id;

                _updaterId = new UpdaterId(_appId, new Guid(
                  "fafbf6b2-4c06-42d4-97c1-d1b4eb593eff"));
            }
            bool test = true;
            public void Execute(UpdaterData data)
            {
                Document doc = data.GetDocument();
                Application app = doc.Application;

                foreach (ElementId id in data.GetModifiedElementIds())
                {
                    View view = doc.GetElement(id) as View;

                    if (null != view
                      && ViewType.DrawingSheet == view.ViewType)
                    {
                        if (test)
                        {
                            test = false;

                            Parameter para = view.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY);
                            para.Set(view.Name);

                            test = true;

                        }

                    }
                }
            }

            public string GetAdditionalInformation()
            {
                return "The Building Coder, "
                  + "http://thebuildingcoder.typepad.com";
            }

            public ChangePriority GetChangePriority()
            {
                return ChangePriority.FloorsRoofsStructuralWalls;
            }

            public UpdaterId GetUpdaterId()
            {
                return _updaterId;
            }

            public string GetUpdaterName()
            {
                return "ElevationWatcherUpdater";
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            // Register updater to react to view creation

            ElevationWatcherUpdater updater
              = new ElevationWatcherUpdater(
                app.ActiveAddInId);

            UpdaterRegistry.RegisterUpdater(updater);

            ElementCategoryFilter f
              = new ElementCategoryFilter(
                BuiltInCategory.OST_Sheets);

            UpdaterRegistry.AddTrigger(
              updater.GetUpdaterId(), f,
              Element.GetChangeTypeParameter(new ElementId((int)BuiltInParameter.SHEET_NAME)));
            return Result.Succeeded;
        }
    }
}
