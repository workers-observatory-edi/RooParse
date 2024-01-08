using Microsoft.Data.Analysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static DataFrameExtensions.DataFrameExtensions;
using static DateTimeExtensions.DateTimeExtensions;

namespace RiderParseLib
{

    namespace Roo

    {
        public class RooSessionAdjCollection : ICollection, IEnumerable<RooSessionAdj>
        {
            public ReadOnlyCollection<RooSessionAdj> Sessions
            {
                get; private set;
            }

            public SummaryDFs Summary { get; private set; }

            // Construct using enumerable of SessionsAdj (constructor)
            public RooSessionAdjCollection(IEnumerable<RooSessionAdj> sessionsAdjEnumerable)
            {
                Sessions = sessionsAdjEnumerable.ToList().AsReadOnly();
                Summary = new(this);
            }

            // Construct using RooPayslip (static method)
            public static RooSessionAdjCollection FromPayslip(RooPayslip payslip)
            {
                RooSessionCollection payslipSessions = payslip.Sessions;
                List<RooSessionAdj> sessionsAdj = new();
                foreach (RooSession session in payslipSessions)
                {
                    RooSessionAdj sessionAdj = new(session, payslip);
                    sessionsAdj.Add(sessionAdj);
                }
                return new RooSessionAdjCollection(sessionsAdj);
            }

            // Construct using IEnumerable<RooPayslip> (static method)
            public static RooSessionAdjCollection FromPayslipEnumerable(IEnumerable<RooPayslip> payslips)
            {
                List<RooSessionAdj> sessionsAdj = new();
                foreach (RooPayslip payslip in payslips)
                {
                    foreach (RooSession session in payslip.Sessions)
                    {
                        RooSessionAdj sessionAdj = new(session, payslip);
                        sessionsAdj.Add(sessionAdj);
                    }
                }
                return new RooSessionAdjCollection(sessionsAdj);
            }

            public class SummaryDFs
            {
                private readonly RooSessionAdjCollection _sessionsAdj;

                public SummaryDFs(RooSessionAdjCollection sessionsAdj)
                {
                    _sessionsAdj = sessionsAdj;
                }

                public DataFrame Session
                {
                    get
                    {
                        List<DataFrame> sessionDataFramesList = new();
                        foreach (RooSessionAdj sessionAdj in _sessionsAdj)
                        {
                            sessionDataFramesList.Add(sessionAdj.SummaryDF);
                        }
                        return ConcatDataFrames(sessionDataFramesList).OrderBy("TimeIn");
                    }
                }

                private static DataFrame ByGroupingColumnSummarise(DataFrame dfToSummarise, string colname)
                {
                    dfToSummarise = dfToSummarise.Select(new string[] {
                    colname, "HoursWorked", "OrdersDelivered",
                    "RegularFees", "ExtraFees", "TipsEstimate",
                    "OtherAdjustments", "AdjustedTotal"});

                    GroupBy summaryDFbyMonth = dfToSummarise.GroupBy(colname);

                    DataFrame summariedDF = summaryDFbyMonth.Sum();

                    summariedDF["PerHourAvg"] = (summariedDF["AdjustedTotal"] / summariedDF["HoursWorked"]).RoundDigits(2);

                    summariedDF["PerOrderAvg"] = (summariedDF["AdjustedTotal"] / summariedDF["OrdersDelivered"]).RoundDigits(2);

                    summariedDF["PerHourAvgRegular"] = (summariedDF["RegularFees"] / summariedDF["HoursWorked"]).RoundDigits(2);

                    summariedDF["PerOrderAvgRegular"] = (summariedDF["RegularFees"] / summariedDF["OrdersDelivered"]).RoundDigits(2);

                    summariedDF["PerHourAvgExtra"] = (summariedDF["ExtraFees"] / summariedDF["HoursWorked"]).RoundDigits(2);

                    summariedDF["PerOrderAvgExtra"] = (summariedDF["ExtraFees"] / summariedDF["OrdersDelivered"]).RoundDigits(2);

                    summariedDF["PerHourAvgTips"] = (summariedDF["TipsEstimate"] / summariedDF["HoursWorked"]).RoundDigits(2);

                    summariedDF["PerOrderAvgTips"] = (summariedDF["TipsEstimate"] / summariedDF["OrdersDelivered"]).RoundDigits(2);

                    return summariedDF;
                }

                public DataFrame Daily
                {
                    get
                    {
                        DataFrame summaryDF = Session.Clone();

                        summaryDF = summaryDF.AddColumn(new PrimitiveDataFrameColumn<DateTime>("Date", from DateTime dateTime
                                                                                                        in summaryDF["TimeIn"]
                                                                                                       select dateTime.Date));

                        summaryDF = ByGroupingColumnSummarise(summaryDF, "Date").OrderBy("Date");

                        return summaryDF;
                    }
                }

                public DataFrame Weekly
                {
                    get
                    {
                        DataFrame summaryDF = Session.Clone();

                        summaryDF = summaryDF.AddColumn(new PrimitiveDataFrameColumn<DateTime>("Week", from DateTime dateTime
                                                                                                            in summaryDF["TimeIn"]
                                                                                                       select dateTime.WeekStartMonday()));

                        summaryDF = ByGroupingColumnSummarise(summaryDF, "Week").OrderBy("Week");

                        return summaryDF;
                    }
                }

                public DataFrame Monthly
                {
                    get
                    {
                        DataFrame summaryDF = Session.Clone();

                        summaryDF = summaryDF.AddColumn(new PrimitiveDataFrameColumn<DateTime>("Month", from DateTime dateTime
                                                                                                            in summaryDF["TimeIn"]
                                                                                                        select dateTime.MonthFirstDayDT()));

                        summaryDF = ByGroupingColumnSummarise(summaryDF, "Month").OrderBy("Month");

                        return summaryDF;
                    }
                }

                public DataFrame Yearly
                {
                    get
                    {
                        DataFrame summaryDF = Session.Clone();

                        summaryDF = summaryDF.AddColumn(new PrimitiveDataFrameColumn<int>("Year", from DateTime dateTime
                                                                                                    in summaryDF["TimeIn"]
                                                                                                  select dateTime.Year));

                        summaryDF = ByGroupingColumnSummarise(summaryDF, "Year").OrderBy("Year");

                        return summaryDF;
                    }
                }

                public DataFrame TaxYear
                {
                    get
                    {
                        DataFrame summaryDF = Session.Clone();

                        summaryDF = summaryDF.AddColumn(new StringDataFrameColumn("Tax Year", from DateTime dateTime
                                                                                                in summaryDF["TimeIn"]
                                                                                              select dateTime.GetTaxYearUK().ToString()));

                        summaryDF = ByGroupingColumnSummarise(summaryDF, "Tax Year").OrderBy("Tax Year");

                        return summaryDF;
                    }
                }
            }

            // CopyTo Method for ICollection
            void ICollection.CopyTo(Array myArr, int index)
            {
                foreach (RooSessionAdj i in Sessions)
                {
                    myArr.SetValue(i, index);
                    index++;
                }
            }

            public DateTime FirstTimeIn
            {
                get
                {
                    return (from session in Sessions
                            orderby session.Session.TimeInDT ascending
                            select session.Session.TimeInDT).First();
                }
            }

            public DateTime LastTimeOut
            {
                get
                {
                    return (from session in Sessions
                            orderby session.Session.TimeInDT descending
                            select session.Session.TimeInDT).First();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Sessions.GetEnumerator();
            }

            public IEnumerator<RooSessionAdj> GetEnumerator()
            { return Sessions.GetEnumerator(); }

            // The IsSynchronized Boolean property returns True if the
            // collection is designed to be thread safe; otherwise, it returns False.
            bool ICollection.IsSynchronized { get { return false; } }

            // The SyncRoot property returns an object, which is used for synchronizing
            // the collection. This returns the instance of the object or returns the
            // SyncRoot of other collections if the collection contains other collections.
            object ICollection.SyncRoot { get { return this; } }

            // The Count read-only property returns the number of items in the collection.
            int ICollection.Count { get { return Sessions.Count; } }
        }
    }

}
