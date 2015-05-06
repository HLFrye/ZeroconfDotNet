using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS.Records
{
    public class AnswerData
    {
        private byte[] data;

        public virtual void Read(PacketReader reader, int size)
        {
            data = reader.ReadBytes(size);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("Unknown record type: {0} bytes", data.Length));
            sb.AppendLine();
            for (int i = 0; i < data.Length; ++i)
            {
                if (i % 16 == 0)
                {
                    sb.AppendLine();
                }
                else if (i % 8 == 0)
                {
                    sb.Append(" ");
                }
                sb.Append(data[i].ToString("X:2"));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public virtual void Write(PacketWriter writer)
        {
            throw new NotImplementedException();
        }

    }
}
