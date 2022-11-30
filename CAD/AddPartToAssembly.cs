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
    
    internal class AddPartToAssembly
    {
        // global variables used throughout
        public static Session theSession = Session.GetSession();
        public static ListingWindow theLW = theSession.ListingWindow;
        public static BasePart basePart = theSession.Parts.BaseWork;

        public static void Main(string[] args)
        {
            // entrypoint for NX
            theLW.Open();
            theLW.WriteFullline("Starting Main() in " + theSession.ExecutingJournal);

            String fileName = "LocationWithFullPath"; // full path of the existing file
            String referenceSetName = "referenceSetName"; // The name of the reference set used to represent the new component
            String componentName = "componentName"; // The name of the new component
            Int16 layer = 1; // The layer to place the new component on.
                           // -1 means use the original layers defined in the component. 0 means use the work layer. 1-256 means use the specified layer.
            Point3d basePoint = new Point3d(0, 0, 0); // Location of the new component
            Matrix3x3 orientation;
            orientation.Xx = 1;
            orientation.Xy = 0;
            orientation.Xz = 0;
            orientation.Yx = 0;
            orientation.Yy = 1;
            orientation.Yz = 0;
            orientation.Zx = 0;
            orientation.Zy = 0;
            orientation.Zz = 1;

            Part Assembly = (Part)basePart;
            PartLoadStatus partLoadStatus;
            Assembly.ComponentAssembly.AddComponent(fileName, referenceSetName, componentName, basePoint, orientation, layer, out partLoadStatus);

            Assembly.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.False);
        }
    }
}
