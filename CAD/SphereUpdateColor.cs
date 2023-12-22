
    using NXOpen; // so we can use NXOpen functionality

    public class Sphere
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

            NXOpen.Features.SphereBuilder sphereBuilder1 = basePart.Features.CreateSphereBuilder(null);
            sphereBuilder1.Diameter.SetFormula("100");

            NXOpen.Features.Sphere sphere =  (NXOpen.Features.Sphere)sphereBuilder1.Commit();
            theLW.WriteFullline(sphere.GetType().ToString());

            Body[] bodies = sphere.GetBodies();
            foreach (Body body in sphere.GetBodies())
            {
                NXOpen.DisplayModification displayModification = theSession.DisplayManager.NewDisplayModification();
                displayModification.ApplyToAllFaces = true;
                displayModification.ApplyToOwningParts = false;
                displayModification.NewColor = 111;
                displayModification.Apply(new DisplayableObject[]{body});
                displayModification.Dispose();
            }
        }
    }
