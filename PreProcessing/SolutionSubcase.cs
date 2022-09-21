namespace TheScriptingEngineerSolutionSubcase
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

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            // open file
            try
            {
                theLW.WriteFullline("Opening file " + args[0]);
                PartLoadStatus partLoadStatus;
                basePart = theSession.Parts.OpenActiveDisplay(args[0], DisplayPartOption.ReplaceExisting, out partLoadStatus);
            }
            catch (System.Exception)
            {
                theLW.WriteFullline("The file " + args[0] + " could not be opened!");
                return;
            }

            CreateSolverSet("DeckLoadPS");
            AddLoadToSolverSet("DeckLoadPS", "DeckLoadPS1");
            AddLoadToSolverSet("DeckLoadPS", "DeckLoadPS2");
            AddLoadToSolverSet("DeckLoadPS", "DeckLoadPS3");

            CreateSolverSet("DeckLoadSB");
            AddLoadToSolverSet("DeckLoadSB", "DeckLoadSB1");
            AddLoadToSolverSet("DeckLoadSB", "DeckLoadSB2");
            AddLoadToSolverSet("DeckLoadSB", "DeckLoadSB3");

            CreateSolverSet("DeckLoadCenter");
            AddLoadToSolverSet("DeckLoadCenter", "DeckLoadCenter1");
            AddLoadToSolverSet("DeckLoadCenter", "DeckLoadCenter2");
            AddLoadToSolverSet("DeckLoadCenter", "DeckLoadCenter3");

            CreateSolverSet("BottomLoadPS");
            AddLoadToSolverSet("BottomLoadPS", "BottomLoadPS1");
            AddLoadToSolverSet("BottomLoadPS", "BottomLoadPS2");
            AddLoadToSolverSet("BottomLoadPS", "BottomLoadPS3");

            CreateSolverSet("BottomLoadSB");
            AddLoadToSolverSet("BottomLoadSB", "BottomLoadSB1");
            AddLoadToSolverSet("BottomLoadSB", "BottomLoadSB2");
            AddLoadToSolverSet("BottomLoadSB", "BottomLoadSB3");

            CreateSolverSet("BottomLoadCenter");
            AddLoadToSolverSet("BottomLoadCenter", "BottomLoadCenter1");
            AddLoadToSolverSet("BottomLoadCenter", "BottomLoadCenter2");
            AddLoadToSolverSet("BottomLoadCenter", "BottomLoadCenter3");

            CreateSolverSet("DeckLoadAft");
            AddLoadToSolverSet("DeckLoadAft", "DeckLoadPS1");
            AddLoadToSolverSet("DeckLoadAft", "DeckLoadSB1");
            AddLoadToSolverSet("DeckLoadAft", "DeckLoadCenter1");

            CreateSolverSet("DeckLoadMiddle");
            AddLoadToSolverSet("DeckLoadMiddle", "DeckLoadPS2");
            AddLoadToSolverSet("DeckLoadMiddle", "DeckLoadSB2");
            AddLoadToSolverSet("DeckLoadMiddle", "DeckLoadCenter2");

            CreateSolverSet("DeckLoadFore");
            AddLoadToSolverSet("DeckLoadFore", "DeckLoadPS3");
            AddLoadToSolverSet("DeckLoadFore", "DeckLoadSB3");
            AddLoadToSolverSet("DeckLoadFore", "DeckLoadCenter3");

            CreateSolverSet("BottomLoadAft");
            AddLoadToSolverSet("BottomLoadAft", "BottomLoadPS1");
            AddLoadToSolverSet("BottomLoadAft", "BottomLoadSB1");
            AddLoadToSolverSet("BottomLoadAft", "BottomLoadCenter1");

            CreateSolverSet("BottomLoadMiddle");
            AddLoadToSolverSet("BottomLoadMiddle", "BottomLoadPS2");
            AddLoadToSolverSet("BottomLoadMiddle", "BottomLoadSB2");
            AddLoadToSolverSet("BottomLoadMiddle", "BottomLoadCenter2");

            CreateSolverSet("BottomLoadFore");
            AddLoadToSolverSet("BottomLoadFore", "BottomLoadPS3");
            AddLoadToSolverSet("BottomLoadFore", "BottomLoadSB3");
            AddLoadToSolverSet("BottomLoadFore", "BottomLoadCenter3");
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////

            theLW.WriteFullline("Creating solution: Transverse");
            CreateSolution("Transverse");
            CreateSubcase("Transverse", "PS");
            CreateSubcase("Transverse", "Center");
            CreateSubcase("Transverse", "SB");

            AddConstraintToSolution("Transverse","XYZ_Fixed");
            AddConstraintToSolution("Transverse","YZ_Fixed");
            AddConstraintToSolution("Transverse","Z_Fixed");

            AddSolverSetToSubcase("Transverse", "PS", "DeckLoadPS");
            AddSolverSetToSubcase("Transverse", "PS", "BottomLoadPS");

            AddSolverSetToSubcase("Transverse", "Center", "DeckLoadCenter");
            AddSolverSetToSubcase("Transverse", "Center", "BottomLoadCenter");

            AddSolverSetToSubcase("Transverse", "SB", "DeckLoadSB");
            AddSolverSetToSubcase("Transverse", "SB", "BottomLoadSB");
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            
            theLW.WriteFullline("Creating solution: Longitudinal");
            CreateSolution("Longitudinal");
            CreateSubcase("Longitudinal", "Aft");
            CreateSubcase("Longitudinal", "Middle");
            CreateSubcase("Longitudinal", "Fwd");

            AddConstraintToSolution("Longitudinal","XYZ_Fixed");
            AddConstraintToSolution("Longitudinal","YZ_Fixed");
            AddConstraintToSolution("Longitudinal","Z_Fixed");

            AddSolverSetToSubcase("Longitudinal", "Aft", "DeckLoadAft");
            AddSolverSetToSubcase("Longitudinal", "Aft", "BottomLoadAft");

            AddSolverSetToSubcase("Longitudinal", "Middle", "DeckLoadMiddle");
            AddSolverSetToSubcase("Longitudinal", "Middle", "BottomLoadMiddle");

            AddSolverSetToSubcase("Longitudinal", "Fwd", "DeckLoadFore");
            AddSolverSetToSubcase("Longitudinal", "Fwd", "BottomLoadFore");
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////

            theLW.WriteFullline("Creating solution: Combined");
            CreateSolution("Combined");
            for (int ii = 0; ii < 3; ii++)
            {
                CreateSubcase("Combined", "Subcase" + (ii + 1).ToString());
            }

            AddConstraintToSolution("Combined","XYZ_Fixed");
            AddConstraintToSolution("Combined","YZ_Fixed");
            AddConstraintToSolution("Combined","Z_Fixed");

            AddSolverSetToSubcase("Combined", "Subcase1", "DeckLoadPS");
            AddSolverSetToSubcase("Combined", "Subcase1", "DeckLoadSB");
            AddSolverSetToSubcase("Combined", "Subcase1", "BottomLoadPS");
            AddSolverSetToSubcase("Combined", "Subcase1", "BottomLoadSB");

            AddSolverSetToSubcase("Combined", "Subcase2", "DeckLoadAft");
            AddSolverSetToSubcase("Combined", "Subcase2", "DeckLoadFore");
            AddSolverSetToSubcase("Combined", "Subcase2", "BottomLoadAft");
            AddSolverSetToSubcase("Combined", "Subcase2", "BottomLoadFore");

            AddSolverSetToSubcase("Combined", "Subcase3", "DeckLoadMiddle");
            AddSolverSetToSubcase("Combined", "Subcase3", "BottomLoadMiddle");
            AddLoadToSubcase("Combined", "Subcase3", "DeckLoadCenter1");
            AddLoadToSubcase("Combined", "Subcase3", "DeckLoadCenter2");
            AddLoadToSubcase("Combined", "Subcase3", "DeckLoadCenter3");
            AddLoadToSubcase("Combined", "Subcase3", "BottomLoadCenter1");
            AddLoadToSubcase("Combined", "Subcase3", "BottomLoadCenter2");
            AddLoadToSubcase("Combined", "Subcase3", "BottomLoadCenter3");


            // save the file
            theLW.WriteFullline("Saving file " + args[0]);
            basePart.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.True);
        }

        public static SimLoadSet CreateSolverSet(string solverSetname)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("CreateSolverSet needs to start from a .sim file!");
                return null;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            SimLoadSet[] simLoadSets = simSimulation.LoadSets.ToArray();
            SimLoadSet simLoadSet = Array.Find(simLoadSets, loadset => loadset.Name.ToLower() == solverSetname.ToLower());
            if (simLoadSet != null)
            {
                // solverset already exits
                theLW.WriteFullline("CreateSolverSet: solver set with name " + solverSetname + " already exists!");
                return null;
            }
            
            NXOpen.CAE.SimLoadSet nullNXOpen_CAE_SimLoadSet = null;
            NXOpen.CAE.SimLoadSetBuilder simLoadSetBuilder;
            simLoadSetBuilder = simSimulation.CreateLoadSetBuilder("StaticLoadSetAppliedLoad", solverSetname, nullNXOpen_CAE_SimLoadSet, 0);

            simLoadSet = (SimLoadSet)simLoadSetBuilder.Commit();
            simLoadSetBuilder.Destroy();

            return simLoadSet;
        }
        
        public static void AddLoadToSolverSet(string solverSetname, string loadName)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("AddLoadToSolverSet needs to start from a .sim file!");
                return;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            SimLoadSet[] simLoadSets = simSimulation.LoadSets.ToArray();
            SimLoadSet simLoadSet = Array.Find(simLoadSets, loadset => loadset.Name.ToLower() == solverSetname.ToLower());
            if (simLoadSet == null)
            {
                // solverset already exits
                theLW.WriteFullline("AddLoadToSolverSet: solver set with name " + solverSetname + " not found!");
                return;
            }

            // get the requested load if it exists
            SimLoad[] simLoads = simSimulation.Loads.ToArray();
            SimLoad simLoad = Array.Find(simLoads, load => load.Name.ToLower() == loadName.ToLower());
            if (simLoad == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddLoadToSolverSet: load with name " + loadName + " not found!");
                return;
            }

            SimLoad[] simLoadMembers = new SimLoad[1];
            simLoadMembers[0] = simLoad;
            simLoadSet.AddMemberLoads(simLoadMembers);
        }
        

        public static void AddSolverSetToSubcase(string solutionName, string subcaseName, string solverSetname)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("AddSolverSetToSubcase needs to start from a .sim file!");
                return;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            // get the requested solution if it exists
            SimSolution[] simSolutions = simSimulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == solutionName.ToLower());
            if (simSolution == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddSolverSetToSubcase: solution with name " + solutionName + " not found!");
                return;
            }

            // check if the subcase already exists in the given solution
            SimSolutionStep simSolutionStep = null;
            for (int ii = 0; ii < simSolution.StepCount; ii++)
            {
                if (simSolution.GetStepByIndex(ii).Name.ToLower() == subcaseName.ToLower())
                {
                    // Subcase already esixts
                    simSolutionStep = simSolution.GetStepByIndex(ii);
                }
            }
            
            //check if subcase found
            if (simSolutionStep == null)
            {
                theLW.WriteFullline("AddSolverSetToSubcase: subcase with name " + subcaseName + " not found in solution " + solutionName + "!");
                return;
            }

            // check if solver set exists
            SimLoadSet[] simLoadSets = simSimulation.LoadSets.ToArray();
            SimLoadSet simLoadSet = Array.Find(simLoadSets, loadset => loadset.Name.ToLower() == solverSetname.ToLower());
            if (simLoadSet == null)
            {
                // solverset already exits
                theLW.WriteFullline("AddSolverSetToSubcase: solver set with name " + solverSetname + " not found!");
                return;
            }

            SimLoadGroup simLoadGroup = (SimLoadGroup)simSolutionStep.Find("Loads");
            // commented code only for reference
            //SimBcGroup[] simBcGroups = simSolutionStep.GetGroups();
            //SimLoadGroup simLoadGroup = (SimLoadGroup)simBcGroups[0];
            simLoadGroup.AddLoadSet(simLoadSet);
        }


        public static void AddLoadToSubcase(string solutionName, string subcaseName, string loadName)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("AddLoadToSubcase needs to start from a .sim file!");
                return;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            // get the requested solution if it exists
            SimSolution[] simSolutions = simSimulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == solutionName.ToLower());
            if (simSolution == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddLoadToSubcase: solution with name " + solutionName + " not found!");
                return;
            }

            // check if the subcase already exists in the given solution
            SimSolutionStep simSolutionStep = null;
            for (int ii = 0; ii < simSolution.StepCount; ii++)
            {
                if (simSolution.GetStepByIndex(ii).Name.ToLower() == subcaseName.ToLower())
                {
                    // Subcase already esixts
                    simSolutionStep = simSolution.GetStepByIndex(ii);
                }
            }
            
            //check if subcase found
            if (simSolutionStep == null)
            {
                theLW.WriteFullline("AddLoadToSubcase: subcase with name " + subcaseName + " not found in solution " + solutionName + "!");
                return;
            }

            // get the requested load if it exists
            SimLoad[] simLoads = simSimulation.Loads.ToArray();
            SimLoad simLoad = Array.Find(simLoads, load => load.Name.ToLower() == loadName.ToLower());
            if (simLoad == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddLoadToSubcase: load with name " + loadName + " not found!");
                return;
            }

            simSolutionStep.AddBc(simLoad);
        }


        public static void AddConstraintToSolution(string solutionName, string constraintName)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("AddConstraintToSolution needs to start from a .sim file!");
                return;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            // get the requested solution if it exists
            SimSolution[] simSolutions = simSimulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == solutionName.ToLower());
            if (simSolution == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddConstraintToSolution: solution with name " + solutionName + " not found!");
                return;
            }

            // get the requested Constraint if it exists
            SimConstraint[] simConstraints = simSimulation.Constraints.ToArray();
            SimConstraint simConstraint = Array.Find(simConstraints, constraint => constraint.Name.ToLower() == constraintName.ToLower());
            if (simConstraint == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("AddConstraintToSolution: constraint with name " + constraintName + " not found!");
                return;
            }

            simSolution.AddBc(simConstraint);
        }
        
        public static SimSolutionStep CreateSubcase(string solutionName, string subcaseName)
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("CreateSubcase needs to start from a .sim file!");
                return null;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            // get the requested solution if it exists
            SimSolution[] simSolutions = simSimulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == solutionName.ToLower());
            if (simSolution == null)
            {
                //Solution with the given name not found
                theLW.WriteFullline("CreateSubcase: Solution with name " + solutionName + " not found!");
                return null;
            }

            // check if the subcase already exists in the given solution
            for (int i = 0; i < simSolution.StepCount; i++)
            {
                if (simSolution.GetStepByIndex(i).Name.ToLower() == subcaseName.ToLower())
                {
                    // Subcase already esixts
                    theLW.WriteFullline("CreateSubcase: subcase with name " + subcaseName + " already exists in solution " + solutionName + "!");
                    return null;
                }
            }
            
            // create the subcase with the given name but don't activate it
            return simSolution.CreateStep(0, false, subcaseName);
        }
        
        public static SimSolution CreateSolution(string solutionName, string outputRequest = "Structural Output Requests1", string bulkDataEchoRequest = "Bulk Data Echo Request1")
        {
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("CreateSolution needs to start from a .sim file!");
                return null;
            }

            SimPart simPart = (SimPart)basePart;
            NXOpen.CAE.SimSimulation simSimulation = simPart.Simulation;

            SimSolution[] simSolutions = simSimulation.Solutions.ToArray();
            SimSolution simSolution = Array.Find(simSolutions, solution => solution.Name.ToLower() == solutionName.ToLower());
            if (simSolution == null)
            {
                // Create the solution
                theLW.WriteFullline("Creating solution " + solutionName);
                simSolution = simSimulation.CreateSolution("NX NASTRAN", "Structural", "SESTATIC 101 - Single Constraint", solutionName, NXOpen.CAE.SimSimulation.AxisymAbstractionType.None);
            }

            // SimSolution property table
            PropertyTable propertyTable = simSolution.PropertyTable;
            
            // obtain all ModelingObjectPropertyTable
            ModelingObjectPropertyTable[] modelingObjectPropertyTables = simPart.ModelingObjectPropertyTables.ToArray();

            // Look for a ModelingObjectPropertyTable with the name "Bulk Data Echo Request1"
            int label = 1000;
            ModelingObjectPropertyTable bulkDataPropertyTable = Array.Find(modelingObjectPropertyTables, table => table.Name.ToLower() == bulkDataEchoRequest.ToLower());
            if (bulkDataPropertyTable == null)
            {
                // did not find ModelingObjectPropertyTable with name bulkDataEchoRequest
                theLW.WriteFullline("Warning: could not find Bulk Data Echo Request with name " + bulkDataEchoRequest + ".Applying default one.");
                // check if default exists
                bulkDataPropertyTable = Array.Find(modelingObjectPropertyTables, table => table.Name.ToLower() == "Bulk Data Echo Request1".ToLower());
                if (bulkDataPropertyTable == null)
                {
                    // create it
                    while (label < 2000)
                    {
                        try
                        {
                            bulkDataPropertyTable = simPart.ModelingObjectPropertyTables.CreateModelingObjectPropertyTable("Bulk Data Echo Request", "NX NASTRAN - Structural", "NX NASTRAN", "Bulk Data Echo Request1", label);
                        }
                        catch (System.Exception)
                        {
                            label++;
                        }
                    }

                    if (label == 2000)
                    {
                        theLW.WriteFullline("Error in CreateSolution: could not create Bulk Data Echo Request with name " + bulkDataEchoRequest);
                        theLW.WriteFullline("No labels available for modeling objects between 1000 and 2000. Please check modeling objects and try again");
                        return null;
                    }
                }
            }

            propertyTable.SetNamedPropertyTablePropertyValue("Bulk Data Echo Request", bulkDataPropertyTable);

            // Look for a ModelingObjectPropertyTable with the name "Bulk Data Echo Request1"
            ModelingObjectPropertyTable outputRequestPropertyTable = Array.Find(modelingObjectPropertyTables, table => table.Name.ToLower() == outputRequest.ToLower());
            if (outputRequestPropertyTable == null)
            {
                // did not find ModelingObjectPropertyTable with name outputRequest
                theLW.WriteFullline("Warning: could not find Output Request with name " + outputRequest + ". Applying default one.");
                // check if default exists
                outputRequestPropertyTable = Array.Find(modelingObjectPropertyTables, table => table.Name.ToLower() == "Structural Output Requests1".ToLower());
                if (outputRequestPropertyTable == null)
                {
                    // create it
                    while (label < 2000)
                    {
                        try
                        {
                            outputRequestPropertyTable = simPart.ModelingObjectPropertyTables.CreateModelingObjectPropertyTable("Structural Output Requests", "NX NASTRAN - Structural", "NX NASTRAN", "Structural Output Requests1", 1001);
                        }
                        catch (System.Exception)
                        {
                            label++;
                        }
                    }

                    if (label == 2000)
                    {
                        theLW.WriteFullline("Error in CreateSolution: could not create Bulk Data Echo Request with name " + bulkDataEchoRequest);
                        theLW.WriteFullline("No labels available for modeling objects between 1000 and 2000. Please check modeling objects and try again");
                        return null;
                    }
                }
            }

            // set Von Mises stress location to corner
            outputRequestPropertyTable.PropertyTable.SetIntegerPropertyValue("Stress - Location", 1);
            propertyTable.SetNamedPropertyTablePropertyValue("Output Requests", outputRequestPropertyTable);

            return simSolution;
        }
    }
}
