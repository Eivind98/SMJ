﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMJAddin
{
    public static class ViewMethods
    {
        public static void CreateSheetAndFilters(Element element)
        {
            if (element is Pipe)
            {



                Document doc = element.Document;
                string paramText = element.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString();
                string text = paramText;

                List<string> strings = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewPlan))
                    .Cast<ViewPlan>()
                    .Select(view => view.Name)
                    .ToList();

                bool checker = true;
                int indx = 1;

                while (checker)
                {
                    if (strings.Contains(text))
                    {
                        text = paramText + " - " + indx;
                        indx++;
                    }
                    else
                    {
                        break;
                    }
                }

                View currentView = doc.ActiveView;
                ElementId newViewId = currentView.Duplicate(ViewDuplicateOption.Duplicate);
                View newView = doc.GetElement(newViewId) as View;

                newView.Name = text;
                List<ViewPlan> allTemplates = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
                ViewPlan theTemplate = allTemplates.First(i => i.Name == "SMJ - (IT) H1 - Flatmynd");
                newView.ViewTemplateId = theTemplate.Id;

                ICollection<Element> tus = new List<Element>();
                newView.CropBoxActive = false;

                var allSimilarElements = new FilteredElementCollector(doc, newViewId)
                    .OfCategory(element.Category.BuiltInCategory)
                    .Where(i => i.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString() == paramText)
                    .Cast<Element>()
                    .ToList();
                BoundingBoxXYZ bounding = GeneralMethods.GetBoundingBox(allSimilarElements);
                newView.CropBox = bounding;
                newView.CropBoxActive = true;

                ICollection<ElementId> builtInCategories = new List<ElementId>
                {
                    new ElementId(BuiltInCategory.OST_DuctAccessory),
                    new ElementId(BuiltInCategory.OST_DuctFitting),
                    new ElementId(BuiltInCategory.OST_DuctInsulations),
                    new ElementId(BuiltInCategory.OST_DuctLinings),
                    new ElementId(BuiltInCategory.OST_DuctCurves),
                    new ElementId(BuiltInCategory.OST_PlaceHolderDucts),
                    new ElementId(BuiltInCategory.OST_FlexDuctCurves),
                    new ElementId(BuiltInCategory.OST_FlexPipeCurves),
                    new ElementId(BuiltInCategory.OST_PipeAccessory),
                    new ElementId(BuiltInCategory.OST_PipeFitting),
                    new ElementId(BuiltInCategory.OST_PipeInsulations),
                    new ElementId(BuiltInCategory.OST_PlaceHolderPipes),
                    new ElementId(BuiltInCategory.OST_PipeCurves),
                    new ElementId(BuiltInCategory.OST_PlumbingFixtures)
                };

                string prefixText = "ZAutoGenerated - ";
                string originalFilterName = prefixText + text;
                string finalFilterName = originalFilterName;

                checker = true;
                indx = 1;

                while (checker)
                {
                    if (!ParameterFilterElement.IsNameUnique(doc, finalFilterName))
                    {
                        finalFilterName = originalFilterName + " - " + indx;
                        indx++;
                    }
                    else
                    {
                        break;
                    }
                }

                FilterRule filterRule = ParameterFilterRuleFactory.CreateNotBeginsWithRule(new ElementId(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM), text);

                ElementParameterFilter to = new ElementParameterFilter(filterRule);

                ParameterFilterElement filter = ParameterFilterElement.Create(doc, finalFilterName, builtInCategories, to);
                ElementId filterId = filter.Id;
                newView.AddFilter(filterId);
                OverrideGraphicSettings graphicSettings = new OverrideGraphicSettings();
                graphicSettings.SetHalftone(true);
                graphicSettings.SetProjectionLineColor(new Color(0, 0, 0));
                graphicSettings.SetSurfaceTransparency(80);

                newView.SetFilterOverrides(filterId, graphicSettings);

                CreateSheetForView(newView, new string[] { "HSKV", paramText });

            }
        }

        public static ViewSheet CreateSheetForView(View view, string[] numberAndName)
        {
            Document doc = view.Document;
            FamilySymbol titleBlock = GetTitleblockBasedOnView(view);

            ViewSheet sheet;
            sheet = ViewSheet.Create(doc, titleBlock.Id);
            sheet.SheetNumber = numberAndName[0];
            sheet.Name = numberAndName[1];

            BoundingBoxXYZ bound = titleBlock.get_BoundingBox(sheet);

            double lengthX = (bound.Max.X - bound.Min.X) / 2;
            double lengthY = (bound.Max.Y - bound.Min.Y) / 2;

            XYZ middlePoint = new XYZ(bound.Min.X + lengthX, bound.Min.Y + lengthY, bound.Min.Z);

            Viewport.Create(doc, sheet.Id, view.Id, middlePoint);


            return sheet;
        }

        public static FamilySymbol GetTitleblockBasedOnView(View view)
        {
            Document doc = view.Document;
            FilteredElementCollector allTitleBlocks = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsElementType();
            BoundingBoxXYZ viewBoundBox = view.CropBox;
            int scale = view.Scale;
            double viewHeight = (viewBoundBox.Max.Y - viewBoundBox.Min.Y) / scale;
            double viewWidth = (viewBoundBox.Max.X - viewBoundBox.Min.X) / scale;

            FamilySymbol currentTitleBlock = null;
            BoundingBoxXYZ currentBound = null;

            foreach (FamilySymbol titleBlock in allTitleBlocks)
            {
                BoundingBoxXYZ titleBlockBox = titleBlock.get_BoundingBox(view);
                double titleHeight = titleBlockBox.Max.Y - titleBlockBox.Min.Y;
                double titleWidth = titleBlockBox.Max.X - titleBlockBox.Min.X;

                if (currentTitleBlock == null)
                {
                    if (titleHeight > viewHeight && titleWidth > viewWidth)
                    {
                        currentTitleBlock = titleBlock;
                        currentBound = titleBlock.get_BoundingBox(view);
                    }
                }
                else
                {
                    if (titleHeight > viewHeight && titleWidth > viewWidth)
                    {
                        double currentHeight = currentBound.Max.Y - currentBound.Min.Y;
                        double currentWidth = currentBound.Max.X - currentBound.Min.X;
                        if (currentHeight > titleHeight && currentWidth > titleWidth)
                        {
                            currentTitleBlock = titleBlock;
                            currentBound = titleBlock.get_BoundingBox(view);
                        }
                    }
                }

            }

            if (currentTitleBlock == null)
            {
                return null;
            }

            return currentTitleBlock;

        }




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
                BuiltInCategory.OST_DuctFitting
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

        public static View GetCurrentViewFromElement(Element ele)
        {
            Document doc = ele.Document;
            if (doc == null)
            {
                return null;
            }
            else
            {
                return doc.ActiveView;
            }
        }


    }
}
