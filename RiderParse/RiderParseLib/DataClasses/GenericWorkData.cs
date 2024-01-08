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

        public record WorkDataVar // TODO check if this need IEquitable implementation to work in set
        {

            public enum VarType
            {
                // Only one instance of pre-defined variables
                JobCount,
                Distance,
                Custom
            }

            public delegate bool ValueVerifier(decimal? value);
            
            public ValueVerifier? VerifyDel { get; }      
            
            public string Name { get; } // Unique identifier for the variable
            public string? Unit { get; }
            public bool IsSummable { get; }
            
            public VarType Type { get; }

      
            

            public WorkDataVar(VarType type)
            {
                
                switch (type)
                {
                    case VarType.Custom:
                        throw new Exception("Use different constructor for custom variables");
                    case VarType.Distance:
                        Name = "Distance";
                        IsSummable = true;
                        Unit = "m";
                        VerifyDel = value => value is null or >= 0;
                        break;
                    case VarType.JobCount:
                        Name = "JobCount";
                        IsSummable = true;
                        Unit = null;
                        VerifyDel = value => value is null | value % 1 == 0;
                        break;
                    default:
                        throw new Exception("Unrecognised WorkDataVar type");
                }

                Type = type;
            }

            public WorkDataVar(string name, string? unit, bool isSummable, ValueVerifier? valueVerifier)
            {
                if (new[] { "Distance", "JobCount" }.Contains(name))
                {
                    throw new Exception("Custom work variable name provided is same as one of the pre-defined variables");
                }
                Type = VarType.Custom;
                Name = name;
                Unit = unit;
                IsSummable = isSummable;
                VerifyDel = valueVerifier; // TODO fix this?
            }

            public bool VerifyValue(decimal? value)
            {
                if (value is null | VerifyDel is null)
                {
                    return true;
                }
                return VerifyDel!(value);
            }
            
            
        }
        

        public abstract record GenericWorkData
        {
            public abstract bool IsAggregate { get; }
            public abstract WorkDataVar[] Variables { get; }
            public abstract decimal?[] Values(string varName);
            public abstract VarValue VarValue(string varName);

            public abstract int DataPointCount { get; }

            public abstract ValidationWarnings Validate();

            public WorkDataVar? GetVar(string varName)
            {
                return Variables.Select(p => p.Name)
                    .Contains(varName)
                    ? Variables.First(p => p.Name == varName)
                    : null;
            }
        }

        public record GenericWorkDataSingle : GenericWorkData
        {
            
            private ReadOnlyDictionary<WorkDataVar, decimal?> WorkData { get; }
            
            public override WorkDataVar[] Variables { get; }

            public override bool IsAggregate => false;

            public override int DataPointCount => 1;
            
            public GenericWorkDataSingle(IDictionary<WorkDataVar, decimal?> workData)
            {
                if (workData.Keys.Select(p => p.Name).ToHashSet().Count < workData.Count)
                {
                    throw new Exception("GenericWorkDataSingle:" +
                                        "contains multiple workDataVar with the same names.");
                }
                // check needed since name may be the same but other values different, thus different WorkDataVar

                var invalidValueVars = workData
                    .Where(p => !p.Key.VerifyValue(p.Value))
                    .Select(p => p.Key.Name).ToArray(); // TODO test this
                
                if (invalidValueVars.Any())
                {
                    throw new Exception("GenericWorkDataSingle: invalid values for variables "
                                        + string.Join(", ", invalidValueVars));
                }

                
                WorkData = new ReadOnlyDictionary<WorkDataVar, decimal?>(workData);
                Variables = WorkData.Keys.ToArray();
            }
            
            public override decimal?[] Values(string varName)
            {
                if (!(Variables.Select(p => p.Name).Contains(varName)))
                {
                    return new decimal?[] { null };
                }

                return new[] { WorkData
                    .First(p => p.Key.Name == varName).Value };
            }
            
            public override VarValue VarValue(string varName)
            {
                var value = Values(varName)[0];
                return value is null 
                    ? new VarValue(null, false, 0, 1) 
                    : new VarValue(value, false, 1, 0);
            }

            public override ValidationWarnings Validate()
            {
                return new ValidationWarnings();
            }
            
        }

        
        public record GenericWorkDataAggregate : GenericWorkData, IEnumerable<GenericWorkData?>
        {
            public IImmutableList<GenericWorkData?> GenericWorkDataList { get; }
            
            public override bool IsAggregate => true;

            public override WorkDataVar[] Variables { get; }

            private readonly ValidationWarnings _aggregatedWarnings;

            public override int DataPointCount => 
                GenericWorkDataList
                .Select(p => p?.DataPointCount ?? 1)
                .Sum(p => p);

            public GenericWorkDataAggregate(IEnumerable<GenericWorkData?> genericWorkDataEnumerable)
            {
                GenericWorkDataList = genericWorkDataEnumerable.ToImmutableList();
                _aggregatedWarnings = ValidationWarnings.Aggregate(GenericWorkDataList.Select(p => p?.Validate()));
                Variables = GenericWorkDataList
                    .Where(p => p is not null)
                    .SelectMany(p => p!.Variables)
                    .ToHashSet().ToArray();

            }
            public override ValidationWarnings Validate()
            {
                List<WorkDataWarning> warnings = new();
                
                var allVariablesSet = GenericWorkDataList
                    .Where(p => p is not null)
                    .Select(p => p!.Variables).ToHashSet();
                if (allVariablesSet.Count > 1) { warnings.Add(WorkDataWarning.GenericWorkDataHeterogeneousVariables);}

                return new ValidationWarnings(warnings) + _aggregatedWarnings;
            }
            
            public override decimal?[] Values(string varName) =>
                GenericWorkDataList
                    .SelectMany(p => p?.Values(varName)).ToArray(); // TODO test this
                // This should recursively attempt to get arrays until only GenericWorkDataSingle objects are reached

            public override VarValue VarValue(string varName)
            {
                var varValues = Values(varName);
                var nullCount = (uint)varValues.Count(p => p is null);
                var nonNullCount = (uint)varValues.Length - nullCount;
                var sum = 
                    nonNullCount == 0
                        ? null
                        : varValues.Where(p => p is not null).Sum();
                return new VarValue(sum, true, nonNullCount, nullCount);
            }
                
            public IEnumerator<GenericWorkData?> GetEnumerator() => GenericWorkDataList.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}