namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen; // so we can use NXOpen functionality
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    using NXOpen.VectorArithmetic;
    
    public class CreateGroupsFromCAD
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            FemPart femPart = null;
            if (basePart as SimPart !=null)
            {
                // we started from a sim file
                SimPart simPart = (SimPart)basePart;
                CaePart caePart = simPart.FemPart;  // ComponentAssembly.RootComponent.GetChildren()[0].Prototype.OwningPart;
                if (caePart as FemPart == null)
                {
                    // simfile is linked to .afem file
                    theLW.WriteFullline("Create groups from CAD does not support .afem files yet.");
                    return;
                }

                femPart = (FemPart)caePart;
            }
            else if (basePart as AssyFemPart != null)
            {
                // we startef from a .afem file
                theLW.WriteFullline("Create groups from CAD does not support .afem files yet.");
                return;
            }
            else if (basePart as FemPart !=null)
            {
                // we started from a fem file
                femPart = (FemPart)basePart;
            }
            else
            {
                // not started from a cae part
                theLW.WriteFullline("Create groups does not work on non-cae parts");
                return;
            }

            CreateGroupsFromNamedPlanes(femPart);
        }

        /// <summary>
        /// This function creates a group with faces and bodies for each named datum plane in the associated cad part.
        /// All named datum planes are collected from the associated cad part.
        /// Then for each datum plane a selection recipe is created, "centered around" the datum plane, with the faces and bodies.
        /// For each selection recipe a group is created and the selection recipe deleted.
        /// Groups are created instead of selection recipes because older versions of Simcenter cannot use selection recipes in post-processing.
        /// Function is idempotent.
        /// </summary>
        /// <param name="femPart">The part in whcih to create the groups.</param>
        public static void CreateGroupsFromNamedPlanes(FemPart femPart)
        {
            // Get the associated cad part
            Part associatedCadPart = GetAssociatedCadPart(femPart);

            // Get an array of all named datum planes
            DatumPlane[] datumPlanes = GetNamedDatumPlanes(associatedCadPart);
            if (datumPlanes.Length == 0)
            {
                theLW.WriteFullline("No named datum planes found in " + associatedCadPart.Name);
                return;
            }

            theLW.WriteFullline("Found the following named datum planes in " + associatedCadPart.Name + ":");
            foreach (DatumPlane item in datumPlanes)
            {
                theLW.WriteFullline(item.Feature.Name);
            }

            // Create selection recipe for each named datum plane
            SelectionRecipe[] selectionRecipes = new SelectionRecipe[datumPlanes.Length];
            
            NXOpen.CAE.CaeSetGroupFilterType[] entitytypes = new NXOpen.CAE.CaeSetGroupFilterType[2];
            entitytypes[0] = NXOpen.CAE.CaeSetGroupFilterType.GeomFace;
            entitytypes[1] = NXOpen.CAE.CaeSetGroupFilterType.GeomBody;
            for (int i = 0; i < datumPlanes.Length; i++)
            {
                selectionRecipes[i] = CreateSelectionRecipe(femPart, datumPlanes[i], entitytypes);
            }

            // Create a group for each recipe
            CaeGroup[] caeGroups = femPart.CaeGroups.ToArray();
            for (int i = 0; i < datumPlanes.Length; i++)
            {
                TaggedObject[] taggedObjects = selectionRecipes[i].GetEntities();
                if (taggedObjects.Length == 0)
                {
                    theLW.WriteFullline("Recipe with name " + selectionRecipes[i].Name + " contains no items to put into a group");
                    continue;
                }

                CaeGroup caeGroup = Array.Find(caeGroups, group => group.Name.ToLower() == datumPlanes[i].Feature.Name.ToLower());
                if (caeGroup == null)
                {
                    // no group found with the feaure name, thus creating
                    femPart.CaeGroups.CreateGroup(datumPlanes[i].Feature.Name, taggedObjects);
                }
                else
                {
                    caeGroup.SetEntities(taggedObjects);
                }
            }

            femPart.SelectionRecipes.Delete(selectionRecipes);
        }

        /// <summary>
        /// This function creates a selection recipe around a given datum plane.
        /// Since a selection recipe is not infinite, the dimensions are hard coded, but can be easily adjusted.
        /// </summary>
        /// <param name="femPart">The part in whcih to create a selection recipe.</param>
        /// <param name="datumPlane">A datum plane, which is used to define the selection recipe.</param>
        /// <param name="entitytypes">An array of filters for the type of objects to add to the selection recipe.</param>
        /// <returns>The created selection recipe.</returns>
        public static SelectionRecipe CreateSelectionRecipe(FemPart femPart, DatumPlane datumPlane, CaeSetGroupFilterType[] entitytypes)
        {
            double recipeThickness = 1;
            double recipeSize = 100000;

            Vector3 origin = new Vector3(datumPlane.Origin.X, datumPlane.Origin.Y, datumPlane.Origin.Z); // define origin also as vector3 so that we can use arithmetic
            Vector3 normal = new Vector3(datumPlane.Normal.X, datumPlane.Normal.Y, datumPlane.Normal.Z);

            Vector3 global = new Vector3(1.0f, 0f, 0f);
            double projection = Math.Abs(normal.Dot(global)); // absolute value so only need to check larger than 0.999
            if (projection >= 0.999)
            {
                global = new Vector3(0f, 1.0f, 0f);
            }

            // we first project the global onto the plane normal
            // then subtract to get the component of global IN the plane which will be are local axis in the recipe definition
            double projectionMagnitude = normal.Dot(global);
            Vector3 globalOnNormal = projectionMagnitude * normal;
            Vector3 globalOnPlane = global - globalOnNormal;
            // normalize
            globalOnPlane.Normalize();
            
            // cross product of globalOnPlane and normal give vector in plane, otrhogonal to globalOnPlane
            Vector3 globalOnPlaneNormal = normal.Cross(globalOnPlane);
            globalOnPlaneNormal.Normalize();

            // offset origin so the recipe is centered around the origin
            // translate half recipeWidth in negative direction of globalOnPlane
            origin = origin - globalOnPlane * recipeSize / 2;

            // translate half recipeWidth in negative direction of globalOnPlaneNormal
            origin = origin - globalOnPlaneNormal * recipeSize / 2;

            // Translate half the thickness in negative normal direction
            origin = origin - normal * recipeThickness / 2;

            Point3d recipeOrigin = new Point3d(origin.x, origin.y, origin.z);
            Vector3d xDirection = new Vector3d(globalOnPlane.x, globalOnPlane.y, globalOnPlane.z);
            Vector3d yDirection = new Vector3d(globalOnPlaneNormal.x, globalOnPlaneNormal.y, globalOnPlaneNormal.z);

            NXOpen.Xform recipeXform = femPart.Xforms.CreateXform(recipeOrigin, xDirection, yDirection, NXOpen.SmartObject.UpdateOption.AfterModeling, 1.0);
            NXOpen.CartesianCoordinateSystem recipeCoordinateSystem = femPart.CoordinateSystems.CreateCoordinateSystem(recipeXform, NXOpen.SmartObject.UpdateOption.AfterModeling);

            NXOpen.Unit unitMilliMeter = (NXOpen.Unit)femPart.UnitCollection.FindObject("MilliMeter");
            NXOpen.Expression expressionLength = femPart.Expressions.CreateSystemExpressionWithUnits(recipeSize.ToString(), unitMilliMeter);
            NXOpen.Expression expressionWidth = femPart.Expressions.CreateSystemExpressionWithUnits(recipeSize.ToString(), unitMilliMeter);
            NXOpen.Expression expressionHeight = femPart.Expressions.CreateSystemExpressionWithUnits(recipeThickness.ToString(), unitMilliMeter);

            // SelectionRecipe selectionRecipe = femPart.SelectionRecipes.CreateBoxBoundingVolumeRecipe(datumPlane.Feature.Name, recipeCoordinateSystem, expressionLength, expressionWidth, expressionHeight, entitytypes);
            // Previous line is for pre NX1847, below is for NX1847 and later
            NXOpen.CAE.SelRecipeBuilder selRecipeBuilder = femPart.SelectionRecipes.CreateSelRecipeBuilder();
            selRecipeBuilder.AddBoxBoundingVolumeStrategy(recipeCoordinateSystem, expressionLength, expressionWidth, expressionHeight, entitytypes, NXOpen.CAE.SelRecipeBuilder.InputFilterType.EntireModel, null);
            selRecipeBuilder.RecipeName = datumPlane.Feature.Name;
            SelectionRecipe selectionRecipe = (SelectionRecipe)selRecipeBuilder.Commit();
            selRecipeBuilder.Destroy();

            return selectionRecipe;
        }

        /// <summary>
        /// This function searches the part for all datum planes with a name and returns them.
        /// Naming a datum plane is done by right-clicking on the plane in the GUI and selecting rename.
        /// </summary>
        /// <param name="cadPart">The part for which to return the named datum planes.</param>
        /// <returns>An array with the named datum planes.</returns>
        public static DatumPlane[] GetNamedDatumPlanes(Part cadPart)
        {
            // using a list to easily add items, turning it into an array before returning.
            List<DatumPlane> namedDatumPlanes = new List<DatumPlane>();
            foreach (DisplayableObject item in cadPart.Datums.ToArray())
            {
                if (item is DatumPlane)
                {
                    if (((DatumPlane)item).Feature.Name != "")
                    {
                        namedDatumPlanes.Add((DatumPlane)item);
                    }
                }
            }

            return namedDatumPlanes.ToArray();
        }

        /// <summary>
        /// This function returns the associated cad part for a given FemPart.
        /// Will load the part if not loaded.
        /// It assumes that the FemPart has an associated cad part (is not an orphan mesh)
        /// </summary>
        /// <param name="femPart">The FemPart for which to return the associated cad part.</param>
        /// <returns>The associated cad part.</returns>
        public static Part GetAssociatedCadPart(FemPart femPart)
        {
            Part associatedCadPart = femPart.AssociatedCadPart;
            PartLoadStatus loadStatus;
            if (associatedCadPart == null)
            {
                // "load" the part (right-click load under fem)
                associatedCadPart = (Part)theSession.Parts.Open(femPart.FullPathForAssociatedCadPart, out loadStatus);
            }

            return associatedCadPart;
        }
    }
}