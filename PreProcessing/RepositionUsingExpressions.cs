// If one has expressions defined in the mastercadpart to control the position of the components,
// this script can update these expressions in the mastercadpart, starting from the sim part.
// when combined with SolveSolution one can reposition and solve several postitions of the model in one run.

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
    
    public class RepositionUsingExpressions
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

            // Check if running from a .sim part
            if (basePart as SimPart == null)
            {
                theLW.WriteFullline("RepositionUsingExpressions needs to start from a .sim file");
                return; 
            }

            SetExpressionsInMasterCadPart(new string[]{"main_boom_angle"}, new string[]{"Degrees"}, new double[]{0});

        }

        /// <summary>
        /// This function updates an array of expressions in the mastercad part of an assembly fem.
        /// The values in the arrays need to be in order.
        /// Needs to start from a sim file.
        /// </summary>
        /// <param name="expressionNames">The array of expression names to update. Case-sensitive!</param>
        /// <param name="expressionUnits">The array of units for the expression. Need to be a known NX units. Case-sensitive!</param>
        /// <param name="expressionValues">The array of new values for the expressions</param>
        public static void SetExpressionsInMasterCadPart(string[] expressionNames, string[] expressionUnits, double[] expressionValues)
        {
            if (expressionNames.Length != expressionUnits.Length || expressionNames.Length != expressionValues.Length)
            {
                theLW.WriteFullline("Error in SetExpressionsInMasterCadPart. Arrays with names, units and values need to be the same length!");
                return;
            }
            // assume start from .sim part
            SimPart simPart = (SimPart)basePart;

            // get the assyfempart
            AssyFemPart assyFemPart = (AssyFemPart)simPart.ComponentAssembly.RootComponent.GetChildren()[0].Prototype.OwningPart;

            // get the mastercadpart linked to the assyfem.
            Part masterCadPart = assyFemPart.MasterCadPart;
            PartLoadStatus partLoadStatus;
            if (masterCadPart == null)
            {
                masterCadPart = (Part)theSession.Parts.OpenActiveDisplay(assyFemPart.FullPathForAssociatedCadPart, DisplayPartOption.ReplaceExisting, out partLoadStatus);
            }

            // update the expression in the mastercadpart
            for (int i = 0; i < expressionNames.Length; i++)
            {
                theLW.WriteFullline("Updating " + expressionNames + " in " + masterCadPart.FullPath + " to value of " + expressionValues.ToString() + " " + expressionUnits);
                SetExpressionValue(masterCadPart, expressionNames[i], expressionUnits[i], expressionValues[i]);
            }


            // update the AssyFeModel
            theSession.Parts.SetActiveDisplay(assyFemPart, DisplayPartOption.ReplaceExisting, PartDisplayPartWorkPartOption.SameAsDisplay, out partLoadStatus);
            assyFemPart.BaseFEModel.UpdateFemodel();

            // save the assyfempart after updating
            assyFemPart.Save(NXOpen.BasePart.SaveComponents.True,NXOpen.BasePart.CloseAfterSave.False);

            //return to the simpart (where we started from)
            theSession.Parts.SetActiveDisplay(simPart, DisplayPartOption.ReplaceExisting, PartDisplayPartWorkPartOption.SameAsDisplay, out partLoadStatus);
        }

        /// <summary>
        /// This function updates an expression within a part.
        /// </summary>
        /// <param name="part">The part in which to update the expression.</param>
        /// <param name="expressionName">The name of the expression to update. Case-sensitive!</param>
        /// <param name="expressionUnit">The unit of the expression. Needs to be one of the known NX units. Case-sensitive!</param>
        /// <param name="expressionValue">The new value for the expression</param>
        public static void SetExpressionValue(NXOpen.Part part, string expressionName, string expressionUnit, double expressionValue)
        {
            NXOpen.Expression expression;
            NXOpen.Unit unit;
            try
            {
                expression = ((NXOpen.Expression)part.Expressions.FindObject(expressionName));
                unit = ((NXOpen.Unit)part.UnitCollection.FindObject(expressionUnit));
            }
            catch (System.Exception)
            {
                theLW.WriteFullline("Error when trying to update " + expressionName + " with unit " + expressionUnit + ". Make sure the expression and unit exist");
                return;
            }

            // only update if value is different.
            if (expression.Value != expressionValue)
            {
                part.Expressions.EditExpressionWithUnits(expression, unit, expressionValue.ToString());
            }
            else
            {
                return;
            }

            NXOpen.Session.UndoMarkId markIdMakeUpToDate = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, "Make Up to Date");
            
            NXOpen.NXObject[] objects = new NXOpen.NXObject[1];
            objects[0] = expression;
            theSession.UpdateManager.MakeUpToDate(objects, markIdMakeUpToDate);
            
            NXOpen.Session.UndoMarkId markIdDoUpdate = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, "NX update");
            
            int nErrs1 = theSession.UpdateManager.DoUpdate(markIdDoUpdate);
        }
    }
}