using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using ZeroconfDotNet.DNS.Records;

namespace ZeroconfDotNet.DNS
{
    public abstract class Service
    {
        string _name;
        public Service(string name)
        {
            _name = name + ".local";
        }

        public string Name { get { return _name; } }

        public abstract Answer[] AddressRecords();
    }

    public class IPv4Service : Service
    {
        public IPv4Service(IPAddress addr, string machineName)
            :base(machineName)
        {
            _addr = addr;
        }

        IPAddress _addr;

        public override Answer[] AddressRecords()
        {
            return new Answer[]
            {
                // new DNS.AAnswer(Name, _addr)
            };
        }

        public static Packet BuildRequest(string name, bool queryMulticast, UInt16 id)
        {
            var ret = new Packet();
            ret.TransactionID = id;
            ret.Flags.IsResponse = false;
            ret.Queries = new Query[] 
            {
                new Query() 
                {
                    Record = new Record(name, 12, 1),
                },
            };
            return ret;
        }

        public static Packet BuildResponse(string name, UInt16 id, ServiceInfo info, string ip4Address, string ip6Address)
        {
            var ret = new Packet();
            ret.TransactionID = id;
            ret.Flags.IsResponse = true;
            ret.Flags.IsAuthoritative = true;
            var dnsName = name;
            var domainName = string.Join(".", info.Name, dnsName);
            var machineName = MachineName;
            var ptr = PTRAnswer.Build(dnsName, domainName, 4500, false, 1);
            var txt = TXTAnswer.Build(domainName, 4500, info.Flags, info.Data, true, 1);
            var srv = SRVAnswer.Build(domainName, 120, info.Priority, info.Weight, info.Port, MachineName, true, 1);
            var a = AAnswer.Build(machineName, IPAddress.Parse(ip4Address), (UInt16)120, true, 1);
            var aaaa = AAAAAnswer.Build(machineName, IPAddress.Parse(ip6Address), (UInt16)120, true, 1);
            
            ret.Answers.Add(ptr);
            ret.Answers.Add(txt);
            ret.Answers.Add(srv);
            ret.Answers.Add(aaaa);
            ret.Answers.Add(a);
            return ret;
        }

        static string _machineName = Environment.MachineName + ".local";
        public static string MachineName
        {
            get
            {
                return _machineName;
            }
            set
            {
                _machineName = value;
            }

        }
    }

    public class IPv6Service : Service
    {
        public IPv6Service(IPAddress ip6, IPAddress ip4, string machineName)
            :base(machineName)
        {
            _ip4 = ip4;
            _ip6 = ip6;
        }

        IPAddress _ip4;
        IPAddress _ip6;

        public override Answer[] AddressRecords()
        {
            return new Answer[]
            {
                // new DNS.ARecord(Name, _ip4),
                // new DNS.AAAARecord(Name, _ip6),
            };
        }


        public static Packet BuildRequest(string name, bool queryMulticast, UInt16 id)
        {
            //TODO
            return null;
        }

        public static Packet BuildResponse(string name, UInt16 id, ServiceInfo info)
        {
            //TODO
            return null;
        }
    }
}
