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
            RibbonPanel panelSpaces = application.CreateRibbonPanel(tabName, "Spaces");

            new ButtonBuilder("CreateOrUpdateSpaceFromLinkedFile", typeof(CopyRoomsFromLinkedFileAsSpaces))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\IconRoomImport.png")
                .Text("Create or Update\n Rooms from Linked file\nas Spaces")
                .Build(panelSpaces);

            new ButtonBuilder("MoveSpaceLocationToCenteroid", typeof(MoveSpaceLocationToCenter))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\CenterRooms.png")
                .Text("Move Space Location\nTo Centeroid")
                .Build(panelSpaces);

            new ButtonBuilder("MoveTagsToSpaceLocation", typeof(MoveTagsToSpaceLocation))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\MoveTagsToSpaces.png")
                .Text("Move Tags to\nSpace Location")
                .Build(panelSpaces);

            RibbonPanel panelTags = application.CreateRibbonPanel(tabName, "Tags");

            new ButtonBuilder("SpaceTagsEvenly", typeof(SpaceTagsEvenlySumOfXAndY))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\SpaceEvenly.png")
                .Text("Space Tags\nEvenly")
                .Build(panelTags);

            new ButtonBuilder("AlignTagsLeft", typeof(AlignTagsLeft))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignLeft.png")
                .Text("Align Tags\nLeft")
                .Build(panelTags);
            


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
