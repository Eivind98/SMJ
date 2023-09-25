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


            //$"{Global.AssemblyDirectory}\\Pictures\\IconRoomImport.png"

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

            SplitButtonData splitButtonData = new SplitButtonData("Tags", "Tag Functions");
            SplitButton splitButton = panelTags.AddItem(splitButtonData) as SplitButton;

            new ButtonBuilder("SpaceTagsFixedDistance", typeof(SpaceTagsFixedDistanceVert))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignFixedDistance.png")
                .Text("Space Tags\nFixed Distance")
                .Build(splitButton);

            new ButtonBuilder("SpaceTagsEvenly", typeof(SpaceTagsEvenly))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\SpaceEvenly.png")
                .Text("Space Tags\nEvenly")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsLeft", typeof(AlignTagsLeft))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignLeft.png")
                .Text("Align Tags\nLeft")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsCenter", typeof(AlignTagsCenter))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignCenter.png")
                .Text("Align Tags\nCenter")
                .Build(splitButton);

            new ButtonBuilder("AlignTagsRight", typeof(AlignTagsRight))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\AlignRight.png")
                .Text("Align Tags\nRight")
                .Build(splitButton);

            RibbonPanel Testing = application.CreateRibbonPanel(tabName, "Testing");

            new ButtonBuilder("CreatingDrawings", typeof(Tester))
                .ImagePath($"{Global.AssemblyDirectory}\\Pictures\\Tester.png")
                .Text("Testing\nCreate Drawings")
                .Build(Testing);



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
