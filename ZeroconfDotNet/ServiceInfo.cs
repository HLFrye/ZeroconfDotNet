using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ZeroconfDotNet
{
    public class ServiceInfo: IEqualityComparer<ServiceInfo>
    {
        public ServiceInfo()
        {
            Flags = new List<string>();
            Data = new Dictionary<string, string>();
        }

        public IPAddress IP4Address { get; set; }
        public IPAddress IP6Address { get; set; }

        public UInt16 Port
        { get; set;}

        public string Name { get; set; }
        public string Protocol { get; set; }
        private int? _textVers = 1;
        public int? TextVers
        {
            get { return _textVers; }
            set
            {
                if (_textVers != value)
                {
                    _textVers = value;
                    _lastTxt = null;
                }
            }
        }

        public IDictionary<string, string> Data { get; set; }
        public IList<string> Flags { get; set; }
        public int Priority { get; set; }
        public int Weight { get; set; }

        private Encoding _encoding = Encoding.UTF8;
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                _encoding = value;
                _lastTxt = null;
            }
        }

        private byte[] _lastTxt;

        /// <summary>
        /// Converts the TextVars flag, the Data dictionary, and the Flags list
        /// into a TXT record.
        /// From RFC 6763:
        /// ---
        /// 6.6.  Example TXT Record
        /// The TXT record below contains three syntactically valid key/value
        /// strings.  (The meaning of these key/value pairs, if any, would depend
        /// on the definitions pertaining to the service in question that is
        /// using them.)
        ///     -------------------------------------------------------
        ///     | 0x09 | key=value | 0x08 | paper=A4 | 0x07 | passreq |
        ///     -------------------------------------------------------
        /// ---
        /// In this case, the Data dictionary would correspond to key=value pairs,
        /// while the Flags list would correspond with bare flags.  The special TextVers
        /// flag corresponds to a special textvars=x line, with a default of 1.  This should
        /// only be changed in the event of breaking changes; client applications should
        /// accept only the highest value they know of.
        /// </summary>
        /// <returns>A byte array representing this ServiceInfo</returns>
        public byte[] ToTxtRecord()
        {
            if (_lastTxt != null)
            {
                return _lastTxt;
            }

            var lines = Data.Select(x => string.Join("=", x.Key, x.Value))
                            .Concat(Flags);
            if (TextVers.HasValue)
            {
                lines = lines.Concat(new[] { "textvers=" + TextVers.Value });
            }

            var bytes = lines.Select(x => Encoding.GetBytes(x));
            var lengths = bytes.Select(x => (byte)x.Length);
            _lastTxt = lengths.Zip(bytes, (x, y) =>
            {
                var result = new byte[y.Length + 1];
                result[0] = x;
                Array.Copy(y, 0, result, 1, y.Length);
                return result;
            }).SelectMany(x => x).ToArray();
            return _lastTxt;
        }

        public static byte[] Serialize(ServiceInfo info)
        {
            throw new NotImplementedException();
        }

        public static ServiceInfo Deserialize(byte[] input)
        {
            throw new NotImplementedException();
        }

        public bool Equals(ServiceInfo x, ServiceInfo y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(ServiceInfo obj)
        {
            throw new NotImplementedException();
        }
    }
}
