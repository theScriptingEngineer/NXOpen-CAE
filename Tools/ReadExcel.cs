// you need to download and install the Microsoft Access Database Engine 2010 Redistributable
// on the machine where you are running the journal: http://www.microsoft.com/en-us/download/details.aspx?id=13255

// Code copied from:
// https://qawithexperts.com/article/c-sharp/read-excel-file-in-c-console-application-example-using-oledb/168
// this still produces an error: The 'Microsoft.Jet.OLEDB.4.0' provider is not registered on the local machine.
// found the solution to the error on following links (change from Microsoft.Jet.OLEDB.4.0 to Microsoft.ACE.OLEDB.12.0)
// https://stackoverflow.com/questions/1991643/microsoft-jet-oledb-4-0-provider-is-not-registered-on-the-local-machine
// Produces an error Microsoft.ACE.OLEDB.12.0' provider is not registered on the local machine (install Microsoft Access Database Engine 2010 Redistributable)
// https://stackoverflow.com/questions/6649363/microsoft-ace-oledb-12-0-provider-is-not-registered-on-the-local-machine

// Tested and working in SimCenter version 2023 release 2022.1

namespace TheScriptingEngineer
{
    using System;
    using System.Data;
    using System.Data.OleDb; // dotnet add package System.Data.OleDb
    using System.IO; // for path operations
    using System.Collections.Generic; // for lists
    using NXOpen; // so we can use NXOpen functionality
    using NXOpen.CAE; // so we don't need to start everything with NXOpen.CAE
    using NXOpenUI;
    using NXOpen.UF;
    using NXOpen.Utilities;
    
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
            
            string fileName = args[0]; // @"C:\temp\Sample1.xlsx";
            //this is the connection string which has OLDB 12 Connection and Source URL of file
            //use HDR=YES if first excel row contains headers, HDR=NO means your excel's first row is not headers and it's data.
            string connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + "; Extended Properties='Excel 8.0;HDR=NO;IMEX=1;'";
            theLW.WriteFullline("Using database connection string: " + connString);
          
            // Create the connection object
            System.Data.OleDb.OleDbConnection oledbConn = new OleDbConnection(connString);
            try
            {
                // Open connection
                oledbConn.Open();

                // Create OleDbCommand object and select data from worksheet Sample-spreadsheet-file
                // here sheet name is Sheet1, usually it is Sheet1, Sheet2 etc..
                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn);

                // Create new OleDbDataAdapter
                OleDbDataAdapter oleda = new OleDbDataAdapter();

                oleda.SelectCommand = cmd;

                // Create a DataSet which will hold the data extracted from the worksheet.
                DataSet ds = new DataSet();

                // Fill the DataSet from the data extracted from the worksheet.
                oleda.Fill(ds, "DataTable");

                //loop through each row
                foreach(DataRowView item in ds.Tables[0].DefaultView)
                {
                    theLW.WriteFullline(item.Row.ItemArray[0] +" "+item.Row.ItemArray[1] +" "+item.Row.ItemArray[2]);
                }
            }
            catch (Exception e)
            {
                theLW.WriteFullline("Error :" + e.Message);
            }
            finally
            {
                // Close connection
                oledbConn.Close();
            }
        }
    }
}
