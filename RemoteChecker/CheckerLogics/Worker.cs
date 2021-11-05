using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using RemoteChecker.Models;

namespace RemoteChecker.CheckerLogics
{
    public class Worker
    {
        private static Worker _instance;
        private Worker()
        {
        }

        public static Worker GetInstance()
        {
            if (_instance == null)
                _instance = new Worker();
            return _instance;
            //throw new Exception("Worker has not been initialized, call InitWorker()");
        }

        private ConcurrentDictionary<(string host, string cron), (List<(int, bool)> crIds, DateTime? nextOccurence)> tasks = new();
        private CheckContext _context;
        private DateTime min;

        public void InitWorker(CheckContext context)
        {
            _context = context;
            min = DateTime.MaxValue;

            foreach (var cr in _context.CheckRequests)
            {
                var time = AddCheckRequest(cr);
                if (time != null && time < min)
                    min = (DateTime)time;
            }

            //RemoveCheckRequest(_context.CheckRequests.Where(x => x.HostAddress == "ibm.com").Skip(0).FirstOrDefault());
        }

        public void RemoveCheckRequest(CheckRequest cr)
        {
            var t1 = (cr.HostAddress, cr.Cron);
            if (!tasks.ContainsKey(t1))
                return;

            var (crIds, _) = tasks[t1];
            var t2 = (cr.ID, cr.Active);
            if (!crIds.Contains(t2))
                return;

            crIds.Remove(t2);

            if (crIds.Count == 0)
                tasks.Remove(t1, out _);
        }

        public DateTime? AddCheckRequest(CheckRequest cr)
        {
            if (cr == null)
                return null;

            var t1 = (cr.HostAddress, cr.Cron);

            DateTimeOffset? offset = CronExpression.Parse(cr.Cron).GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            var nextUtc = offset?.DateTime;

            if (tasks.Keys.Contains(t1))
            {
                var l = tasks[t1].crIds;
                l.Add((cr.ID, cr.Active));
                tasks[t1] = (l, nextUtc);
            }
            else
            {
                var crIds = new List<(int, bool)>
                    {
                        (cr.ID, cr.Active)
                    };

                Action<string> f = (z) => throw new Exception(z);

                tasks.AddOrUpdate(t1, (crIds, nextUtc), (a1, a2) => (crIds, nextUtc));
            }

            return nextUtc;
        }

        public void ChangeCheckRequestActivity(CheckRequest cr)
        {
            var t1 = (cr.HostAddress, cr.Cron);

            if (!tasks.ContainsKey(t1))
                return;

            var a = tasks[t1].crIds;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Item1 == cr.ID)
                {
                    a[i] = (cr.ID, cr.Active);
                        break;
                }
            }
        }
    }
}
