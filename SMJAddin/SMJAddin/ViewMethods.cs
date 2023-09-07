using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMJAddin
{
    public static class ViewMethods
    {

        public static View3D CreateViewForRay(Document doc)
        {
            string name = "Auto Generated View for Raytracing";
            var maybe3DView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Where(i => i is View3D).Cast<View3D>();
            View3D newView = null;
            if (maybe3DView.Any(i => i.Name == name))
            {
                newView = maybe3DView.First(i => i.Name == name);
            }

            if (newView == null)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                if (viewFamilyType == null)
                {
                    return null;
                }
                newView = View3D.CreateIsometric(doc, viewFamilyType.Id);
            }
            
            List<BuiltInCategory> demCategoriesToShow = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Ceilings,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_RoomSeparationLines,
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_CurtainGridsWall,
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_RvtLinks,
                BuiltInCategory.OST_FoundationSlabAnalytical
            };

            List<BuiltInCategory> demCategoriesToHide = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Furniture,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_ElectricalEquipment,
                BuiltInCategory.OST_Stairs,
                BuiltInCategory.OST_Planting,
                BuiltInCategory.OST_Casework,
                BuiltInCategory.OST_GenericModel,
                BuiltInCategory.OST_PlumbingEquipment,
                BuiltInCategory.OST_Entourage,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_FurnitureSystems,
                BuiltInCategory.OST_Site,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_SpecialityEquipment,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_Railings,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctSystem
            };

            newView.IsSectionBoxActive = false;
            newView.Name = name;
            var categories = doc.Settings.Categories;

            foreach (Category cat in categories)
            {
                BuiltInCategory builtIn = cat.BuiltInCategory;
                if (demCategoriesToShow.Contains(builtIn))
                {
                    cat.set_Visible(newView, true);
                }
                else if (demCategoriesToHide.Contains(builtIn))
                {
                    cat.set_Visible(newView, false);
                }
            }

            return newView;
        }


    }
}
