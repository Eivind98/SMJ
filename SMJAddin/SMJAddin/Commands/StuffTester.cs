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

#endregion

namespace SMJAddin
{
    [Transaction(TransactionMode.Manual)]
    public class StuffTester : IExternalCommand
    {

        static void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            IList<string> strings = e.GetTransactionNames();
            if(strings.Contains("Name View"))
            {
                Document doc = e.GetDocument();
                ICollection<ElementId> eleId = e.GetModifiedElementIds();

                foreach(ElementId elementId in eleId)
                {
                    Element ele = doc.GetElement(elementId);
                    if(ele != null && ele is ViewSheet)
                    {
                        Parameter drawnByPara = ele.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY);

                        if(drawnByPara.AsValueString() != ele.Name)
                        {
                            try
                            {
                                using (var tx = new Transaction(doc))
                                {
                                    tx.Start("Aligning tags to the Left");

                                    drawnByPara.Set(ele.Name);

                                    tx.Commit();
                                }
                            }catch (Exception ex)
                            {
                                Trace.Write(ex);
                            }
                        }
                    }
                }

                TaskDialog.Show("sometest", string.Join(", ", strings));
            }
        }

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            app.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);


            return Result.Succeeded;
        }
    }
}
