using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public interface IGrammarFilter
    {
        DerivationRule[] ApplyFilter(DerivationRule[] grammar);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grammar"></param>
        /// <param name="newRuleIndices">
        /// Array of indices that indicates the indices of the given rules in the previous grammar.
        /// A newly created rule will have the index set to -1.
        /// </param>
        /// <returns></returns>
        DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices);
    }
}
