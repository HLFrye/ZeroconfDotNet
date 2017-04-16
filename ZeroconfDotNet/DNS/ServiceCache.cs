using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using DiscoveryDotNet.Utils;
using DiscoveryDotNet.DNS.Records;

namespace DiscoveryDotNet.DNS
{
    delegate void RecordUpdated(string name, IEnumerable<Answer> answers);

    public class ServiceCache : IServiceCache
    {
        private readonly ITimer _timer;
        private int? nextCheck = null;

        public ServiceCache(ITimer timer)
        {
            _timer = timer;
            _timer.Fired += _timer_Fired;
        }

        void _timer_Fired()
        {
            var expired = GetExpiredAnswers();
            var updates = expired.Select(x => new Tuple<string, int>(x.Record.Name, x.Record.RecordType));
            RequestUpdate(updates.ToArray());

            nextCheck = GetNextTTL();
            if (nextCheck.HasValue)
                _timer.FireNext(nextCheck.Value);
        }

        IEnumerable<Answer> GetExpiredAnswers()
        {
            var ret = Enumerable.Empty<Answer>();
            foreach (var cache in _cache.Values)
            {
                ret = ret.Concat(cache.GetExpiredAnswers());
            }
            return ret;
        }

        int? GetNextTTL()
        {
            return _cache.Values.Min(x => x.GetNextTTL());
        }

        private readonly Dictionary<string, RecordCache> _cache = new Dictionary<string, RecordCache>();

        public void AddPacket(Packet p)
        {
            foreach (var ans in p.Answers)
            {
                var key = ans.Record.Name;
                if (!_cache.ContainsKey(key))
                {
                    _cache[key] = new RecordCache();
                }
                _cache[key].AddRecord(ans);                

                if ((!nextCheck.HasValue) || (nextCheck > ans.TTL))
                {
                    _timer.FireNext((int)ans.TTL);
                    nextCheck = (int)ans.TTL;
                }
            }
        }

        public event RequestUpdateDelegate RequestUpdate = delegate { };

        public IEnumerable<Answer> CachedAnswers(string name)
        {
            return _cache[name].GetCachedAnswers();
        }

        class RecordCache
        {
            private Dictionary<int, CachedAnswer> Answers = new Dictionary<int, CachedAnswer>();

            public void AddRecord(Answer ans)
            {
                Answers[ans.Record.RecordType] = new CachedAnswer(ans);
            }

            public IEnumerable<Answer> GetCachedAnswers()
            {
                return Answers.Values.Where(x => !x.Expired).Select(x => x.Answer).ToList();
            }

            public IEnumerable<Answer> GetExpiredAnswers()
            {
                var expired = Answers.Values.Where(x => x.Expired).Select(x => x.Answer).ToList();
                foreach (var recordType in expired.Select(x => x.Record.RecordType))
                {
                    Answers.Remove(recordType);
                }
                return expired;
            }

            public int? GetNextTTL()
            {
                var ret = Answers.Values.Min(x => (int?)x.Answer.TTL);
                return ret;
            }
        }

        class CachedAnswer
        {
            readonly DateTime CachedAt = DateTime.Now;
            public readonly Answer Answer;

            public CachedAnswer(Answer ans)
            {
                Answer = ans;
            }

            public bool Expired
            {
                get
                {
                    return (DateTime.Now - CachedAt) > TimeSpan.FromSeconds(Answer.TTL);
                }
            }
        }
    }

    class WatchedService
    {
        public WatchedService(string serviceName)
        {

        }

        public void AddAnswer(PTRAnswer ptr)
        {

        }
    }

    interface IRecordCache { }

    class ServiceCache2 : IServiceCache2
    {
        IRecordCache _recordCache;

        public ServiceCache2(IRecordCache recordCache)
        {
            _recordCache = recordCache;

        }

        public void AnswerAdded(Answer a)
        {

        }

        public void AnswerExpired(Answer a)
        {

        }

        public void WatchService(string name)
        {
            
        }



        public event Action<ServiceInfo> ServiceAdded;

        public event Action<ServiceInfo> ServiceExpired;
    }
}
