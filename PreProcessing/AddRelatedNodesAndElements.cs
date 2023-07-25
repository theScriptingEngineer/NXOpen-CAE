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
    
    public class RelatedNodesAndElements
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            if (basePart as CaePart == null)
            {
                theLW.WriteFullline("AddRelatedNodesAndElements needs to start from a CAE part.");
                return;
            }

            AddRelatedNodesAndElements((CaePart)basePart);
        }

        /// <summary>
        /// This function cycles through all cae groups in a CaePart.
        /// For each group it adds the related nodes and elements for the bodies and faces in the group.
        /// Practical for repopulating groups after a (partial) remesh.
        /// Function is idempotent.
        /// </summary>
        /// <param name="caePart">The CaePart to perform this operation on.</param>
        public static void AddRelatedNodesAndElements(CaePart caePart)
        {
            CaeGroup[] caeGroups = caePart.CaeGroups.ToArray();
            foreach (CaeGroup item in caeGroups)
            {
                theLW.WriteFullline("Processing group " + item.Name);
                List<CAEBody> seedsBody = new List<CAEBody>();
                List<CAEFace> seedsFace = new List<CAEFace>();

                foreach (TaggedObject taggedObject in item.GetEntities())
                {
                    if (taggedObject is CAEBody)
                    {
                        seedsBody.Add((CAEBody)taggedObject);
                    }
                    else if (taggedObject is CAEFace)
                    {
                        seedsFace.Add((CAEFace)taggedObject);
                    }
                }

                SmartSelectionManager smartSelectionManager = caePart.SmartSelectionMgr;

                RelatedElemMethod relatedElemMethodBody = smartSelectionManager.CreateRelatedElemMethod(seedsBody.ToArray(), false);
                RelatedNodeMethod relatedNodeMethodBody = smartSelectionManager.CreateRelatedNodeMethod(seedsBody.ToArray(), false);
                // comment previous line and uncomment next line for NX version 2007 (release 2022.1) and later
                // RelatedNodeMethod relatedNodeMethodBody = smartSelectionManager.CreateNewRelatedNodeMethodFromBodies(seedsBody.ToArray(), false, false);
                
                item.AddEntities(relatedElemMethodBody.GetElements());
                item.AddEntities(relatedNodeMethodBody.GetNodes());

                RelatedElemMethod relatedElemMethodFace = smartSelectionManager.CreateRelatedElemMethod(seedsFace.ToArray(), false);
                RelatedNodeMethod relatedNodeMethodFace = smartSelectionManager.CreateRelatedNodeMethod(seedsFace.ToArray(), false);
                // comment previous line and uncomment next line for NX version 2007 (release 2022.1) and later
                // RelatedNodeMethod relatedNodeMethodFace = smartSelectionManager.CreateNewRelatedNodeMethodFromFaces(seedsFace.ToArray(), false, false);

                item.AddEntities(relatedElemMethodFace.GetElements());
                item.AddEntities(relatedNodeMethodFace.GetNodes());
            }
        }
    }
}