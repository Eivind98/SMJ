#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace SMJAddin
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            var tabName = "SMJ";
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "SMJAddin Tab");

            //C:\Program Files\Vormadal Brothers\SMJAddin\Pictures

            new ButtonBuilder("Export IFC", typeof(CopyRoomsFromLinkedFile))
                .ImagePath("C:\\Program Files\\Vormadal Brothers\\SMJAddin\\Pictures\\IconRoomImport.png")
                .Text("SMJAddin")
                .Build(panel);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
