using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Analysis;

namespace DateTimeExtensions
{
  public static class DateTimeExtensions
  {
      public static DateTime MonthFirstDayDT(this DateTime dateTime)
      {
          return new DateTime(dateTime.Year, dateTime.Month, 1);
      }

      public static DateTime WeekStartMonday(this DateTime dateTime)
      {
          // Returns the last Monday, or same Monday if it is Monday.
          int dayOfWeek = (int) dateTime.DayOfWeek;
          int offset;
          if (dayOfWeek == 0) { offset = -6; }
          else { offset =  1 - dayOfWeek;  }
          return dateTime.Date.AddDays(offset);

      }

      public static TaxYearUK GetTaxYearUK(this DateTime dateTime) { return new TaxYearUK(dateTime); }

      public readonly struct TaxYearUK
      {
          public readonly DateTime Start { get; }
          public readonly DateTime End { get; }

          public TaxYearUK(DateTime datetime)
          {
              int dateYear = datetime.Year;

              int taxYearFirst = (datetime >= new DateTime(dateYear, 4, 6, 0, 0, 0)) ? dateYear : dateYear - 1;

              Start = new DateTime(taxYearFirst, 4, 6, 0, 0, 0);
              End = new DateTime(taxYearFirst + 1, 4, 5, 23, 59, 59);

          }

          public override string ToString()
          {
              return $"{Start.Year}-{End.Year}";
          }
      }
  }
}
