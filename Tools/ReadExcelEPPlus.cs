// By using the EPPlus library you can not only read and write the excel file, but also calculate (eg update the result of formulas after an cell change).
// The downside is that you cannot run this as a journal, since you need to add the EPPLus package.

// Untested!

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
    
    public class ReadExcelEPPlus
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

            string fileName = args[0]; // @"C:\temp\Sample1.xlsx";
        }
    }
}