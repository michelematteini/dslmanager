using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class StartFilter : AbstractFilter
    {
        private string startSymName, newStartSym;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startSymName">Name of the initial symbol for this grammar.</param>
        /// <param name="newStartSym">Name of the new starting symbol.</param>
        public StartFilter(string startSymName, string newStartSym)
        {
            this.startSymName = startSymName;
            this.newStartSym = newStartSym;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startSymName">Name of the initial symbol for this grammar.</param>
        public StartFilter(string startSymName)
            : this(startSymName, EbnfParser.DEF_INITIAL_SYM)
        { }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            DerivationRule[] sGrammar = new DerivationRule[grammar.Length + 1];
            newRuleIndices = new int[grammar.Length + 1];

            sGrammar[0] = new DerivationRule(new Variable(newStartSym), new Variable(startSymName));
            newRuleIndices[0] = -1;
            for (int i = 0; i < grammar.Length; i++)
            {
                sGrammar[i + 1] = new DerivationRule(grammar[i].Variable, grammar[i].Expression);
                newRuleIndices[i + 1] = i;
            }

            return sGrammar;
        }
    }
}
