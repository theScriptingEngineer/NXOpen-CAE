// Read Excel data using late binding.
// This allows you to read Excel data in a journal, without the need for compiling.
// Note that it requires that Excel in installed on the machine you run this script on.

// untested
namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using System.Reflection; // for using late binding
    using NXOpen; // so we can use NXOpen functionality
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    
    public class ReadExcelLateBinding
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

            Excel excelApp = new Excel(@"C:\PathTo\Microsoft.Office.Interop.Excel.dll");
            // set visible for testing
            excelApp.SetVisible();
            object myWorkbook = excelApp.OpenWorkbook("C:\\example.xlsx");
            object myWorksheet = excelApp.GetSheet(myWorkbook, 1);

            int lastRow = excelApp.GetLastRow(myWorksheet);

            // Print all rows in the worksheet
            for (int i = 1; i <= lastRow; i++)
            {
                object value = excelApp.GetCell(myWorksheet, "A" + i.ToString());
                theLW.WriteFullline(value.ToString());
            }

            // Close the workbook and quit Excel
            excelApp.CloseWorkbook(myWorkbook);
            excelApp.Quit();
        }
    }
    

    public class Excel
    {
        object app { get; set; }
        System.Type type { get; set; }

        public Excel(string filePath)
        {
            // Load the Microsoft.Office.Interop.Excel assembly at runtime
            //Assembly officeInteropAssembly = Assembly.LoadFrom(@"C:\PathTo\Microsoft.Office.Interop.Excel.dll");
            Assembly officeInteropAssembly = Assembly.LoadFrom(filePath);

            // Get the Excel.Application type from the assembly
            System.Type excelType = officeInteropAssembly.GetType("Microsoft.Office.Interop.Excel.Application");
            type = excelType;

            if (excelType != null)
            {
                app = Activator.CreateInstance(excelType);
            }
            else
            {
                Console.WriteLine("Excel is not installed on this machine.");
                app = null;
            }
        }


        public void Quit()
        {
            type.InvokeMember("Quit", BindingFlags.InvokeMethod, null, app, null);
            app = null;
            type = null;
        }


        public void SetVisible()
        {
            type.InvokeMember("Visible", BindingFlags.SetProperty, null, app, new object[] { true });
        }


        public object OpenWorkbook(string filePath)
        {
            object workbooks = type.InvokeMember("Workbooks", BindingFlags.GetProperty, null, app, null);
            object workbook = type.InvokeMember("Open", BindingFlags.InvokeMethod, null, workbooks, new object[] { filePath });
            return workbook;
        }


        public void CloseWorkbook(object workbook)
        {
            type.InvokeMember("Close", BindingFlags.InvokeMethod, null, workbook, new object[] { false });
        }


        public object GetSheet(object workbook, int index)
        {
            // Get a reference to a worksheet
            object worksheets = type.InvokeMember("Worksheets", BindingFlags.GetProperty, null, workbook, null);
            object worksheet = type.InvokeMember("Item", BindingFlags.GetProperty, null, worksheets, new object[] { index });
            return worksheet;
        }


        public int GetLastRow(object worksheet)
        {
            // Get the last used row in the worksheet
            object usedRange = type.InvokeMember("UsedRange", BindingFlags.GetProperty, null, worksheet, null);
            int lastRow = (int)type.InvokeMember("Row", BindingFlags.GetProperty, null, type.InvokeMember("Cells", BindingFlags.GetProperty, null, usedRange, new object[] { usedRange }), null);
            return lastRow;
        }


        public object GetCell(object worksheet, string reference)
        {
            // Get the value of a single cell
            object cell = type.InvokeMember("Range", BindingFlags.InvokeMethod, null, worksheet, new object[] { reference });
            object value = type.InvokeMember("Value", BindingFlags.GetProperty, null, cell, null);
            return value;
        }
    }

        
}