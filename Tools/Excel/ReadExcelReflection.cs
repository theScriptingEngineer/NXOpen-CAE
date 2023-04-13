// Read Excel data using late binding.
// This allows you to read Excel data in a journal, without the need for compiling.
// Note that it requires that Excel in installed on the machine you run this script on.

// The main contains 2 blocks of code.
// The first block uses a class NXOpenExcel which exposes some basic funtionality to open excel files and read data from it.
// The second block shows how to 

// untested
namespace TheScriptingEngineer
{
    using System;
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using System.Reflection; // for using late binding. This namespace is located in mscorelib.dll and thus available in journaling.
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

            // Get the file to open
            string fileName = args[0]; // @"C:\temp\Sample1.xlsx";

            // using a wrapper class NXOpenExcel to make life easier
            NXOpenExcel xlApp = new NXOpenExcel();
            // This is the path to Microsoft.Office.Interop.Excel.dll
            // It is typically located somewhere buried under C:\WINDOWS\assembly\
            // you could keep a copy on a shared folder, so everyone using this script does not have to look for it on his or her machine.
            xlApp.Init(@"C:\WINDOWS\assembly\GAC_MSIL\Microsoft.Office.Interop.Excel\15.0.0.0__71e9bce111e9429c\Microsoft.Office.Interop.Excel.dll");
            try
            {
                // xlApp.SetVisible(); // uncomment if you want to see what happens.
                object xlWorkbook = xlApp.OpenWorkbook(fileName);
                object xlWorksheet = xlApp.GetSheet(xlWorkbook, 1);
                int lastRow = xlApp.GetLastRow(xlWorksheet);
                int lastColumn = xlApp.GetLastColumn(xlWorksheet);
                theLW.WriteFullline("Cell[A1] = " + xlApp.GetCellValueA1Notation(xlWorksheet, "A1").ToString());
                
                for (int i = 1; i <= lastRow; i++)
                {
                    for (int j = 1; j <= lastColumn; j++)
                    {
                        object value2 = xlApp.GetCellValueIndex(xlWorksheet, i, j);
                        theLW.WriteFullline(value2.ToString());
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                xlApp.Quit();
            }

            // // Using reflection, without a wrapper class.
            // // This gives you assess to all Interop.Excel functionality
            // // This is the path to Microsoft.Office.Interop.Excel.dll
            // // It is typically located somewhere buried under C:\WINDOWS\assembly\
            // // you could keep a copy on a shared folder, so everyone using this script does not have to look for it on his or her machine.
            // System.Reflection.Assembly excelInteropDll = System.Reflection.Assembly.LoadFile(@"C:\WINDOWS\assembly\GAC_MSIL\Microsoft.Office.Interop.Excel\15.0.0.0__71e9bce111e9429c\Microsoft.Office.Interop.Excel.dll");
            // System.Type xlAppType = excelInteropDll.GetType("Microsoft.Office.Interop.Excel.ApplicationClass");
            // object xlApp = Activator.CreateInstance(xlAppType);
            // try
            // {
            //     xlApp.GetType().InvokeMember("Visible", BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance, null, xlApp, new object[] { true });
            //     object xlWorkbooks = xlApp.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, xlApp, null);
            //     object xlWorkbook = xlWorkbooks.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, xlWorkbooks, new object[] { @"C:\temp\ExcelReflection\test.xlsx" });
            //     object xlWorksheets = xlWorkbook.GetType().InvokeMember("Worksheets", BindingFlags.GetProperty, null, xlWorkbook, null);
            //     object xlWorksheet = xlWorksheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, xlWorksheets, new object[] { 1 });
            //     object xlUsedRange = xlWorksheet.GetType().InvokeMember("UsedRange", BindingFlags.GetProperty, null, xlWorksheet, null);
            //     object xlCells = xlUsedRange.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, xlUsedRange, null);
            //     object xlRows = xlCells.GetType().InvokeMember("Rows", BindingFlags.GetProperty, null, xlCells, null);
            //     object xlColumns = xlCells.GetType().InvokeMember("Columns", BindingFlags.GetProperty, null, xlCells, null);
            //     int xlRowsCount = (int)xlRows.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, xlRows, null);
            //     int xlColumnsCount = (int)xlRows.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, xlColumns, null);
            //     string[] reference = new string[] { "A1" };
            //     object rangeA1 = xlWorksheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, null, xlWorksheet, reference);
            //     object value2A1 = rangeA1.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, rangeA1, null);
            //     theLW.WriteFullline("Cell[A1] = " + value2A1.ToString());
            //     for (int i = 1; i <= xlRowsCount; i++)
            //     {
            //         for (int j = 1; j <= xlColumnsCount; j++)
            //         {
            //             object[] id = new object[] { i,  j};
            //             object range = xlWorksheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, xlWorksheet, id);
            //             object value2 = range.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, range, null);
            //             theLW.WriteFullline(value2.ToString());
            //         }
            //     }
            // }
            // catch (System.Exception)
            // {
                
            //     throw;
            // }
            // finally
            // {
            //     xlApp.GetType().InvokeMember("Quit", BindingFlags.InvokeMethod, null, xlApp, null);
            // }
        }
    }

    public class NXOpenExcel
    {
        object excelInstance { get; set; }

        public void Init(string filePath)
        {
            // Load the Microsoft.Office.Interop.Excel assembly at runtime
            Assembly officeInteropAssembly = Assembly.LoadFrom(filePath);

            // Get the Excel.Application type from the assembly
            System.Type excelType = officeInteropAssembly.GetType("Microsoft.Office.Interop.Excel.ApplicationClass");

            if (excelType != null)
            {
                excelInstance = Activator.CreateInstance(excelType);
            }
            else
            {
                Console.WriteLine("Excel is not installed on this machine.");
                excelInstance = null;
            }
        }


        public void Quit()
        {
            excelInstance.GetType().InvokeMember("Quit", BindingFlags.InvokeMethod, null, excelInstance, null);
            excelInstance = null;
        }


        public void SetVisible()
        {
            excelInstance.GetType().InvokeMember("Visible", BindingFlags.SetProperty, null, excelInstance, new object[] { true });
        }


        public object OpenWorkbook(string filePath)
        {
            object workbooks = excelInstance.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, excelInstance, null);
            object workbook = workbooks.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, workbooks, new object[] { filePath });
            return workbook;
        }


        public void CloseWorkbook(object workbook)
        {
            excelInstance.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, workbook, new object[] { false });
        }


        public object GetSheet(object workbook, int index)
        {
            // Get a reference to a worksheet
            object worksheets = workbook.GetType().InvokeMember("Worksheets", BindingFlags.GetProperty, null, workbook, null);
            object worksheet = worksheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, worksheets, new object[] { index });
            return worksheet;
        }


        public int GetLastRow(object worksheet)
        {
            // Get the last used row in the worksheet
            object usedRange = worksheet.GetType().InvokeMember("UsedRange", BindingFlags.GetProperty, null, worksheet, null);
            object cells = usedRange.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, usedRange, null);
            object rows = cells.GetType().InvokeMember("Rows", BindingFlags.GetProperty, null, cells, null);
            int lastRow = (int)rows.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, rows, null);

            return lastRow;
        }

        public int GetLastColumn(object worksheet)
        {
            // Get the last used row in the worksheet
            object usedRange = worksheet.GetType().InvokeMember("UsedRange", BindingFlags.GetProperty, null, worksheet, null);
            object cells = usedRange.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, usedRange, null);
            object columns = cells.GetType().InvokeMember("Columns", BindingFlags.GetProperty, null, cells, null);
            int lastColumn = (int)columns.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, columns, null);

            return lastColumn;
        }

        public object GetCellValueA1Notation(object worksheet, string A1Notation)
        {
            // Get the value of a single cell
            string[] id = new string[] { A1Notation };
            object range = worksheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, null, worksheet, id);
            object value2 = range.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, range, null);
            return value2;
        }

        public object GetCellValueIndex(object worksheet, int rowIndex, int columnIndex)
        {
            // Get the value of a single cell
            object[] id = new object[] { rowIndex,  columnIndex};
            object range = worksheet.GetType().InvokeMember("Cells", BindingFlags.GetProperty, null, worksheet, id);
            object value2 = range.GetType().InvokeMember("Value2", BindingFlags.GetProperty, null, range, null);
            return value2;
        }

        public string GetColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public int GetColumnNumber(string columnName)
        {
            int columnNumber = 0;
            int factor = 1;

            for (int i = columnName.Length - 1; i >= 0; i--)
            {
                char letter = columnName[i];
                int value = letter - 'A' + 1;

                columnNumber += value * factor;
                factor *= 26;
            }

            return columnNumber;
        }
    }
}
