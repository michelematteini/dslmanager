using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Ebnf;

namespace DSLManager.Parsing
{
    public class LR1Item
    {
        private const string DOT_SYM = "*";

        private DerivationRule rule;
        private EbnfExpression[] symbols;
        private HashSet<EbnfToken> follows;
        private int curIndex;

        public LR1Item(DerivationRule rule)
            : this(rule, 0)
        {
        }

        private LR1Item(DerivationRule rule, int curIndex)
        {
            this.rule = rule;
            this.symbols = rule.Expression.ToSymbolArray();
            this.curIndex = curIndex;
            this.follows = new HashSet<EbnfToken>();
        }

        public bool CanShift
        {
            get
            {
                return curIndex < symbols.Length;
            }
        }

        public EbnfExpression CurrentSymbol
        {
            get
            {
                if (CanShift)
                {
                    return symbols[curIndex];
                }
                return null;
            }
        }

        public DerivationRule Rule
        {
            get
            {
                return this.rule;
            }
        }

        public LR1Item Shift()
        {
            if (CanShift)
            {
                LR1Item shifted = new LR1Item(rule, curIndex + 1);
                shifted.AddFollowsFrom(this);
                return shifted;
            }
            throw new InvalidOperationException("This item cannot be shifted anymore.");
        }

        public bool IsReducibleWith(EbnfToken t)
        {
            return !CanShift && CanBeFollowedBy(t);
        }

        public bool AddFollow(EbnfToken t)
        {
            return follows.Add(t);
        }

        public EbnfToken[] Follows
        {
            get
            {
                return follows.ToArray();
            }
        }

        public bool AddFollowsFrom(LR1Item item)
        {
            return AddFollowsFrom(item.follows);
        }

        public bool AddFollowsFrom(HashSet<EbnfToken> follows)
        {
            int followCount = this.follows.Count;
            this.follows.UnionWith(follows);
            return followCount < this.follows.Count;
        }

        public bool CanBeFollowedBy(EbnfToken t)
        {
            foreach (EbnfToken ti in follows)
            {
                if (ti == t)
                {
                    return true;
                }
            }
            return false;
        }

        public string ToEbnfNoFollows()
        {
            string itemStr = rule.Expression.ToEbnf();
           
            if (curIndex == symbols.Length)
            //the last position means that u can reduce, dot placed ad the end of the expression.
            {
                itemStr += DOT_SYM;
            }
            else
            //search the position of the current symbol and place a dot before it.
            {
                string newSymbol;
                int dotIndex = 0, lastSymbolLen = 0;

                for (int i = 0; i <= curIndex; i++)
                {
                    newSymbol = symbols[i].ToEbnf();
                    dotIndex = itemStr.IndexOf(newSymbol, dotIndex + lastSymbolLen);
                    lastSymbolLen = newSymbol.Length;
                }

                itemStr = itemStr.Insert(dotIndex, DOT_SYM);
            }
          
            return String.Format("{0} {1} {2};", rule.Variable.Name, EbnfParser.OP_PRODUCE, itemStr); ;
        }

        public string ToEbnf()
        {
            string itemStr = this.ToEbnfNoFollows();
            itemStr += " {";
            if (follows.Count > 0)
            {
                EbnfToken[] followStr = follows.ToArray();
                itemStr += followStr[0];
                for (int i = 1; i < followStr.Length; i++)
                {
                    itemStr += ", " + followStr[i].ToEbnf();
                }
            }
            itemStr += "}";
            return itemStr;
        }

        public override string ToString()
        {
            return this.ToEbnf();
        }

    }
}
