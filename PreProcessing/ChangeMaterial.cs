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
    
    public class ChangeMaterial
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static UFSession theUFSession = UFSession.GetUFSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // entrypoint for NX
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            ReplaceMaterial("name of the new material");
        }


        /// <summary>
        /// Replaces the material in all physical property tables of the current base part.
        /// </summary>
        /// <param name="newMaterialName">The name of the new material to set in the tables.</param>
        public static void ReplaceMaterial(string newMaterialName)
        {
            BaseFemPart baseFemPart = (BaseFemPart)basePart;
            foreach (PhysicalPropertyTable table in baseFemPart.PhysicalPropertyTables.ToArray())
            {
                theLW.WriteFullline("Updating table " + table.JournalIdentifier);
                PhysicalPropertyChangeMaterial(table, newMaterialName);
            }
        }


        /// <summary>
        /// Changes the material of a physical property table to the specified material.
        /// Note that the material needs to exist.
        /// </summary>
        /// <param name="physicalPropertyTable">The physical property table to modify.</param>
        /// <param name="newMaterialName">The name of the new material to set.</param>
        public static void PhysicalPropertyChangeMaterial(PhysicalPropertyTable physicalPropertyTable, string newMaterialName)
        {
            BaseFemPart baseFemPart = (BaseFemPart)basePart;
            PropertyTable propertyTable = physicalPropertyTable.PropertyTable;

            PhysicalMaterial[] physicalMaterials = baseFemPart.MaterialManager.PhysicalMaterials.GetUsedMaterials();
            PhysicalMaterial physicalMaterial = Array.Find(physicalMaterials, item => item.Name == newMaterialName);
            if (physicalMaterial == null)
            {
                theLW.WriteFullline("No material could be found with the name " + newMaterialName);
                return;
            }
            propertyTable.SetMaterialPropertyValue("material", false, physicalMaterial);

            NXOpen.Session.UndoMarkId markId= theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, null);
            int nErrs = theSession.UpdateManager.DoUpdate(markId);
        }
    }
}
