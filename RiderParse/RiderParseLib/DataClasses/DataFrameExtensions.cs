using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Analysis;

namespace DataFrameExtensions
{
    public static class DataFrameExtensions
    {
        public static DataFrame ConcatDataFrames(IEnumerable<DataFrame> dataframes)
        {
            DataFrame returnDataFrame;

            List<DataFrame> dataFramesList = dataframes.ToList();

            returnDataFrame = dataFramesList[0].Clone();
            for (int i = 1; i < dataFramesList.Count; i++)
            {
                returnDataFrame.Append(dataFramesList[i].Clone().Rows, true);
            }
            return returnDataFrame;
        }

        public static DataFrame BindDataFrames(IEnumerable<DataFrame> dataframes)
        {
            DataFrame returnDataFrame;
            List<DataFrame> dataFramesList = dataframes.ToList();

            returnDataFrame = dataFramesList[0].Clone();

            for (int i = 1; i < dataFramesList.Count; i++)
            {
                returnDataFrame = new DataFrame(returnDataFrame.Columns.Concat(dataFramesList[i].Columns));
            }
            return returnDataFrame;
        }

        public static DataFrame DataFrameAddColumn(DataFrame dataFrame, DataFrameColumn newColumn)
        {
            return BindDataFrames(new List<DataFrame> { dataFrame, new DataFrame(new[] { newColumn }) });
        }

        public static DataFrame AddColumn(this DataFrame thisDataFrame, DataFrameColumn newColumn)
        {
            return DataFrameAddColumn(thisDataFrame, newColumn);
        }

        public static DataFrame Bind(this DataFrame thisDataFrame, DataFrame otherDatatFrame)
        {
            return BindDataFrames(new[] { thisDataFrame, otherDatatFrame });
        }

        public static DataFrame Select(this DataFrame thisDataFrame, IEnumerable<string> columnNames)
        {
            List<DataFrameColumn> columns = new();
            foreach(string colName in columnNames)
            {
                columns.Add(thisDataFrame[colName].Clone());
            }
            return new DataFrame(columns);

        }

        public static DataFrameColumn RoundDigits(this DataFrameColumn decimalCol, int digits)
        {
            return new DecimalDataFrameColumn(decimalCol.Name,
                from decimal decimalValue
                in decimalCol
                select decimal.Round(decimalValue, digits));
        }

        public static PrimitiveDataFrameColumn<TResult> Apply<T, TResult>(this PrimitiveDataFrameColumn<T> column,
            Func<T, TResult> func)
            where T : unmanaged
            where TResult : unmanaged
        {
            var resultColumn = new PrimitiveDataFrameColumn<TResult>(string.Empty, 0);

            foreach (var row in column)
                resultColumn.Append(func(row.Value));

            return resultColumn;
        }

    }

}
