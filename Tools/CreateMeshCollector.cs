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

    public class ProgramSection2
    {
        static NXOpen.Session theSession = NXOpen.Session.GetSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static CaePart caePart = (CaePart)theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            double[] thicknesses = {6, 8, 10, 12, 14, 15, 16, 18, 20, 22, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100};
            for (int ii = 0; ii < thicknesses.Length; ii++)
            {
                CreateMeshCollector(thicknesses[ii], ii + 1);
            }
            //CreateMeshCollector(8, 5);
            //CreateMeshCollector(10, 6);
            //CreateMeshCollector(12, 7);
            //CreateMeshCollector(14, 8);
        }

        /// <summary>
        /// Creates a 2d mesh collector with the given thickness and label.
        /// The color of the mesh collector is set an an integer of 10 times the label.
        /// </summary>
        /// <param name="thickness">The thickness of the 2d mesh.</param>
        /// <param name="label">The label of the physical property. Needs to be unique and thus cannot already be used in the part.</param>
        public static void CreateMeshCollector(double thickness, int label)
        {
            Session theSession = Session.GetSession();
            FemPart workFemPart = (FemPart)theSession.Parts.BaseWork;
            FemPart displayFemPart = (FemPart)theSession.Parts.BaseDisplay;
            
            FEModel fEModel1 = (FEModel)workFemPart.BaseFEModel;
            MeshManager meshManager1 = (MeshManager)fEModel1.MeshManager;
            
            MeshCollector nullNXOpen_CAE_MeshCollector = null;
            MeshCollectorBuilder meshCollectorBuilder2;
            meshCollectorBuilder2 = meshManager1.CreateCollectorBuilder(nullNXOpen_CAE_MeshCollector, "ThinShell");
            
            PhysicalPropertyTable physicalPropertyTable1;
            physicalPropertyTable1 = workFemPart.PhysicalPropertyTables.CreatePhysicalPropertyTable("PSHELL", "NX NASTRAN - Structural", "NX NASTRAN", "PSHELL2", label);
            physicalPropertyTable1.SetName(thickness.ToString() + "mm");
            
            //NXOpen.PhysicalMaterial physicalMaterial1 = (NXOpen.PhysicalMaterial)workFemPart.MaterialManager.PhysicalMaterials.FindObject("PhysicalMaterial[Steel]");
            PhysicalMaterial[] physicalMaterials = workFemPart.MaterialManager.PhysicalMaterials.GetUsedMaterials();
            PhysicalMaterial steel = Array.Find(physicalMaterials, material => material.Name == "Steel");
            if (steel == null)
            {
                steel = workFemPart.MaterialManager.PhysicalMaterials.LoadFromNxlibrary("Steel");
            }
            
            PropertyTable propertyTable1;
            propertyTable1 = physicalPropertyTable1.PropertyTable;
            propertyTable1.SetMaterialPropertyValue("material", false, steel);
            propertyTable1.SetTablePropertyWithoutValue("bending material");
            propertyTable1.SetTablePropertyWithoutValue("transverse shear material");
            propertyTable1.SetTablePropertyWithoutValue("membrane-bending coupling material");
            
            Unit unit1 = (Unit)workFemPart.UnitCollection.FindObject("MilliMeter");
            propertyTable1.SetBaseScalarWithDataPropertyValue("element thickness", thickness.ToString(), unit1);
            
            meshCollectorBuilder2.CollectorName = thickness.ToString() + "mm"; //"8mm";
            meshCollectorBuilder2.PropertyTable.SetNamedPropertyTablePropertyValue("Shell Property", physicalPropertyTable1);
            
            NXObject nXObject1;
            nXObject1 = meshCollectorBuilder2.Commit();
            
            meshCollectorBuilder2.Destroy();

            // Setting the color of the MeshCollector we just created
            MeshCollector meshCollector1 = (MeshCollector)nXObject1;
            MeshCollectorDisplayDefaults meshCollectorDisplayDefaults1;
            meshCollectorDisplayDefaults1 = meshCollector1.GetMeshDisplayDefaults();
            
            // we set the color as label * 10 to make a distinction between the colors. The maximum color number is 216, therefore we take the modulus to not exceed this numer (eg. 15%4 -> 3)
            meshCollectorDisplayDefaults1.Color = NXColor.Factory._Get((label * 10) % 216); //workFemPart.Colors.Find("Smoke Gray");
            
            meshCollectorDisplayDefaults1.Dispose();
        }
    }
    
}