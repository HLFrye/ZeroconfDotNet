
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryDotNet.DNS.Exceptions
{
    class UnrecognizedRecordTypeException : Exception
    {
        public int RecordType {get; set;}
        public UnrecognizedRecordTypeException(int recordType)
            : base(string.Format("Unrecognized record type {0}", recordType))
        {
            RecordType = recordType;
        }
    }
}
