using Microsoft.Data.Analysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DataFrameExtensions;
using DateTimeExtensions;

namespace RiderParseLib
{

    namespace Roo
    {
        public class RooSession
        {
            public readonly string RawStr;
            public readonly DateTime DateDT;
            public readonly DateTime TimeInDT;
            public readonly DateTime TimeOutDT;
            public readonly decimal HoursWorked;
            public readonly int OrdersDelivered;
            public readonly decimal OrdersDeliveredColEarnings;
            public readonly decimal RegularFees;
            public readonly decimal ExtraFees;
            public readonly decimal SessionTotalEarnings;

            // TODO implement IEquitable

            public RooSession(string sessionStr)
            {
                RawStr = sessionStr;

                sessionStr = sessionStr.ToLower();
                sessionStr = Regex.Replace(sessionStr, @"\s+", "");

                string dateStr = RooRegexObjects.regexDate.Match(sessionStr).Value;

                // DateDT = DateTime.ParseExact(dateStr, "d MMMM yyyy", new CultureInfo("en-US"));
                DateDT = DateTime.ParseExact(dateStr, "dMMMMyyyy", new CultureInfo("en-US"));

                HoursWorked = decimal.Parse(RooRegexObjects.regexHours.Match(sessionStr).Value);
                OrdersDelivered = int.Parse(RooRegexObjects.regexOrders.Match(sessionStr).Value);

                MatchCollection timeMatchList = RooRegexObjects.regexTime.Matches(sessionStr);
                var times = timeMatchList.Cast<Match>().Select(timeMatchList => timeMatchList.Value).ToList();

                //TimeInDT = DateTime.ParseExact(dateStr + " " + times[0], "d MMMM yyyy HH:mm", new CultureInfo("en-US"));
                //TimeOutDT = DateTime.ParseExact(dateStr + " " + times[1], "d MMMM yyyy HH:mm", new CultureInfo("en-US"));

                TimeInDT = DateTime.ParseExact(dateStr + " " + times[0], "dMMMMyyyy HH:mm", new CultureInfo("en-US"));
                TimeOutDT = DateTime.ParseExact(dateStr + " " + times[1], "dMMMMyyyy HH:mm", new CultureInfo("en-US"));

                MatchCollection poundMatchList = RooRegexObjects.regexPounds.Matches(sessionStr);
                List<string> poundList = poundMatchList.Cast<Match>().Select(poundMatchList => poundMatchList.Value).ToList();

                if (poundList.Count == 2)
                {
                    OrdersDeliveredColEarnings = decimal.Parse(poundList[0]);
                    SessionTotalEarnings = decimal.Parse(poundList[1]);
                    ExtraFees = 0;
                }
                else if (poundList.Count == 3)
                {
                    OrdersDeliveredColEarnings = decimal.Parse(poundList[0]);
                    SessionTotalEarnings = decimal.Parse(poundList[2]);
                    ExtraFees = decimal.Parse(RooRegexObjects.regexExtra.Match(sessionStr).Value);

                    if (ExtraFees != decimal.Parse(poundList[1]))
                    {
                        throw new Exception("Failed to parse payslip: ExtraFees regex match in unexpected position on page.");
                    }
                }
                else
                {
                    throw new Exception("Failed to parse payslip: Matched currency strings for session not two or three for the session.");
                }

                RegularFees = SessionTotalEarnings - ExtraFees;
            }

            public DataFrame SummaryDF
            {
                get
                {
                    PrimitiveDataFrameColumn<DateTime> timeInDT = new("TimeIn", new[] { TimeInDT });
                    PrimitiveDataFrameColumn<DateTime> timeOutDT = new("TimeOut", new[] { TimeOutDT });
                    DecimalDataFrameColumn hoursWorked = new("HoursWorked", new[] { HoursWorked });
                    Int32DataFrameColumn ordersDelivered = new("OrdersDelivered", new[] { OrdersDelivered });
                    DecimalDataFrameColumn regularFees = new("RegularFees", new[] { RegularFees });
                    DecimalDataFrameColumn extraFees = new("ExtraFees", new[] { ExtraFees });
                    DecimalDataFrameColumn sessionTotal = new("SessionTotal", new[] { SessionTotalEarnings });

                    DataFrame returnDataFrame = new(new List<DataFrameColumn> {
                    timeInDT, timeOutDT, hoursWorked, ordersDelivered, regularFees, extraFees, sessionTotal
                });

                    return returnDataFrame;
                }
            }

            public override string ToString()
            { return SummaryDF.ToString(); }
        }
    }

}
