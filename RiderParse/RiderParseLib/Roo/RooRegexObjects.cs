using System.Text.RegularExpressions;

namespace RiderParseLib
{

    namespace Roo
    {
        internal static class RooRegexObjects
        {

            public static readonly string[] _weekdays = new string[] { "monday", "tuesday", "wednesday",
            "thursday", "friday", "saturday", "sunday" };
            public static readonly string _weekdaysRegexStr = string.Join("|", _weekdays);

            public static readonly string[] _sessionStrEndWords = new string[] { "monday", "tuesday", "wednesday",
            "thursday", "friday", "saturday", "sunday", "feeadjustment", "summary"  };
            public static readonly string _sessionEndRegexStr = string.Join("|", _sessionStrEndWords);

            // If empty spaces are not removed:

            //public static readonly Regex regexNoStopwords = new($"(?<= ({_splitWordsRegex}))(.+?)(?=({_splitWordsRegex}))");

            //public static readonly Regex regexDate = new(@"\d{1,2} [a-zA-Z]+ 20\d\d");
            //public static readonly Regex regexTime = new(@"\d{1,2}:\d\d");
            //public static readonly Regex regexHours = new(@"((\d{1,2}\.\d{1,2})|(\d{1,2}\.\d{1,2} ))(?=h)");
            //public static readonly Regex regexPounds = new(@"(?<=£)(\d+\.\d{1,2})");
            //public static readonly Regex regexOrders = new(@"(\d{1,3})(?=(: £|:£))");
            //public static readonly Regex regexExtra = new(@"(?<=£)(\d+\.\d{1,2})(?= extra fees)");

            //public static readonly Regex regexTips = new(@"(?<=Tips £)(\d+\.\d{1,2})");
            //public static readonly Regex regexAdjustments = new(@"(?<=Adjustments £)(\d+\.\d{1,2})");
            //public static readonly Regex regexDropFeesTotal = new(@"(?<=Drop Fees £)(\d+\.\d{1,2})");
            //public static readonly Regex regexTotal = new(@"(?<=Total £)(\d+\.\d{1,2})");


            // If empty spaces are removed and letters decapitalised (TODO complete):

            public static readonly Regex regexNoStopwords = new($"(?<=({_weekdaysRegexStr}))(.+?)(?=({_sessionEndRegexStr}))");

            public static readonly Regex regexDate = new(@"\d{2}[a-zA-Z]+20\d\d");
            public static readonly Regex regexTime = new(@"\d\d:\d\d");
            public static readonly Regex regexHours = new(@"(?<=:\d\d)((\d{1,2}\.\d{1,2})|(\d{1,2}\.\d{1,2} ))(?=h)");
            public static readonly Regex regexPounds = new(@"(?<=£)(\d+\.\d{2})");
            public static readonly Regex regexOrders = new(@"(?<=h)(\d{1,3})(?=(:£))");
            public static readonly Regex regexExtra = new(@"(?<=£)(\d+\.\d{2})(?=extrafees)");

            public static readonly Regex regexSummary = new(@"(?<=summary).+$");

            public static readonly Regex regexTips = new(@"(?<=tips£)((-)?\d+\.\d{2})");
            public static readonly Regex regexAdjustments = new(@"(?<=adjustments£)((-)?\d+\.\d{2})");
            public static readonly Regex regexDropFeesTotal = new(@"(?<=dropfees£)((-)?\d+\.\d{2})");
            public static readonly Regex regexTransactionFee = new(@"(?<=transactionfee£)((-)?\d+\.\d\d)");
            public static readonly Regex regexTotal = new(@"(?<=total£)((-)?\d+\.\d{2})");
        }
    }
    
}