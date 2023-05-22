using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class FullNormalizingFilter : AbstractFilter
    {
        private IGrammarFilter bnfFilter, epsilonFilter, unitRuleFilter, tokenRuleFilter, startFilter, gensymFilter, reasymFilter;

        public FullNormalizingFilter()
        {
            bnfFilter = new EbnfToBnfFilter();
            epsilonFilter = new EpsilonFilter();
            unitRuleFilter = new UnitPFilter();
            tokenRuleFilter = new TokenPFilter();
            gensymFilter = new GenerativeSymFilter();
            reasymFilter = new ReachableSymFilter();
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            startFilter = new StartFilter(grammar[0].Variable.Name);
            return new MultiFilter(bnfFilter, epsilonFilter, unitRuleFilter, tokenRuleFilter, startFilter, gensymFilter, reasymFilter).ApplyFilter(grammar, out newRuleIndices);
        }
    }
}
