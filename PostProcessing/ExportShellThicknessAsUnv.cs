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

    public class ExportShellThicknessAsUnv
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

            bool sIUnits = false;
            if (!theSession.IsBatch)
            {
                string inputString = NXOpenUI.NXInputBox.GetInputString("Export in SI units? (yes or no)", "Please select units", "yes");
                if (inputString == "")
                {
                    // user pressed cancel
                    return;
                }
                else if (inputString.Trim().ToLower() == "yes")
                {
                    sIUnits = true;
                }
                else if (inputString.Trim().ToLower() == "no")
                {
                    sIUnits = false;
                }
                else
                {
                    UI theUI = NXOpen.UI.GetUI();
                    theUI.NXMessageBox.Show("Export shell thickness as universal file", NXMessageBox.DialogType.Error, "Please type yes or no");
                    return;
                }

            }

            WriteThicknessResults(baseFemPart, "Thickness.unv", sIUnits);

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
        public static void WriteThicknessResults(BaseFemPart baseFemPart, string fileName, bool sIUnits = true)
        {
            string[][] datasets = CreateThicknessDatasets(baseFemPart, sIUnits);
            fileName = CreateFullPath(fileName);

            theUFSession.Ui.SetStatus("Writing universal file");
            using(StreamWriter writetext = new StreamWriter(fileName))
            {
                for (int i = 0; i < datasets.Length; i++)
                {
                    for (int j = 0; j < datasets[i].Length; j++)
                    {
                        writetext.Write(datasets[i][j]);
                    }

                    writetext.Write(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// This method generates the universal file thickness datasets as both elemental and 
        /// element-nodal result. SI units can be specified.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to generate the thickness result for.</param>
        /// <param name="sIUnits">True divides thickness value by 1000, false leaves thickness value in SC3D units</param>
        public static string[][] CreateThicknessDatasets(BaseFemPart baseFemPart, bool sIUnits)
        {
            theLW.WriteFullline("---------- WARNING ----------");
            theLW.WriteFullline("The Element-Nodal result Record 14 field 2 is set to 2: ");
            theLW.WriteFullline("'Data present for only first node, all other nodes the same'");
            theLW.WriteFullline("While all nodes are listed individually in Record 15, which is contradictory.");
            theLW.WriteFullline("When using externally, update Record 14 field 2 to 1!");
            theLW.WriteFullline("-------- END WARNING ---------");

            SortedList<int, FEElement> allElements = GetAllFEElements(baseFemPart);

            string[] thicknessDatasetElemental = new string[allElements.Count + 2];
            string[] thicknessDatasetElementNodal = new string[allElements.Count + 2];
            thicknessDatasetElemental[0] = CreateThicknessHeader(1, "Thickness", "Elemental");
            thicknessDatasetElementNodal[0] = CreateThicknessHeader(1, "Thickness", "Element-Nodal"); // by providing the same label, both results will get grouped under the loadcase thickness

            //  foreach (KeyValuePair<int, FEElement> item in allElements)
            for (int i = 0; i < allElements.Count; i++)
            {
                if (allElements.Values[i].Shape.ToString() == "Quad" || allElements.Values[i].Shape.ToString() == "Tri")
                {
                    thicknessDatasetElemental[i + 1] = CreateThicknessRecords(allElements.Values[i], sIUnits)[0];
                    thicknessDatasetElementNodal[i + 1] = CreateThicknessRecords(allElements.Values[i], sIUnits)[1];
                }
            }

            thicknessDatasetElemental[thicknessDatasetElemental.Length - 1] = String.Format("{0, 6}", "-1");
            thicknessDatasetElementNodal[thicknessDatasetElementNodal.Length - 1] = String.Format("{0, 6}", "-1");

            string[][] thicknessDataSets = { thicknessDatasetElemental, thicknessDatasetElementNodal };
            return thicknessDataSets;
        }

        /// <summary>
        /// This function generates result records where the result is a shell element thickness.
        /// </summary>
        /// <param name="fEElement">The FEElement to generate the thickness datasets for.</param>
        /// <param name="sIUnits">True divides thickness value by 1000, false leaves thickness value in SC3D units</param>
        /// <returns>An array with the elemental and element-nodal record for the given FEElement.</returns>
        public static string[] CreateThicknessRecords(FEElement fEElement, bool sIUnits)
        {
            // passing elementAssociatedDataUtils object for performance, so it does not need be be created for each element.
            
            // user feedback, but not for all, otherwise some performance hit.
            if (fEElement.Label % 1000 == 0)
            {
                theUFSession.Ui.SetStatus("Generating records for element " + fEElement.Label.ToString());
            }
            
            double thickness = -1;
            Unit thicknessUnit;
            fEElement.Mesh.MeshCollector.ElementPropertyTable.GetNamedPropertyTablePropertyValue("Shell Property").PropertyTable.GetScalarWithDataPropertyValue("element thickness", out thickness, out thicknessUnit);

            if (sIUnits) {thickness = thickness / 1000;}

            string Record14Elemental = String.Format("{0, 10}", fEElement.Label) + String.Format("{0, 10}", "1") + Environment.NewLine;
            string Record15Elemental = String.Format("{0, 13}", thickness) + Environment.NewLine;

            // even though the second record is 1 (data present for all nodes) SimCenter will not read it properly use the first value for all nodes!!
            // some versions of (NX12) will even give a "result file in wrong format" error. In this case, simply change the value to 2
            string Record14ElementNodal = String.Format("{0, 10}", fEElement.Label) + String.Format("{0, 10}", "2") +  String.Format("{0, 10}", fEElement.GetNodes().Length) + String.Format("{0, 10}", "1") + Environment.NewLine;

            string Record15ElementNodal = "";
            Record15ElementNodal = Record15ElementNodal + String.Format("{0, 13}", String.Format("{0:#.#####E+00}", thickness));

            Record15ElementNodal = Record15ElementNodal + Environment.NewLine;

            string[] result = { Record14Elemental + Record15Elemental, Record14ElementNodal + Record15ElementNodal };
            return result;
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
        /// Creates a universal file dataset header.
        /// </summary>
        /// <param name="datasetLabel">The label for the dataset.</param>
        /// <param name="datasetName">The name for the dataset.</param>
        /// <param name="type">The type of result for the dataset: "Elemental" or "Element-Nodal"</param>
        /// <returns>The header as a string.</returns>
        public static string CreateThicknessHeader(int datasetLabel, string datasetName, string type)
        {
            string header = "";
            header = header + String.Format("{0, 6}", "-1") + Environment.NewLine; // every dataset starts with -1
            header = header + String.Format("{0, 6}", "2414") + Environment.NewLine; // this is the header for dataset 2414
            header = header + String.Format("{0, 10}", datasetLabel) + Environment.NewLine; // record 1
            header = header + "LOADCASE_NAME_KEY " + datasetName + Environment.NewLine; // record 2 - analysis dataset name 40A2: using this syntax, SimCenter will set the load case name to "datasetName"
            
            // record 3
            if (type.ToLower().Trim() == "elemental")
            {
                header = header + String.Format("{0, 10}", "2") + Environment.NewLine; // Record 3 - dataset location - data on elements
            }
            else if (type.ToLower().Trim() == "element-nodal" || type.ToLower().Trim() == "elementnodal" || type.ToLower().Trim() == "element nodal")
            {
                header = header + String.Format("{0, 10}", "3") + Environment.NewLine; // Record 3 - dataset location - data at nodes on element
            }
            else
            {
                theLW.WriteFullline("Unsupported type " + type + " in CreateThicknessHeader. Should be \"elemental\" or \"element-nodal\"");
                return null;
            }
            
            header = header + "RESULT_NAME_KEY " + datasetName + Environment.NewLine; // record 4 - analysis dataset name 40A2: using this syntax, will set the resulttype to Thickness
            header = header + "NONE" + Environment.NewLine; // record 5 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "EXPRESSION_NAME_KEY " + datasetName + Environment.NewLine; // record 6 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "Creation time: "  + DateTime.UtcNow.ToLongDateString() + "\n"; // record 7 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "NONE" + Environment.NewLine; // record 8 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + String.Format("{0,10}", "1") + String.Format("{0,10}", "1") + String.Format("{0,10}", "1") + String.Format("{0,10}", "94") + String.Format("{0,10}", "2") + String.Format("{0,10}", "1") + Environment.NewLine; // record 9
            header = header + String.Format("{0,10}", "1") + String.Format("{0,10}", "0") + String.Format("{0,10}", datasetLabel) + String.Format("{0,10}", "0") + String.Format("{0,10}", "1") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + Environment.NewLine; //record 10: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + "\n"; //record 11: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + Environment.NewLine; // record 12: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + Environment.NewLine; // record 13: using this syntax for Simcenter to parse it properly
            
            return header;
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
    }
}
