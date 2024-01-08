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
        public enum WorkRes
        {
            Job,
            Session,
            Date,
            Week,
            Month,
            Year,
            TaxYearUk
        }

        public record VarValue
        {
            public decimal? Value { get; }
            public bool IsAggregate { get; }
            public uint NonNullCount { get; }
            public uint NullCount { get; }

            public VarValue(decimal? value, bool isAggregate, uint nonNullCount, uint nullCount)
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
        public abstract record WorkItem
        {
            public virtual WorkRes Res { get; }
            
            public virtual bool IsSummary { get; }
            
            public virtual PayData? PayData { get; }

            public virtual GenericWorkData? AdditionalData { get; }

            public virtual TimeData TimeData { get; }
            
            public virtual int? JobCount { get; }

            // TODO fix and implement
            // public bool hasDate => TimeData.Date is not null;
            // public decimal? PayPerHour => PayData?.TotalPay / TimeData.HoursWorked;
            // public decimal? PayPerJob => PayData?.TotalPay / JobCount; // TODO check if these are equivalent

            public virtual ValidationWarnings Validate() => PayData?.Validate() ?? new ValidationWarnings();

        }

        // Used as a "unit" of data for a specific interval.
        public abstract record IntervalWork: WorkItem
        {
            // public override ValidationResult Validate() { return base.Validate(); }
        }
        
        public record Session: IntervalWork
        {
            public override bool IsSummary { get; } = false;

            // public override ValidationResult Validate() { return base.Validate(); }

        }
        
        public record DateWork: IntervalWork
        {
            public DateOnly Date { get; init; }
            
            
            // public override ValidationResult Validate() { return base.Validate(); }

        }
        
        public sealed record Job: WorkItem
        {
            public override WorkRes Res => WorkRes.Job;
            public override int? JobCount => 1;

            public DateOnly Date { get; init; }

            // public override ValidationResult Validate() { return base.Validate(); }

        }

        public record WorkItemCollection
        {
            public virtual IReadOnlyList<WorkItem> Items { get; init; }
            
        }

        public sealed record JobCollection : WorkItemCollection
        {
            public IImmutableList<Job> Jobs { get; init; }
            public override IReadOnlyList<WorkItem> Items => Jobs; // TODO test if this works without casting
            
            public DateOnly[] Date => (from elem in Jobs select elem.Date).ToArray();


            public JobCollection DateSubset(DateOnly startDate, DateOnly endDate)
            {
                return new JobCollection(from elem in Jobs
                    where elem.Date >= startDate & elem.Date <= endDate
                    select elem);
            }

            public JobCollection(IEnumerable<Job> jobs)
            {
                Jobs = jobs.ToImmutableList();
            }
            
        }

    }

}
