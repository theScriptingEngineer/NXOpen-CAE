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
    
    public class ExportAllSolutions
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            if (!(basePart is SimPart))
            {
                theLW.WriteFullline("ExportAllSolutions needs to start from a .sim file.");
                return;
            }

            // export the results per solution
            SimSolution[] simSolutions = ((SimPart)basePart).Simulation.Solutions.ToArray();
            // keep track of the exported files so we can assemble them
            List<string> allFileNames = new List<string>();
            string dir = System.IO.Path.GetDirectoryName(basePart.FullPath);
            foreach (SimSolution sol in simSolutions)
            {
                PostInput[] exportInputs = new PostInput[sol.StepCount]; // declares the array, NOT the objects in the array (which are still null)
                // initialize all PostInput with the values
                for (int i = 0; i < exportInputs.Length; i++)
                {
                    exportInputs[i] = new PostInput(sol.Name, i + 1, 1, "Stress - Element-Nodal"); // Note that the user starts counting at 1!
                    allFileNames.Add(System.IO.Path.Combine(dir, sol.Name + "_" + (i + 1).ToString() + ".unv"));
                }
                ExportResults(exportInputs);
            }

            // combine all in one file
            AppendFiles(allFileNames.ToArray(), System.IO.Path.Combine(dir, "AllResultsExported.unv"));
        }


        public static void AppendFiles(string[] fileNames, string outputFile)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(outputFile, true)) // "true" for append mode
                {
                    foreach (string fileName in fileNames)
                    {
                        if (File.Exists(fileName))
                        {
                            using (StreamReader reader = new StreamReader(fileName))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    writer.WriteLine(line);
                                }
                            }
                        }
                        else
                        {
                            theLW.WriteFullline("File not found: " + fileName);
                        }
                    }
                }
                theLW.WriteFullline("Files appended successfully!");
            }
            catch (Exception ex)
            {
                theLW.WriteFullline("An error occurred: " + ex.Message);
            }
        }


        public static void ExportResults(PostInput[] postInputs)
        {
            foreach (PostInput item in postInputs)
            {
                ExportResult(item, item.Solution + "_" + item.Subcase.ToString(), true);
            }
        }

        /// <summary>
        /// This function exports a single result.
        /// </summary>
        /// <param name="postInput">The input for the export as a single PostInput. Identifier is ignored.</param>
        /// <param name="unvFileName">Name of the .unv file to write the combined result to. Can be with or without path and file extension. If without, file with .unv extension is saved with the .sim file.</param>
        /// <param name="sIUnits">Force the export to SI units.</param>
        public static void ExportResult(PostInput postInput, string unvFileName, bool sIUnits = false)
        {
            // user feedback
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("ExportResult needs to be started from a .sim file!");
                return;
            }

            SimPart simPart = (SimPart)basePart;

            PostInput[] postInputArray = new PostInput[] { postInput };
            
            // check input and catch errors so that the user doesn't get a error pop-up in SC
            try
            {
                CheckPostInput(postInputArray);
            }
            catch (System.Exception)
            {
                return;
            }

            // add .unv and path from .sim file to unvFileName if required
            string unvFullName = CreateFullPath(unvFileName);

            // Check if unvFullName is not already in use by another companion result
            // No risk of checking the file for this companion result as DeleteCompanionResult has already been called.
            try
            {
                CheckUnvFileName(unvFullName);
            }
            catch (System.Exception ex)
            {
                // ChechUnvFileName throws an error with the message containing the filename and the companion result.
                theLW.WriteFullline(ex.Message);
                return;
            }

            // Load all results
            SolutionResult[] solutionResults = LoadResults(postInputArray);
            
            // Get the requested results
            BaseResultType[] resultTypes = GetResultTypes(postInputArray, solutionResults);
            
            // Get the identifiers
            string[] identifiers = new string[] { "nxopenexportresult" };

            // get the full result name for user feedback. Do this before the try catch block, otherwise the variable is no longer available
            string[] fullResultNames = GetFullResultNames(postInputArray, solutionResults);
            
            // get the unit for each resultType from the result itself
            NXOpen.Unit[] resultUnits = GetResultUnits(resultTypes);

            ResultsCombinationBuilder resultsCombinationBuilder;
            resultsCombinationBuilder = theSession.ResultManager.CreateResultsCombinationBuilder();
            resultsCombinationBuilder.SetResultTypes(resultTypes, identifiers, resultUnits);
            resultsCombinationBuilder.SetFormula("nxopenexportresult");
            resultsCombinationBuilder.SetOutputResultType(ResultsManipulationBuilder.OutputResultType.Full);
            resultsCombinationBuilder.SetIncludeModel(false);
            resultsCombinationBuilder.SetOutputQuantity(resultTypes[0].Quantity);
            resultsCombinationBuilder.SetImportResult(false);
            resultsCombinationBuilder.SetOutputName(fullResultNames[0]);
            resultsCombinationBuilder.SetLoadcaseName(fullResultNames[0]);
            resultsCombinationBuilder.SetOutputFile(unvFullName);
            resultsCombinationBuilder.SetIncompatibleResultsOption(ResultsCombinationBuilder.IncompatibleResults.Skip);
            resultsCombinationBuilder.SetNoDataOption(ResultsCombinationBuilder.NoData.Skip);
            resultsCombinationBuilder.SetEvaluationErrorOption(ResultsCombinationBuilder.EvaluationError.Skip);

            // The following 2 lines have no effect if SetIncludeModel is set to false
            // If SetIncludeModel is true these 2 lines adds dataset 164 to the .unv file
            //resultsCombinationBuilder.SetUnitsSystem(ResultsManipulationBuilder.UnitsSystem.FromResult);
            //resultsCombinationBuilder.SetUnitsSystemResult(solutionResults[0]);

            if (sIUnits)
            {
                // in case you want to set a userdefined units system
                Result.ResultBasicUnit userDefinedUnitSystem = new Result.ResultBasicUnit();
                NXOpen.Unit[] units = simPart.UnitCollection.ToArray();
                // // Print a list of all available units
                // foreach (Unit item in units)
                // {
                //     theLW.WriteFullline(item.TypeName);
                // }
                userDefinedUnitSystem.AngleUnit = Array.Find(units, unit => unit.TypeName == "Radian");
                userDefinedUnitSystem.LengthUnit = Array.Find(units, unit => unit.TypeName == "Meter");
                userDefinedUnitSystem.MassUnit = Array.Find(units, unit => unit.TypeName == "Kilogram");
                userDefinedUnitSystem.TemperatureUnit = Array.Find(units, unit => unit.TypeName == "Celsius");
                userDefinedUnitSystem.ThermalenergyUnit = Array.Find(units, unit => unit.TypeName == "ThermalEnergy_Metric1");
                userDefinedUnitSystem.TimeUnit = Array.Find(units, unit => unit.TypeName == "Second");
                resultsCombinationBuilder.SetUnitsSystem(ResultsManipulationBuilder.UnitsSystem.UserDefined);
                resultsCombinationBuilder.SetUserDefinedUnitsSystem(userDefinedUnitSystem);
                // if set to false, dataset 164 is not added and the results are ambiguos for external use
                resultsCombinationBuilder.SetIncludeModel(false);
            }

            try
            {
                NXOpen.NXObject nXObject = resultsCombinationBuilder.Commit();

                // user feedback
                theLW.WriteFullline("Exported result: ");
                theLW.WriteFullline("Result used: ");
                theLW.WriteFullline(fullResultNames[0]);
            }
            catch (System.Exception)
            {
                theLW.WriteFullline("Error in ExportResult!");
                throw;
            }
            finally
            {
                resultsCombinationBuilder.Destroy();
                Expression[] expressions = simPart.Expressions.ToArray();
                Expression checkExpression = Array.Find(expressions, expression => expression.Name.ToLower() == identifiers[0].ToLower());
                if (checkExpression != null)
                {
                    simPart.Expressions.Delete(checkExpression);
                }
            }
        }

        /// <summary>
        /// This function returns the SimSolution object with the given name.
        /// </summary>
        /// <param name="SolutionName">The name of the solution to return. Case insensitive.</param>
        /// <returns>The SimSolution object if found, Null otherwise.</returns>
        public static SimSolution GetSolution(string SolutionName)
        {
            SimPart simPart = (SimPart)basePart;
            SimSolution[] simSolutions = simPart.Simulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == SolutionName.ToLower());
            return simSolution;
        }

        /// <summary>
        /// Loads the results for the given list of PostInput and returns a list of SolutionResult.
        /// An exception is raised if the result does not exist (-> to check if CreateReferenceResult raises error or returns None)
        /// </summary>
        /// <param name="postInputs">The result of each of the provided solutions is loaded.</param>
        /// <param name="referenceType">The type of SimResultReference eg. Structural</param>
        /// <returns>Returns an array of SolutionResult.</returns>
        public static SolutionResult[] LoadResults(PostInput[] postInputs, string referenceType = "Structural")
        {
            SolutionResult[] solutionResults = new SolutionResult[postInputs.Length];
            SimPart simPart = (SimPart)basePart;
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
        /// This function returns the unit of the first component in each resulttype.
        /// Note that the unit is taken from the SolutionResult and not the SimSolution!
        /// </summary>
        /// <param name="baseResultTypes">The array of BaseResultType defining the results.</param>
        /// <returns>Array of unit for each resulttype.</returns>
        public static Unit[] GetResultUnits(BaseResultType[] baseResultTypes)
        {
            Unit[] resultUnits = new Unit[baseResultTypes.Length];

            for (int i = 0; i < baseResultTypes.Length; i++)
            {
                Result.Component[] components;
                baseResultTypes[i].AskComponents(out components);
                resultUnits[i] = baseResultTypes[i].AskDefaultUnitForComponent(components[0]);
            }

            return resultUnits;
        }


        /// <summary>
        /// Helper function for CombineResults and EnvelopeResults.
        /// Returns the SimResultReferece for the given solution
        /// </summary>
        /// <param name="solutionName">The solution for which to get the "structural" SimResultReference.</param>
        /// <param name="referenceType">The type of SimResultReference eg. Structural</param>
        /// <returns>Returns the "Structural" simresultreference.</returns>
        public static SimResultReference GetSimResultReference(string solutionName, string referenceType = "Structural")
        {
            SimSolution simSolution = GetSolution(solutionName);
            if (simSolution == null)
            {
                // solution with name solutionName not found
                theLW.WriteFullline("GetSimResultReference: Solution with name " + solutionName + " not found.");
                return null;
            }
            SimResultReference simResultReference = (SimResultReference)simSolution.Find(referenceType);
            // SimResultReference simResultReference = simSolution.GetResultReferenceByIndex(0); // for structural

            return simResultReference;
        }

        /// <summary>
        /// This function takes a filename and adds the .unv extension and path of the part if not provided by the user.
        /// If the fileName contains an extension, this function leaves it untouched, othwerwise adds .unv as extension.
        /// If the fileName contains a path, this function leaves it untouched, otherwise adds the path of the BasePart as the path.
        /// Undefined behaviour if basePart has not yet been saved (eg FullPath not available)
        /// </summary>
        /// <param name="fileName">The filename with or without path and .unv extension.</param>
        /// <returns>A string with .unv extension and path of the basePart if the fileName parameter did not include a path.</returns>
        public static string CreateFullPath(string fileName)
        {
            // check if .unv is included in fileName
            if (Path.GetExtension(fileName).Length == 0)
            {
                fileName = fileName + ".unv";
            }

            // check if path is included in fileName, if not add path of the .sim file
            string unvFilePath = Path.GetDirectoryName(fileName);
            if (unvFilePath == "")
            {
                // if the .sim file has never been saved, the next will give an error
                fileName = Path.Combine(Path.GetDirectoryName(basePart.FullPath), fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Check if the provided list of PostInput will not return an error when used in CombineResults.
        /// Identifiers are checked with separate function CheckPostInputIdentifiers
        /// Raises exceptions which can be caught by the user.
        /// </summary>
        /// <param name="postInputs">The array of PostInput to check.</param>
        public static void CheckPostInput(PostInput[] postInputs)
        {
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
                    throw;
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
                    throw;
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
                    throw;
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

        /// <summary>
        /// This function verifies the identifiers in all PostInputs:
        /// Null or empty string.
        /// Reserved expression name.
        /// Use of an expression which already exists.
        /// </summary>
        /// <param name="postInputs">The array of PostInput to check.</param>
        public static void CheckPostInputIdentifiers(PostInput[] postInputs)
        {
            SimPart simPart = (SimPart)basePart;
            foreach (PostInput item in postInputs)
            {
                // is the identifier not null
                if (item.Identifier == "")
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("No identifier provided for solution " + item.Solution + " SubCase " + item.Subcase.ToString() + " iteration " + item.Iteration.ToString() + " ResultType " + item.ResultType);
                    throw new ArgumentException("No identifier provided for solution " + item.Solution + " SubCase " + item.Subcase.ToString() + " iteration " + item.Iteration.ToString() + " ResultType " + item.ResultType);
                }

                // check for reserved expressions
                string[] nxReservedExpressions = {"angle", "angular velocity", "axial", "contact pressure", "Corner ID", "depth", "dynamic viscosity", "edge_id", "element_id", "face_id", "fluid", "fluid temperature", "frequency", "gap distance", "heat flow rate", "iter_val", "length", "mass density", "mass flow rate", "node_id", "nx", "ny", "nz", "phi", "pressure", "radius", "result", "rotational speed", "solid", "solution", "specific heat", "step", "temperature", "temperature difference", "thermal capacitance", "thermal conductivity", "theta", "thickness", "time", "u", "v", "velocity", "volume flow rate", "w", "x", "y", "z"};
                string check = Array.Find(nxReservedExpressions, exp => exp.ToLower() == item.Identifier.ToLower());
                if (check != null)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("Expression with name " + item.Identifier + " is a reserved expression in nx and cannot be used as an identifier.");
                    throw new ArgumentException("Expression with name " + item.Identifier + " is a reserved expression in nx and cannot be used as an identifier.");
                }

                // check if identifier is not already in use as an expression
                Expression[] expressions = basePart.Expressions.ToArray();
                Expression expression = Array.Find(expressions, exp => exp.Name.ToLower() == item.Identifier.ToLower());
                if (expression != null)
                {
                    theLW.WriteFullline("Error in input " + item.ToString());
                    theLW.WriteFullline("Expression with name " + item.Identifier + " already exist in this part and cannot be used as an identifier.");
                    throw new ArgumentException("Expression with name " + item.Identifier + " already exist in this part and cannot be used as an identifier.");
                }
            }
        }

        /// <summary>
        /// This method loops through all solutions and all companion results in these solutions.
        /// It checks if the file name is not already in use by another companion result.
        /// And throws an error if so.
        /// </summary>
        /// <param name="unvFileName">The file name to look for.</param>
        public static void CheckUnvFileName(string unvFileName)
        {
            // Don't perform checks on the file itself in the file system!
            SimPart simPart = (SimPart)basePart;
            // loop through all solutions
            foreach (SimSolution item in simPart.Simulation.Solutions.ToArray())
            {
                SimResultReference simResultReference = GetSimResultReference(item.Name);
                // loop through each companion result.
                foreach (CompanionResult item2 in simResultReference.CompanionResults.ToArray())
                {
                    // create the builder with the companion result, so can access the CompanionResultsFile
                    CompanionResultBuilder companionResultBuilder = simResultReference.CompanionResults.CreateCompanionResultBuilder(item2);
                    if (companionResultBuilder.CompanionResultsFile.ToLower() == unvFileName.ToLower())
                    {
                        // the file is the same, so throw exception
                        throw new ArgumentException("Companion results file name " + unvFileName + " is already used by companion result " + item2.Name);
                    }
                }
            }
        }

        /// <summary>
        /// This function returns a representation of the Results used in PostInputs.
        /// Note that the representation is taken from the SolutionResult and not the SimSolution!
        /// </summary>
        /// <param name="postInputs">The array of PostInput defining the results.</param>
        /// <param name="solutionResults">The solution results from which the representation is obtained.</param>
        /// <returns>Array of string with each representation.</returns>
        public static string[] GetFullResultNames(PostInput[] postInputs, SolutionResult[] solutionResults)
        {
            string[] fullResultNames = new string[postInputs.Length];
            
            for (int i = 0; i < postInputs.Length; i++)
            {
                fullResultNames[i] = fullResultNames[i] + postInputs[i].Solution;
                BaseLoadcase[] baseLoadcases = solutionResults[i].GetLoadcases();
                Loadcase loadcase = (Loadcase)baseLoadcases[postInputs[i].Subcase - 1]; // user starts counting at 1!
                fullResultNames[i] = fullResultNames[i] + "::" + loadcase.Name;

                BaseIteration[] baseIterations = loadcase.GetIterations();
                Iteration iteration = (Iteration)baseIterations[postInputs[i].Iteration - 1]; // user starts counting at 1!
                fullResultNames[i] = fullResultNames[i] + "::" + iteration.Name;

                BaseResultType[] resultTypes = iteration.GetResultTypes();
                ResultType resultType = (ResultType)Array.Find(resultTypes, type => type.Name.ToLower() == postInputs[i].ResultType.ToLower());
                fullResultNames[i] = fullResultNames[i] + "::" + resultType.Name;
            }

            return fullResultNames;
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