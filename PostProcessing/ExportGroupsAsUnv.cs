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
    
    public class ExportGroupsAsUnv
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static UFSession theUFSession = UFSession.GetUFSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            if (basePart is CaePart)
            {
                CaePart caePart = (CaePart)basePart;
                WriteGroups(caePart, "Groups.unv");
            }
            else
            {
                theLW.WriteFullline("This function needs to start from a .sim, .afem or .fem.");
                return;
            }
        }

        /// <summary>
        /// This function writes nodes and elements in a group to a universal file.
        /// The content of the file are all groups and the nodes and elements in these groups.
        /// </summary>
        /// <param name="baseFemPart">The BaseFemPart to export the groups for.</param>
        /// <param name="fileName">The name of the universal file to write the groups to</param>
        public static void WriteGroups(CaePart caePart, string fileName)
        {
            string dataset2429 = CreateGroupDatasets(caePart);
            fileName = CreateFullPath(fileName);

            File.WriteAllText(fileName, dataset2429);
        }


        public static string CreateGroupDatasets(CaePart caePart)
        {
            string dataset2429 = "";
            foreach (CaeGroup group in caePart.CaeGroups)
            {
                theUFSession.Ui.SetStatus("Processing group " + group.Name);

                // add the actual nodes and elements
                string[] dataset = CreateGroupDataSet(caePart, group);
                if (dataset.Length == 0)
                {
                    continue;
                }

                // add the headers for the group
                int nOfItems = 0;
                if (dataset.Length == 1)
                {
                    nOfItems = dataset[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length / 2;
                }
                else
                {
                    nOfItems = (dataset.Length - 1) * 4 + (dataset[dataset.Length - 1]).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length / 2;
                }
                dataset2429 = dataset2429 +  "    -1" + Environment.NewLine;
                dataset2429 = dataset2429 + String.Format("{0,6}", "2429") + Environment.NewLine;
                dataset2429 = dataset2429 + String.Format("{0,10}", group.Label) + 
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", "0") +
                                            String.Format("{0,10}", nOfItems) + Environment.NewLine;
                dataset2429 = dataset2429 + group.Name + Environment.NewLine;
                dataset2429 = dataset2429 + string.Join(Environment.NewLine, dataset) + Environment.NewLine;
                dataset2429 = dataset2429 +  "    -1" + Environment.NewLine;
            }

            return dataset2429;
        }


        public static string[] CreateGroupDataSet(CaePart caePart, CaeGroup group)
        {
            theLW.WriteFullline("Processing " + group.Name);
            List<string> datasetItems = new List<string>();

            // Get the node labels in the group
            IList<int> allNodeLabels = GetNodesInGroup(caePart, group).Keys;
            theLW.WriteFullline("Found " + allNodeLabels.Count + " nodes");

            if (allNodeLabels.Count != 0)
            {
                foreach (int label in allNodeLabels)
                {
                    datasetItems.Add(String.Format("{0,10}", 7) + String.Format("{0,10}", label));
                }
            }

            // Get the element labels in the group
            IList<int> allElementLabels = GetElementsInGroup(caePart, group).Keys;
            theLW.WriteFullline("Found " + allElementLabels.Count + " elements");

            if (allElementLabels.Count != 0)
            {
                foreach (int label in allElementLabels)
                {
                    datasetItems.Add(String.Format("{0,10}", 8) + String.Format("{0,10}", label));
                }
            }

            // More items could be implemented here (eg bodies, faces, ...)

            // Generate the string for the dataset
            List<string> dataset = new List<string>();
            for (int i = 0; i < datasetItems.Count - datasetItems.Count % 4; i += 4)
            {
                dataset.Add(datasetItems[i] + datasetItems[i + 1] + datasetItems[i + 2] + datasetItems[i + 3]);
            }

            // Handle the remainder of items (less than 4)
            string temp = "";
            for (int i = datasetItems.Count - datasetItems.Count % 4; i < datasetItems.Count; i++)
            {
                temp = temp + datasetItems[i];
            }

            if (temp != "" ) 
            {
                dataset.Add(temp);
                temp = "";
            }
            
            return dataset.ToArray();

        }


        /// <summary>
        /// Get all all nodes in a group, including the nodes in a mesh should the group contain meshes.
        /// </summary>
        /// <param name="group">The group object to get the nodes from.</param>
        /// <returns>An sorted list of the node label and the node object.</returns>
        public static SortedList<int, FENode> GetNodesInGroup(CaePart caePart, CaeGroup group)
        {
            SmartSelectionManager smartSelectionManager = caePart.SmartSelectionMgr;
            SortedList<int, FENode> allNodes = new SortedList<int, FENode>();
            foreach (TaggedObject taggedObject in group.GetEntities())
            {
                if (taggedObject is FENode)
                {
                    allNodes.Add(((FENode)taggedObject).Label, (FENode)taggedObject);
                }
                if (taggedObject is Mesh)
                {
                    Mesh seedsMesh = (Mesh)taggedObject;
                    RelatedNodeMethod relatedNodeMethodMesh = smartSelectionManager.CreateNewRelatedNodeMethodFromMesh(seedsMesh, false, false);
                    foreach (FENode node in relatedNodeMethodMesh.GetNodes())
                    {
                        // meshes share nodes. cannot add the same node twice
                        try
                        {
                            allNodes.Add(node.Label, node);
                        }
                        catch (System.Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            return allNodes;
        }


        /// <summary>
        /// Get all all nodes in a group, including the nodes in a mesh should the group contain meshes.
        /// </summary>
        /// <param name="group">The group object to get the nodes from.</param>
        /// <returns>An sorted list of the node label and the node object.</returns>
        public static SortedList<int, FEElement> GetElementsInGroup(CaePart caePart, CaeGroup group)
        {
            SmartSelectionManager smartSelectionManager = caePart.SmartSelectionMgr;
            SortedList<int, FEElement> allElements = new SortedList<int, FEElement>();
            foreach (TaggedObject taggedObject in group.GetEntities())
            {
                if (taggedObject is FEElement)
                {
                    allElements.Add(((FEElement)taggedObject).Label, (FEElement)taggedObject);
                }
                if (taggedObject is Mesh)
                {
                    Mesh seedsMesh = (Mesh)taggedObject;
                    RelatedElemMethod relatedElemMethodMesh = smartSelectionManager.CreateRelatedElemMethod(seedsMesh, false);
                    foreach (FEElement element in relatedElemMethodMesh.GetElements())
                    {
                        allElements.Add(element.Label, element);
                    }
                }
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
        public static string CreateGroupHeader(int datasetLabel, string datasetName, string type)
        {
            theUFSession.Ui.SetStatus("Creating group header");
            string header = "";
            header = header + String.Format("{0, 6}", "-1") + Environment.NewLine; // every dataset starts with -1
            header = header + String.Format("{0, 6}", "2414") + Environment.NewLine; // this is the header for dataset 2414
            header = header + String.Format("{0, 10}", datasetLabel) + Environment.NewLine; // record 1
            header = header + "LOADCASE_NAME_KEY " + datasetName + Environment.NewLine; // record 2 - analysis dataset name 40A2: using this syntax, SimCenter will set the load case name to "datasetName"
            
            // record 3
            header = header + String.Format("{0, 10}", "2") + Environment.NewLine; // Record 3 - dataset location - data on elements
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