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
    
    public class ScreenshotGenerator
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;

        public static void Main(string[] args)
        {

        }


        public static void SaveViewToFile(string fileLocation)
        {
            // possibly additional license required for using studioImageCaptureBuilder
            // should add alternatives
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            NXOpen.Display.StudioImageCaptureBuilder studioImageCaptureBuilder = simPart.Views.CreateStudioImageCaptureBuilder();
            studioImageCaptureBuilder.NativeFileBrowser = fileLocation;
            studioImageCaptureBuilder.DpiEnum = NXOpen.Display.StudioImageCaptureBuilder.DPIEnumType.Dpi150;
            studioImageCaptureBuilder.AASamplesEnum = NXOpen.Display.StudioImageCaptureBuilder.AASamplesEnumType.Sam0X;
            studioImageCaptureBuilder.EnhanceEdges = false;
            studioImageCaptureBuilder.Commit();
            studioImageCaptureBuilder.Destroy();

        }

        public static void SetResult()
        {

            // Define the result file path
            string resultFilePath = @"C:\example\result.sim";

            // Open the result file
            Result result = theSession.ResultManager.Open(resultFilePath);

            // Get the active result object
            Result activeResult = theSession.ResultManager.ActiveResult;

            resultParameters: NXOpen.CAE.ResultParameters
            resultParameters.SetResultComponent

            // Check if an active result object exists
            if (activeResult != null)
            {
                // Display the active result object
                theUI.ResultViewManager.Show(activeResult);

                // Get the active result view
                ResultView activeView = theUI.ResultViewManager.GetActiveView();

                return activeView;
            }
            else
            {
                // Display an error message if an active result object does not exist
                theLW.WriteFullline("Error: No active result object found");
            }
        }


        public static void SetPostTemplate()
        {
            
        }


        public static void SetGroup(int postViewId, string groupName)
        {
            // NXOpen.CAE.Result[] results = new NXOpen.CAE.Result[1];
            // results[0] = solutionResult;
            
            // find the group number from the name
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            CaeGroup[] caeGroups = simPart.CaeGroups.ToArray();
            CaeGroup caeGroup = Array.Find(caeGroups, group => group.Name.ToLower() == groupName);
            int[] userGroupIds = new int[1];
            userGroupIds[0] = caeGroup.Label;

            theSession.Post.PostviewApplyUserGroupVisibility(postViewId, userGroupIds, NXOpen.CAE.Post.GroupVisibility.ShowOnly)

        }


        public static void CreateAnnotation(string annotationText)
        {
            // maybe also look into NXOpen.CAE.CaeNoteBuilder
            // workSimPart.Notes.CreateCaeNoteBuilder
            NXOpen.CAE.PostAnnotationBuilder postAnnotationBuilder = theSession.Post.CreateAnnotationBuilder(1)
            postAnnotationBuilder.SetName("AnnotationName");
            postAnnotationBuilder.SetAnnotationType(NXOpen.CAE.PostAnnotationBuilder.Type.Userloc);
            postAnnotationBuilder.SetCoordinate(0.3, 0.15);

            string[] userText = new string[1];
            userText[0] = annotationText;
            postAnnotationBuilder.SetUsertext(userText);

            PostAnnotation postAnnotation = postAnnotationBuilder.CommitAnnotation();

            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            postAnnotation.DrawBox = true;
            postAnnotation.BoxTranslucency = false;
            postAnnotation.BoxFill = true;
            postAnnotation.BoxColor = simPart.Colors.Find("White");
            postAnnotation.Draw();
        }

        public static void 
    }
}