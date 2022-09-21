// "c:\Program Files\Siemens\Simcenter 12.0\NXBIN\run_journal.exe" "C:\Users\Frederik\Documents\basicCsCourse\Section9\Program.cs" -args "C:\Users\Frederik\Documents\SC12\Section9\hullModelNX12_fem1_sim1.sim"
// "c:\Program Files\Siemens\Simcenter3D_2022.1\NXBIN\run_journal.exe" "C:\Users\Frederik\Documents\basicCsCourse\Section9\Program.cs" -args "C:\Users\Frederik\Documents\SC2022\Section9\hullModelNX12_fem1_sim1.sim"

namespace TheScriptingEngineerSolveSolution
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen; // so we can use NXOpen functionality
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    
    public class Program
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;
        //public static SimSolveManager simSolveManager = SimSolveManager.GetSimSolveManager(theSession);

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            if (args.Length == 0)
            {
                // no arguments passesd
                // write some sort of a help
                theLW.WriteFullline("Need to pass the full path of the .sim file as a first argument.");
                theLW.WriteFullline("All additional parameters should be solutions to solve.");
                theLW.WriteFullline("If no additional parameters are passed, all solutions in the sim file are solved.");
                return;
            }
            // in batch, so need to open file
            // open the file with the first argument
            theLW.WriteFullline("Opening file " + args[0]);
            try
            {
                PartLoadStatus partLoadStatus;
                basePart = theSession.Parts.OpenActiveDisplay(args[0], DisplayPartOption.ReplaceExisting, out partLoadStatus);
            }
            catch (System.Exception)
            {
                theLW.WriteFullline("The file " + args[0] + " could not be opened!");
                return;
            }

            // Check if running from a .sim part
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("SolveSolution needs to start from a .sim file");
                return; 
            }

            if (args.Length == 1)
            {
                // only one argument (file to open) so solve all solutions
                SolveAllSolutions();
            }
            else
            {
                for (int i = 1; i < args.Length; i++)
                {
                    // 2 or more arguments. Solve the solution for each argument. (skip arg[0] becasue that's the sim file)
                    SolveSolution(args[i]);
                }
            }
        }

        /// <summary>This function solves a all solutions in a .sim file.</summary>
        public static void SolveAllSolutions()
        {
            // Note: don't loop over the solutions and solve. This will give a memory access violation error, but will still solve.
            // The error can be avoided by making the simSolveManager a global variable, so it's not on each call.
            theLW.WriteFullline("Solving all solutions:");
            Int32 numsolutionssolved = -1;
            Int32 numsolutionsfailed = -1;
            Int32 numsolutionsskipped = -1;
            SimSolveManager simSolveManager = SimSolveManager.GetSimSolveManager(theSession);
            simSolveManager.SolveAllSolutions(SimSolution.SolveOption.Solve, SimSolution.SetupCheckOption.DoNotCheck, SimSolution.SolveMode.Foreground, false, out numsolutionssolved, out numsolutionsfailed, out numsolutionsskipped);
        }
        
        /// <summary>This function solves a single solution in a .sim file.
        /// NOTE: don't loop over the solutions and solve. This will give a memory access violation error, but will still solve.
        /// The error can be avoided by making the simSolveManager a global variable, so it's not recreated on each call.</summary>
        /// <param name="solutionName">The name of the solution to solve. Case insensitive.</param>
        public static void SolveSolution(string solutionName)
        {
            theLW.WriteFullline("Solving " + solutionName);
            SimPart simPart = (SimPart)basePart;

            // Get the requested solution
            SimSolution[] simSolutions = simPart.Simulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, sol => sol.Name.ToLower() == solutionName.ToLower());
            // check if requested solution has been found
            if (simSolution == null)
            {
                theLW.WriteFullline("Solution with name " + solutionName + " could not be found in " + simPart.FullPath);
            }

            // solve the solution
            SimSolution[] chain = new SimSolution[1];
            chain[0] = simSolution;

            //NXOpen.CAE.SimSolveManager simSolveManager = NXOpen.CAE.SimSolveManager.GetSimSolveManager(theSession);
            Int32 numsolutionssolved = -1;
            Int32 numsolutionsfailed = -1;
            Int32 numsolutionsskipped = -1;
            // SimSolution.SolveMode.Foreground will make the code in this journal wait for the solve to finish. As a result, if you make multiple calls to SolveSolution, all solves will run sequentially
            // SimSolution.SolveMode.Background will kick off the solve and immediately continue the code in this journal. As a result, if you make multiple calls to SolveSolution, all solves will run in parallell
            SimSolveManager simSolveManager = SimSolveManager.GetSimSolveManager(theSession);
            simSolveManager.SolveChainOfSolutions(chain, SimSolution.SolveOption.Solve, SimSolution.SetupCheckOption.DoNotCheck, SimSolution.SolveMode.Foreground, out numsolutionssolved, out numsolutionsfailed, out numsolutionsskipped);

            // user feedback
            theLW.WriteFullline("Solved solution " + solutionName + ". Number solved: " + numsolutionssolved.ToString() + " failed: " + numsolutionsfailed.ToString() + " skipped: " + numsolutionsskipped.ToString());
        }

        /// <summary>
        /// This function solves a .dat file by directly calling the nastran.exe executable.
        /// It takes the location of the nastran.exe executable form the environmental variable UGII_NX_NASTRAN.
        /// By directly calling the nastran executable, a standalone license for the executable is required!
        /// Running this with a desktop license wil result in an error:
        /// "Could not check out license for module: Simcenter Nastran Basic"
        /// </summary>
        /// <param name="datFile">The full path of the .dat file.</param>
        public static void SolveDatFile(string datFile)
        {
            // get the location nastran.exe via the environmental variable
            string UGII_NX_NASTRAN = theSession.GetEnvironmentVariableValue("UGII_NX_NASTRAN");
            theLW.WriteFullline(UGII_NX_NASTRAN);

            // process datFile for path and extension
            string fullDatFile = CreateFullPath(datFile, ".dat");
            
            // create a process to run the nastran executable on the .dat file provided
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = UGII_NX_NASTRAN;
            process.StartInfo.Arguments = fullDatFile;
            // set the working directory to the directory in the .dat file.
            // otherwise the process starts from the location of the nastran.exe, which is very likely not writable by the user (only by the admin)
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(fullDatFile);

            // start the process
            process.Start();
            theLW.WriteFullline("Solve started for" + fullDatFile);
            
            // wait for the process to finish
            process.WaitForExit();
            theLW.WriteFullline("Solve finished.");
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