using System;
using RiderParseLib.Roo;

namespace RiderParseCL
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {

                Console.WriteLine("Input dir path of PDFs:");
                string dirPath = Console.ReadLine().Trim();

                if (dirPath == "") { break; }

                RooPayslipCollection payslipsImported = RooPayslipCollection.FromDir(dirPath);
                var plots = new RooPayslipCollectionGraphs(payslipsImported);

                plots.SaveSummaryPlot(dirPath + @"\Weekly Per Hour.png", "week", "PerHourAvg");
                plots.SaveSummaryPlot(dirPath + @"\Weekly Per Order.png", "week", "PerOrderAvg");
                plots.SaveSummaryPlot(dirPath + @"\Monthly Per Hour.png", "month", "PerHourAvg");
                plots.SaveSummaryPlot(dirPath + @"\Monthly Per Order.png", "month", "PerOrderAvg");

                Console.WriteLine(payslipsImported.Summary.Session);
                Console.WriteLine(payslipsImported.Summary.Daily);
                Console.WriteLine(payslipsImported.Summary.Weekly);
                Console.WriteLine(payslipsImported.Summary.Monthly);
                Console.WriteLine(payslipsImported.Summary.Yearly);
                Console.WriteLine(payslipsImported.Summary.TaxYear);
                Console.ReadLine();
            }
        }
    }
}
