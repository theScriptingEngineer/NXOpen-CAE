// SimCenter support for universal file:
// NX12 https://docs.sw.siemens.com/en-US/product/289054037/doc/PL20190719090640300.advanced/html/xid1404617
// Release SC2019.1 https://docs.sw.siemens.com/en-US/product/289054037/doc/PL20190702084816205.advanced/html/xid1404617
// Release SC2020.1 https://docs.sw.siemens.com/en-US/product/289054037/doc/PL20191009145841552.advanced/html/xid1404617
// Release SC2021.1 https://docs.sw.siemens.com/en-US/product/289054037/doc/PL20200601120302950.advanced/html/xid1404617
// Release SC2022.1 https://docs.sw.siemens.com/en-US/product/289054037/doc/PL20201105151514625.advanced/html/xid1404617

// Fortran format codes
// https://www.l3harrisgeospatial.com/docs/format_codes_fortran.html
// https://help.perforce.com/pv-wave/2017.1/PVWAVE_Online_Help/pvwave.html#page/Foundation/ap.a.format.066.09.html

namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for using list
    using NXOpen;
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpen.Utilities;
    using NXOpen.UF;
    using NXOpenUI;
    using NXOpen.VectorArithmetic;

    public class ExportOrientationThicknessAsUnv
    {
        static NXOpen.Session theSession = NXOpen.Session.GetSession();
        static NXOpen.UF.UFSession theUFSession = UFSession.GetUFSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            BaseFemPart baseFemPart;
            if (basePart as SimPart != null)
            {
                theLW.WriteFullline("Starting from sim file.");
                SimPart simPart = (SimPart)basePart;
                baseFemPart = (BaseFemPart)simPart.FemPart;

                // if the baseFemPart is an AssyFemPart then need to make it work for the code to run.
                theSession.Parts.SetWork(baseFemPart);
            }
            else if (basePart as BaseFemPart != null)
            {
                theLW.WriteFullline("Starting from fem or afem file.");
                baseFemPart = (BaseFemPart)basePart;
            }
            else
            {
                theLW.WriteFullline("This function needs to start from a .sim, .afem or .fem.");
                return;
            }

            WriteOrientation(baseFemPart, "Orientation.unv");

            // return to original work part.
            theSession.Parts.SetWork(basePart);
        }

        /// <summary>
        /// This function writes an elemental and element-nodal result to a universal file.
        /// The content of the result is the shell thickness of all shell elements in the model.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to generate the thickness result for.</param>
        /// <param name="fileName">The name of the universal file to write the results to</param>
        /// <param name="sIUnits">True divides thickness value by 1000, false leaves thickness value in SC3D units</param>
        public static void WriteOrientation(BaseFemPart baseFemPart, string fileName, bool sIUnits = true)
        {
            string[] dataset = CreateOrientationDataset(baseFemPart);
            fileName = CreateFullPath(fileName);
            theUFSession.Ui.SetStatus("Writing universal file " + fileName);

            using(StreamWriter writetext = new StreamWriter(fileName))
            {
                for (int i = 0; i < dataset.Length; i++)
                {
                    writetext.Write(dataset[i]);
                    writetext.Write(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// This method generates the universal file material orientation dataset.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to generate the material orientation dataset for.</param>
        public static string[] CreateOrientationDataset(BaseFemPart baseFemPart)
        {
            // version 2312.4001 gives a memory access violation error
            // 2212 and 
            if (theSession.FullReleaseNumber == "2312.4001")
            {
                theLW.WriteFullline("ERROR - Simcenter " + theSession.FullReleaseNumber + " results in a memory access violation when requesting material orientation. Please change to another version.");
                theLW.WriteFullline("Simcenter versions 2212 and 2306 do not have this issue. Later versions were not available at the time of writing this script.");
                return new string[0];
            }
            if (theSession.FullReleaseNumber.Contains("2312"))
            {
                theLW.WriteFullline("Warning: Memory access violation might occur in Simcenter " + theSession.FullReleaseNumber + " when requesting material orientation. Please change to another version if this happens.");
            }
            

            theUFSession.Ui.SetStatus("Querying material orientation for all shell elements in the model");
            // get the material orientation for all shell and beam elements
            List<FEElement> allShellElements = GetAllShellElements(baseFemPart);

            NXOpen.CAE.ModelCheckManager modelCheckManager = baseFemPart.ModelCheckMgr;
            NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder elementMaterialOrientationCheckBuilder = modelCheckManager.CreateElementMaterialOrientationCheckBuilder();
            elementMaterialOrientationCheckBuilder.CheckScopeOption = NXOpen.CAE.ModelCheck.CheckScope.Selected;
            elementMaterialOrientationCheckBuilder.SetCheckOrientation(NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType.Shell, true);
            elementMaterialOrientationCheckBuilder.SetCheckOrientation(NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType.SolidFirstDirection, false);
            elementMaterialOrientationCheckBuilder.SetCheckOrientation(NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType.SolidSecondDirection, false);
            elementMaterialOrientationCheckBuilder.SetCheckOrientation(NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType.SolidThirdDirection, false);
            NXOpen.SelectTaggedObjectList selectTaggedObjectList = elementMaterialOrientationCheckBuilder.SelectionList;
            selectTaggedObjectList.Clear();
            selectTaggedObjectList.SetArray(allShellElements.ToArray());

            NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType[] materialOrientationTypes = new NXOpen.CAE.ModelCheck.ElementMaterialOrientationCheckBuilder.MaterialOrientationType[allShellElements.Count * 2];
            NXOpen.CAE.FEElement[] shellElementsOrientation = new NXOpen.CAE.FEElement[allShellElements.Count * 2];
            // materialOrientationVectors is the material orientation vector for each element in global coordinate system
            NXOpen.Vector3d[] materialOrientationVectors = elementMaterialOrientationCheckBuilder.DoCheck(out materialOrientationTypes, out shellElementsOrientation);

            elementMaterialOrientationCheckBuilder.Destroy();

            // get the in-plane normals for all shell elements
            theUFSession.Ui.SetStatus("Calculating in-plane normals for all shell elements in the model");
            NXOpen.Vector3d[] inPlaneNormals = GetShellInPlaneNormal(shellElementsOrientation, materialOrientationVectors);

            // generate the dataset
            // don't concatenate strings in a loop, use a string array and join them at the end
            // that is much faster
            // theUFSession.Ui.SetStatus("Generating material orientation dataset");
            // string[] materialOrientationDataSet = new string[shellElementsOrientation.Length * 5 + 2];
            // materialOrientationDataSet[0] = String.Format("{0, 6}", "-1"); // every dataset starts with -1
            // materialOrientationDataSet[1] = String.Format("{0, 6}", "2438"); // this is the header for dataset 2438
            
            // int descriptorId = -1;
            // for (int i = 0; i < shellElementsOrientation.Length; i++)
            // {
            //     if (shellElementsOrientation[i].Shape == NXOpen.CAE.ElementTypes.Shape.Quad) { descriptorId = 94; }
            //     else if (shellElementsOrientation[i].Shape == NXOpen.CAE.ElementTypes.Shape.Tri) { descriptorId = 95; }
            //     else { descriptorId = -1; theLW.WriteFullline("ERROR: Element " + shellElementsOrientation[i].Label + " is not a quad or tri."); }

            //     materialOrientationDataSet[i * 5 + 2] = String.Format("{0,10}", shellElementsOrientation[i].Label.ToString()) + String.Format("{0,10}", descriptorId);
            //     materialOrientationDataSet[i * 5 + 3] = String.Format("{0,10}", "0") + String.Format("{0,10}", "0");
            //     materialOrientationDataSet[i * 5 + 4] = String.Format("{0,15}", materialOrientationVectors[i].X.ToString("#.0000000E+00")) + String.Format("{0,15}", materialOrientationVectors[i].Y.ToString("#.0000000E+00")) + String.Format("{0,15}", materialOrientationVectors[i].Z.ToString("#.0000000E+00"));
            //     materialOrientationDataSet[i * 5 + 5] = String.Format("{0,15}", "0.0000000E+00");
            //     materialOrientationDataSet[i * 5 + 6] = String.Format("{0,15}", "0.0000000E+00");
            // }

            theUFSession.Ui.SetStatus("Generating material orientation dataset");
            string[] materialOrientationDataSet = new string[shellElementsOrientation.Length * 2 + 16];
            int datasetLabel = 1;
            string datasetName = "MaterialOrientation";
            materialOrientationDataSet[0] = String.Format("{0, 6}", "-1"); // every dataset starts with -1
            materialOrientationDataSet[1] = String.Format("{0, 6}", "2414"); // this is the header for dataset 2414
            materialOrientationDataSet[2] = String.Format("{0, 10}", datasetLabel); // record 1
            materialOrientationDataSet[3] = "LOADCASE_NAME_KEY " + datasetName; // record 2 - analysis dataset name 40A2: using this syntax, SimCenter will set the load case name to "datasetName"
            materialOrientationDataSet[4] = String.Format("{0, 10}", "2"); // Record 3 - dataset location - data on elements
            materialOrientationDataSet[5] = "RESULT_NAME_KEY " + datasetName; // record 4 - analysis dataset name 40A2: using this syntax, will set the resulttype to MaterialOrientation
            materialOrientationDataSet[6] = "NONE"; // record 5 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            materialOrientationDataSet[7] = "EXPRESSION_NAME_KEY " + datasetName; // record 6 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            materialOrientationDataSet[8] = "Creation time: "  + DateTime.UtcNow.ToString("dd-MMM-yy   HH:mm:sszzz"); // record 7 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            materialOrientationDataSet[9] = "NONE"; // record 8 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            materialOrientationDataSet[10] = String.Format("{0,10}", "1") + String.Format("{0,10}", "1") + String.Format("{0,10}", "3") + String.Format("{0,10}", "96") + String.Format("{0,10}", "2") + String.Format("{0,10}", "6"); // record 9
            materialOrientationDataSet[11] = String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", datasetLabel) + String.Format("{0,10}", "0") + String.Format("{0,10}", "1") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0"); //record 10: using this syntax for Simcenter to parse it properly
            materialOrientationDataSet[12] = String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0"); //record 10: using this syntax for Simcenter to parse it properly
            materialOrientationDataSet[13] = String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00"); // record 12: using this syntax for Simcenter to parse it properly
            materialOrientationDataSet[14] = String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00"); // record 13: using this syntax for Simcenter to parse it properly

            for (int i = 0; i < shellElementsOrientation.Length; i++)
            {
                materialOrientationDataSet[i * 2 + 15] = String.Format("{0,10}", shellElementsOrientation[i].Label.ToString()) + String.Format("{0,10}", "6");
                materialOrientationDataSet[i * 2 + 16] = String.Format("{0,13}", materialOrientationVectors[i].X.ToString("#.00000E+00")) + String.Format("{0,13}", materialOrientationVectors[i].Y.ToString("#.00000E+00")) + String.Format("{0,13}", materialOrientationVectors[i].Z.ToString("#.00000E+00")) + String.Format("{0,13}", inPlaneNormals[i].X.ToString("#.00000E+00")) + String.Format("{0,13}", inPlaneNormals[i].Y.ToString("#.00000E+00")) + String.Format("{0,13}", inPlaneNormals[i].Z.ToString("#.00000E+00"));
            }

            materialOrientationDataSet[materialOrientationDataSet.Length - 1] = String.Format("{0, 6}", "-1"); // every dataset ends with -1

            return materialOrientationDataSet;
        }


        /// <summary>
        /// Get all elements from the model.
        /// Note that this is the most performant way to do so.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to get the elements from.</param>
        /// <returns>An array of all FEElements in the baseFemPart.</returns>
        public static SortedList<int, FEElement> GetAllFEElements(BaseFemPart baseFemPart)
        {
            theUFSession.Ui.SetStatus("Getting all element information from the SC3D database");
            SortedList<int, FEElement> allElements = new SortedList<int, FEElement>();

            FEElementLabelMap fEElementLabelMap = baseFemPart.BaseFEModel.FeelementLabelMap;
            int elementLabel = fEElementLabelMap.AskNextElementLabel(0);
            while (elementLabel > 0)
            {
                allElements.Add(elementLabel, fEElementLabelMap.GetElement(elementLabel));
                elementLabel = fEElementLabelMap.AskNextElementLabel(elementLabel);
            }

            return allElements;
        }

        /// <summary>
        /// Get all shell elements from the model.
        /// Note that this is the most performant way to do so.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to get the elements from.</param>
        /// <returns>An array of all shell (of type quad and tri) FEElements  in the baseFemPart.</returns>
        public static List<FEElement> GetAllShellElements(BaseFemPart baseFemPart)
        {
            SortedList<int, FEElement> allElements = GetAllFEElements(baseFemPart);
            List<FEElement> allShellElements = new List<FEElement>(allElements.Count);

            int counter = 0;
            for (int i = 0; i < allElements.Count; i++)
            {
                if (allElements.Values[i].Shape.ToString() == "Quad" || allElements.Values[i].Shape.ToString() == "Tri")
                {
                    allShellElements.Add(allElements.Values[i]);
                    counter++;
                }
            }

            allShellElements.TrimExcess(); // Trim the capacity of the allShellElements list to match the number of elements

            return allShellElements;

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
                fileName = Path.Combine(Path.GetDirectoryName(basePart.FullPath), fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Calculates the in-plane normals for a given array of shell elements and material orientation vectors.
        /// </summary>
        /// <param name="shellElements">An array of shell elements.</param>
        /// <param name="materialOrientationVectors">An array of material orientation vectors. There are assumed to lay in the plane of the element, but given in global coordinate system.</param>
        /// <returns>An array of in-plane normals.</returns>
        public static NXOpen.Vector3d[] GetShellInPlaneNormal(NXOpen.CAE.FEElement[] shellElements, NXOpen.Vector3d[] materialOrientationVectors)
        {
            NXOpen.Vector3d[] inPlaneNormals = new NXOpen.Vector3d[shellElements.Length];

            for (int i = 0; i < shellElements.Length; i++)
            {
                Vector3 matDirection = new Vector3(materialOrientationVectors[i].X, materialOrientationVectors[i].Y, materialOrientationVectors[i].Z);
                // check if matDirection is in the plane of the element
                // this hurts performance though, so might want to remove it for larger models.
                double test = matDirection.Dot(new Vector3(shellElements[i].GetFaceNormal(0).X, shellElements[i].GetFaceNormal(0).Y, shellElements[i].GetFaceNormal(0).Z));
                if (Math.Abs(test) > 1e-6)
                {
                    theLW.WriteFullline("ERROR: Material orientation vector for element " + shellElements[i].Label + " is not in the plane of the element.");
                }
                Vector3 inPlaneNormal = matDirection.Cross(new Vector3(shellElements[i].GetFaceNormal(0).X, shellElements[i].GetFaceNormal(0).Y, shellElements[i].GetFaceNormal(0).Z));
                inPlaneNormals[i] = new Vector3d(inPlaneNormal.x, inPlaneNormal.y, inPlaneNormal.z);
            }

            return inPlaneNormals;
        }
    }
}
