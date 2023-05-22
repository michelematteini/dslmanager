using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class UnitPFilter : AbstractFilter
    {
        public UnitPFilter() { }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            List<DerivationRule> normGrammar = new List<DerivationRule>(grammar);
            List<int> ruleIndices = new List<int>();
            for (int i = 0; i < grammar.Length; i++) { ruleIndices.Add(i); }
            bool unitRuleRemoved = true;

            while (unitRuleRemoved)
            {
                unitRuleRemoved = false;
                for (int i = 0; i < normGrammar.Count; i++)
                //search grammar for unit rules A -> B
                {
                    if (normGrammar[i].Expression.ExprType == EbnfExprType.Variable)
                    //unit rule?
                    {
						//could be an unit rule, but it's explicitly declared by user.
						if(!IsGenerated((Variable)normGrammar[i].Expression)) continue;

                        //search for multiple definitions
                        int defCount = 0;
                        for (int j = 0; j < normGrammar.Count; j++)
                        {
                            if (normGrammar[i].Variable == normGrammar[j].Variable)
                                defCount++;//definition found
                        }

                        if (defCount > 1) continue;//multiple definitions, not a unit rule.


                        //vvvvvvvvvvvv- Unit rule removal -vvvvvvvvvvvvvv
                        int curRuleCount = normGrammar.Count;
                        Variable unitRuleVar = (Variable)normGrammar[i].Expression;//B

                        for (int j = 0; j < curRuleCount; j++)
                        //search for all rules B -> C in grammar, and add a rule A -> C
                        {
                            if (unitRuleVar.Name == normGrammar[j].Variable.Name)
                            {
                                normGrammar.Add(new DerivationRule(normGrammar[i].Variable, normGrammar[j].Expression));
                                //alle nuove regole aggiunte, verrà mantenuto l'indice della produzione unità
                                ruleIndices.Add(ruleIndices[/*j*/i]);
                            }
                        }

                        normGrammar.RemoveAt(i);//remove unit rule
                        ruleIndices.RemoveAt(i);
                        unitRuleRemoved = true;
                        break;
                    }
                }
            }

            newRuleIndices = ruleIndices.ToArray();
            return normGrammar.ToArray();
        }

    }
}
