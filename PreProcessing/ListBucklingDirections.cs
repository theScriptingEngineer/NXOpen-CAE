// This journal list the buckling directions as they should be provided for buckling analysis.
// In the group there should be 3 points (Menu -> Insert -> Model preparation - > Point) which define the stiffener direction and the plane.
// The 2 points with color 162 (Deep brown, rgb 102 51 0) define the stiffener direction (longitudinal direction) in terms of buckling.
// The 3rd point only defines the plane. The direction normal to the stiffener in the plane is determined and listed as the transverse direction in terms of buckling

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
    using NXOpen.VectorArithmetic;
    
    public class ListBuckling
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            NXOpen.CAE.CaePart caePart = (NXOpen.CAE.CaePart)basePart;

            // One could reposition the model (eg afem) to get the buckling directions for different positions of certain parts.

            ListBucklingDirections(caePart);
        }

        public static void ListBucklingDirections(NXOpen.CAE.CaePart caePart, int stiffenerColor = 162)
        {
            foreach (NXOpen.CAE.CaeGroup group in caePart.CaeGroups)
            {
                if (group.Name.Contains("Bcklng"))
                {
                    List<NXOpen.Point> points = new List<NXOpen.Point>();
                    foreach (TaggedObject entity in group.GetEntities())
                    {
                        if (entity is NXOpen.Point)
                        {
                            points.Add((NXOpen.Point)entity);
                        }
                    }

                    if (points.Count != 3)
                    {
                        theLW.WriteFullline("Group " + group.Name + " contains " + points.Count.ToString() + ". Should be 3.");
                        continue;
                    }

                    // get the points with color 162 which define the stiffener dirction
                    List<NXOpen.Point> stiffenerPoints = new List<NXOpen.Point>();
                    foreach (NXOpen.Point pt in points)
                    {
                        if (pt.Color == stiffenerColor)
                        {
                            stiffenerPoints.Add(pt);
                        }
                    }

                    if (stiffenerPoints.Count != 2)
                    {
                        theLW.WriteFullline("Group " + group.Name + " contains " + stiffenerPoints.Count.ToString() + " points with color" + stiffenerColor.ToString() + "(Deep brown). Should be 2.");
                        continue;
                    }

                    // Note using the using NXOpen.VectorArithmetic which has built in functions
                    // Create the stiffener vector
                    Vector3 stiffenerVector = GetDirectionFromPoints(stiffenerPoints);

                    // Create another vector which defines the plane of the 3 points together with the stiffener vector
                    points.Remove(stiffenerPoints[0]);
                    Vector3 vectorInPlane = GetDirectionFromPoints(points);

                    // The cross product is the normal vector of the plane
                    Vector3 normal = stiffenerVector.Cross(vectorInPlane);

                    // The cross of hte normal and the stiffener vector is in the plane and normal to the stiffener vector
                    Vector3 transverse = stiffenerVector.Cross(normal);

                    theLW.WriteFullline(group.Name + ": Longitudinal: [" + stiffenerVector.x.ToString("0.###") + ", " + stiffenerVector.y.ToString("0.###") + ", " + stiffenerVector.z.ToString("0.###") + 
                                                    "] Transverse: [" + transverse.x.ToString("0.###") + ", " + transverse.y.ToString("0.###") + ", " + transverse.z.ToString("0.###") + "]");
                }
            }
        }

        public static Vector3 GetDirectionFromPoints(List<NXOpen.Point> points)
        {
            // Note using the using NXOpen.VectorArithmetic which has built in functions
            if (points.Count != 2)
            {
                theLW.WriteFullline("GetDirectionFromPoints only works with 2 points. " + points.Count.ToString() + "were given.");
                return new Vector3(0, 0, 0);
            }

            Vector3 vector = new Vector3(points[1].Coordinates.X - points[0].Coordinates.X, points[1].Coordinates.Y - points[0].Coordinates.Y, points[1].Coordinates.Z - points[0].Coordinates.Z);
            vector.Normalize();

            return vector;
        }
    }
}