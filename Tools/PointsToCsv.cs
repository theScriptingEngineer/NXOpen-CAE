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
    
    public class PointsToCsv
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

            Point[] allPoints = GetAllPoints(basePart);
            // List all points in the listing window
            ListCsv(allPoints);
            // Write points to csv file
            WriteCsv(@"C:\myPoints.csv", allPoints);
        }

        /// <summary>
        /// This function returns all points in a part.
        /// </summary>
        /// <param name="basePart">The part for which to to return the points</param>
        /// <returns>An array with all points in the part.</returns>
        public static Point[] GetAllPoints(BasePart basePart)
        {
            Point[] allPoints = basePart.Points.ToArray();

            return allPoints;
        }

        /// <summary>
        /// This function writes the coordinates of all points to a .csv file.
        /// </summary>
        /// <param name="fileName">The file name and path for the csv file. If no path provided, it is written to the location of the part.</param>
        /// <param name="points">The array of points to write the coordinates to file.</param>
        public static void WriteCsv(string fileName, Point[] points)
        {
            string FullPath = CreateFullPath(fileName, ".csv");
            string fileContent = "";
            foreach (Point item in points)
            {
                fileContent = fileContent + item.Coordinates.X.ToString().Replace(",",".") + ";" + item.Coordinates.Y.ToString().Replace(",",".") + ";" + item.Coordinates.Z.ToString().Replace(",",".") + Environment.NewLine;
            }

            File.WriteAllText(FullPath, fileContent);
        }

        /// <summary>
        /// This function lists the coordinates of all points to the listing window.
        /// </summary>
        /// <param name="points">The array of points to list the coordinates for.</param>
        public static void ListCsv(Point[] points)
        {
            foreach (Point item in points)
            {
                theLW.WriteFullline(item.Coordinates.X.ToString().Replace(",",".") + ";" + item.Coordinates.Y.ToString().Replace(",",".") + ";" + item.Coordinates.Z.ToString().Replace(",","."));
            }
        }

        /// <summary>
        /// This function takes a filename and adds an extension and path of the part if not provided by the user.
        /// If the fileName contains an extension, this function leaves it untouched, othwerwise adds the provided extension, which defaults to .unv.
        /// If the fileName contains a path, this function leaves it untouched, otherwise adds the path of the BasePart as the path.
        /// </summary>
        /// <param name="fileName">The filename with or without path and .unv extension.</param>
        /// <param name="extension">Optional: The extension to add if missing. Defaults to .unv.</param>
        /// <returns>A string with extension and path of basePart if the fileName parameter did not include a path.</returns>
        public static string CreateFullPath(string fileName, string extension = ".unv")
        {
            // check if .unv is included in fileName
            if (Path.GetExtension(fileName).Length == 0)
            {
                fileName = fileName + extension;
            }

            // check if path is included in fileName, if not add path of the .sim file
            string unvFilePath = Path.GetDirectoryName(fileName);
            if (unvFilePath == "")
            {
                // if the basePart file has never been saved, the next will give an error
                fileName = Path.Combine(Path.GetDirectoryName(basePart.FullPath), fileName);
            }

            return fileName;
        }
    }
}
