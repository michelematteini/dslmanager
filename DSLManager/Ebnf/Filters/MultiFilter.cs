using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class MultiFilter : AbstractFilter
    {
        private List<IGrammarFilter> filters;

        public MultiFilter(params IGrammarFilter[] filters)
        {
            this.filters = new List<IGrammarFilter>();
            this.filters.AddRange(filters);
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            //initialize identity indices
            newRuleIndices = new int[grammar.Length];
            for (int i = 0; i < grammar.Length; i++) { newRuleIndices[i] = i; }
            int[] localRuleIndices = null, curRuleIndices = newRuleIndices;

            //apply each filter
            foreach (IGrammarFilter f in filters)
            {
                //apply filter
                grammar = f.ApplyFilter(grammar, out localRuleIndices);
                
                //remap indices with the previous step
                newRuleIndices = new int[grammar.Length];
                for (int i = 0; i < grammar.Length; i++)
                {
                    int localIndex = localRuleIndices[i];
                    newRuleIndices[i] = localIndex < 0 ? localIndex : curRuleIndices[localRuleIndices[i]];
                }
                curRuleIndices = newRuleIndices;
            }

            return grammar;
        }


    }
}
