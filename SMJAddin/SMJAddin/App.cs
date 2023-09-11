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
            RibbonPanel panelAddin = application.CreateRibbonPanel(tabName, "Space");

            //C:\\Program Files\\Vormadal Brothers\\SMJAddin\\Pictures\\

            new ButtonBuilder("CreateOrUpdateSpaceFromLinkedFile", typeof(CopyRoomsFromLinkedFileAsSpaces))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\IconRoomImport.png")
                .Text("Create or Update\n Rooms from Linked file\nas Spaces")
                .Build(panelAddin);

            new ButtonBuilder("MoveSpaceLocationToCenteroid", typeof(MoveSpaceLocationToCenter))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\CenterRooms.png")
                .Text("Move Space Location\nTo Centeroid")
                .Build(panelAddin);

            new ButtonBuilder("MoveTagsToSpaceLocation", typeof(MoveTagsToSpaceLocation))
                .ImagePath($"{Global.ApplicationPath}\\Pictures\\MoveTagsToRoom.png")
                .Text("Move Tags to\nSpace Location")
                .Build(panelAddin);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
