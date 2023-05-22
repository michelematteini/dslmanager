using DSLManager.Ebnf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSLManager.Parsing
{
    public class ParseTable
    {
        private Dictionary<ulong, LRAction> ptable;
        private Dictionary<int, List<EbnfExpression>> expected;
        private LRAction nop;

        public ParseTable()
        {
            ptable = new Dictionary<ulong, LRAction>();
            expected = new Dictionary<int, List<EbnfExpression>>();
            nop = new LRAction(LRActionType.Nop, 0);
        }

        public LRAction this[int state, EbnfExpression next]
        {
            get
            {
                ulong hashKey = hash(state, next);
                if (ptable.ContainsKey(hashKey))
                {
                    return ptable[hash(state, next)];
                }
                else
                {
                    return nop;
                }
            }
            set
            {
                ptable[hash(state, next)] = value;
                //save expected tokens
                if (next is EbnfToken)
                {
                    if (!expected.ContainsKey(state))
                    {
                        expected[state] = new List<EbnfExpression>();
                    }
                    expected[state].Add(next);
                }
            }
        }

        private ulong hash(int state, EbnfExpression next)
        {
            return (((ulong)next.GetHashCode()) << 0x20) + (ulong)state;
        }

        public List<EbnfExpression> ExpectedTokens(int state)
        {
            return expected[state];
        }

    }


}
