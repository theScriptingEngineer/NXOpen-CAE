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
    
    public class CreateRBE3
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static UFSession theUFSession = UFSession.GetUFSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // entrypoint for NX
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            int nodeLabel = 1;
            CreateRBE3NodeToRecipe("PSAftPush", nodeLabel, new string[] {"PSAftPush1", "PSAftPush2", "PSAftPush3", "PSAftPush4", "PSAftPush5", "PSAftPush6", "PSAftPush7", "PSAftPush8", "PSAftPush9", "PSAftPush10", "PSAftPush11", "PSAftPush12"});
            CreateRBE3NodeToRecipe("PSFwdPush", nodeLabel + 1, new string[] {"PSFwdPush1", "PSFwdPush2", "PSFwdPush3", "PSFwdPush4", "PSFwdPush5", "PSFwdPush6", "PSFwdPush7", "PSFwdPush8", "PSFwdPush9", "PSFwdPush10", "PSFwdPush11", "PSFwdPush12"});
            CreateRBE3NodeToRecipe("SBFwdPush", nodeLabel + 2, new string[] {"SBFwdPush1", "SBFwdPush2", "SBFwdPush3", "SBFwdPush4", "SBFwdPush5", "SBFwdPush6", "SBFwdPush7", "SBFwdPush8", "SBFwdPush9", "SBFwdPush10", "SBFwdPush11", "SBFwdPush12"});
            CreateRBE3NodeToRecipe("SBAftPush", nodeLabel + 3, new string[] {"SBAftPush1", "SBAftPush2", "SBAftPush3", "SBAftPush4", "SBAftPush5", "SBAftPush6", "SBAftPush7", "SBAftPush8", "SBAftPush9", "SBAftPush10", "SBAftPush11", "SBAftPush12"});

            nodeLabel = 21;
            CreateRBE3NodeToRecipe("PSAftLowerGuide", nodeLabel, new string[] { "PSAftLowerGuideTarget" });
            CreateRBE3NodeToRecipe("PSFwdLowerGuide", nodeLabel + 1, new string[] { "PSFwdLowerGuideTarget" });
            CreateRBE3NodeToRecipe("SBFwdLowerGuide", nodeLabel + 2, new string[] { "SBFwdLowerGuideTarget" });
            CreateRBE3NodeToRecipe("SBAftLowerGuide", nodeLabel + 3, new string[] { "SBAftLowerGuideTarget" });

        }

        /// <summary>
        /// Creates an RBE3 node and assigns it to a specified connection recipe.
        /// </summary>
        /// <param name="name">The name to assign to the created mesh.</param>
        /// <param name="nodeLabel">The label of the source node for the RBE3 element.</param>
        /// <param name="connectionRecipeNames">
        /// An array of strings representing the names of the target connection recipes.
        /// Each recipe specifies the target nodes for the RBE3 connection.
        /// </param>
        /// <exception cref="System.Exception">
        /// Thrown if the commit operation fails, typically because the specified nodes do not exist.
        /// </exception>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        /// <item>Creates an RBE3 element using the specified source node and target connection recipes.</item>
        /// <item>Associates the created RBE3 element with a specified mesh collector.</item>
        /// <item>Attempts to move the resulting mesh to a new mesh collector if applicable.</item>
        /// <item>Handles errors in target recipe resolution or commit operation gracefully with logs.</item>
        /// </list>
        /// </remarks>
        public static void CreateRBE3NodeToRecipe(string name, int nodeLabel, string[] connectionRecipeNames)
        {
            string collectorName = "myCollectorName";
            BaseFemPart baseFemPart = (BaseFemPart)theSession.Parts.BaseWork;
            BaseFEModel baseFEModel = baseFemPart.BaseFEModel;
            CAEConnectionBuilder cAEConnectionBuilder = baseFEModel.CaeConnections.CreateConnectionBuilder(null);
            cAEConnectionBuilder.ElementType.ElementDimension = ElementTypeBuilder.ElementType.Connection;
            cAEConnectionBuilder.ElementTypeRbe3.ElementDimension = ElementTypeBuilder.ElementType.Spider;
            cAEConnectionBuilder.ElementType.ElementTypeName = "RBE3";
            
            MeshCollector meshCollector = GetMeshCollector(collectorName);
            cAEConnectionBuilder.ElementType.DestinationCollector.ElementContainer = meshCollector;
            cAEConnectionBuilder.ElementTypeRbe3.DestinationCollector.ElementContainer = meshCollector;

            TaggedObject[] sourceObjects = new TaggedObject[1];
            sourceObjects[0] = baseFEModel.FenodeLabelMap.GetNode(nodeLabel);
            bool added1;
            added1 = cAEConnectionBuilder.SourceNodesSelection.Add(sourceObjects);

            TaggedObject[] targetObjects = new NXOpen.TaggedObject[connectionRecipeNames.Length];
            SelectionRecipe[] allSelectionRecipes = baseFemPart.SelectionRecipes.ToArray();
            for (int i = 0; i < connectionRecipeNames.Length; i++)
            {
                BoundingVolumeSelectionRecipe targetRecipe = (BoundingVolumeSelectionRecipe)Array.Find(allSelectionRecipes, recipe => recipe.Name == connectionRecipeNames[i]);
                if (targetRecipe == null)
                {
                    theLW.WriteFullline("ERROR: ELEMENT NOT CREATED: Could not find " + connectionRecipeNames[i]);
                    return;
                }
                targetObjects[i] = targetRecipe;
            }
            added1 = cAEConnectionBuilder.TargetNodesSelection.Add(targetObjects);
            
            try
            {
                cAEConnectionBuilder.Commit();
            }
            catch (System.Exception)
            {
                theLW.WriteFullline("Error in commit. Do the nodes exist?");
                throw;
            }
            
            Mesh mesh = cAEConnectionBuilder.Mesh;
            mesh.SetName(name);
            // move mesh to collector if it exists and cleanup
            MeshCollector newMeshCollector = GetMeshCollector(collectorName);
            MeshCollector oldMeshCollector;
            MeshManager meshManager = (MeshManager)baseFEModel.MeshManager;
            if (newMeshCollector != null)
            {
                oldMeshCollector = (MeshCollector)mesh.MeshCollector;
                meshManager.MoveMeshToNewCollector(mesh, false, oldMeshCollector, newMeshCollector);
                theSession.UpdateManager.AddToDeleteList(oldMeshCollector);
            }

            cAEConnectionBuilder.Destroy();
        }


        /// <summary>
        /// Returns the MeshCollector with the given name, null otherwise.
        /// </summary>
        /// <param name="name">The name of the MeshCollector to return.</param>
        /// <returns>The MeshCollector if found, the default value for T (null?) othwerwise.</returns>
        public static MeshCollector GetMeshCollector(string name)
        {
            BaseFemPart baseFemPart = (BaseFemPart)theSession.Parts.BaseWork;
            BaseFEModel baseFEModel = baseFemPart.BaseFEModel;
            IMeshCollector[] meshCollectors = baseFEModel.MeshManager.GetMeshCollectors();
            MeshCollector meshCollector = (MeshCollector)Array.Find(meshCollectors, collector => collector.Name.ToLower() == name.ToLower());
            return meshCollector;
        }

        /// <summary>
        /// Creates an RBE3 connection element linking a source selection recipe to one or more target selection recipes.
        /// </summary>
        /// <param name="sourceRecipeName">The name of the source selection recipe.</param>
        /// <param name="connectionRecipeNames">
        /// An array of strings representing the names of the target selection recipes.
        /// Each recipe specifies the target nodes for the RBE3 connection.
        /// </param>
        /// <remarks>
        /// This method performs the following tasks:
        /// <list type="bullet">
        /// <item>Identifies the source and target selection recipes by their names.</item>
        /// <item>Creates an RBE3 connection element using the source recipe as the origin.</item>
        /// <item>Links the created connection element to the specified target recipes.</item>
        /// <item>Uses a predefined mesh collector for element storage.</item>
        /// <item>Logs errors if any selection recipe cannot be found.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// Propagated if there is an error during the commit operation.
        /// </exception>
        public static void CreateRBE3RecipeToRecipe(string sourceRecipeName, string[] connectionRecipeNames)
        {
            BaseFemPart baseFemPart = (BaseFemPart)theSession.Parts.BaseWork;
            BaseFEModel baseFEModel = (BaseFEModel)baseFemPart.BaseFEModel;
            CAEConnectionBuilder cAEConnectionBuilder = baseFEModel.CaeConnections.CreateConnectionBuilder(null);
            cAEConnectionBuilder.ElementType.ElementDimension = NXOpen.CAE.ElementTypeBuilder.ElementType.Connection;
            cAEConnectionBuilder.ElementTypeRbe3.ElementDimension = NXOpen.CAE.ElementTypeBuilder.ElementType.Spider;
            cAEConnectionBuilder.ElementType.ElementTypeName = "RBE3";
            
            MeshCollector meshCollector = (MeshCollector)baseFEModel.MeshManager.FindObject("MeshCollector[myMeshCollectorName]");
            cAEConnectionBuilder.ElementType.DestinationCollector.ElementContainer = meshCollector;
            cAEConnectionBuilder.ElementTypeRbe3.DestinationCollector.ElementContainer = meshCollector;

            SelectionRecipe[] allSelectionRecipes = baseFemPart.SelectionRecipes.ToArray();
            BoundingVolumeSelectionRecipe sourceRecipe = (BoundingVolumeSelectionRecipe)Array.Find(allSelectionRecipes, recipe => recipe.Name == sourceRecipeName);
            if (sourceRecipe == null) 
            {
                theLW.WriteFullline("Could not find " + sourceRecipeName);
                return; 
            }

            NXOpen.TaggedObject[] sourceObjects = new NXOpen.TaggedObject[1];
            sourceObjects[0] = sourceRecipe;
            bool added1;
            added1 = cAEConnectionBuilder.SourceNodesSelection.Add(sourceObjects);

            TaggedObject[] targetObjects = new NXOpen.TaggedObject[connectionRecipeNames.Length];
            for (int i = 0; i < connectionRecipeNames.Length; i++)
            {
                BoundingVolumeSelectionRecipe targetRecipe = (BoundingVolumeSelectionRecipe)Array.Find(allSelectionRecipes, recipe => recipe.Name == connectionRecipeNames[i]);
                if (targetRecipe == null)
                {
                    theLW.WriteFullline("Could not find " + connectionRecipeNames[i]);
                    return;
                }
                targetObjects[i] = targetRecipe;
            }
            added1 = cAEConnectionBuilder.TargetNodesSelection.Add(targetObjects);

            cAEConnectionBuilder.Commit();
            cAEConnectionBuilder.Destroy();
        }
    }
}
