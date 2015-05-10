using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace ZeroconfDotNet.DNS
{
    delegate void RecordUpdated(string name, IEnumerable<Answer> answers);


    class ServiceCache
    {
        private readonly ServiceCore _service;
        private readonly Dictionary<string, RecordCache> _cache = new Dictionary<string, RecordCache>();
        

        public ServiceCache(ServiceCore service)
        {
            _service = service;
            _service.PacketReceived += AddRecord;
        }

        public RecordUpdated RecordUpdated = delegate { };

        public void AddRecord(Packet p, IPEndPoint endPoint)
        {
            if (!p.IsQuery)
            {
                foreach (var answer in p.Answers)
                {
                    var key = answer.Record.Name;
                    if (!_cache.ContainsKey(key))
                    {
                        _cache[key] = new RecordCache();
                    }                    
                    _cache[key].AddRecord(answer);
                }
            }
        }

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
}
