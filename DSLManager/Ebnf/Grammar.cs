using System;
using System.Collections.Generic;

namespace DSLManager.Ebnf
{
    public class Grammar
    {
        private Dictionary<EbnfExpression, List<DerivationRule>> rulesForVariable;
        private List<DerivationRule> emptyResult;

        public DerivationRule[] Rules { get; private set; }


        public Grammar(DerivationRule[] rules)
        {
            Rules = rules;
            emptyResult = new List<DerivationRule>();

            // fill rules by variable index
            rulesForVariable = new Dictionary<EbnfExpression, List<DerivationRule>>();
            foreach (DerivationRule r in rules)
            {
                List<DerivationRule> rulesForVar;
                if(!rulesForVariable.TryGetValue(r.Variable, out rulesForVar))
                {
                    rulesForVar = new List<DerivationRule>();
                    rulesForVariable.Add(r.Variable, rulesForVar);
                }

                rulesForVar.Add(r);
            }
        }

        public IReadOnlyList<DerivationRule> GetRulesStartingWith(EbnfExpression v)
        {
            List<DerivationRule> rulesForVar;
            if (!rulesForVariable.TryGetValue(v, out rulesForVar))
            {
                return emptyResult;
            }
            return rulesForVar;
        }

    }
}
