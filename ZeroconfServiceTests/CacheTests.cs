using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiscoveryDotNet.DNS;
using DiscoveryDotNet.Utils;
using Moq;

namespace ZeroconfServiceTests
{    
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public void OneRecordExpire()
        {
            var name = "scooby.doo.local";
            var recordType = 1;
            var ttl = 1;    

            var timer = new Mock<ITimer>();
            var serviceCache = new ServiceCache(timer.Object);
            var packet = CreatePacket(new[] { new Tuple<string, int, int>(name, recordType, ttl) });
            var verified = false;
            serviceCache.AddPacket(packet);
            serviceCache.RequestUpdate += (x) =>
                {
                    Assert.AreEqual(x.Length, 1);
                    Assert.AreEqual(x[0].Item1, name);
                    Assert.AreEqual(x[0].Item2, recordType);
                    verified = true;
                };
            Thread.Sleep(1000);
            timer.Raise(x => x.Fired += null);
            Assert.IsTrue(verified);
        }

        [TestMethod]
        public void OneRecordRefreshed()
        {
            var name = "scooby.doo.local";
            var recordType = 1;
            var ttl = 1;
            
            var timer = new Mock<ITimer>();
            var serviceCache = new ServiceCache(timer.Object);
            var packet = CreatePacket(new[] { new Tuple<string, int, int>(name, recordType, ttl) });
            var verified = true;
            serviceCache.AddPacket(packet);

            serviceCache.RequestUpdate += (x) =>
            {
                if (x.Length != 0)
                    verified = false;
            };
            Thread.Sleep(1000);

            var packet2 = CreatePacket(new[] { new Tuple<string, int, int>(name, recordType, ttl * 10) });
            serviceCache.AddPacket(packet2);

            timer.Raise(x => x.Fired += null);
            Assert.IsTrue(verified);
        }

        private static Packet CreatePacket(Tuple<string, int, int>[] RecordInfos)
        {
            var Answers = new List<Answer>();
            foreach (var item in RecordInfos)
            {
                Answers.Add(new Answer 
                {
                    Record = new Record
                    {
                        RecordType = (UInt16)item.Item2,
                        Name = item.Item1,
                    },
                    CacheFlush = false,
                    TTL = (uint)item.Item3,
                });
            }


            var ret = new Packet
            {                
                Answers = Answers,
            };
            ret.Flags.IsResponse = true;
            ret.Flags.IsAuthoritative = true;
            return ret;
        }
    }
}
