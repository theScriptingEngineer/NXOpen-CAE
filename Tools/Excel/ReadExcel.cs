// This is the default way to read and write excel files.
// Note that it requires that Excel in installed on the machine you run this script on.
// The downside is that you cannot run this as a journal, since you need to add the Microsoft.Office.Interop.Excel package in C#

// Code taken from: https://stackoverflow.com/questions/657131/how-to-read-data-of-an-excel-file-using-c

// untested
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

    using Excel = Microsoft.Office.Interop.Excel;
    
    public class ReadExcel
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

            // Get the file to open
            string fileName = args[0]; // @"C:\temp\Sample1.xlsx";

            // The classic way using Microsoft.Office.Interop.Excel
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbooks xlWorkbooks = xlApp.Workbooks;
            xlApp.Visible = true;
            Excel.Workbook xlWorkbook = xlWorkbooks.Open(fileName);
            Excel._Worksheet xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            Console.WriteLine(xlRange.Cells.Rows.Count);
            Console.WriteLine(xlRange.Cells.Columns.Count);
            for (int i = 1; i <= xlRange.Cells.Rows.Count; i++)
            {
                //Excel.Range cell = xlWorksheet.Cells[i, 1] as Excel.Range;
                Excel.Range cell = xlWorksheet.Range["A" + i.ToString(), System.Type.Missing];
                double value2 = (double)cell.Value2;
                Console.WriteLine(value2);
            }
            xlApp.Quit();
        }
    }
}