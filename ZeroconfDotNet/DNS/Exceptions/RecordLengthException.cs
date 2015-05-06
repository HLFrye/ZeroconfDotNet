using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS.Exceptions
{
    class RecordLengthException: Exception
    {
        public int ExpectedByteCount { get; set; }
        public int ActualByteCount { get; set; }

        public RecordLengthException(int expected, int actual)
            :base(string.Format("Record length exception.  Expected : {0}; Read {1}", expected, actual))
        {
            ExpectedByteCount = expected;
            ActualByteCount = actual;
        }
    }
}
