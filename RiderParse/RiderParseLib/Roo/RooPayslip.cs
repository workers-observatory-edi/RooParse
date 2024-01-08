using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Data.Analysis;
using DataFrameExtensions;

namespace RiderParseLib
{
    namespace Roo
    {
        public class RooPayslip : IEquatable<RooPayslip>
    {

        public readonly string RawStr;
        public readonly string Filename;
        public readonly string CleanFullStr;

        public readonly bool ParseSuccess;
        public readonly Exception ParseFailException;

        public readonly RooSessionCollection Sessions;

        public readonly decimal DropFeesTotal;
        public readonly decimal TipsTotal;
        public readonly decimal OtherAdjustmentsTotal;
        public readonly decimal TransactionFee;

        public readonly decimal PayslipTotal;
        public RooSessionAdjCollection SessionsAdj;

        public DateTime FirstTimeIn { get { return Sessions.FirstTimeIn; } }
        public DateTime LastTimeOut { get { return Sessions.LastTimeOut; } }

        public decimal RegularFeesTotal
        {
            get
            {
                return (from session in Sessions
                        select session.RegularFees).Sum();
            }
        }

        public decimal ExtraFeesTotal
        {
            get
            {
                return (from session in Sessions
                        select session.ExtraFees).Sum();
            }
        }

        public int OrdersTotal
        {
            get
            {
                return (from session in Sessions select session.OrdersDelivered).Sum();
            }
        }

        public readonly decimal HoursTotal;

        public RooPayslip(string fullString, string filepath)
        {

            RawStr = fullString;
            Filename = (filepath == "") ? null : filepath;

            try
            {

                CleanFullStr = Regex.Replace(RawStr, @"\r\n?|\n", " "); // Replace newlines with empty space
                CleanFullStr = Regex.Replace(CleanFullStr, @"\s+", " "); // Replace multiple empty spaces with empty space

                string noWhiteSpaceString = Regex.Replace(CleanFullStr, @"\s+", "").ToLower();

                MatchCollection sessionMatchList = RooRegexObjects.regexNoStopwords.Matches(noWhiteSpaceString);
                // MatchCollection sessionMatchList = RegexObjects.regexNoStopwords.Matches(CleanFullStr);

                var sessionStrList = sessionMatchList.Cast<Match>().Select(sessionMatchList => sessionMatchList.Value).ToList();

                Sessions = new RooSessionCollection(sessionStrList);

                string summaryStr = RooRegexObjects.regexSummary.Match(noWhiteSpaceString).Value;

                PayslipTotal = decimal.Parse(RooRegexObjects.regexTotal.Match(summaryStr).Value);
                DropFeesTotal = decimal.Parse(RooRegexObjects.regexDropFeesTotal.Match(summaryStr).Value);

                Match tryMatchTips = RooRegexObjects.regexTips.Match(summaryStr);
                if (tryMatchTips.Success) { TipsTotal = decimal.Parse(tryMatchTips.Value); }
                else { TipsTotal = 0; }

                Match tryMatchAdjustments = RooRegexObjects.regexAdjustments.Match(summaryStr);
                if (tryMatchAdjustments.Success) { OtherAdjustmentsTotal = decimal.Parse(tryMatchAdjustments.Value); }
                else { OtherAdjustmentsTotal = 0; }

                Match tryMatchTransactionFee = RooRegexObjects.regexTransactionFee.Match(summaryStr);
                if (tryMatchTransactionFee.Success) { TransactionFee = decimal.Parse(tryMatchTransactionFee.Value); }
                else { TransactionFee = 0; }

                HoursTotal = 0;
                foreach (RooSession session in Sessions)
                {
                    HoursTotal += session.HoursWorked;
                }

                SessionsAdj = RooSessionAdjCollection.FromPayslip(this);

                ParseSuccess = true;

            }

            catch (Exception e)
            {
                ParseSuccess = false;
                ParseFailException = e;
            }

        }

        public static RooPayslip FromPath(string path)
        {

            static List<string> ExtractTextFromPDF(string filePath)
            {
                PdfReader pdfReader = new(filePath);
                PdfDocument pdfDoc = new(pdfReader);
                List<string> allContent = new();
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                    allContent.Add(pageContent);
                }
                pdfDoc.Close();
                pdfReader.Close();

                return allContent;
            }

            List<string> allContent = ExtractTextFromPDF(path);
            RooPayslip payslipImported = new(allContent[0], path);
            return payslipImported;
        }

        public DataFrame DataFrameSummary
        {
            get
            {
                if (ParseSuccess)
                {
                    PrimitiveDataFrameColumn<DateTime> firstTimeIn = new("FirstTimeIn", new[] { FirstTimeIn });
                    PrimitiveDataFrameColumn<DateTime> lastTimeOut = new("LastTimeOut", new[] { LastTimeOut });

                    DecimalDataFrameColumn dropFeesTotal = new("DropFeesTotal", new[] { DropFeesTotal });
                    DecimalDataFrameColumn regularFeesTotal = new("RegularFeesTotal", new[] { RegularFeesTotal });
                    DecimalDataFrameColumn extraFeesTotal = new("ExtraFeesTotal", new[] { ExtraFeesTotal });

                    Int32DataFrameColumn ordersTotal = new("OrdersTotal", new[] { OrdersTotal });
                    DecimalDataFrameColumn hoursTotal = new("HoursTotal", new[] { HoursTotal });

                    DecimalDataFrameColumn tipsTotal = new("TipsTotal", new[] { TipsTotal });
                    DecimalDataFrameColumn otherAdjustmentsTotal = new("OtherAdjustmentsTotal", new[] { OtherAdjustmentsTotal });
                    DecimalDataFrameColumn transactionFee = new("TransactionFee", new[] { TransactionFee });

                    DecimalDataFrameColumn payslipTotal = new("PayslipTotal", new[] { PayslipTotal });

                    DataFrame summaryDF = new(new List<DataFrameColumn>
                        {
                            firstTimeIn, lastTimeOut,
                            dropFeesTotal, regularFeesTotal, extraFeesTotal,
                            ordersTotal, hoursTotal,
                            tipsTotal, otherAdjustmentsTotal, transactionFee,
                            payslipTotal

                        });

                    return summaryDF;
                }
                else
                {
                    throw new Exception("Can't provide DataFrameSummary: ParseSuccess is false");
                }

            }
        }

        public bool Equals(RooPayslip other)
        {
            if (this.ParseSuccess == false)
            {
                return false;
            }
            else if (this.FirstTimeIn == other.FirstTimeIn &&
                this.LastTimeOut == other.LastTimeOut &&
                this.PayslipTotal == other.PayslipTotal)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is not RooPayslip payslip)
                return false;

            return Equals(payslip);
        }

        public static bool operator ==(RooPayslip p1, object obj)
        {
            return p1.Equals(obj);
        }

        public static bool operator !=(RooPayslip p1, object obj)
        {
            return !(p1 == obj);
        }
        public override int GetHashCode() { return ($"{FirstTimeIn}{LastTimeOut}{PayslipTotal}").GetHashCode(); }

    }

    }

  }
