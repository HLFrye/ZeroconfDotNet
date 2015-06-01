using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroconfDotNet.DNS
{
    public class Packet
    {
        public UInt16 TransactionID { get; set; }
        public UInt16 Flags { get; set; }
        public UInt16 Questions { get { return (UInt16)Queries.Count; } }
        public UInt16 AnswerRRs { get { return (UInt16)Answers.Count; } }
        public UInt16 AuthorityRRs { get { return (UInt16)Authority.Count; } }
        public UInt16 AdditionalRRs { get { return (UInt16)Additional.Count; } }
        public IList<Query> Queries { get; set; }
        public IList<Answer> Answers { get; set; }
        public IList<Authority> Authority { get; set; }
        public IList<Additional> Additional { get; set; }

        public bool IsQuery
        {
            get
            {
                return (Flags & 0x80) == 0;
            }
            set
            {
                Flags = (UInt16)((Flags & 0x7f) | (value ? 0x00 : 0x80));
            }

        }

        public void Add(Query query)
        {
            Queries.Add(query);
        }

        public Packet()
        {
            var rnd = new Random();
            TransactionID = (UInt16)rnd.Next();
            Flags = 0;
            Queries = new List<Query>();
            Answers = new List<Answer>();
            Authority = new List<Authority>();
            Additional = new List<Additional>();
        }

        public Packet(UInt16 transactionId, UInt16 flags, Query[] queries, Answer[] answers, Authority[] auth, Additional[] addl)
        {
            TransactionID = transactionId;
            Flags = flags;
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
