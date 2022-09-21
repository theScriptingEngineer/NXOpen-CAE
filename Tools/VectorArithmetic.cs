namespace TheScriptingEngineerVectorArithmetic
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen;
    using NXOpen.CAE;
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    using NXOpen.VectorArithmetic;

    public class Program
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static CaePart baseCAEPart = (CaePart)theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // entrypoint for NX
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            Vector3 test = new Vector3(1.0f, 2.0f, 3.0f);
            Vector3 sum = test + test;
            theLW.WriteFullline(sum.ToString());

            Vector3 globalX = new Vector3(1.0f, 0f, 0f);
            Vector3 globalY = new Vector3(0f, 1.0f, 0f);
            Vector3 globalZ = new Vector3(0f, 0f, 1.0f);

            theLW.WriteFullline("Some examples of vector arithmetic using NXOpen.VectorArithmetic:");
            theLW.WriteFullline("Using the following vectors it the examples:");
            theLW.WriteFullline("Test: " + PrintVector3(test));
            theLW.WriteFullline("GlobalX: " + PrintVector3(globalX));
            theLW.WriteFullline("GlobalY: " + PrintVector3(globalY));
            theLW.WriteFullline("GlobalZ: " + PrintVector3(globalZ));
            theLW.WriteFullline("");

            theLW.WriteFullline("Test + GlobalX = " + PrintVector3(test + globalX));
            theLW.WriteFullline("Test - GlobalX = " + PrintVector3(test - globalX));
            theLW.WriteFullline("Test x 2 = " + PrintVector3(test * 2));
            theLW.WriteFullline("GlobalZ x 2 = " + PrintVector3(globalZ * 2));
            theLW.WriteFullline("Test - GlobalX - 2 x GlobalY - 3 x GlobalZ = " + PrintVector3(test - globalX - 2 * globalY - 3 * globalZ));
            theLW.WriteFullline("");

            theLW.WriteFullline("Dot product of test and globalX = " + test.Dot(globalX).ToString());
            theLW.WriteFullline("Dot product of test and globalY = " + test.Dot(globalY).ToString());
            theLW.WriteFullline("Dot product of test and globalZ = " + test.Dot(globalZ).ToString());
            theLW.WriteFullline("");

            theLW.WriteFullline("Cross product of test and globalX = " + PrintVector3(test.Cross(globalX)));
            theLW.WriteFullline("Cross product of globalX and globalY = " + PrintVector3(globalX.Cross(globalY)));
            theLW.WriteFullline("");

            test.Normalize();
            theLW.WriteFullline("Test normalized = " + PrintVector3(test));
        }

        public static string PrintVector3(Vector3 vector)
        {
            return "(" + vector.x.ToString() + ", " + vector.y.ToString() + ", " + vector.z.ToString() + ")";
        }
    }
}
