using System;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZeroconfDotNet.Utils;
using ZeroconfDotNet.DNS;

namespace ZeroconfServiceTests
{
    interface IRequestSender
    {
        void AddRecordRequest(string name, int recordType);
        void AddRecordInfo(Packet p);
        Packet GetQuery(DateTime time, out int nextSend);
    }

    [TestClass]
    public class RequestSenderTests
    {
        [TestMethod]
        public void SendInitialRequest()
        {
            var sender = (IRequestSender)null;
            sender.AddRecordRequest("scooby.doo.local", 1);
            int next;
            var packet = sender.GetQuery(DateTime.Now, out next);
            Assert.IsTrue(packet.IsQuery);
            Assert.AreEqual(packet.Questions, 1);
            Assert.AreEqual(next, 1);
        }
    }
}
