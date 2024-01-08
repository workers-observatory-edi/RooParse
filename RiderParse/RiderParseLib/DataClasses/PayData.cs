using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.IO;

namespace RiderParseLib.DataClasses
{

    namespace WorkData
    {
        public abstract record PayData
        {
            public abstract bool IsAggregate { get; }
            public abstract string[] Categories { get; }

            public abstract decimal?[] Values(string category);
            public abstract VarValue VarValue(string category);
            
            public abstract uint DataPointCount { get; }

            public abstract ValidationWarnings Validate();
        }

        public record PayDataSingle: PayData
        {
            public decimal? TotalPay { get; }
            
            public override string[] Categories { get; }
            
            private readonly ValidationWarnings? _otherProvidedWarnings;
            public override bool IsAggregate => false;
            
            public override uint DataPointCount => 1;
            
            protected ReadOnlyDictionary<string, decimal?> PayBreakdownDict { get; init; }
            
            public PayDataSingle(
                decimal? totalPay,
                ReadOnlyDictionary<string, decimal?> payBreakdownDict,
                ValidationWarnings? validationWarnings)
            {
                
                _otherProvidedWarnings = validationWarnings; 
                TotalPay = totalPay;
                Categories = payBreakdownDict.Keys.ToArray();
                PayBreakdownDict = payBreakdownDict;
            }

            public override decimal?[] Values(string category)
            {
                if (category != "TotalPay" & !PayBreakdownDict.ContainsKey(category))
                {
                    return new decimal?[] { null };
                }
                return category == "TotalPay" ? new[] { TotalPay } : new[] { PayBreakdownDict[category] };
            }
            
            public override VarValue VarValue(string category)
            {
                var value = Values(category)[0];
                return value is null 
                    ? new VarValue(null, false, 0, 1) 
                    : new VarValue(value, false, 1, 0);
            }
            
            public decimal? PayBreakdownSum
            {
                
                get
                {
                    if (AllBreakdownValuesNull)
                    {
                        return null;
                    }
                    return PayBreakdownDict.Values.Sum(p => p ?? decimal.Zero);
                }
            }
            
            public bool AllBreakdownValuesNull => PayBreakdownDict.Values.All(p => p is null);
            public bool HasBreakdownValuesNull => PayBreakdownDict.Values.Any(p => p is null);

            public override ValidationWarnings Validate()
            {
                var warnings = new List<WorkDataWarning>();
                
                if (TotalPay is not null && PayBreakdownSum is not null)
                {
                    if (PayBreakdownSum > TotalPay)
                    {
                        throw new Exception("Pay Breakdown values add up to more than Total Pay");
                    }

                    if (PayBreakdownSum < TotalPay)
                    {
                        warnings.Add(WorkDataWarning.PayBreakdownLessThanTotal);
                    }
                    
                }
                
                // TODO check if any values at all are contained and if not throw an error (or a warning - in cases of automatic parsing which produces some black results a warning should be fine

                if (TotalPay is null)
                {
                    warnings.Add(WorkDataWarning.TotalPayNull);
                }
                
                if (AllBreakdownValuesNull)
                {
                    warnings.Add(WorkDataWarning.PayBreakdownNull);
                }
                
                else if (HasBreakdownValuesNull)
                {
                    warnings.Add(WorkDataWarning.PayBreakdownSomeNull);
                }

                return _otherProvidedWarnings + new ValidationWarnings(warnings);
            }
            
            
        }
        
        public record PayDataAggregate: PayData
        {
            public override bool IsAggregate => true;
            
            public override string[] Categories { get; }
            public ImmutableList<PayData?> PayDataList { get; }
            
            public override uint DataPointCount => 
                (uint)
                PayDataList
                    .Select(p => p?.DataPointCount ?? 1)
                    .Sum(p => p);

            public PayDataAggregate(IEnumerable<PayData?> payDataEnumerable)
            {
                PayDataList = payDataEnumerable.ToImmutableList();
                Categories = PayDataList
                    .Where(p => p is not null)
                    .SelectMany(p => p!.Categories)
                    .ToHashSet().ToArray();
            }

            public override decimal?[] Values(string category) =>
                PayDataList.SelectMany(p => p?.Values(category)).ToArray(); // TODO test this

            public override VarValue VarValue(string category)
            {
                var categoryValues = Values(category);
                var nullCount = (uint)categoryValues.Count(p => p is null);
                var nonNullCount = (uint)categoryValues.Length - nullCount;
                var sum = 
                    nonNullCount == 0
                    ? null
                    : categoryValues.Where(p => p is not null).Sum();
                return new VarValue(sum, true, nonNullCount, nullCount);
            }

            public override ValidationWarnings Validate()
            {
                List<WorkDataWarning> warnings = new();
                
                var allCategoriesSet = (from elem in PayDataList select elem?.Categories.ToHashSet()).ToHashSet();
                if (allCategoriesSet.Count > 1)
                {
                    warnings.Add(WorkDataWarning.PayBreakdownHeterogeneousCategories);
                }
                return new ValidationWarnings(warnings) + ValidationWarnings.Aggregate(PayDataList.Select(p => p?.Validate()));
            }
        }
    }
}