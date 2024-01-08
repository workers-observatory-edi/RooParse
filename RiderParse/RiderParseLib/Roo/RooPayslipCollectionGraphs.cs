using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Analysis;
using ScottPlot;
using ScottPlot.Plottable;
using DataFrameExtensions;

namespace RiderParseLib
{
    namespace Roo
    {
        public class RooPayslipCollectionGraphs
        {
            public readonly RooPayslipCollection Payslips;
            public readonly Dictionary<string, dynamic> PlotTemplates;
            public RooPayslipCollectionGraphs(RooPayslipCollection payslips)
            {
                Payslips = payslips;

                PlotTemplates = new()
                {
                    ["week"] = new Dictionary<string, dynamic>()
                    {
                        ["TableObject"] = payslips.Summary.Weekly,
                        ["ColumnName"] = "Week",
                        ["PerHourAvg"] = new Dictionary<string, dynamic>()
                        {
                            ["PlotTitle"] = "Earnings per HOUR - Weekly average",
                            ["YTitle"] = "Earnings per hour (£)",
                            ["XTitle"] = "Week",
                        },
                        ["PerOrderAvg"] = new Dictionary<string, dynamic>()
                        {
                            ["PlotTitle"] = "Earnings per ORDER - Weekly average",
                            ["YTitle"] = "Earnings per order (£)",
                            ["XTitle"] = "Week",
                        },
                    },
                    ["month"] = new Dictionary<string, dynamic>()
                    {
                        ["TableObject"] = payslips.Summary.Monthly,
                        ["ColumnName"] = "Month",
                        ["PerHourAvg"] = new Dictionary<string, dynamic>()
                        {
                            ["PlotTitle"] = "Earnings per HOUR - Monthly average",
                            ["YTitle"] = "Earnings per hour (£)",
                            ["XTitle"] = "Month",
                        },
                        ["PerOrderAvg"] = new Dictionary<string, dynamic>()
                        {
                            ["PlotTitle"] = "Earnings per ORDER - Monthly average",
                            ["YTitle"] = "Earnings per order (£)",
                            ["XTitle"] = "Month",
                        },
                    }
                };
            }

            public ScatterPlot PlottableScatter(string timePeriod, string yVar)
            {
                DataFrame modellingDF = PlotTemplates[timePeriod]["TableObject"];
                Double[] varX = (from DateTime dT
                                    in modellingDF[(string)PlotTemplates[timePeriod]["ColumnName"]]
                                 select dT.ToOADate()).ToArray();
                Double[] varY = (from Decimal yvar
                                    in modellingDF[yVar]
                                 select (double)yvar).ToArray();

                ScatterPlot plottable = new(varX, varY);

                return plottable;

            }

            public void SummaryPlot(ref Plot plot, string timePeriod, string yVar)
            {
                plot.XAxis.DateTimeFormat(true);
                plot.Add(PlottableScatter(timePeriod, yVar));

                plot.XAxis.Label(PlotTemplates[timePeriod][yVar]["XTitle"]);
                plot.YAxis.Label(PlotTemplates[timePeriod][yVar]["YTitle"]);

                plot.XAxis2.Label(PlotTemplates[timePeriod][yVar]["PlotTitle"]);
            }

            public void SaveSummaryPlot(string path, string timePeriod, string yVar,
                int width = 500, int height = 400)
            {
                Plot plot = new(width, height);
                SummaryPlot(ref plot, timePeriod, yVar);
                plot.SaveFig(path);

            }
        }

    }
    
}
