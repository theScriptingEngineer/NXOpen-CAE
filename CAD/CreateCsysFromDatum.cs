namespace TheScriptingEngineerCsysFromDatum
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for using list
    using NXOpen;
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpen.Utilities;
    using NXOpen.UF;
    using NXOpenUI;

    public class Program
    {
        static NXOpen.Session theSession = NXOpen.Session.GetSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);
        }

        /// <summary> 
        /// Creates a Cartesion coordinates system using a datumPlane as input.
        /// The orthogonal vectors are constructed in the plane to define the coordinate system, using basic geometry.
        /// </summary>
        /// <param name="datumPlane">Instance of the datumPlane to create the coordinate system for</param>
        /// <returns>A coordinate system which has the same origin as the plane with x and y axis in the plane.</returns>
        public static CartesianCoordinateSystem CreateCsysFromDatum(DatumPlane datumPlane)
        {
            Point3d origin = datumPlane.Origin;
            Vector3d normal = datumPlane.Normal;

            NXOpen.Vector3d global = new Vector3d(1, 0, 0);
            double projection = Math.Abs(normal.X * global.X + normal.Y * global.Y + normal.Z * global.Z);
            if (projection >= 0.999)
            {
                global.X = 0;
                global.Y = 1;
                global.Z = 0;
            }

            // we first project the global onto the plane normal
            // then subtract to get the component of global IN the plane which will be are local axis in the recipe definition
            double projectionMagnitude = global.X * normal.X + global.Y * normal.Y + global.Z * normal.Z;
            NXOpen.Vector3d globalOnNormal = new Vector3d(projectionMagnitude * normal.X, projectionMagnitude * normal.Y, projectionMagnitude * normal.Z);
            NXOpen.Vector3d globalOnPlane = new Vector3d(global.X - globalOnNormal.X, global.Y - globalOnNormal.Y, global.Z - globalOnNormal.Z);

            // normalize
            double magnitude = Math.Sqrt(globalOnPlane.X * globalOnPlane.X + globalOnPlane.Y * globalOnPlane.Y + globalOnPlane.Z * globalOnPlane.Z);
            globalOnPlane = new Vector3d(globalOnPlane.X / magnitude, globalOnPlane.Y / magnitude, globalOnPlane.Z / magnitude);
            
            // cross product of globalOnPlane and normal give vector in plane, otrhogonal to globalOnPlane
            NXOpen.Vector3d globalOnPlaneNormal = new Vector3d(normal.Y * globalOnPlane.Z - normal.Z * globalOnPlane.Y,
                                           -normal.X * globalOnPlane.Z + normal.Z * globalOnPlane.X,
                                           normal.X * globalOnPlane.Y - normal.Y * globalOnPlane.X);

            magnitude = Math.Sqrt(globalOnPlaneNormal.X * globalOnPlaneNormal.X + globalOnPlaneNormal.Y * globalOnPlaneNormal.Y + globalOnPlaneNormal.Z * globalOnPlaneNormal.Z);
            globalOnPlaneNormal = new Vector3d(globalOnPlaneNormal.X / magnitude, globalOnPlaneNormal.Y / magnitude, globalOnPlaneNormal.Z / magnitude);

            NXOpen.Xform xform = basePart.Xforms.CreateXform(origin, globalOnPlane, globalOnPlaneNormal, NXOpen.SmartObject.UpdateOption.AfterModeling, 1.0);
            NXOpen.CartesianCoordinateSystem coordinateSystem = basePart.CoordinateSystems.CreateCoordinateSystem(xform, NXOpen.SmartObject.UpdateOption.AfterModeling);

            return coordinateSystem;
        }
    }
}