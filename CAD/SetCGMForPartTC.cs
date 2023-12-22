namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen; // so we can use NXOpen functionality
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    using NXOpen.CAE;
    using NXOpen.CAM;

    public class SetCGMForPartTC
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

            // login to teamcenter (likely not required if executing from the GUI eg. Alt + F8 -> run this file)
            string[] teamcenterCredentials = new string[] { "-pim=yes", "-u=username", "-p=password" };
            theUFSession.Ugmgr.Initialize(teamcenterCredentials.Length, args);
            theUFSession.UF.IsUgmanagerActive(out bool isConnected);
            if (!isConnected)
            {
                throw new Exception("Invalid credentials.");
            }
            
            // delcare a list of files with revision you want to process (ABC: partnumber; 123 partrevision)
            Dictionary<string, string> partNumbersAndRevision = new Dictionary<string, string>(){ { "ABC", "123" },  { "DEF", "456" } };
            
            theLW.WriteFullline("The following parts have been processed:");
            foreach (KeyValuePair<string, string> file in partNumbersAndRevision)
            {
                theUFSession.Ui.SetStatus("Processing file " + file);
                // https://community.sw.siemens.com/s/question/0D54O00007FewyGSAR/error-opening-existing-part-from-teamcenter
                UFSession.GetUFSession().Ugmgr.EncodePartFilename(file.Key, file.Value, null, null, out string encodedName);
                PartLoadStatus loadStatus;
                Part part = Session.GetSession().Parts.OpenDisplay(encodedName, out loadStatus);
                part.SaveOptions.DrawingCgmData = true;
                part.SaveOptions.PatternDataToSave = PartSaveOptions.PatternData.SaveNoShadedOrPattern;
                part.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.True);
                
                // print the file which has been processed
                theLW.WriteFullline(file.Key + " " + file.Value);
            }
        }
    }
}
