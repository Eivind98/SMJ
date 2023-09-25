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
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using SMJAddin.UI;

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class StuffTester : IExternalCommand
    {

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            SpaceTagsFixedDistance test = new SpaceTagsFixedDistance(commandData.Application.ActiveUIDocument);

            test.InitializeComponent();


            return Result.Succeeded;
        }
    }
}
