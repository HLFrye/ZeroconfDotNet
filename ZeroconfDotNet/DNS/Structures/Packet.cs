using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscoveryDotNet.DNS.Structures;

namespace DiscoveryDotNet.DNS
{
    public class Flags
    {
        public bool IsResponse;
        private uint _opCode;
        public uint OpCode 
        {
            get
            {
                return _opCode;
            }
            set
            {
                if (value > 0x0F)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _opCode = value;
            }
        }
        public bool IsAuthoritative;
        public bool IsTruncated;
        public bool IsRecursionDesired;
        public bool IsRecursionAvailable;
        public bool IsAuthenticated;
        private uint _replyCode;
        public uint ReplyCode 
        {
            get { return _replyCode; }
            set
            {
                if (value > 0x0F)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _replyCode = value;
            }
        }
        public bool AcceptUnauthenticatedData;

        internal UInt16 StoreFlags()
        {
            UInt16 output = 0;
            output |= (UInt16) (IsResponse ? 0x8000 : 0);
            output |= (UInt16) (OpCode << 12);
            output |= (UInt16)(IsAuthoritative ? 0x0400 : 0);
            output |= (UInt16)(IsTruncated ? 0x0200 : 0);
            output |= (UInt16)(IsRecursionDesired ? 0x0100 : 0);
            output |= (UInt16)(IsRecursionAvailable ? 0x0080 : 0);
            output |= (UInt16)(IsAuthenticated ? 0x0020 : 0);
            output |= (UInt16)(AcceptUnauthenticatedData ? 0x0010 : 0);
            output |= (UInt16)ReplyCode;
            return output;
        }

        internal void ReadFlags(UInt16 rawFlags)
        {
            IsResponse = (rawFlags & 0x8000) != 0;
            OpCode = (uint)(rawFlags & 0x7800) >> 12;
            IsAuthoritative = (rawFlags & 0x0400) != 0;
            IsTruncated = (rawFlags & 0x0200) != 0;
            IsRecursionDesired = (rawFlags & 0x0100) != 0;
            IsRecursionAvailable = (rawFlags & 0x0080) != 0;
            IsAuthenticated = (rawFlags & 0x0020) != 0;
            AcceptUnauthenticatedData = (rawFlags & 0x0010) != 0;
            ReplyCode = (uint)rawFlags & 0x000F;
        }
    }


    public class Packet
    {
        public UInt16 TransactionID { get; set; }
        private readonly Flags _flags = new Flags();
        public Flags Flags { get { return _flags; } }
        public UInt16 Questions { get { return (UInt16)Queries.Count; } }
        public UInt16 AnswerRRs { get { return (UInt16)Answers.Count; } }
        public UInt16 AuthorityRRs { get { return (UInt16)Authority.Count; } }
        public UInt16 AdditionalRRs { get { return (UInt16)Additional.Count; } }
        public IList<Query> Queries { get; set; }
        public IList<Answer> Answers { get; set; }
        public IList<Authority> Authority { get; set; }
        public IList<Additional> Additional { get; set; }

        public void Add(Query query)
        {
            Queries.Add(query);
        }

        public Packet()
        {
            var rnd = new Random();
            TransactionID = (UInt16)rnd.Next();
            Queries = new List<Query>();
            Answers = new List<Answer>();
            Authority = new List<Authority>();
            Additional = new List<Additional>();
        }

        public Packet(Header header, Query[] queries, Answer[] answers, Authority[] auth, Additional[] addl)
        {
            TransactionID = header.TransactionID;
            Flags.ReadFlags(header.Flags);
            Queries = new List<Query>(queries);
            Answers = new List<Answer>(answers);
            Authority = new List<Authority>(auth);
            Additional = new List<Additional>(addl);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Packet, Transaction ID {0}, Flags 0x{1:X}", TransactionID, Flags);
            sb.AppendLine();
            sb.AppendFormat("Questions: {0}", Questions);
            sb.AppendLine();
            foreach (var i in Queries)
            {
                sb.Append(i);
                sb.AppendLine();
            }
            sb.AppendFormat("Answers: {0}", AnswerRRs);
            sb.AppendLine();
            foreach (var i in Answers)
            {
                sb.Append(i);
                sb.AppendLine();
            }
            sb.AppendFormat("Authority: {0}", AuthorityRRs);
            sb.AppendLine();
            foreach (var i in Authority)
            {
                sb.Append(i);
                sb.AppendLine();
            }
            sb.AppendFormat("Additional: {0}", AdditionalRRs);
            sb.AppendLine();
            foreach (var i in Additional)
            {
                sb.Append(i);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
