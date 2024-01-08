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
        
        // An instance or summary of (one or) multiple values in seconds
        public record SecondsVal
        {
            public int? Value { get; }
            public bool IsAggregate { get; }
            public int NonNullCount { get; }
            public int NullCount { get; }

            public SecondsVal(int? value, bool isAggregate, int nonNullCount, int nullCount)
            {
                if (!isAggregate & (nonNullCount + nullCount != 1))
                {
                    throw new Exception("VarValue: not valid nonNullCount/nullCount when isAggregate is false");
                }
                Value = value;
                IsAggregate = isAggregate;
                NonNullCount = nonNullCount;
                NullCount = nullCount;
            }
            

        }

        // For time data in Datetime format
        public record DatetimeDataVar
        {

            public enum VarType
            {
                // Only one instance of pre-defined variables
                // These are specific variables which can be used for specific verifications, summaries
                // and visualisations, and thus, different types are used instead of simple string names.
                
                WorkStart, // when actual work/job started (appropriate for jobs, etc)
                WorkEnd, // when actual work/job ended (appropriate for jobs, etc)
                IntervalStart, // when interval of time started (appropriate for aggregates),
                               // should come along with IntervalEnd
                IntervalEnd, // when interval of time ended
                Custom
            }

            public string Name { get; } // Unique identifier for the variable
            public VarType Type { get; }

            public DatetimeDataVar(string name)
            {
                if (new[] { "WorkStart", "WorkEnd", "IntervalStart", "IntervalEnd" }.Contains(name))
                {
                    
                    switch (name)
                    {
                        case "WorkStart":
                            Type = VarType.WorkStart;
                            break;
                        case "WorkEnd":
                            Type = VarType.WorkEnd;
                            break;
                        case "IntervalStart":
                            Type = VarType.IntervalStart;
                            break;
                        case "IntervalEnd":
                            Type = VarType.IntervalEnd;
                            break;
                    }

                    Name = name;
                }

                else
                {
                    Type = VarType.Custom;
                    Name = name;
                }


            }

            public bool VerifyValue(decimal? value) => true; // This will probably be redundant.

        }

        // TODO integrate DatetimeDataVars into TimeData
        public abstract record TimeData
        {
            
            // Single or aggregate distinctions
            public abstract bool IsAggregate { get; }
            
            public abstract int DataPointCount { get; }
            
            // Time Breakdown - breakdown of time spent on different activities (unit: seconds) 
            public abstract string[] TimeBreakdownCategories { get; }

            public abstract int?[] TimeBreakdownValues(string category);
            public abstract SecondsVal TimeBreakdownSecondsValues(string category);

            // Datetime variables
            public abstract DatetimeDataVar[] DatetimeVariables { get; }
            public abstract DateTime?[] DatetimeValues(string varName);

            // Validation
            public abstract ValidationWarnings Validate();
        }

        public record TimeDataSingle: TimeData
        {
            public int? TotalTime { get; }
            
            // TO BE USED for aggregating for weeks, months, etc.
            public DateOnly Date { get; }
            
            protected ReadOnlyDictionary<DatetimeDataVar, DateTime?> DatetimeVarsDict { get; init; }
            
            protected ReadOnlyDictionary<string, int?> TimeBreakdownDict { get; init; }

            public override string[] TimeBreakdownCategories { get; }
            
            public override DatetimeDataVar[] DatetimeVariables { get; }

            public override DateTime?[] DatetimeValues(string varName)
            {
                if (!(DatetimeVariables.Select(p => p.Name).Contains(varName)))
                {
                    return new DateTime?[] { null };
                }

                return new[] { DatetimeVarsDict
                    .First(p => p.Key.Name == varName).Value };
            }

            private readonly ValidationWarnings? _otherProvidedWarnings;
            public override bool IsAggregate => false;
            
            public override int DataPointCount => 1;
            

            public TimeDataSingle(
                int? totalTime,
                ReadOnlyDictionary<string, int?> timeBreakdownDict,
                ReadOnlyDictionary<DatetimeDataVar, DateTime?> datetimeVarsDict,
                ValidationWarnings? validationWarnings,
                DateOnly date
                )
            {

                _otherProvidedWarnings = validationWarnings; 
                TotalTime = totalTime;
                TimeBreakdownCategories = timeBreakdownDict.Keys.ToArray();
                TimeBreakdownDict = timeBreakdownDict;
                DatetimeVariables = datetimeVarsDict.Keys.ToArray();
                DatetimeVarsDict = datetimeVarsDict;
                
                Date = date;
            }

            public override int?[] TimeBreakdownValues(string category)
            {
                if (category != "TotalTime" & !TimeBreakdownDict.ContainsKey(category))
                {
                    return new int?[] { null };
                }
                return category == "TotalTime" ? new[] { TotalTime } : new[] { TimeBreakdownDict[category] };
            }
            
            public override SecondsVal TimeBreakdownSecondsValues(string category)
            {
                var value = TimeBreakdownValues(category)[0];
                return value is null 
                    ? new SecondsVal(null, false, 0, 1) 
                    : new SecondsVal(value, false, 1, 0);
            }
            
            public decimal? TimeBreakdownSum
            {
                
                get
                {
                    if (AllBreakdownValuesNull)
                    {
                        return null;
                    }
                    return TimeBreakdownDict.Values.Sum(p => p ?? decimal.Zero);
                }
            }
            
            public bool AllBreakdownValuesNull => TimeBreakdownDict.Values.All(p => p is null);
            public bool HasBreakdownValuesNull => TimeBreakdownDict.Values.Any(p => p is null);

            public override ValidationWarnings Validate()
            {
                var warnings = new List<WorkDataWarning>();
                
                if (TotalTime is not null && TimeBreakdownSum is not null)
                {
                    if (TimeBreakdownSum > TotalTime)
                    {
                        throw new Exception("Time Breakdown values add up to more than Total Time");
                    }

                    if (TimeBreakdownSum < TotalTime)
                    {
                        warnings.Add(WorkDataWarning.TimeBreakdownLessThanTotal);
                    }
                    
                }
                
                // TODO check if any values at all are contained and if not throw an error (or a warning - in cases of automatic parsing which produces some black results a warning should be fine

                if (TotalTime is null)
                {
                    warnings.Add(WorkDataWarning.TotalTimeNull);
                }
                
                if (AllBreakdownValuesNull)
                {
                    warnings.Add(WorkDataWarning.TimeBreakdownNull);
                }
                
                else if (HasBreakdownValuesNull)
                {
                    warnings.Add(WorkDataWarning.TimeBreakdownSomeNull);
                }

                return _otherProvidedWarnings + new ValidationWarnings(warnings);
            }
            
            
        }
        
        public record TimeDataAggregate: TimeData
        {
            public override bool IsAggregate => true;
            
            public override string[] TimeBreakdownCategories { get; }
            public ImmutableList<TimeData?> TimeDataList { get; }

            public override DatetimeDataVar[] DatetimeVariables { get; }

            public override DateTime?[] DatetimeValues(string varName) => TimeDataList
                    .SelectMany(p => p?.DatetimeValues(varName)).ToArray(); // TODO test this
                // This should recursively attempt to get arrays until only TimeDataSingle objects are reached
                

            public override int DataPointCount => 
            (int)
            TimeDataList
                .Select(p => p?.DataPointCount ?? 1)
                .Sum(p => p);

            public TimeDataAggregate(IEnumerable<TimeData?> timeDataEnumerable)
            {
                TimeDataList = timeDataEnumerable.ToImmutableList();
                TimeBreakdownCategories = TimeDataList
                    .Where(p => p is not null)
                    .SelectMany(p => p!.TimeBreakdownCategories)
                    .ToHashSet().ToArray();
            }

            public override int?[] TimeBreakdownValues(string category) =>
                TimeDataList.SelectMany(p => p?.TimeBreakdownValues(category)).ToArray(); // TODO test this

            public override SecondsVal TimeBreakdownSecondsValues(string category)
            {
                var categoryValues = TimeBreakdownValues(category);
                var nullCount = (int)categoryValues.Count(p => p is null);
                var nonNullCount = (int)categoryValues.Length - nullCount;
                var sum = 
                    nonNullCount == 0
                    ? null
                    : categoryValues.Where(p => p is not null).Sum();
                return new SecondsVal(sum, true, nonNullCount, nullCount);
            }

            public override ValidationWarnings Validate()
            {
                List<WorkDataWarning> warnings = new();
                
                var allCategoriesSet = (from elem in TimeDataList select elem?.TimeBreakdownCategories.ToHashSet()).ToHashSet();
                if (allCategoriesSet.Count > 1)
                {
                    warnings.Add(WorkDataWarning.TimeBreakdownHeterogeneousCategories);
                }
                return new ValidationWarnings(warnings) + ValidationWarnings.Aggregate(TimeDataList.Select(p => p?.Validate()));
            }
        }
        
        
        
    }
}