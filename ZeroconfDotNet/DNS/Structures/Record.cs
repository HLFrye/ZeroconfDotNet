using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS
{
    public class Record
    {
        UInt16 _rawClass;
        public Record(string name, int recordType, int cls)
        {
            Name = name;
            RecordType = (UInt16)recordType;
            _rawClass = (UInt16)cls;
        }

        public Record()
        {
        }

        public void SetRawClass(UInt16 val)
        {
            _rawClass = val;
        }

        public UInt16 GetRawClass()
        {
            return _rawClass;
        }

        public UInt16 RecordType { get; set; }
        public UInt16 Class {
            get
            {
                return (UInt16)(_rawClass & 0x7fff);
            }
            set
            {
                _rawClass = (UInt16)((value & 0x7fff) | (_rawClass & 0x8000));
            }
        }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("Record Type {0}, Class {1}, Name {2}", RecordType, Class, Name);
        }
    }
}
