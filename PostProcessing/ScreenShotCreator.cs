// This script automatically generates screenshots of post processing results using the definitions in a .csv file.
// Needs to be run from the .sim file. 
// The definitions of the screenshots are provided as a csv file.
// The file format is as follows:
// FileName, AnnotationText, TemplateName, GroupName, CameraName, Solution, Subcase, Iteration, RestultType, ComponentName
// eg.
// screenshot1.tif, Text displayed on top of screenshot1, Template 1, Group 1, TopView, Solution 1, 1, 1, Stress - Element-Nodal, VonMises
// screenshot2.tif, Text displayed on top of screenshot2, Template 2, Group 2, SideView, Solution 1, 2, 1, Stress - Element-Nodal, VonMises

// These are the entries:
// - FileName: the file name for the screenshot, with or without path. If no path is included, it is saved in the location of the .sim file.
// - AnnotationText: the text to be displayed on top of the screenshot.
// - TemplateName: the name of the PostView template to apply to the displayed results. This contains HOW results are displayed (eg colorbar, feature edges,...)
//                 Note that the definition of the result to be displayed needs to be manually removed by editing the xml file, see NOTE for more details
// - GroupName: The name of the CaeGroup to be displayed. Only one group can be provided. 
//              The name is not case sensitive and if multiple groups with the same name, the first one found will be displayed.
// - CameraName: The name of the camera (as seen in the GUI) to be applied before taking the screenshot. This "orients" what is displayed.
//               Note that you need to save the camera while a result is being displayed, otherwise you might get unexpected results.
// - Solution: The name of the solution to display
// - Subcase: The follower number for the subcase in the solution. Counting starts at 1.
//            Note that this number can depend on the amount of output results.
// - Iteration: The follower number for the iteration. Counting starts at 1. Is 1 for results witout iterations (eg. static structural results)
// - RestultType: The name of the ResultType eg. "Stress - Element-Nodal", "Displacement - Nodal", ...
// - ComponentName: Name of the result component (case-sensitive) eg. 
//                  Scalar, X, Y, Z, Magnitude, Xx, Yy, Zz, Xy, Yz, Zx, MaximumPrincipal, VonMises, MembraneXX, BendingZZ, ShearYZ,... 

// easily create a csv from excel data by concatenating the cell data with commas

//  NOTE that the user needs to manually delete the result from the post template xml file.
//  - Delete the following entries in your template.xml file under the tag <ResultOptions>:
//    - <LoadCase>0</LoadCase>
//    - <Iteration>0</Iteration>
//    - <SubIteration>-1</SubIteration>
//    - <Result>[Displacement][Nodal]</Result>
//    - <Component>Magnitude</Component>

//  Update the group visibility with the following (assuming there are less than 1000 groups in the model).
//  Note that other types might exist (like <Num3DGroups>). Adjust accordingly.
//		<GroupVisibilities>
//			<Num1DGroups>1000</Num1DGroups>
//			<Visibilities1DGroups>1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1</Visibilities1DGroups>
//			<Num2DGroups>1000</Num2DGroups>
//			<Visibilities2DGroups>1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1</Visibilities2DGroups>
//		</GroupVisibilities>


//  Post processing XML template files are located under the location where UGII_CAE_POST_TEMPLATE_USER_DIR is pointing to.
//  This can be found in the log file.
//  If you also set UGII_CAE_POST_TEMPLATE_EDITOR to for example notepad++.exe,
//  you can directly edit by right-clicking the template in the NX GUI

// Tested in:
// - NX12
// - Simcenter3D release 2022.1 (version 2023)
// - Simcenter3D 2212

namespace TheScriptingEngineerScreenShotCreator
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using System.Windows.Forms; // for displaying user forms
    using System.Drawing; // for displaying user forms
    using NXOpen; // so we can use NXOpen functionality
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    
    public class ScreenShotCreator
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static UFSession theUFSession = UFSession.GetUFSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static string nXVersion = theSession.GetEnvironmentVariableValue("UGII_VERSION"); // theSession.BuildNumber only available from version 1926 onwards

        public static void Main(string[] args)
        {
            theLW.Open();

            // user feedback
            if (theSession.Parts.BaseWork as SimPart == null)
            {
                theLW.WriteFullline("ScreenShotCreator needs to be started from a .sim file!");
                return;
            }

            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog?view=netframework-4.8
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "csv files (*.csv)|*.csv";
                DialogResult openFileDialogResult = openFileDialog.ShowDialog();

                if (openFileDialogResult == DialogResult.Cancel)
                {
                    // user pressed cancel
                    return;
                }

                filePath = openFileDialog.FileName;
            }

            // read the input file into an array of ScreenShot
            // it's user input, so errors can occur
            ScreenShot[] screenShots = null;
            try
            {
                screenShots = ReadScreenShotDefinitions(filePath);
            }
            catch (Exception ex)
            {
                theLW.WriteFullline("Failed to parse file " + filePath + ". Please check the screenshot definitions in the file.");
                theLW.WriteFullline(ex.Message);
                return;
            }
            
            // check for empty file
            if (screenShots.Length == 0)
            {
                theLW.WriteFullline("The file " + filePath + " is empty.");
                return;
            }

            // check input and catch errors so that the user doesn't get a error pop-up in SC
            try
            {
                CheckScreenShots(screenShots);
            }
            catch (Exception ex)
            {
                theLW.WriteFullline("The following error occured while checking the screenshot definitions:");
                theLW.WriteFullline(ex.Message);
                return;
            }

            // sort for performance in NX
            // we don't put requirements on the order of the screen shots,
            // only changing the group to display is very fast, compared to changing the result.
            // Sorting minimizes the amount of switches between solutions and subcases and thus improves performance
            ScreenShot.SortScreenShots(screenShots);

            // load all results before the loop
            SolutionResult[] solutionResults = LoadResults(screenShots);

            // Keep track of all original CaeGroups, so the (possible) Created PostGroups can be tracked and deleted
            // Without accidentaly deleting user groups which might start with PostGroup.
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            CaeGroup[] caeGroupsOriginal = simPart.CaeGroups.ToArray();

            int postViewId = -1;
            // process the screen shots
            theLW.WriteFullline("Generated " + screenShots.Length + " screenshots with the following input:");
            for (int i = 0; i < screenShots.Length; i++)
            {
                theUFSession.Ui.SetStatus("Generating ScreenShot " + screenShots[i].FileName);
                theLW.WriteFullline("File name: " + screenShots[i].FileName + "; Template name: " + screenShots[i].TemplateName + "; Group name: " + screenShots[i].GroupName + 
                                    "; Camera name: " + screenShots[i].CameraName + "; Solution name: " + screenShots[i].Solution + "; Subcase: " + screenShots[i].Subcase.ToString() + 
                                    "; Iteration: " + screenShots[i].Iteration + "; Result type: " + screenShots[i].ResultType + "; Component name " + screenShots[i].ComponentName + 
                                    "; Annotation text: " + screenShots[i].AnnotationText);
                // set the result to be displayed
                // don't change if not required (but need to always display the first time):
                if (i != 0)
                {
                    if (screenShots[i].NeedChangeResult(screenShots[i - 1]))
                    {
                        postViewId = DisplayResult(screenShots[i], solutionResults[i], screenShots[i].ComponentName);
                    }
                }
                else
                {
                    postViewId = DisplayResult(screenShots[i], solutionResults[i], screenShots[i].ComponentName);
                }
                
                // set the post template (do this before setting the group, as the template might have group visibility still in it)
                // no need to set if it hasn't changed, but displaying another solution removes the template settings so also need to set the template after changing the solution.
                // Note that you need to delete de definition of the 'result to display' from the template xml file! 
                // Otherwise applying the template changes the displayed result.
                if (i != 0)
                {
                    if (screenShots[i].TemplateName != screenShots[i - 1].TemplateName || screenShots[i].Solution != screenShots[i - 1].Solution)
                    {
                        SetPostTemplate(postViewId, screenShots[i].TemplateName);
                    }
                    else
                    {
                        SetPostTemplate(postViewId, screenShots[i].TemplateName);
                    }
                }
                else
                {
                    SetPostTemplate(postViewId, screenShots[i].TemplateName);
                }

                // Removing the <component> tag, makes NX used the default component (eg. Magnitude for Displacement, Von-Mises for stress, ...)
                // Therefore setting the correct component again after applying the template
                // postViewId = DisplayResult(screenShots[i], solutionResults[i], screenShots[i].ComponentName);
                ChangeComponent(postViewId, screenShots[i].ComponentName);
                

                // set the group
                DisplayElementsInGroup(postViewId, screenShots[i].GroupName);

                // create the annotation, but only if one given.
                PostAnnotation postAnnotation = null;
                if (!(screenShots[i].AnnotationText == "" || screenShots[i].AnnotationText == null))
                {
                    postAnnotation = CreateAnnotation(postViewId, screenShots[i].AnnotationText);
                }

                // position the result in the view with the camera.
                // cameras are created in the GUI
                SetCamera(screenShots[i].CameraName);
                
                // save the screenshot to file.
                SaveViewToFile(screenShots[i].FileName);

                // Clean up annotations, otherwise annotations pile up
                if (postAnnotation != null)
                {
                    postAnnotation.Delete();
                }
            }

            // Clean up post groups
            CaeGroup[] caeGroups = simPart.CaeGroups.ToArray();
            if (caeGroups.Length != caeGroupsOriginal.Length)
            {
                DeletePostGroups(caeGroups, caeGroupsOriginal);
                theLW.WriteFullline("Removed automatically created PostGroups");
            }

            PrintMessage();
        }

        /// <summary>
        /// Saves a view to a .tiff file with the specified file name. If the file name does not include a path, the file is saved in the simpart directory with the .tiff file extension.
        /// </summary>
        /// <param name="fileName">The name of the file to save the view to.</param>
        /// <remarks>
        /// The method creates an image export builder and sets its options, such as whether to enhance edges and whether to use a transparent background.
        /// It then commits the builder to save the view to the specified file. Finally, it destroys the builder to free up resources.
        /// </remarks>
        public static void SaveViewToFile(string fileName)
        {
            // TODO: add options for file formats other than .tiff
            // check if fileName contains a path. If not save with the .sim file
            string filePathWithoutExtension = CreateFullPath(fileName, ""); // should be in line with imageExportBuilder.FileFormat
            // Get full path without extension
            filePathWithoutExtension = Path.ChangeExtension(filePathWithoutExtension, null);

            // delete existing file to mimic overwriting
            string filePath = filePathWithoutExtension + ".tif";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            NXOpen.UI theUI = NXOpen.UI.GetUI();
            NXOpen.Gateway.ImageExportBuilder imageExportBuilder = theUI.CreateImageExportBuilder();
            try
            {
               // Options
               imageExportBuilder.EnhanceEdges = true;
               imageExportBuilder.RegionMode = false;
               imageExportBuilder.FileFormat = NXOpen.Gateway.ImageExportBuilder.FileFormats.Tiff; // should be in line with the fileName
               imageExportBuilder.FileName = filePathWithoutExtension; // NX adds the extension for the specific file format
               imageExportBuilder.BackgroundOption = NXOpen.Gateway.ImageExportBuilder.BackgroundOptions.Transparent;
               // Commit the builder
               imageExportBuilder.Commit();
            }
            catch (Exception ex)
            {
                theLW.WriteFullline(ex.Message);
            }
            finally
            {
                imageExportBuilder.Destroy();
            }
        }

        /// <summary>
        /// This function takes a filename and adds an extension and path of the part if not provided by the user.
        /// If the fileName contains an extension, this function leaves it untouched, othwerwise adds the provided extension, which defaults to .unv.
        /// If the fileName contains a path, this function leaves it untouched, otherwise adds the path of the BasePart as the path.
        /// </summary>
        /// <param name="fileName">The filename with or without path and .unv extension.</param>
        /// <param name="extension">Optional: The extension to add if missing. Defaults to .unv.</param>
        /// <returns>A string with extension and path of basePart if the fileName parameter did not include a path.</returns>
        public static string CreateFullPath(string fileName, string extension = ".unv")
        {
            // check if .unv is included in fileName
            if (Path.GetExtension(fileName).Length == 0)
            {
                fileName = fileName + extension;
            }

            // check if path is included in fileName, if not add path of the .sim file
            string unvFilePath = Path.GetDirectoryName(fileName);
            if (unvFilePath == "")
            {
                // if the basePart file has never been saved, the next will give an error
                fileName = Path.Combine(Path.GetDirectoryName(theSession.Parts.BaseWork.FullPath), fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Helper function for CombineResults and GetResultParameters.
        /// Returns the ResultTypes specified in PostInputs
        /// </summary>
        /// <param name="postInputs">The input as an array of PostInput.</param>
        /// <param name="solutionResults">The already loaded results to search through for the results.</param>
        /// <returns>Returns the result objects.</returns>
        public static ResultType[] GetResultTypes(PostInput[] postInputs, SolutionResult[] solutionResults)
        {
            ResultType[] resultTypes = new ResultType[postInputs.Length];

            for (int i = 0; i < postInputs.Length; i++)
            {
                BaseLoadcase[] baseLoadcases = solutionResults[i].GetLoadcases();
                Loadcase loadcase = (Loadcase)baseLoadcases[postInputs[i].Subcase - 1]; // user starts counting at 1
                BaseIteration[] baseIterations = loadcase.GetIterations();
                Iteration iteration = (Iteration)baseIterations[postInputs[i].Iteration - 1]; // user starts counting at 1
                BaseResultType[] baseResultTypes = iteration.GetResultTypes();
                ResultType resultType = (ResultType)Array.Find(baseResultTypes, type => type.Name.ToLower().Trim() == postInputs[i].ResultType.ToLower().Trim());
                resultTypes[i] = resultType;
            }

            return resultTypes;
        }

        /// <summary>
        /// Display the result as defined in the postInput for the given componentName.
        /// The solutionResult for the postInput is also required (performance)
        /// </summary>
        /// <param name="postInput">The post input to use for displaying the result.</param>
        /// <param name="solutionResult">The solution result to display.</param>
        /// <param name="componentName">The name of the component to display the result for.</param>
        /// <returns>The ID of the post view created for the result.</returns>
        public static int DisplayResult(PostInput postInput, SolutionResult solutionResult, string componentName)
        {
            // Only set the result and the component, the rest is through the template.
            ResultType resultType = GetResultTypes(new PostInput[] { postInput }, new SolutionResult[] { solutionResult })[0];
            // Get the component object from the string componentName
            Result.Component component = (Result.Component)Enum.Parse(typeof(Result.Component), componentName);
            ResultParameters resultParameters = theSession.ResultManager.CreateResultParameters();
            resultParameters.SetGenericResultType(resultType);
            resultParameters.SetResultComponent(component);
            int postViewId = theSession.Post.CreatePostviewForResult(0, solutionResult, false, resultParameters);

            return postViewId;
        }


        /// <summary>
        /// Changes the displayed component for an already displayed result.
        /// </summary>
        /// <param name="postViewId">The post input to use for displaying the result.</param>
        /// <param name="componentName">The name of the component to display the result for.</param>
        /// <returns>The ID of the post view created for the result.</returns>
        public static void ChangeComponent(int postViewId, string componentName)
        {
            Result.Component component = (Result.Component)Enum.Parse(typeof(Result.Component), componentName);
            Result result;
            ResultParameters resultParameters;
            theSession.Post.GetResultForPostview(postViewId, out result, out resultParameters);
            resultParameters.SetResultComponent(component);
            theSession.Post.PostviewSetResult(postViewId, resultParameters);
            theSession.Post.PostviewUpdate(postViewId);
        }


        /// <summary>
        /// Sets a template for a given post view.
        /// </summary>
        /// <param name="postViewId">The ID of the post view to which the template should be applied.</param>
        /// <param name="templateName">The name of the template to be applied.</param>
        /// <remarks>
        /// This method searches for the specified template by name and applies it to the post view with the given ID.
        /// If the template cannot be found, no action is taken.
        /// </remarks>
        public static void SetPostTemplate(int postViewId, string templateName)
        {
            int templateId = theSession.Post.TemplateSearch(templateName);
            theSession.Post.PostviewApplyTemplate(postViewId, templateId);
        }


        /// <summary>
        /// Sets the group visibility for a given post view to show only the specified group.
        /// </summary>
        /// <param name="postViewId">The ID of the post view to set the group visibility for.</param>
        /// <param name="groupName">The name of the group to show.</param>
        public static void DisplayElementsInGroup(int postViewId, string groupName)
        {
            if (nXVersion == "v12")
            {
                DisplayElementsInGroupViaPostGroup(postViewId, groupName);
            }
            else
            {
                int[] usergroupsGids;
                string[] userGroupNames = new string[] {groupName};
                // The next function only works for Groups created in the .sim file (at least when used like this)
                // And not for groups inherited from the fem or afem file. Also using the JournalIdentifier did not work.
                // Therefore a workaround with a postgroup for these fem or afem groups.
                theSession.Post.PostviewGetUserGroupGids(postViewId, userGroupNames, out usergroupsGids);
                if (usergroupsGids.Length == 0)
                {
                    // theLW.WriteFullline("Warning: group " + groupName + " is not a sim groups and is displayed through a temporaty PostGroup");
                    DisplayElementsInGroupViaPostGroup(postViewId, groupName);
                }
                else
                {
                    theSession.Post.PostviewApplyUserGroupVisibility(postViewId, usergroupsGids, NXOpen.CAE.Post.GroupVisibility.ShowOnly);
                }
            }
        }

        /// <summary>
        /// Helper function for DisplayElementsInGroup.
        /// This function creates an "on the fly" postgroup which is then used to display a specific group.
        /// </summary>
        /// <param name="postViewId">The ID of the post view to set the group visibility for.</param>
        /// <param name="groupName">The name of the group to show.</param>
        /// <remarks>
        /// It is the responsibility of the user to delete the PostGroups which are automatically created here.
        /// </remarks>
        public static void DisplayElementsInGroupViaPostGroup(int postViewId, string groupName)
        {
                // NX creates it's own postgroups from the groups in the sim.
                // It only creates a postgroup if either nodes or elements are present in the group.
                // Therefore it's hard to relate the postgroup labels to the group labels in the simfile...
                SimPart simPart = (SimPart)theSession.Parts.BaseWork;
                CaeGroup[] caeGroups = simPart.CaeGroups.ToArray();
                CaeGroup caeGroup = Array.Find(caeGroups, group => group.Name.ToLower() == groupName.ToLower());

                TaggedObject[] groupItems = caeGroup.GetEntities();
                // one longer, otherwise a single element is missing from each screenshot (bug in NX)
                int[] groupElementLabels = new int[groupItems.Length + 1];
                groupElementLabels[0] = 0;
                for (int i = 0; i < groupItems.Length; i++)
                {
                    if (groupItems[i] is FEElement)
                    {
                        groupElementLabels[i + 1] = ((FEElement)groupItems[i]).Label;
                    }
                }

                int[] userGroupIds = new int[1];
                // This creates a "PostGroup"
                userGroupIds[0] = theSession.Post.CreateUserGroupFromEntityLabels(postViewId, CaeGroupCollection.EntityType.Element, groupElementLabels);
                theSession.Post.PostviewApplyUserGroupVisibility(postViewId, userGroupIds, NXOpen.CAE.Post.GroupVisibility.ShowOnly);
        }

        /// <summary>
        /// Delete all PostGroups created earlier
        /// If a user creates a group which contains "PostGroup", then it will also be deleted.
        /// </summary>
        public static void DeletePostGroups(CaeGroup[] caeGroups, CaeGroup[] caeGroupsOriginal)
        {
            foreach(NXOpen.CAE.CaeGroup group in caeGroups)
            {
                if(!Array.Exists(caeGroupsOriginal, item => item.Name == group.Name))
                {
                    theSession.UpdateManager.AddToDeleteList((NXObject)group);
                }
            }
            
            Session.UndoMarkId undoMarkId = theSession.SetUndoMark(Session.MarkVisibility.Invisible, "deletePostGroup");
            theSession.UpdateManager.DoUpdate(undoMarkId);
        }

        /// <summary>
        /// Creates a new PostAnnotation object with the given text and draws it on the current part.
        /// </summary>
        /// <param name="postViewId">The ID of the post view to create the annotation in.</param>
        /// <param name="annotationText">The text to display in the annotation.</param>
        /// <returns>The PostAnnotation object representing the newly created annotation.</returns>
        public static PostAnnotation CreateAnnotation(int postViewId, string annotationText)
        {
            // maybe also look into NXOpen.CAE.CaeNoteBuilder
            // SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            // simPart.Notes.CreateCaeNoteBuilder()
            
            NXOpen.CAE.PostAnnotationBuilder postAnnotationBuilder = theSession.Post.CreateAnnotationBuilder(postViewId);
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

            postAnnotationBuilder.Dispose();

            return postAnnotation;
        }

        /// <summary>
        /// Sets the active camera view to the camera with the specified name.
        /// It is case insensitive, but applies the camera with the case as it is defined within NX
        /// </summary>
        /// <param name="cameraName">The name of the camera to set as active.</param>
        public static void SetCamera(string cameraName)
        {
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            NXOpen.Display.Camera[] cameras = simPart.Cameras.ToArray();
            NXOpen.Display.Camera camera = Array.Find(cameras, item => item.Name.ToLower() == cameraName.ToLower());
            camera.ApplyToView(simPart.ModelingViews.WorkView);
        }

        /// <summary>
        /// Loads the results in the provided array of PostInput.
        /// Does not load a second time if the result is already loaded.
        /// </summary>
        /// <param name="postInputs">The result of each of the provided solutions is loaded.</param>
        /// <param name="referenceType">The type of SimResultReference eg. Structural</param>
        /// <returns>Returns an array of SolutionResult.</returns>
        public static SolutionResult[] LoadResults(PostInput[] postInputs, string referenceType = "Structural")
        {
            SolutionResult[] solutionResults = new SolutionResult[postInputs.Length];
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            SimSimulation simSimulation = (SimSimulation)simPart.Simulation;

            for (int i = 0; i < postInputs.Length; i++)
            {                
                SimSolution simSolution = GetSolution(postInputs[i].Solution);
                SimResultReference simResultReference = (SimResultReference)simSolution.Find(referenceType);
                //SimResultReference simResultReference = simSolution.GetResultReferenceByIndex(0); // for structural

                try
                {
                    // SolutionResult[filename_solutionname]
                    solutionResults[i] = (SolutionResult)theSession.ResultManager.FindObject("SolutionResult[" + System.IO.Path.GetFileName(simPart.FullPath) + "_" + simSolution.Name + "]");
                }
                catch (System.Exception)
                {
                    solutionResults[i] = theSession.ResultManager.CreateReferenceResult(simResultReference);
                }
            }

            return solutionResults;
        }

        /// <summary>
        /// This function returns the SimSolution object with the given name.
        /// It is case insensitive, but returns the solution with the case as it is defined within NX
        /// </summary>
        /// <param name="SolutionName">The name of the solution to return. Case insensitive.</param>
        /// <returns>The SimSolution object if found, Null otherwise.</returns>
        public static SimSolution GetSolution(string SolutionName)
        {
            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            SimSolution[] simSolutions = simPart.Simulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == SolutionName.ToLower());
            return simSolution;
        }

        /// <summary>
        /// Reads an array of ScreenShot definitions from a specified file path.
        /// </summary>
        /// <param name="filePath">The file path to the JSON file containing the ScreenShot definitions.</param>
        /// <returns>An array of ScreenShot objects read from the JSON file, or null if the file is empty.</returns>
        public static ScreenShot[] ReadScreenShotDefinitions(string filePath)
        {
            if (!File.Exists(filePath))
            {
                theLW.WriteFullline("Error: could not find " + filePath);
            }
            string csvString = File.ReadAllText(filePath);
            // Check if the file is empty
            if (string.IsNullOrWhiteSpace(csvString))
            {
                return null;
            }

            List<ScreenShot> screenshotsFromFile = new List<ScreenShot>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // skip empty line
                    if (line == "")
                    {
                        continue;
                    }
                    // process the line
                    string[] values = line.Split(',');
                    if (values.Length != 10)
                    {
                        for (int i = 0; i < values.Length; i++)
                        {
                            theLW.WriteFullline("Item " + i.ToString() + ": " + values[i]);
                        }
                        throw new Exception("There should be 10 items in each input line, separated by commas. Please check the input above and make sure not to use commas in the names.");
                    }
                    ScreenShot entry = new ScreenShot();
                    entry.FileName = values[0].Trim();
                    entry.AnnotationText = values[1].Trim();
                    entry.TemplateName = values[2].Trim();
                    entry.GroupName = values[3].Trim();
                    entry.CameraName = values[4].Trim();
                    entry.Solution = values[5].Trim();
                    entry.Subcase = int.Parse(values[6].Trim());
                    entry.Iteration = int.Parse(values[7].Trim());
                    entry.ResultType = values[8].Trim();
                    entry.ComponentName = values[9].Trim();
                    screenshotsFromFile.Add(entry);
                }
            }

            return screenshotsFromFile.ToArray();
        }


        /// <summary>
        /// This function verifies if all PostInputs to make sure no errors occur further down the line due to input errors.
        /// </summary>
        /// <param name="postInputs">The array of PostInput to check.</param>
        public static void CheckPostInput(PostInput[] postInputs)
        {
            // Raising ValueError with my own message, instead of simply raising which is the proper way to keep the stack trace.
            // This journal is meant for non developers, so I think a simple clear message is more important than a stack trace.
            foreach (PostInput item in postInputs)
            {
                // Does the solution exist?
                SimSolution simSolution = GetSolution(item.Solution);
                if (simSolution == null)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("Solution with name " + item.Solution + " not found");
                    throw new ArgumentException("Solution with name " + item.Solution + " not found");
                }

                // Does the result exist
                SolutionResult[] solutionResult;
                try
                {
                    solutionResult = LoadResults(new PostInput[] { item });
                }
                catch (System.Exception)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("No result for solution with name " + item.Solution);
                    throw new ArgumentException("No result for solution with name " + item.Solution);
                }

                // Does the Subcase exist
                BaseLoadcase[] baseLoadcases = solutionResult[0].GetLoadcases();
                Loadcase loadcase;
                try
                {
                    loadcase = (Loadcase)baseLoadcases[item.Subcase - 1]; // user starts counting at 1!
                }
                catch (System.Exception)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("SubCase with number " + item.Subcase.ToString() + " not found in solution with name " + item.Solution);
                    throw new ArgumentException("SubCase with number " + item.Subcase.ToString() + " not found in solution with name " + item.Solution);
                }

                // Does the Iteration exist
                BaseIteration[] baseIterations = loadcase.GetIterations();
                Iteration iteration;
                try
                {
                    iteration = (Iteration)baseIterations[item.Iteration - 1]; // user starts counting at 1!
                }
                catch (System.Exception)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("Iteration number " + item.Iteration.ToString() + "not found in SubCase with number " + item.Subcase.ToString() + " in solution with name " + item.Solution);
                    throw new ArgumentException("Iteration number " + item.Iteration.ToString() + "not found in SubCase with number " + item.Subcase.ToString() + " in solution with name " + item.Solution);
                }

                // Does the ResultType exist
                BaseResultType[] baseResultTypes = iteration.GetResultTypes();
                ResultType resultType;
                resultType = (ResultType)Array.Find(baseResultTypes, type => type.Name.ToLower() == item.ResultType.ToLower());
                if (resultType == null)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("ResultType " + item.ResultType + "not found in iteration number " + item.Iteration.ToString() + " in SubCase with number " + item.Subcase.ToString() + " in solution with name " + item.Solution);
                    throw new ArgumentException("ResultType " + item.ResultType + "not found in iteration number " + item.Iteration.ToString() + " in SubCase with number " + item.Subcase.ToString() + " in solution with name " + item.Solution);
                }
            }
        }


        public static void CheckScreenShots(ScreenShot[] screenShots)
        {
            // Raising ValueError with my own message, instead of simply raising which is the proper way to keep the stack trace.
            // This journal is meant for non developers, so I think a simple clear message is more important than a stack trace.
            try
            {
                CheckPostInput(screenShots);
            }
            catch
            {
                // Simply let the exception bubble up
                throw;
            }

            SimPart simPart = (SimPart)theSession.Parts.BaseWork;
            CaeGroup[] caeGroups = simPart.CaeGroups.ToArray();
            NXOpen.Display.Camera[] cameras = simPart.Cameras.ToArray();
            // Reload in case template was just created
            theSession.Post.ReloadTemplates();

            foreach (ScreenShot screenShot in screenShots)
            {
                // Check the groups
                CaeGroup caeGroup = Array.Find(caeGroups, item => item.Name.ToLower() == screenShot.GroupName.ToLower());
                if (caeGroup == null)
                {
                    theLW.WriteFullline("Error in input " + screenShot.FileName);
                    theLW.WriteFullline("Group with name " + screenShot.GroupName + " not found");
                    throw new ArgumentException("Group with name " + screenShot.GroupName + " not found");
                }

                // Check the templates
                // TemplateSearch throws an error if the template is not found.
                try
                {
                    theSession.Post.TemplateSearch(screenShot.TemplateName);
                }
                catch
                {
                    theLW.WriteFullline("Error in input " + screenShot.FileName);
                    throw new ArgumentException("Template with name " + screenShot.TemplateName + " not found");
                }


                // Check the component name
                try
                {
                    // Get the component object from the ComonentName as string
                    Result.Component component = (Result.Component)Enum.Parse(typeof(Result.Component), screenShot.ComponentName);
                }
                catch (Exception)
                {
                    theLW.WriteFullline("Error in input " + screenShot.FileName);
                    theLW.WriteFullline("Component with name " + screenShot.ComponentName + " is not a valid identifier.");
                    theLW.WriteFullline("Valid identifiers are:");
                    foreach (Result.Component component in Enum.GetValues(typeof(Result.Component)))
                    {
                        theLW.WriteFullline(component.ToString());
                    }
                    throw new ArgumentException("Component with name " + screenShot.ComponentName + " is not a valid identifier.");
                }

                // check the cameras
                NXOpen.Display.Camera camera = Array.Find(cameras, item => item.Name.ToLower() == screenShot.CameraName.ToLower());
                if (camera == null)
                {
                    theLW.WriteFullline("Error in input " + screenShot.FileName);
                    theLW.WriteFullline("Camera with name " + screenShot.CameraName + " not found");
                    throw new ArgumentException("Camera with name " + screenShot.CameraName + " not found");
                }
            }
        }

        public static void PrintMessage()
        {
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR");
            theLW.WriteFullline("IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,");
            theLW.WriteFullline("FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE");
            theLW.WriteFullline("AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER");
            theLW.WriteFullline("LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,");
            theLW.WriteFullline("OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE");
            theLW.WriteFullline("SOFTWARE.");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("                        You have just experienced the power of scripting                          ");
            theLW.WriteFullline("                             brought to you by theScriptingEngineer                               ");
            theLW.WriteFullline("                                   www.theScriptingEngineer.com                                   ");
            theLW.WriteFullline("                                  More journals can be found at:                                  ");
            theLW.WriteFullline("                        https://github.com/theScriptingEngineer/NXOpen-CAE                        ");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("                                          Learn NXOpen at                                         ");
            theLW.WriteFullline("https://www.udemy.com/course/simcenter3d-basic-nxopen-course/?referralCode=4ABC27CFD7D2C57D220B%20");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
            theLW.WriteFullline("##################################################################################################");
        }
    }


    public class ScreenShot : PostInput, IComparable<ScreenShot>
    {
        /// <summary>
        /// The file name of the screenshot. If not a full path, it's saved with the .sim file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The text to print to the screenshot as an annotation.
        /// </summary>
        public string AnnotationText { get; set; }

        /// <summary>
        /// The name of the post processing template to apply to the result.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// The name of the CaeGroup to display.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The NXOpen Result.Component to display.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// The name of the camera as shown in the gui. This is used to position the view.
        /// </summary>
        public string CameraName { get; set; }

        /// <summary>
        /// Can be used to sort a List of ScreenShot
        /// </summary>
        /// <param name="other">The object to compare to.</param>
        // List<PostInput> inputs = new List<PostInput>()
        // {
        //     new PostInput("solution1", 2, 0, "result2", "identifier1"),
        //     new PostInput("solution2", 1, 1, "result1", "identifier2"),
        //     new PostInput("solution1", 1, 0, "result1", "identifier3")
        // };
        // call using inputs.Sort()
        public int CompareTo(ScreenShot other)
        {
            if (other == null) return 1;

            int result = Solution.CompareTo(other.Solution);
            if (result == 0)
            {
                result = Subcase.CompareTo(other.Subcase);
                if (result == 0)
                {
                    result = Iteration.CompareTo(other.Iteration);
                    if (result == 0)
                    {
                        result = ResultType.CompareTo(other.ResultType);
                    }
                }
            }
            return result;
            
        }

        /// <summary>
        /// Sorts an array of ScreenShot objects based on their PostInput properties.
        /// Sorts in place.
        /// </summary>
        /// <param name="screenshots">The array of ScreenShot objects to sort.</param>
        public static void SortScreenShots(ScreenShot[] screenshots)
        {
            Array.Sort(screenshots, new ScreenshotComparer());
        }

        public bool NeedChangeResult(ScreenShot other)
        {
            if (Solution != other.Solution)
            {
                return true;
            }
            if (Subcase != other.Subcase)
            {
                return true;
            }
            if (Iteration != other.Iteration)
            {
                return true;
            }
            if (ResultType != other.ResultType)
            {
                return true;
            }
            if (ComponentName != other.ComponentName)
            {
                return true;
            }

            return false;
        }
    }

    public class ScreenshotComparer : IComparer<ScreenShot>
    {
        public int Compare(ScreenShot x, ScreenShot y)
        {
            if (x == null || y == null) return 0;

            // Compare Solution
            int result = string.Compare(x.Solution, y.Solution, StringComparison.OrdinalIgnoreCase);
            if (result != 0) return result;

            // Compare Subcase
            result = x.Subcase.CompareTo(y.Subcase);
            if (result != 0) return result;

            // Compare Iteration
            result = x.Iteration.CompareTo(y.Iteration);
            if (result != 0) return result;

            // Compare ResultType
            return string.Compare(x.ResultType, y.ResultType, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Class for defining results in postprocessing
    /// For all selections, the user starts counting at 1!.
    /// </summary>
    public class PostInput
    {
        /// <summary>
        /// The solution to which the loadcase and iteration belong to.
        /// </summary>
        public string Solution { get; set; }

        /// <summary>
        /// The loadcase to which the iteration belongs to.
        /// </summary>
        public int Subcase { get; set; }

        /// <summary>
        /// The iteration, defaults to 0 for linear results.
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// The result number. This is the index of the result as show in the GUI.
        /// </summary>
        public string ResultType { get; set; }
             
        /// <summary>
        /// The identifier for the input as used in the formula.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public PostInput()
        {
            Solution = "";
            Subcase = -1;
            Iteration = -1;
            ResultType = "";
            Identifier = "";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostInput(string solution, int subcase, int iteration, string resulttype, string identifier)
        {
            Solution = solution;
            Subcase = subcase;
            Iteration = iteration;
            ResultType = resulttype;
            Identifier = identifier;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostInput(string solution, int subcase, int iteration, string resulttype)
        {
            Solution = solution;
            Subcase = subcase;
            Iteration = iteration;
            ResultType = resulttype;
            Identifier = "";
        }

        /// <summary>
        /// Returns a string representation of PostInput.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            if (Identifier != "")
            {
                return "Solution: " + Solution + " Subcase: " + Subcase.ToString() + " Iteration: " + Iteration.ToString() + " ResultType: " + ResultType + " Identifier: " + Identifier;
            }
            else
            {
                return "Solution: " + Solution + " Subcase: " + Subcase.ToString() + " Iteration: " + Iteration.ToString() + " ResultType: " + ResultType;
            }
        }
        
        /// <summary>
        /// This function returns all the Identifiers in PostInputs in an array of string
        /// </summary>
        /// <param name="postInputs">The array of PostInput for which to get the Identifiers.</param>
        /// <returns>The Identifiers in the PostInputs.</returns>
        public static string[] GetIdentifiers(PostInput[] postInputs)
        {
            string[] identifiers = new string[postInputs.Length];
            for (int i = 0; i < postInputs.Length; i++)
            {
                identifiers[i] = postInputs[i].Identifier;
            }

            return identifiers;
        }
    }
}