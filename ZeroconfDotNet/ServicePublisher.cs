﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using ZeroconfDotNet.DNS;
using ZeroconfDotNet.DNS.Records;

namespace ZeroconfDotNet
{
    public delegate ServiceInfo ServiceCallback();

    /// <summary>
    /// Main entry point.  Manages a mDNS listener that will respond
    /// to queries with any services registered to this publisher
    /// </summary>
    public class ServicePublisher : IDisposable
    {
        private readonly IServiceCore _service;
        private ILookup<string, ServiceCallback> _lookup;
        private IList<Tuple<string, ServiceCallback>> _callbacks = new List<Tuple<string, ServiceCallback>>();
        private object _lookupLock = new object();
        private bool _started = false;
        public ServicePublisher(IServiceCore core)
        {
            _service = core;
            LocalName = Environment.MachineName;
            _service.PacketReceived += _service_PacketReceived;
            //_listener.FindService
            //listener.FindServices += listener_FindServices;
        }

        void _service_PacketReceived(Packet p, System.Net.IPEndPoint endPoint)
        {
 	        if (p.IsQuery)
            {
                foreach (var query in p.Queries.Where(x => x.Record.RecordType == PTRAnswer.RecordType))
                {
                    SendServiceResponses(query.Record.Name, p.TransactionID);
                }
            }
            else
            {
                foreach (var resp in p.Answers.Where(x => x.Record.Name == LocalName + ".local"))
                {
                    switch (resp.Record.RecordType)
                    {
                        case AAnswer.RecordType:
                            if (!IsMyAddress((resp.Data as AAnswer).Address))
                            {
                                UpdateName();
                            }
                            break;
                        case AAAAAnswer.RecordType:
                            if (!IsMyAddress((resp.Data as AAAAAnswer).Address))
                            {
                                UpdateName();
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private bool IsMyAddress(IPAddress name)
        {
            return _service.Network.Addresses.Any(x => x.Equals(name));
        }

        private void UpdateName()
        {
            var regex = new Regex(@"(.*)(\d+)");
            var match = regex.Match(LocalName);

            string baseName = LocalName;
            int lastNumber = 0;
            if (match.Success)
            {
                lastNumber = int.Parse(match.Groups[1].Value);
                baseName = match.Groups[0].Value;
            }

            LocalName = baseName + ++lastNumber;
            SendNameCheck();
            NameUpdated(LocalName);
        }

        private void SendNameCheck()
        {
            var packet = new Packet();
            packet.IsQuery = true;
            var query = new Query();
            query.IsMulticast = true;
            query.Record = new Record();
            query.Record.Name = LocalName + ".local";
            query.Record.RecordType = 255; //ANY
            packet.Queries.Add(query);
            _service.SendPacket(packet);
        }

        private void SendServiceResponses(string name, UInt16 transactionId)
        {
            var services = _lookup[name].Select(x => x());
            var packets = services.Select(x => BuildResponse(name, transactionId, x, GetIP4Address(), GetIP6Address()));
            foreach (var packet in packets)
            {
                _service.SendPacket(packet);
            }
        }

        private string GetIP4Address()
        {
            return _service.Network.Addresses.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x.Address.ToString()).FirstOrDefault();
        }

        private string GetIP6Address()
        {
            return _service.Network.Addresses.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).Select(x => x.Address.ToString()).FirstOrDefault();
        }

       private  Packet BuildResponse(string name, UInt16 id, ServiceInfo info, string ip4Address, string ip6Address)
        {
            var ret = new Packet();
            ret.TransactionID = id;
            ret.Flags = 0x8400;
            var dnsName = name;
            var domainName = string.Join(".", info.Name, dnsName);
            var machineName = LocalName;
            var ptr = PTRAnswer.Build(dnsName, domainName, 4500, false, 1);
            var txt = TXTAnswer.Build(domainName, 4500, info.Flags, info.Data, true, 1);
            var srv = SRVAnswer.Build(domainName, 120, info.Priority, info.Weight, info.Port, machineName, true, 1);
            var a = AAnswer.Build(machineName, IPAddress.Parse(ip4Address), (UInt16)120, true, 1);
            var aaaa = AAAAAnswer.Build(machineName, IPAddress.Parse(ip6Address), (UInt16)120, true, 1);

            ret.Answers.Add(ptr);
            ret.Answers.Add(txt);
            ret.Answers.Add(srv);
            ret.Answers.Add(aaaa);
            ret.Answers.Add(a);
            return ret;
        }

        public event Action<String> NameUpdated = delegate { };

        private string _localName = Environment.MachineName;
        public string LocalName 
        {
            get
            {
                return _localName;
            }
            set
            {
                _localName = value;
                if (_started)
                {
                    SendNameCheck();
                }
            }
        }

        IEnumerable<ServiceInfo> listener_FindServices(string host)
        {
            ServiceCallback[] callbacks;
            lock (_lookupLock)
            {
                callbacks = _lookup[host].ToArray();
            }
            return callbacks.Select(x => x());
        }

        public void AddService(string host, ServiceInfo service)
        {
            _callbacks.Add(Tuple.Create<string, ServiceCallback>(host, () => service));
            CreateLookup();
        }

        private void CreateLookup()
        {
            if (_started)
            {
                Monitor.Enter(_lookupLock);
            }
            _lookup = _callbacks.ToLookup(x => x.Item1, x => x.Item2);
            if (_started)
            {
                Monitor.Exit(_lookupLock);
            }
        }

        public void AddService(string host, ServiceCallback callback)
        {
            _callbacks.Add(Tuple.Create(host, callback));
            CreateLookup();
        }

        public void Start()
        {
            _started = true;
            CreateLookup();
            _service.Start();
            SendNameCheck();
        }

        public void Stop()
        {
            _service.Stop();
            _started = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
