using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class GenerativeSymFilter : AbstractFilter
    {
        public GenerativeSymFilter()
        {
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            HashSet<string> gensym = new HashSet<string>();

            //add all terminals
            foreach (DerivationRule r in grammar)
            {
                EbnfExpression[] esym = r.Expression.ToSymbolArray();

                foreach (EbnfExpression sym in esym)
                {
                    if (sym is EbnfToken)
                    {
                        gensym.Add(sym.ToEbnf());
                    }
                }
            }

            //iterative computing of generative symbols
            bool setChanged = true, generativeExpr;
            while (setChanged)
            {
                setChanged = false;

                foreach (DerivationRule r in grammar)
                {
                    EbnfExpression[] esym = r.Expression.ToSymbolArray();
                    generativeExpr = true;
                    foreach (EbnfExpression sym in esym)
                    {
                        if (!gensym.Contains(sym.ToEbnf()))
                        {
                            generativeExpr = false;
                            break;
                        }
                    }

                    if (generativeExpr)
                    {
                        setChanged = setChanged || gensym.Add(r.Variable.ToEbnf());                   
                    }
                }
            }

            //clean grammar from non-generative rules
            List<int> ruleIndices = new List<int>();
            List<DerivationRule> cleanGrammar = new List<DerivationRule>();
            for (int i = 0; i < grammar.Length; i++)
            {
                DerivationRule r = grammar[i];
                if (gensym.Contains(r.Variable.ToEbnf()))
                //the variable have a generative production... (each variable can have many alternative productions)
                {
                    //check if this production is a generative one (each symbol produced is generative)
                    bool isGenerativeRule = true;
                    EbnfExpression[] ruleSymbols = r.Expression.ToSymbolArray();
                    for (int sym = 0; sym < ruleSymbols.Length && isGenerativeRule; sym++)
                    {
                        isGenerativeRule = isGenerativeRule && gensym.Contains(ruleSymbols[sym].ToEbnf());
                    }

                    if (isGenerativeRule)
                    //if its generative, add to the grammar
                    {
                        cleanGrammar.Add(r);
                        ruleIndices.Add(i);
                    }
                }
            }

            newRuleIndices = ruleIndices.ToArray();
            return cleanGrammar.ToArray();
        }

    }
}
