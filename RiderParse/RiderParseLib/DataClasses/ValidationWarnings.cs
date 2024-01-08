using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace RiderParseLib.DataClasses
{
    namespace WorkData
    {

        public enum WorkDataWarning
        {
            PayBreakdownLessThanTotal,
            TotalPayNull,
            PayBreakdownNull,
            PayBreakdownSomeNull,
            PayBreakdownHeterogeneousCategories,
            GenericWorkDataHeterogeneousVariables,
            TimeBreakdownLessThanTotal,
            TotalTimeNull,
            TimeBreakdownNull,
            TimeBreakdownSomeNull,
            TimeBreakdownHeterogeneousCategories
            
        }
        public record ValidationWarnings : IEnumerable<KeyValuePair<WorkDataWarning, int>>
        {
            public bool HasWarnings => Warnings.Count > 0;
            private ReadOnlyDictionary<WorkDataWarning, int> Warnings { get; }

            public ValidationWarnings()
            {
                Warnings = new ReadOnlyDictionary<WorkDataWarning, int>(new Dictionary<WorkDataWarning, int>());
            }

            public ValidationWarnings(IEnumerable<KeyValuePair<WorkDataWarning, int>>? warnings)
            {
                Warnings = warnings is null
                    ? new ReadOnlyDictionary<WorkDataWarning, int>(new Dictionary<WorkDataWarning, int>())
                    : new ReadOnlyDictionary<WorkDataWarning, int>(new Dictionary<WorkDataWarning, int>(warnings));
            }

            public ValidationWarnings(IEnumerable<WorkDataWarning> warningsStrings)
            {
                var resultDict = new Dictionary<WorkDataWarning, int>();
                foreach (WorkDataWarning warning in warningsStrings)
                {
                    resultDict[warning] = 1;
                }

                Warnings = new ReadOnlyDictionary<WorkDataWarning, int>(
                    resultDict); // TODO test this.. see if it can be faster with LINQ or whatever
            }

            public static ValidationWarnings operator +(ValidationWarnings? first, ValidationWarnings? second)
            {
                return Aggregate(new[] { first, second });
            }

            public static ValidationWarnings Aggregate(IEnumerable<ValidationWarnings?> validationWarnings)
            {
                var allKeyValuePairs =
                    validationWarnings
                        //.Select(p => p) // is this line needed?
                        .Where(p => p is not null)
                        .SelectMany(p => p);

                var test2 = new Dictionary<WorkDataWarning, int>
                (
                    (from kvpair in allKeyValuePairs
                        group kvpair by kvpair.Key
                        into kgroup
                        select new KeyValuePair<WorkDataWarning, int>(kgroup.Key, kgroup.Sum(l => l.Value)))
                );
                // TODO test both of these!

                var test = new Dictionary<WorkDataWarning, int>
                (
                    allKeyValuePairs.Select(p => p)
                        .GroupBy(p => p.Key)
                        .Select(
                            p =>
                                new KeyValuePair<WorkDataWarning, int>(p.Key, p.Sum(m => m.Value)))
                );

                return new ValidationWarnings(test);

            }

            public IEnumerator<KeyValuePair<WorkDataWarning, int>> GetEnumerator() => Warnings.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        }
    }
}