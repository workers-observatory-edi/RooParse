using Microsoft.Data.Analysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static DataFrameExtensions.DataFrameExtensions;

namespace RiderParseLib
{

    namespace Roo
    {
        public class RooSessionCollection : ICollection, IEnumerable<RooSession>
        {
            public HashSet<RooSession> Sessions
            {
                get; private set;
            }

            // TODO convert some of these to static methods and leave only 1 constructor
            // Construct using list of session strings
            public RooSessionCollection(List<string> sessionStrList)
            {
                Sessions = new();
                foreach (string sessionStr in sessionStrList)
                {
                    RooSession thisIterSession = new(sessionStr);
                    Sessions.Add(thisIterSession);
                }
            }

            // Construct using list of Sessions
            public RooSessionCollection(IEnumerable<RooSession> sessionsEnumerable)
            { Sessions = sessionsEnumerable.ToHashSet<RooSession>(); }

            // Construct using one session only
            public RooSessionCollection(RooSession session)
            { Sessions = new(); Sessions.Add(session); }

            public RooSessionCollection(IEnumerable<RooSessionCollection> sessionCollectionsEnumerable)
            {
                Sessions = new();
                foreach (RooSessionCollection sessionCollection in sessionCollectionsEnumerable)
                {
                    foreach (RooSession session in sessionCollection)
                    {
                        Sessions.Add(session);
                    }
                }
            }

            public RooSessionCollection(IEnumerable<RooPayslip> payslipEnumerable)
            {
                Sessions = new();
                foreach (RooPayslip payslip in payslipEnumerable)
                {
                    foreach (RooSession session in payslip.Sessions)
                    {
                        Sessions.Add(session);
                    }
                }
            }

            public DataFrame SessionsSummaryDF
            {
                get
                {
                    List<DataFrame> sessionDataFramesList = new();
                    foreach (RooSession session in Sessions)
                    {
                        sessionDataFramesList.Add(session.SummaryDF);
                    }
                    return ConcatDataFrames(sessionDataFramesList);
                }
            }

            public DateTime FirstTimeIn
            {
                get
                {
                    return (from session in Sessions
                            orderby session.TimeInDT ascending
                            select session.TimeInDT).First();
                }
            }

            public DateTime LastTimeOut
            {
                get
                {
                    return (from session in Sessions
                            orderby session.TimeOutDT descending
                            select session.TimeOutDT).First();
                }
            }

            // CopyTo Method for ICollection
            void ICollection.CopyTo(Array myArr, int index)
            {
                foreach (RooSession i in Sessions)
                {
                    myArr.SetValue(i, index);
                    index++;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Sessions.GetEnumerator();
            }

            public IEnumerator<RooSession> GetEnumerator()
            { return Sessions.GetEnumerator(); }

            // The IsSynchronized Boolean property returns True if the
            // collection is designed to be thread safe; otherwise, it returns False.
            bool ICollection.IsSynchronized { get { return false; } }

            // The SyncRoot property returns an object, which is used for synchronizing
            // the collection. This returns the instance of the object or returns the
            // SyncRoot of other collections if the collection contains other collections.
            object ICollection.SyncRoot { get { return this; } }

            // The Count read-only property returns the number of items in the collection.
            int ICollection.Count { get { return Sessions.Count; } }
        }
    }

}
