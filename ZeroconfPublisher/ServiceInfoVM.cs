using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using ZeroconfDotNet;

namespace ZeroconfPublisher
{
    class ServiceInfoVM
    {
        public ServiceInfoVM()
            : base()
        {
            Data = new ObservableCollection<DataItem>();
        }

        public ObservableCollection<DataItem> Data { get; set; }
        public string IP4Address { get; set; }
        public string IP6Address { get; set; }

        public UInt16 Port
        { get; set; }

        public string Name { get; set; }
        public string Protocol { get; set; }
        public int Priority { get; set; }
        public int Weight { get; set; }

        public ServiceInfo GetInfo()
        {
            var ret = new ServiceInfo();
            ret.Name = Name;
            ret.Protocol = Protocol;
            ret.Weight = Weight;
            ret.Priority = Priority;
            ret.Port = Port;
            ret.IP4Address = IPAddress.Parse(IP4Address);
            ret.IP6Address = IPAddress.Parse(IP6Address);
            ret.Flags = Data.Where(x => string.IsNullOrEmpty(x.Value)).Select(x => x.Name).ToList();
            ret.Data = Data.Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Name, x => x.Value);
            return ret;
        }

        public class DataItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
