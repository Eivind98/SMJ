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
            RibbonPanel panelAddin = application.CreateRibbonPanel(tabName, "SMJAddin Tab");

            //C:\\Program Files\\Vormadal Brothers\\SMJAddin\\Pictures\\

            new ButtonBuilder("CreateOrUpdateRoomFromLinkedFile", typeof(CopyRoomsFromLinkedFile))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\IconRoomImport.png")
                .Text("Create or update\nrooms from Linked file")
                .Build(panelAddin);

            new ButtonBuilder("MoveRoomLocationToRoomCenteroid", typeof(MoveRoomLocationToCenter))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\CenterRooms.png")
                .Text("Move Room Location\nTo Centeroid")
                .Build(panelAddin);

            new ButtonBuilder("MoveTagsToRoomLocation", typeof(MoveTagsToRoomLocation))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\MoveTagsToRoom.png")
                .Text("Move Tags to\nRoom Location")
                .Build(panelAddin);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
