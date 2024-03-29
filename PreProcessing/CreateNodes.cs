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

    public class CreateNodes
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // this test with 1000 nodes takes about 20s.
            // would probably be faster if not destroying the builder each time.
            // also there is an option SingleOption on the builder
            for (int i = 0; i < 1000; i++)
            {
              CreateNode(12000 + i, 0, 0 ,12000 +i);
            }
        }

        /// <summary>
        /// This method creates a single node with the given label on the given coordinates
        /// </summary>
        /// <param name="label">The array of ResultType used in the envelope</param>
        /// <param name="xCoordinate">The X-Coordinate of the node in the global coordinate system</param>
        /// <param name="yCoordinate">The Y-Coordinate of the node in the global coordinate system</param>
        /// <param name="zCoordinate">The Z-Coordinate of the node in the global coordinate system</param>
        public static void CreateNode(int label, double xCoordinate, double yCoordinate, double zCoordinate)
        {
            FemPart femPart = (FemPart)theSession.Parts.BaseWork;
            
            FEModel fEModel = (FEModel)femPart.BaseFEModel;
            NodeCreateBuilder nodeCreateBuilder = fEModel.NodeElementMgr.CreateNodeCreateBuilder();
            
            nodeCreateBuilder.Label = label;
            
            CoordinateSystem nullNXOpen_CoordinateSystem = null;
            nodeCreateBuilder.Csys = nullNXOpen_CoordinateSystem;
            nodeCreateBuilder.SingleOption = true;
            
            // nodeCreateBuilder.X.SetFormula(XCoordinate.ToString());
            // nodeCreateBuilder.Y.SetFormula(YCoordinate.ToString());
            // nodeCreateBuilder.Z.SetFormula(ZCoordinate.ToString());
            nodeCreateBuilder.X.Value = xCoordinate;
            nodeCreateBuilder.Y.Value = yCoordinate;
            nodeCreateBuilder.Z.Value = zCoordinate;

            Point3d coordinates = new Point3d(xCoordinate, xCoordinate, xCoordinate);
            Point point = femPart.Points.CreatePoint(coordinates);
            nodeCreateBuilder.Point = point;
            
            NXObject nXObject = nodeCreateBuilder.Commit();
            
            nodeCreateBuilder.Csys = nullNXOpen_CoordinateSystem;
            nodeCreateBuilder.DispCsys = nullNXOpen_CoordinateSystem;
            
            nodeCreateBuilder.Destroy();
        }
    }
}
