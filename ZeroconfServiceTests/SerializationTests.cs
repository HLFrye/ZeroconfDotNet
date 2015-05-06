using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using ZeroconfDotNet;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.DNS.Records;

namespace ZeroconfServiceTests
{
    [TestClass]
    public class SerializationTests
    {
        byte[] ParseWiresharkString(string s)
        {
            var ret = new byte[s.Length / 2];
            for (var i = 0; i < s.Length; i += 2)
            {
                ret[i / 2] = byte.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }

        [TestMethod]
        public void DeserializeRequest()
        {
            var reqHex = "000000000001000000000000085f70756274657374045f746370056c6f63616c00000c0001";
            var reqBytes = ParseWiresharkString(reqHex);

            var packet = PacketReader.Read(reqBytes);

            Assert.IsTrue(packet.IsQuery);
            Assert.AreEqual(packet.TransactionID, 0);
            Assert.AreEqual(packet.Questions, 1);
            Assert.AreEqual(packet.AnswerRRs, 0);
            Assert.AreEqual(packet.AuthorityRRs, 0);
            Assert.AreEqual(packet.AdditionalRRs, 0);

            var query = packet.Queries[0];
            Assert.AreEqual(query.Record.Class, 1);
            Assert.AreEqual(query.Record.RecordType, 12);
            Assert.AreEqual("_pubtest._tcp.local", query.Record.Name);
        }

        [TestMethod]
        public void DeserializeResponse()
        {
            var packetHex = "000084000000000500000000085f70756274657374045f746370056c6f63616c00000c000100001194000a0750756274657374c00cc02b0010800100001194000100c02b0021800100000078000f00000000270f067562756e7475c01ac054001c8001000000780010fe80000000000000020c29fffe0de789c05400018001000000780004c0a81080";
            var packetBytes = ParseWiresharkString(packetHex);
            var packet = PacketReader.Read(packetBytes);

            Assert.IsFalse(packet.IsQuery);
            Assert.AreEqual(packet.TransactionID, 0);
            Assert.AreEqual(packet.Questions, 0);
            Assert.AreEqual(packet.AnswerRRs, 5);
            Assert.AreEqual(packet.AuthorityRRs, 0);
            Assert.AreEqual(packet.AdditionalRRs, 0);

            var ptrAns = packet.Answers.Single(x => x.Record.RecordType == 12);
            Assert.IsTrue(ptrAns.Record.Name.Equals("_pubtest._tcp.local"));
            Assert.AreEqual(ptrAns.Record.Class, 1);
            Assert.IsFalse(ptrAns.CacheFlush);
            Assert.AreEqual(ptrAns.TTL, (uint)4500);
            var ptrData = ptrAns.Data as PTRAnswer;
            Assert.IsTrue(ptrData.DomainName.Equals("Pubtest._pubtest._tcp.local"));

            var txtAns = packet.Answers.Single(x => x.Record.RecordType == 16);
            Assert.IsTrue(txtAns.Record.Name.Equals("Pubtest._pubtest._tcp.local"));
            Assert.AreEqual(txtAns.Record.Class, 1);
            Assert.IsTrue(txtAns.CacheFlush);
            Assert.AreEqual(txtAns.TTL, (uint)4500);
            var txtData = txtAns.Data as TXTAnswer;
            Assert.AreEqual(txtData.Data.Count, 0);
            Assert.AreEqual(txtData.Flags.Count, 0);

            var srv = packet.Answers.Single(x => x.Record.RecordType == 33);
            Assert.AreEqual(srv.Record.Class, 1);
            Assert.IsTrue(srv.CacheFlush);
            Assert.AreEqual(srv.TTL, (uint)120);

            var srvData = srv.Data as SRVAnswer;
            Assert.AreEqual(SRVAnswer.GetService(srv.Record), "Pubtest");
            Assert.AreEqual(SRVAnswer.GetProtocol(srv.Record), "_pubtest");
            Assert.AreEqual(SRVAnswer.GetName(srv.Record), "_tcp.local");
            Assert.AreEqual(srvData.Priority, 0);
            Assert.AreEqual(srvData.Weight, 0);
            Assert.AreEqual(srvData.Port, 9999);
            Assert.IsTrue(srvData.Target.Equals("ubuntu.local"));

            var aaaa = packet.Answers.Single(x => x.Record.RecordType == 28);
            Assert.AreEqual(aaaa.Record.Name, "ubuntu.local");
            Assert.AreEqual(aaaa.Record.Class, 1);
            Assert.IsTrue(aaaa.CacheFlush);
            Assert.AreEqual(aaaa.TTL, (uint)120);
            var aaaaData = aaaa.Data as AAAAAnswer;
            Assert.AreEqual(aaaaData.Address, IPAddress.Parse("fe80::20c:29ff:fe0d:e789"));

            var a = packet.Answers.Single(x => x.Record.RecordType == 1);
            Assert.AreEqual(a.Record.Name, "ubuntu.local");
            Assert.AreEqual(a.Record.Class, 1);
            Assert.IsTrue(a.CacheFlush);
            Assert.AreEqual(a.TTL, (uint)120);
            var aData = a.Data as AAnswer;
            Assert.AreEqual(aData.Address, IPAddress.Parse("192.168.16.128"));
        }

        [TestMethod]
        public void SerializeRequest()
        {
            var req = IPv4Service.BuildRequest("_pubtest._tcp.local", true, 0);
            var data = PacketWriter.Write(req);            

            var reqHex = "000000000001000000000000085f70756274657374045f746370056c6f63616c00000c0001";
            var reqBytes = ParseWiresharkString(reqHex);
            CompareBytes(reqBytes, data);
        }

        void CompareBytes(byte[] expected, byte[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < actual.Length; ++i)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void SerializeResponse()
        {
            var info = new ServiceInfo();
            info.Name = "Pubtest";
            info.Port = (UInt16)9999;
            IPv4Service.MachineName = "ubuntu.local";
            var resp = IPv4Service.BuildResponse("_pubtest._tcp.local", 0, info, "192.168.16.128", "fe80::20c:29ff:fe0d:e789");
            var data = PacketWriter.Write(resp);

            var respHex = "000084000000000500000000085f70756274657374045f746370056c6f63616c00000c000100001194000a0750756274657374c00cc02b0010800100001194000100c02b0021800100000078000f00000000270f067562756e7475c01ac054001c8001000000780010fe80000000000000020c29fffe0de789c05400018001000000780004c0a81080";
            var respBytes = ParseWiresharkString(respHex);
            CompareBytes(respBytes, data);
        }
    }
}
