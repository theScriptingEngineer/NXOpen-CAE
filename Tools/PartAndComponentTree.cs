// answer to https://community.sw.siemens.com/s/question/0D54O00007a5m2sSAA/how-can-i-find-a-components-owning-assembly-in-nxopen
// answer to https://community.sw.siemens.com/s/question/0D54O00007bOTKnSAO/how-to-get-component-in-main-assembly-to-the-sub-assembly-level
namespace TheScriptingEngineerPrintComponentTree
{
    using System;
    using NXOpen;
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE

    public class Program
    {
        static NXOpen.Session theSession = NXOpen.Session.GetSession();
        static ListingWindow theLW = theSession.ListingWindow;
        static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            BasePart[] allPartsInSession = theSession.Parts.ToArray();
            theLW.WriteFullline("The following parts are loaded in the session: ");
            foreach (BasePart item in allPartsInSession)
            {
                theLW.WriteFullline(string.Format("\t{0, -50}{1, -128}", item.Name, item.FullPath));
            }
            theLW.WriteFullline("");

            BasePart baseDisplayPart = theSession.Parts.BaseDisplay;
            theLW.WriteFullline("The current workpart is: " + basePart.Name + " located in " + basePart.FullPath);
            theLW.WriteFullline("The current displaypart is: " + baseDisplayPart.Name + " located in " + baseDisplayPart.FullPath);
            theLW.WriteFullline("");

            PrintComponentTree(basePart.ComponentAssembly.RootComponent);
            PrintPartTree(basePart);
        }

        /// <summary>
        /// Prints the component tree for the given component to the listing window.
        /// </summary>
        /// <param name="component">Name of the SolverSet.</param>
        /// <param name="requestedLevel">Optional parameter used for creating indentations.</param>
        public static void PrintComponentTree(NXOpen.Assemblies.Component component, int requestedLevel = 0)
        {
            int level = requestedLevel;
            theLW.WriteFullline(Indentation(level) + "| " + component.JournalIdentifier + " is a compont(instance) of " + component.Prototype.OwningPart.Name + " located in " + component.OwningPart.Name);
            NXOpen.Assemblies.Component[] children = component.GetChildren();
            for (int i = children.Length - 1; i >= 0 ; i--)
            {
                PrintComponentTree(children[i], level + 1);
            }
        }

        /// <summary>
        /// Prints the part tree for the given BasePart to the listing window.
        /// </summary>
        /// <param name="basePart">The BasePart to print the tree for.</param>
        /// <param name="requestedLevel">Optional parameter used for creating indentations.</param>
        public static void PrintPartTree(BasePart basePart, int requestedLevel = 0)
        {
            int level = requestedLevel;
            if (basePart as SimPart != null)
            {
                SimPart simPart = (SimPart)basePart;
                theLW.WriteFullline(simPart.Name);

                // PrintPartTree(simPart.ComponentAssembly.RootComponent.GetChildren()[0].Prototype.OwningPart);
                PrintPartTree(simPart.FemPart);
            }
            else if (basePart as AssyFemPart != null)
            {
                AssyFemPart assyFemPart = (AssyFemPart)basePart;
                theLW.WriteFullline(Indentation(level) + "| " + assyFemPart.Name + " located in " + assyFemPart.FullPath + " linked to part " + assyFemPart.FullPathForAssociatedCadPart);
                NXOpen.Assemblies.Component[] children = assyFemPart.ComponentAssembly.RootComponent.GetChildren();
                for (int i = 0; i < children.Length; i++)
                {
                    PrintPartTree(children[i].Prototype.OwningPart, level + 1);
                }
            }
            else if (basePart as FemPart != null)
            {
                FemPart femPart = (FemPart)basePart;
                // try catch since calling femPart.FullPathForAssociatedCadPart on a part which has no cad part results in an error
                try
                {
                    // femPart.MasterCadPart returns the actual part, but is null if the part is not loaded.
                    theLW.WriteFullline(Indentation(level) + "| " + femPart.Name + " which is linked to part " + femPart.FullPathForAssociatedCadPart);
                }
                catch (System.Exception)
                {
                    // femPart has no associated cad part
                    theLW.WriteFullline(Indentation(level) + "| " + femPart.Name + " not linked to a part.");
                }
            }
            else
            {
                theLW.WriteFullline(Indentation(level) + "| " + basePart.Name + " located in " + basePart.FullPath);
                NXOpen.Assemblies.Component[] children = basePart.ComponentAssembly.RootComponent.GetChildren();
                for (int i = 0; i < children.Length; i++)
                {
                    PrintPartTree(children[i].Prototype.OwningPart, level + 1);
                }
            }

        }

        /// <summary>
        /// Helper method to create indentations in the listing window.
        /// </summary>
        /// <param name="level">The depth of the indentations.</param>
        public static string Indentation(int level)
        {
            string indentation = "";
            for (int i = 0; i < level + 1; i++)
            {
                indentation = indentation + "\t";
            }

            return indentation;
        }
    }
}