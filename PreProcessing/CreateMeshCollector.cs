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
    
    public class CreateMeshCollectorClass
    {
        // global variables used throughout
        static Session theSession = Session.GetSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static CaePart caePart = (CaePart)theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            double[] thicknesses = {6, 8, 10, 12, 14, 15, 16, 18, 20, 22, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100};
            for (int ii = 0; ii < thicknesses.Length; ii++)
            {
                CreateMeshCollector(thicknesses[ii]);
            }
            
            //CreateMeshCollector(8, 5);
            //CreateMeshCollector(10, 6);
            //CreateMeshCollector(12, 7);
            //CreateMeshCollector(14, 8);
        }

        /// <summary>
        /// Creates a 2d mesh collector with the given thickness.
        /// The color of the mesh collector is set an an integer of 10 times the label.
        /// The label is one higher from the label of the last physical property. 
        /// This implicilty assumes that physical properties are created with ascending labels.
        /// </summary>
        /// <param name="thickness">The thickness of the 2d mesh.</param>
        /// <param name="color">OPTIONAL: The color of the 2d mesh collector (meshes will inherit this color).</param>
        public static void CreateMeshCollector(double thickness, int color = -1)
        {
            FemPart femPart = (FemPart)theSession.Parts.BaseWork;
            
            FEModel fEModel = (FEModel)femPart.BaseFEModel;
            MeshManager meshManager = (MeshManager)fEModel.MeshManager;
            
            MeshCollector nullNXOpen_CAE_MeshCollector = null;
            MeshCollectorBuilder meshCollectorBuilder = meshManager.CreateCollectorBuilder(nullNXOpen_CAE_MeshCollector, "ThinShell");
            
            // Get the highest label from the physical properties to then pass as parameter in the creation of a physical property.
            PhysicalPropertyTable[] physicalPropertyTables = caePart.PhysicalPropertyTables.ToArray();
            int maxLabel = 1;
            if (physicalPropertyTables.Length != 0)
            {
                maxLabel = physicalPropertyTables[physicalPropertyTables.Length - 1].Label + 1;
            }

            PhysicalPropertyTable physicalPropertyTable;
            physicalPropertyTable = femPart.PhysicalPropertyTables.CreatePhysicalPropertyTable("PSHELL", "NX NASTRAN - Structural", "NX NASTRAN", "PSHELL2", maxLabel);
            physicalPropertyTable.SetName(thickness.ToString() + "mm");
            
            //NXOpen.PhysicalMaterial physicalMaterial1 = (NXOpen.PhysicalMaterial)workFemPart.MaterialManager.PhysicalMaterials.FindObject("PhysicalMaterial[Steel]");
            PhysicalMaterial[] physicalMaterials = femPart.MaterialManager.PhysicalMaterials.GetUsedMaterials();
            PhysicalMaterial steel = Array.Find(physicalMaterials, material => material.Name == "Steel");
            if (steel == null)
            {
                steel = femPart.MaterialManager.PhysicalMaterials.LoadFromNxlibrary("Steel");
            }
            
            PropertyTable propertyTable;
            propertyTable = physicalPropertyTable.PropertyTable;
            propertyTable.SetMaterialPropertyValue("material", false, steel);
            propertyTable.SetTablePropertyWithoutValue("bending material");
            propertyTable.SetTablePropertyWithoutValue("transverse shear material");
            propertyTable.SetTablePropertyWithoutValue("membrane-bending coupling material");
            
            Unit unitMilliMeter = (Unit)femPart.UnitCollection.FindObject("MilliMeter");
            propertyTable.SetBaseScalarWithDataPropertyValue("element thickness", thickness.ToString(), unitMilliMeter);
            
            meshCollectorBuilder.CollectorName = thickness.ToString() + "mm"; //"8mm";
            meshCollectorBuilder.PropertyTable.SetNamedPropertyTablePropertyValue("Shell Property", physicalPropertyTable);
            
            NXObject nXObject = meshCollectorBuilder.Commit();
            
            meshCollectorBuilder.Destroy();

            // Setting the color of the MeshCollector we just created
            MeshCollector meshCollector = (MeshCollector)nXObject;
            MeshCollectorDisplayDefaults meshCollectorDisplayDefaults1;
            meshCollectorDisplayDefaults1 = meshCollector.GetMeshDisplayDefaults();
            
            // we set the color as label * 10 to make a distinction between the colors. The maximum color number is 216, therefore we take the modulus to not exceed this numer (eg. 15%4 -> 3)
            if (color == -1)
            {
                meshCollectorDisplayDefaults1.Color = NXColor.Factory._Get((maxLabel * 10) % 216); //workFemPart.Colors.Find("Smoke Gray");
            }
            else
            {
                meshCollectorDisplayDefaults1.Color = NXColor.Factory._Get(color);
            }
            
            
            meshCollectorDisplayDefaults1.Dispose();
        }
    }
    
}