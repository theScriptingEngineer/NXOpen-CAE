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

    public class ListReactionForces
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // entrypoint for NX
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            int nodeLabel = 2289198;
            ListNodalValues("SolutionName", 1, 1, "Reaction Moment - Nodal", nodeLabel);

        }

        /// <summary>
        /// Retrieves and prints nodal values for a specified node label.
        /// Currently hard coded for a nodal result with X,Y,Z and Magnitude as components
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="subcase">Subcase number.</param>
        /// <param name="iteration">Iteration number.</param>
        /// <param name="resultType">Type of result.</param>
        /// <param name="nodeLabel">Label of the node.</param>
        public static void ListNodalValues(string solutionName, int subcase, int iteration, string resultType, int nodeLabel)
        {
            PostInput postInput = new PostInput(solutionName, subcase, iteration, resultType); // Note that the user starts counting at 1!
            PostInput[] postInputArray = new PostInput[] { postInput };
            SolutionResult[] solutionResults = LoadResults(postInputArray);
            Result result = (Result)solutionResults[0];
            ResultType[] resultTypes = GetResultTypes(postInputArray, solutionResults);
            ResultParameters[] resultParameters = GetResultParamaters(resultTypes, Result.ShellSection.Maximum, Result.Component.Magnitude, false);
            ResultAccess resultAccess = theSession.ResultManager.CreateResultAccess(result, resultParameters[0]);
            resultAccess.AskNodalResultAllComponents(solutionResults[0].AskNodeIndex(nodeLabel), out double[] nodalData);
            // printing is hard coded as X Y Z Magnitude
            theLW.WriteFullline("X:\t" + nodalData[0].ToString() + "\tY:\t" + nodalData[1].ToString() + "\tZ:\t" + nodalData[2].ToString() + "\tMagnitude:\t" + nodalData[3].ToString());
        }

        /// <summary>
        /// Retrieves and prints Element-Nodal values for a specified element label.
        /// Currently hard coded for Beam resultants forces with 2 nodes and 6 components per node.
        /// </summary>
        /// <param name="solutionName">Name of the solution.</param>
        /// <param name="subcase">Subcase number.</param>
        /// <param name="iteration">Iteration number.</param>
        /// <param name="resultType">Type of result.</param>
        /// <param name="elementLabel">Label of the element to list the Element-Nodal results for.</param>
        public static void ListElementNodalValues(string solutionName, int subcase, int iteration, string resultType, int elementLabel)
        {
            PostInput postInput = new PostInput(solutionName, subcase, iteration, resultType); // Note that the user starts counting at 1!
            PostInput[] postInputArray = new PostInput[] { postInput };
            SolutionResult[] solutionResults = LoadResults(postInputArray);
            Result result = (Result)solutionResults[0];
            ResultType[] resultTypes = GetResultTypes(postInputArray, solutionResults);
            ResultParameters[] resultParameters = GetResultParamaters(resultTypes, Result.ShellSection.Maximum, Result.Component.Axial, false);
            ResultAccess resultAccess = theSession.ResultManager.CreateResultAccess(result, resultParameters[0]);
            resultAccess.AskElementNodalResultAllComponents(solutionResults[0].AskElementIndex(elementLabel), out int[] nodeIndex, out int numComponents, out double[] elementNodalData);
            //   for (int i = 0; i < nodeIndex.Length; i++)
            //   {
            //     theLW.WriteFullline("Results for node " + solutionResults[0].AskNodeLabel(nodeIndex[i]));
            //     for (int j = 0; j < numComponents; j++)
            //     {
            //         theLW.WriteFullline("Value: " + elementNodalData[i * numComponents + j]);
            //     }
            //   }

            // printing is hard coded as beam resultant forces
            // String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[0]) for scientific notation, all with 13 length
            // .PadRight(13) for left align (note the 13 in both)
            theLW.WriteFullline(String.Format("{0, 13}", "ID:".PadRight(13)) + String.Format("{0, 13}", "Nxx:".PadRight(13)) + String.Format("{0, 13}", "Myy:".PadRight(13)) + String.Format("{0, 13}", "Mzz:".PadRight(13)) +String.Format("{0, 13}", "Mxx:".PadRight(13)) + String.Format("{0, 13}", "Qxy:".PadRight(13)) + String.Format("{0, 13}", "Qxz:".PadRight(13)));
            theLW.WriteFullline(String.Format("{0, 13}", solutionResults[0].AskNodeLabel(nodeIndex[0]).ToString().PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[0]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[1]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[2]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[3]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[4]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[5]).PadRight(13)));
            theLW.WriteFullline(String.Format("{0, 13}", solutionResults[0].AskNodeLabel(nodeIndex[1]).ToString().PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[6]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[7]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[8]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[9]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[10]).PadRight(13)) +
                                String.Format("{0, 13}", String.Format("{0:#.#####E+00}", elementNodalData[11]).PadRight(13)));
        }


        /// <summary>
        /// Helper function for EnvelopeResults.
        /// Returns an array of resultparameters with the given parameters for each ResultType.
        /// </summary>
        /// <param name="resultTypes">The array of ResultType used in the envelope</param>
        /// <param name="resultComponent">The component for which to perform the envelope operation.</param>
        /// <param name="resultShellSection">The section to use in the envelope operation.</param>
        /// <param name="absolute">envelope using absolute values or signed values. Note that you sort the absolute values and not take the absolute value of the sorted result.</param>
        /// <returns>Returns an array of ResultParameters.</returns>
        public static ResultParameters[] GetResultParamaters(BaseResultType[] resultTypes, Result.ShellSection resultShellSection, Result.Component resultComponent, bool absolute)
        {
            ResultParameters[] resultParametersArray = new ResultParameters[resultTypes.Length];

            for (int i = 0; i < resultTypes.Length; i++)
            {
                ResultParameters resultParameters = theSession.ResultManager.CreateResultParameters();
                resultParameters.SetGenericResultType(resultTypes[i]);
                resultParameters.SetShellSection(resultShellSection);
                resultParameters.SetResultComponent(resultComponent);
                resultParameters.SetCoordinateSystem(Result.CoordinateSystem.AbsoluteRectangular);
                resultParameters.SetSelectedCoordinateSystem(Result.CoordinateSystemSource.None, -1);
                resultParameters.MakeElementResult(false);
                
                Result.Component[] components;
                resultTypes[i].AskComponents(out components);
                Unit unit = resultTypes[i].AskDefaultUnitForComponent(components[0]);
                resultParameters.SetUnit(unit);

                resultParameters.SetAbsoluteValue(absolute);
                resultParameters.SetTensorComponentAbsoluteValue(Result.TensorDerivedAbsolute.DerivedComponent);

                resultParametersArray[i] = resultParameters;
            }
            
            return resultParametersArray;
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
        /// Helper function for CombineResults, ExportResult and SortResults.
        /// Loads the results in the provided array of PostInput
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