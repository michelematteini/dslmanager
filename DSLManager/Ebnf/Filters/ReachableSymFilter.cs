using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class ReachableSymFilter : AbstractFilter
    {
        public ReachableSymFilter()
        {
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            //initialize reachable symbol set
            HashSet<string> reasym = new HashSet<string>();
            reasym.Add(EbnfParser.DEF_INITIAL_SYM);

            //iterative computing of reachable symbols
            bool setChanged = true;
            while (setChanged)
            {
                setChanged = false;

                foreach (DerivationRule r in grammar)
                {
                    if (!reasym.Contains(r.Variable.ToEbnf())) continue;

                    EbnfExpression[] esym = r.Expression.ToSymbolArray();
                    foreach (EbnfExpression e in esym)
                    {
                        setChanged = setChanged || reasym.Add(e.ToEbnf());
                    }
                }
            }

            //clean grammar from non-reachable rules
            List<int> ruleIndices = new List<int>();
            List<DerivationRule> cleanGrammar = new List<DerivationRule>();
            for (int i = 0; i < grammar.Length; i++)
            {
                DerivationRule r = grammar[i];
                if (reasym.Contains(r.Variable.ToEbnf()))
                {
                    cleanGrammar.Add(r);
                    ruleIndices.Add(i);
                }
            }
            newRuleIndices = ruleIndices.ToArray();
            return cleanGrammar.ToArray();
        }
    }
}
