namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen; // so we can use NXOpen functionality
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;

    public class SetCGMToAllPartsInFolder
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

            string[] teamcenterCredentials = new string[] { "-pim=yes", "-u=username", "-p=password" };
            theUFSession.Ugmgr.Initialize(teamcenterCredentials.Length, args);
            theUFSession.UF.IsUgmanagerActive(out bool isConnected);
            if (!isConnected)
            {
                throw new Exception("Invalid credentials.");
            }
            
            // update your path here with the path containing the files.
            string path = "c:\\temp";
            
            string[] prtFiles = GetPrtFiles(path);
            theLW.WriteFullline("The following files have been processed:");
            foreach (string file in prtFiles)
            {
                theUFSession.Ui.SetStatus("Processing file " + file);
                PartLoadStatus loadStatus;
                Part part = theSession.Parts.Open(file, out loadStatus);
                part.SaveOptions.DrawingCgmData = true;
                part.SaveOptions.PatternDataToSave = PartSaveOptions.PatternData.SaveNoShadedOrPattern;
                part.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.True);
                theLW.WriteFullline(file);
            }
        }


        public static string[] GetPrtFiles(string folderPath)
        {
            try
            {
                // Check if the folder exists
                if (Directory.Exists(folderPath))
                {
                    // Get all files with a .prt extension in the folder
                    string[] prtFiles = Directory.GetFiles(folderPath, "*.prt");

                    if (prtFiles.Length > 0)
                    {
                        theLW.WriteFullline("List of .prt files:");

                        foreach (string prtFile in prtFiles)
                        {
                            theLW.WriteFullline(prtFile);
                        }

                            return prtFiles;
                    }
                    else
                    {
                        theLW.WriteFullline("No .prt files found in the specified folder.");
                        return null;
                    }
                }
                else
                {
                    theLW.WriteFullline("The specified folder does not exist.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

    }
}
