using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using Microsoft.EntityFrameworkCore;
using RemoteChecker.Models;

namespace RemoteChecker.CheckerLogics
{
    public class Worker
    {
        public static string Connection { get; set; }


        private static Worker _instance;
        private bool isInit = false;
        private CheckContext _context;

        public Worker()
        {
            _context = new CheckContext(Connection);
        }

        public static Worker GetInstance()
        {
            if (_instance == null)
                _instance = new Worker();
            return _instance;
        }

        private ConcurrentDictionary<(string Host, string Cron), (List<(int, bool)> CrIds, DateTime? NextOccurence)> tasks = new();
        private DateTime min;

        public async Task InitWorker()
        {
            min = DateTime.MaxValue;
            isInit = true;


            foreach (var cr in _context.CheckRequests)
            {
                var time = await AddCheckRequest(cr);
                if (time != null && time < min)
                    min = (DateTime)time;
            }

            await CallCheckRequestTasks();
        }

        public async Task RemoveCheckRequest(CheckRequest cr, (string, string) realkey = default)
        {
            if (!isInit)
                await InitWorker();

            var t1 = realkey == default ? (cr.HostAddress, cr.Cron) : realkey;
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

        public async Task<DateTime?> AddCheckRequest(CheckRequest cr)
        {
            if (!isInit)
                await InitWorker();


            if (cr == null)
                return null;

            var t1 = (cr.HostAddress, cr.Cron);

            DateTimeOffset? offset = CronExpression.Parse(cr.Cron).GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            var nextUtc = offset?.DateTime;

            if (tasks.Keys.Contains(t1))
            {
                var l = tasks[t1].CrIds;
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

        public async Task<int> ForceCheckRequest(CheckRequest cr)
        {
            if (!isInit)
                await InitWorker();
            var (HostAddress, _) = (cr.HostAddress, cr.Cron);

            int res = await PingUrl.PingUrlAsync(HostAddress);
            return res;
        }

        public async Task EditCheckRequest(CheckRequest cr)
        {
            if (!isInit)
                await InitWorker();

            var t1 = (cr.HostAddress, cr.Cron);

            foreach (var key in tasks.Keys)
            {
                var item = tasks[key];
                for (int i = 0; i < item.CrIds.Count; i++)
                {
                    if (item.CrIds[i].Item1 == cr.ID)
                    {
                        if (item.CrIds[i].Item2 != cr.Active)
                        {
                            await ChangeCheckRequestActivity(cr);
                            return;
                        }

                        if (t1 != key)
                        {
                            await RemoveCheckRequest(cr, key);
                            await AddCheckRequest(cr);
                            return;
                        }
                    }
                }
            }
        }

        public async Task ChangeCheckRequestActivity(CheckRequest cr)
        {
            if (!isInit)
                await InitWorker();

            var t1 = (cr.HostAddress, cr.Cron);

            if (!tasks.ContainsKey(t1))
                return;

            var a = tasks[t1].CrIds;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Item1 == cr.ID)
                {
                    a[i] = (cr.ID, cr.Active);
                    break;
                }
            }
        }

        public async Task CallCheckRequestTasks()
        {
            if (!isInit)
                await InitWorker();

            DateTime dt = DateTime.Now;

            foreach (var task in tasks)
            {
                if (dt >= task.Value.NextOccurence)
                {
                    DateTimeOffset? offset = CronExpression.Parse(task.Key.Cron).GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
                    var nextUtc = offset?.DateTime;

                    tasks[task.Key] = (task.Value.CrIds, nextUtc);

                    for (int i = 0; i < task.Value.CrIds.Count; i++)
                    {
                        if (task.Value.CrIds[i].Item2)
                        {
                            int res = await PingUrl.PingUrlAsync(task.Key.Host);
                            CheckRequest cr = _context.CheckRequests.Where(x => x.ID == task.Value.CrIds[i].Item1).FirstOrDefault();
                            CheckHistory ch = new()
                            {
                                CheckID = task.Value.CrIds[i].Item1,
                                Moment = dt,
                                Result = res,
                                CheckRequest = cr
                            };
                            _context.Add(ch);
                            await _context.SaveChangesAsync();
                        }
                    }

                }
            }
        }

        /*public Task Work()
        {
            return;
        }*/

    }
}
