using Microsoft.Data.Analysis;
using System.Collections.Generic;
using static DataFrameExtensions.DataFrameExtensions;

namespace RiderParseLib
{

    namespace Roo
    {
        public class RooSessionAdj
        {
            // Adjust session with other information from payslip. E.g. split tips among sessions equally depending on hours.
            public readonly RooSession Session;

            // public readonly RooPayslip RooPayslip;

            public readonly decimal SessionTipsEstimate;
            public readonly decimal TransactionFeeEstimate;
            public readonly decimal SessionOtherAdjustments;

            public readonly decimal SessionAdjustedTotal;

            // TODO Implement IEquitable

            public RooSessionAdj(RooSession session, RooPayslip payslip)
            {
                Session = session;
                // RooPayslip = payslip;

                decimal sessionHoursProportion = session.HoursWorked / payslip.HoursTotal;
                SessionTipsEstimate = decimal.Round(sessionHoursProportion * payslip.TipsTotal, 2);
                TransactionFeeEstimate = decimal.Round(sessionHoursProportion * payslip.TransactionFee, 2);
                SessionOtherAdjustments = decimal.Round(sessionHoursProportion * payslip.OtherAdjustmentsTotal, 2);

                SessionAdjustedTotal = decimal.Round(session.SessionTotalEarnings + SessionTipsEstimate + SessionOtherAdjustments, 2);
            }

            public DataFrame SummaryDF
            {
                get
                {
                    DecimalDataFrameColumn tipsEstimate = new("TipsEstimate", new[] { SessionTipsEstimate });
                    DecimalDataFrameColumn sessionOtherAdjustments = new("OtherAdjustments", new[] { SessionOtherAdjustments });
                    DecimalDataFrameColumn sessionAdjustedTotal = new("AdjustedTotal", new[] { SessionAdjustedTotal });

                    DecimalDataFrameColumn transactionFeeEstimate = new("TransactionFeeEstimate", new[] { TransactionFeeEstimate });

                    DataFrame adjustmentsOnlyDF = new(new List<DataFrameColumn> {
                    tipsEstimate, sessionOtherAdjustments, sessionAdjustedTotal, transactionFeeEstimate
                });

                    DataFrame adjDF = BindDataFrames(new[] { Session.SummaryDF, adjustmentsOnlyDF });

                    return adjDF;
                }
            }
        }
    }


}
