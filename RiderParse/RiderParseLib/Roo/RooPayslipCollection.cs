using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Immutable;

namespace RiderParseLib
{

    namespace Roo
    {
        public class RooPayslipCollection : ICollection, IEnumerable<RooPayslip>
    {
        private readonly HashSet<RooPayslip> _payslips;
        private readonly HashSet<RooPayslip> _payslipsFailed; // TODO fix this
        private readonly RooSessionCollection _sessions;
        private readonly RooSessionAdjCollection _sessionsAdj;

        public IEnumerable<RooPayslip> Payslips => _payslips.ToImmutableHashSet<RooPayslip>();

        public ImmutableHashSet<RooPayslip> PayslipsFailed => _payslipsFailed.ToImmutableHashSet<RooPayslip>();

        public ImmutableHashSet<RooSession> Sessions => _sessions.ToImmutableHashSet<RooSession>();

        public RooSessionAdjCollection.SummaryDFs Summary { get; private set; }

        // Construct using enumerable of Payslips
        public RooPayslipCollection(IEnumerable<RooPayslip> payslips)
        {
            List<RooPayslip> payslipsList = payslips.ToList();
            _payslips = new();
            _payslipsFailed = new();

            foreach (RooPayslip payslip in payslipsList)
            {
                switch (payslip.ParseSuccess)
                {
                    case true:
                        _payslips.Add(payslip); break;
                    case false:
                        _payslipsFailed.Add(payslip); break;
                    default:
                        throw new Exception("payslip.ParseSuccess must be true or false");
                }
            }

            _sessions = new RooSessionCollection(_payslips);
            _sessionsAdj = RooSessionAdjCollection.FromPayslipEnumerable(_payslips);
            Summary = _sessionsAdj.Summary;

        }

        public static RooPayslipCollection FromStrings(IEnumerable<string> payslipStrings)
        {
            List<RooPayslip> payslipsList = new();
            foreach (string payslipString in payslipStrings)
            {
                payslipsList.Add(new RooPayslip(payslipString, ""));
            }
            RooPayslipCollection payslipCollection = new(payslipsList);
            return payslipCollection;
        }

        public static RooPayslipCollection FromDir(string path)
        {
            string[] allFiles = Directory.GetFiles(path, "*.pdf");
            List<RooPayslip> payslips = new();
            foreach (string filepath in allFiles) { payslips.Add(RooPayslip.FromPath(filepath)); }
            return new RooPayslipCollection(payslips);

        }

        // Methods for ICollection
        void ICollection.CopyTo(Array myArr, int index)
        {
            foreach (RooPayslip i in _payslips)
            {
                myArr.SetValue(i, index);
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() { return _payslips.GetEnumerator(); }

        public IEnumerator<RooPayslip> GetEnumerator() { return _payslips.GetEnumerator(); }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        int ICollection.Count => _payslips.Count;
    }
    }

}
