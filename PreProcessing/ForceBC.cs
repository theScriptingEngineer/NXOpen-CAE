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
    
    public class ForceBC
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

            CreateNodalForce(1871, 0, 0, -9810, "DeckLoadPS1");
            CreateNodalForce(1948, 0, 0, -9810, "DeckLoadPS2");
            CreateNodalForce(1908, 0, 0, -9810, "DeckLoadPS3");

            CreateNodalForce(1870, 0, 0, -9810, "DeckLoadSB1");
            CreateNodalForce(1938, 0, 0, -9810, "DeckLoadSB2");
            CreateNodalForce(1907, 0, 0, -9810, "DeckLoadSB3");

            CreateNodalForce(1882, 0, 0, -9810, "DeckLoadCenter1");
            CreateNodalForce(1927, 0, 0, -9810, "DeckLoadCenter2");
            CreateNodalForce(1918, 0, 0, -9810, "DeckLoadCenter3");

            CreateNodalForce(3810, 0, 0, 9810, "BottomLoadPS1");
            CreateNodalForce(3692, 0, 0, 9810, "BottomLoadPS2");
            CreateNodalForce(3739, 0, 0, 9810, "BottomLoadPS3");

            CreateNodalForce(3649, 0, 0, 9810, "BottomLoadSB1");
            CreateNodalForce(3684, 0, 0, 9810, "BottomLoadSB2");
            CreateNodalForce(3710, 0, 0, 9810, "BottomLoadSB3");

            CreateNodalForce(3773, 0, 0, 9810, "BottomLoadCenter1");
            CreateNodalForce(3668, 0, 0, 9810, "BottomLoadCenter2");
            CreateNodalForce(3705, 0, 0, 9810, "BottomLoadCenter3");

            CreateConstraint(1969, 0, 0, 0, -777777, -777777, -777777, "XYZ_Fixed");
            CreateConstraint(2010, -777777, 0, 0, -777777, -777777, -777777, "YZ_Fixed");
            CreateConstraint(2012, -777777, -777777, 0, -777777, -777777, -777777, "Z_Fixed");

            // save the file
            theLW.WriteFullline("Saving file " + args[0]);
            basePart.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.True);
        }

        /// <summary>
        /// Create a constraint on a single node with givel label, using the given settings and name.
        /// </summary>
        /// <param name="nodeLabel">The array of ResultType used in the envelope</param>
        /// <param name="dx">Imposed displacement in global X direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="dy">Imposed displacement in global Y direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="dz">Imposed displacement in global Z direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="rx">Imposed rotation in global X direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="ry">Imposed rotation in global Y direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="rz">Imposed rotation in global Z direction for the constraint. 0: fixed, -777777: free.</param>
        /// <param name="constraintName">Name of the constraint.</param>
        /// <returns>Returns the created constraint object.</returns>
        public static SimBC CreateConstraint(int nodeLabel, double dx, double dy, double dz, double rx, double ry, double rz, string constraintName)
        {
            if (basePart as SimPart == null)
            {
                // caePart is not a SimPart
                theLW.WriteFullline("This program needs to start from a .sim file. Exiting");
                return null;
            }
            SimPart simPart = (SimPart)basePart;
            SimSimulation simSimulation = simPart.Simulation;

            // set solution to inactive so constraint is not automatically added upon creation
            simPart.Simulation.ActiveSolution = null;

            // Check if constraint already exists
            SimBCBuilder simBCBuilder;
            SimConstraint[] simConstraints = simPart.Simulation.Constraints.ToArray();
            SimConstraint simConstraint = Array.Find(simConstraints, constraint => constraint.Name.ToLower() == constraintName.ToLower());
            if (simConstraint == null)
            {
                // no constraint with the given name, thus creating the constraint
                simBCBuilder = simSimulation.CreateBcBuilderForConstraintDescriptor("UserDefinedDisplacementConstraint", constraintName);
            }
            else
            {
                // a constraint with the given name already exists therefore editing the constraint
                simBCBuilder = simSimulation.CreateBcBuilderForBc(simConstraint);
            }

            PropertyTable propertyTable = simBCBuilder.PropertyTable;
            NXOpen.Fields.FieldExpression fieldExpression1 = propertyTable.GetScalarFieldPropertyValue("DOF1");
            NXOpen.Fields.FieldExpression fieldExpression2 = propertyTable.GetScalarFieldPropertyValue("DOF2");
            NXOpen.Fields.FieldExpression fieldExpression3 = propertyTable.GetScalarFieldPropertyValue("DOF3");
            NXOpen.Fields.FieldExpression fieldExpression4 = propertyTable.GetScalarFieldPropertyValue("DOF4");
            NXOpen.Fields.FieldExpression fieldExpression5 = propertyTable.GetScalarFieldPropertyValue("DOF5");
            NXOpen.Fields.FieldExpression fieldExpression6 = propertyTable.GetScalarFieldPropertyValue("DOF6");

            Unit unit1 = (Unit)simPart.UnitCollection.FindObject("MilliMeter");
            NXOpen.Fields.FieldVariable[] indepVarArray1 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression1.EditFieldExpression(dx.ToString(), unit1, indepVarArray1, false);
            propertyTable.SetScalarFieldPropertyValue("DOF1", fieldExpression1);
            
            NXOpen.Fields.FieldVariable[] indepVarArray2 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression2.EditFieldExpression(dy.ToString(), unit1, indepVarArray2, false);
            propertyTable.SetScalarFieldPropertyValue("DOF2", fieldExpression2);
            
            NXOpen.Fields.FieldVariable[] indepVarArray3 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression3.EditFieldExpression(dz.ToString(), unit1, indepVarArray3, false);
            propertyTable.SetScalarFieldPropertyValue("DOF3", fieldExpression3);
            
            NXOpen.Unit unit2 = (NXOpen.Unit)simPart.UnitCollection.FindObject("Degrees");
            NXOpen.Fields.FieldVariable[] indepVarArray4 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression4.EditFieldExpression(rx.ToString(), unit2, indepVarArray4, false);
            propertyTable.SetScalarFieldPropertyValue("DOF4", fieldExpression4);
            
            NXOpen.Fields.FieldVariable[] indepVarArray5 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression5.EditFieldExpression(ry.ToString(), unit2, indepVarArray5, false);
            propertyTable.SetScalarFieldPropertyValue("DOF5", fieldExpression5);
            
            NXOpen.Fields.FieldVariable[] indepVarArray6 = new NXOpen.Fields.FieldVariable[0];
            fieldExpression6.EditFieldExpression(rz.ToString(), unit2, indepVarArray6, false);
            propertyTable.SetScalarFieldPropertyValue("DOF6", fieldExpression6);

            SetManager setManager = simBCBuilder.TargetSetManager;

            FENode fENode = simPart.Simulation.Femodel.FenodeLabelMap.GetNode(nodeLabel);
            if (fENode == null)
            {
                theLW.WriteFullline("No node with label " + nodeLabel + " found in the model! Constraint not created");
                return null;
            }
            SetObject[] objects = new SetObject[1];
            objects[0].Obj = fENode;
            objects[0].SubType = CaeSetObjectSubType.None;
            objects[0].SubId = 0;
            setManager.SetTargetSetMembers(0, CaeSetGroupFilterType.Node, objects);

            SimBC simBC = simBCBuilder.CommitAddBc();
    
            simBCBuilder.Destroy();

            return simBC;
        }

        /// <summary>
        /// Create a nodal force on a node with givel label, using the given force components and default name.
        /// </summary>
        /// <param name="nodeLabel">The array of ResultType used in the envelope</param>
        /// <param name="fx">X component of the force in Newton and global X direction</param>
        /// <param name="fy">Y component of the force in Newton and global y direction</param>
        /// <param name="fz">Z component of the force in Newton and global Z direction</param>
        /// <returns>Returns the created force object.</returns>
        public static SimBC CreateNodalForce(int nodeLabel, double fx, double fy, double fz)
        {
            string defaultName = "Load(" + nodeLabel.ToString() + ")";
            return CreateNodalForce(nodeLabel, fx, fy, fz, defaultName);
        }


        /// <summary>
        /// Create a nodal force on a node with givel label, using the given force components and name.
        /// </summary>
        /// <param name="nodeLabel">The array of ResultType used in the envelope</param>
        /// <param name="fx">X component of the force in Newton and global X direction</param>
        /// <param name="fy">Y component of the force in Newton and global y direction</param>
        /// <param name="fz">Z component of the force in Newton and global Z direction</param>
        /// <param name="forceName">Name of the nodal force.</param>
        /// <returns>Returns the created force object.</returns>
        public static SimBC CreateNodalForce(int nodeLabel, double fx, double fy, double fz, string forceName)
        {         
            if (basePart as SimPart == null)
            {
                // caePart is not a SimPart
                theLW.WriteFullline("This program needs to start from a .sim file. Exiting");
                return null;
            }
            SimPart simPart = (SimPart)basePart;
            SimSimulation simSimulation = simPart.Simulation;

            // set solution to inactive so load is not automatically added upon creation
            simPart.Simulation.ActiveSolution = null;

            // Check if load already exists
            SimBCBuilder simBCBuilder;
            SimLoad[] simLoads = simPart.Simulation.Loads.ToArray();
            SimLoad simLoad = Array.Find(simLoads, load => load.Name.ToLower() == forceName.ToLower());
            if (simLoad == null)
            {
                // no load with the given name, thus creating the load
                simBCBuilder = simSimulation.CreateBcBuilderForLoadDescriptor("ComponentForceField", forceName);
            }
            else
            {
                // a load with the given name already exists therefore editing the load
                simBCBuilder = simSimulation.CreateBcBuilderForBc(simLoad);
            }

            PropertyTable propertyTable = simBCBuilder.PropertyTable;
            SetManager setManager = simBCBuilder.TargetSetManager;

            FENode fENode = simPart.Simulation.Femodel.FenodeLabelMap.GetNode(nodeLabel);
            if (fENode == null)
            {
                theLW.WriteFullline("No node with label " + nodeLabel + " found in the model! Force not created");
                return null;
            }
            SetObject[] objects = new SetObject[1];
            objects[0].Obj = fENode;
            objects[0].SubType = CaeSetObjectSubType.None;
            objects[0].SubId = 0;
            setManager.SetTargetSetMembers(0, CaeSetGroupFilterType.Node, objects);
            
            NXOpen.Fields.VectorFieldWrapper vectorFieldWrapper1 = propertyTable.GetVectorFieldWrapperPropertyValue("CartesianMagnitude");
            
            Unit unit1 = (NXOpen.Unit)simPart.UnitCollection.FindObject("Newton");
            Expression expression1 = simPart.Expressions.CreateSystemExpressionWithUnits(fx.ToString(), unit1);
            Expression expression2 = simPart.Expressions.CreateSystemExpressionWithUnits(fy.ToString(), unit1);
            Expression expression3 = simPart.Expressions.CreateSystemExpressionWithUnits(fz.ToString(), unit1);
            
            NXOpen.Fields.FieldManager fieldManager1 = (NXOpen.Fields.FieldManager)simPart.FindObject("FieldManager");
            Expression[] expressions1 = new Expression[3];
            expressions1[0] = expression1;
            expressions1[1] = expression2;
            expressions1[2] = expression3;
            NXOpen.Fields.VectorFieldWrapper vectorFieldWrapper = fieldManager1.CreateVectorFieldWrapperWithExpressions(expressions1);
            
            propertyTable.SetVectorFieldWrapperPropertyValue("CartesianMagnitude", vectorFieldWrapper);
            propertyTable.SetTablePropertyWithoutValue("CylindricalMagnitude");
            NXOpen.Fields.VectorFieldWrapper nullNXOpen_Fields_VectorFieldWrapper = null;
            propertyTable.SetVectorFieldWrapperPropertyValue("CylindricalMagnitude", nullNXOpen_Fields_VectorFieldWrapper);
            propertyTable.SetTablePropertyWithoutValue("SphericalMagnitude");
            propertyTable.SetVectorFieldWrapperPropertyValue("SphericalMagnitude", nullNXOpen_Fields_VectorFieldWrapper);
            propertyTable.SetTablePropertyWithoutValue("DistributionField");
            NXOpen.Fields.ScalarFieldWrapper nullNXOpen_Fields_ScalarFieldWrapper = null;
            propertyTable.SetScalarFieldWrapperPropertyValue("DistributionField", nullNXOpen_Fields_ScalarFieldWrapper);
            propertyTable.SetTablePropertyWithoutValue("ComponentsDistributionField");
            propertyTable.SetVectorFieldWrapperPropertyValue("ComponentsDistributionField", nullNXOpen_Fields_VectorFieldWrapper);
            
            SimBC simBC = simBCBuilder.CommitAddBc();
            
            simBCBuilder.Destroy();

            return simBC;
        }
    }
}