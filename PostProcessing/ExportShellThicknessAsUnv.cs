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
                PartLoadStatus loadStatus;
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

            WriteThicknessResults(baseFemPart, "Thickness.unv");

            // return to original work part.
            theSession.Parts.SetWork(basePart);
        }

        public static void WriteThicknessResults(BaseFemPart baseFemPart, string fileName)
        {
            string[] datasets = CreateThicknessDatasets(baseFemPart);
            fileName = CreateFullPath(fileName);

            // concatenate all datasets
            string unvFile = datasets[0] + Environment.NewLine + datasets[1];

            File.WriteAllText(fileName, unvFile);
        }

        public static string[] CreateThicknessDatasets(BaseFemPart baseFemPart)
        {
            theLW.WriteFullline("---------- WARNING ----------");
            theLW.WriteFullline("The Element-Nodal result Record 14 field 2 is set to 2: ");
            theLW.WriteFullline("'Data present for only first node, all other nodes the same'");
            theLW.WriteFullline("While all nodes are listed individually in Record 15, which is contradictory.");
            theLW.WriteFullline("When using externally, update Record 14 field 2 to 1!");
            theLW.WriteFullline("-------- END WARNING ---------");

            string thicknessDatasetElemental = CreateThicknessHeader(1, "Thickness", "Elemental");
            string thicknessDatasetElementNodal = CreateThicknessHeader(1, "Thickness", "Element-Nodal"); // by providing the same label, both results will get grouped under the loadcase thickness

            SortedList<int, FEElement> allElements = GetAllFEElements(baseFemPart);
            foreach (KeyValuePair<int, FEElement> item in allElements)
            {
                if (item.Value.Shape.ToString() == "Quad" || item.Value.Shape.ToString() == "Tri")
                {
                    thicknessDatasetElemental = thicknessDatasetElemental + CreateThicknessRecord(baseFemPart, item.Value)[0];
                    thicknessDatasetElementNodal = thicknessDatasetElementNodal + CreateThicknessRecord(baseFemPart, item.Value)[1];
                }
            }

            thicknessDatasetElemental = thicknessDatasetElemental + String.Format("{0, 6}", "-1");
            thicknessDatasetElementNodal = thicknessDatasetElementNodal + String.Format("{0, 6}", "-1");

            string[] thicknessDataSets = { thicknessDatasetElemental, thicknessDatasetElementNodal };
            return thicknessDataSets;
        }

        public static string[] CreateThicknessRecord(BaseFemPart baseFemPart, FEElement fEElement)
        {
            // user feedback, but not for all, otherwise some performance hit.
            if (fEElement.Label % 1000 == 0)
            {
                theUFSession.Ui.SetStatus("Generating records for element " + fEElement.Label.ToString()); 
            }
            
            double thickness = -1;
            Unit thicknessUnit;
            fEElement.Mesh.MeshCollector.ElementPropertyTable.GetNamedPropertyTablePropertyValue("Shell Property").PropertyTable.GetScalarWithDataPropertyValue("element thickness", out thickness, out thicknessUnit);

            string Record14Elemental = String.Format("{0, 10}", fEElement.Label) + String.Format("{0, 10}", "1") + Environment.NewLine;
            string Record15Elemental = String.Format("{0, 13}", thickness) + Environment.NewLine;

            // even though the second record is 1 (data present for all nodes) SimCenter will not read it properly use the first value for all nodes!!
            // some versions of (NX12) will even give a "result file in wrong format" error. In this case, simply change the value to 2
            string Record14ElemenNodal = String.Format("{0, 10}", fEElement.Label) + String.Format("{0, 10}", "2") +  String.Format("{0, 10}", fEElement.GetNodes().Length) + String.Format("{0, 10}", "1") + Environment.NewLine;

            // Get the element nodal thickness form the element associated data (if defined)
            ElementAssociatedDataUtils elementAssociatedDataUtils = baseFemPart.BaseFEModel.NodeElementMgr.ElemAssociatedDataUtils;
            bool hasAssociatedDataDefined;
            double[] cornerNodeThicknesses = new double[fEElement.GetNodes().Length];
            double[] cornerNodeGapValues = new double[fEElement.GetNodes().Length];
            double zOffset;
            PhysicalPropertyTable physicalPropertyTable;
            CaeElementAssociatedDataUtilsMatOrientationMethod matOriMethod;
            CoordinateSystem coordinateSystem;
            double matOriAngle;
            CaeElementAssociatedDataUtilsCsysDataType caeElementAssociatedDataUtilsCsysDataType;
            Point3d originPoint = new NXOpen.Point3d(0, 0, 0);
            Point3d zAxisPoint = new NXOpen.Point3d(0, 0, 0);
            Point3d planePoint = new NXOpen.Point3d(0, 0, 0);
            int preferredLabel;
            elementAssociatedDataUtils.AskShellData(fEElement, out hasAssociatedDataDefined, out cornerNodeThicknesses, out cornerNodeGapValues, out zOffset, out physicalPropertyTable, out matOriMethod, out coordinateSystem, out matOriAngle, out  caeElementAssociatedDataUtilsCsysDataType, originPoint, zAxisPoint, planePoint, out preferredLabel);
            
            string Record15ElementNodal = "";
            if (!hasAssociatedDataDefined)
            {
                // element has no associated data defined.
                // cornerNodeThicknesses does not contain any data, so need to use element thickness for each node.
                foreach (FENode item in fEElement.GetNodes())
                {
                    Record15ElementNodal = Record15ElementNodal + String.Format("{0, 13}", String.Format("{0:#.#####E+00}", thickness));
                }
            }
            else
            {
                // element has associated data, however this doesn't necessarily mean for thickness, could easily be for example orientation.
                // Somehow the NX developers have been creative again and provide -777777 as thickness if it has not been defined...
                // therefore need to catch it!
                foreach (Double item in cornerNodeThicknesses)
                {
                    if (item == -777777)
                    {
                        // no explicit associated thickness set for this node, so using elemental thickness.
                        Record15ElementNodal = Record15ElementNodal + String.Format("{0, 13}", String.Format("{0:#.#####E+00}", thickness));
                    }
                    else
                    {
                        Record15ElementNodal = Record15ElementNodal + String.Format("{0, 13}", String.Format("{0:#.#####E+00}", item));
                    }
                    
                }
            }


            Record15ElementNodal = Record15ElementNodal + Environment.NewLine;

            string[] result = { Record14Elemental + Record15Elemental, Record14ElemenNodal + Record15ElementNodal };
            return result;
        }

        public static SortedList<int, FEElement> GetAllFEElements(BaseFemPart baseFemPart)
        {
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

        public static string CreateThicknessHeader(int datasetLabel, string datasetName, string type)
        {
            theUFSession.Ui.SetStatus("Creating thickness header");
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
            
            header = header + "RESULT_NAME_KEY " + datasetName + "\n"; // record 4 - analysis dataset name 40A2: using this syntax, will set the resulttype to Thickness
            header = header + "NONE" + "\n"; // record 5 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "EXPRESSION_NAME_KEY " + datasetName + "\n"; // record 6 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "Creation time: "  + DateTime.UtcNow.ToLongDateString() + "\n"; // record 7 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + "NONE" + "\n"; // record 8 - analysis dataset name 40A2: using this syntax, Simcenter will parse it and show in the GUI.
            header = header + String.Format("{0,10}", "1") + String.Format("{0,10}", "1") + String.Format("{0,10}", "1") + String.Format("{0,10}", "94") + String.Format("{0,10}", "2") + String.Format("{0,10}", "1") + "\n"; // record 9
            header = header + String.Format("{0,10}", "1") + String.Format("{0,10}", "0") + String.Format("{0,10}", datasetLabel) + String.Format("{0,10}", "0") + String.Format("{0,10}", "1") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + Environment.NewLine; //record 10: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,10}", "0") + String.Format("{0,10}", "0") + "\n"; //record 11: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + "\n"; // record 12: using this syntax for Simcenter to parse it properly
            header = header + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + String.Format("{0,13}", "0.00000E+00") + "\n"; // record 13: using this syntax for Simcenter to parse it properly
            
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
