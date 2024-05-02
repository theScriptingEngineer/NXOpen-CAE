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
    
    public class SaveCopyClass
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

            string newFileName = "D:\\Temp\\newFileName.prt";
            SaveAsSwitchToOriginal(basePart, newFileName);
        }

        public static void SaveCopy(NXOpen.BasePart basePart, String newFileName)
        {
            NXOpen.PartSaveStatus partSaveStatus;
            partSaveStatus = basePart.SaveAs(newFileName);
            partSaveStatus.Dispose();

            theSession.Parts.CloseAll(NXOpen.BasePart.CloseModified.CloseModified, null);
        }

        public static void SaveAsSwitchToOriginal(BasePart basePart, String newFileName)
        {
            String fullPathOriginal = basePart.FullPath;
            SaveCopy(basePart, newFileName);

            PartLoadStatus partLoadStatus;
            basePart = theSession.Parts.OpenActiveDisplay(fullPathOriginal, DisplayPartOption.ReplaceExisting, out partLoadStatus);
        }
    }
}
