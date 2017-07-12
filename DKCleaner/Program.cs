using MiscUtil.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DKCleaner
{
    class Program
    {
        /// <summary>
        /// Clean up door king files extracted from their system so we can use them in excel and dataframe's, etc.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Input file as an argument or a pipe.            
            var input = args.Length == 1
                ? File.OpenText(args[0])
                : new StreamReader(Console.OpenStandardInput());

            // Read each line and parse.
            var fixedLines = new LineReader(() => input)
                .Select(l => l.Replace("Kim, G.", "Kim G."))
                .Select(l => l.Replace("Mariano, M.", "Mariano M."))
                .Select(l => l.Replace("Eriksen, K.", "Eriksen K."))
                .Select(l => l.Replace("Eriksen, D.", "Eriksen D."))
                .Select(l => FixTheDateLine(l))
                .Select(l => CombindDateAndTime(l));

            // We always push to std out.
            var output = new StreamWriter(Console.OpenStandardOutput());

            // Dump it to a file.
            foreach (var line in fixedLines)
            {
                output.WriteLine(line);
            }
        }

        /// <summary>
        /// Make the date and time a single column - excel and others have an easier time of it.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        private static string CombindDateAndTime(string l)
        {
            var parts = l.Split(new char[] { ',' }, 3);
            return $"{parts[0]} {parts[1]}, {parts[2]}";
        }

        /// <summary>
        /// Given a line from the file
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        private static string FixTheDateLine(string l)
        {
            var originalDate = l.Split(',')[0];
            var newDate = FixTheDate(originalDate);
            newDate = FormatForReasonablePeople(newDate);

            return originalDate == newDate
                ? l
                : l.Replace(originalDate, newDate);
        }

        /// <summary>
        /// Fix date to be year-month-day.
        /// </summary>
        /// <param name="newDate"></param>
        /// <returns></returns>
        private static string FormatForReasonablePeople(string newDate)
        {
            var dateParts = newDate.Split('/');
            if (dateParts.Length != 3)
            {
                return newDate;
            }
            return $"{int.Parse(dateParts[2]) + 2000}-{dateParts[0]}-{dateParts[1]}";
        }

        /// <summary>
        /// Given a DoorKing date, fix it.
        /// </summary>
        /// <param name="originalDate"></param>
        /// <returns></returns>
        private static string FixTheDate(string originalDate)
        {
            // If the date is already in the form "xx/xx/xx" then we do nothing.
            if (originalDate.Contains("/") || !originalDate.Contains("-"))
            {
                return originalDate;
            }

            // ok - split it up
            var dateBits = originalDate.Split('-');
            if (dateBits.Length != 3)
            {
                return originalDate;
            }

            // The year is what we have to fix. Everything else is good.
            var year = int.Parse(dateBits[0]) - 2000;
            return $"{year}/{dateBits[1]}/{dateBits[2]}";
        }
    }
}
