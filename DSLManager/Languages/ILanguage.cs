using DSLManager.Ebnf;
using DSLManager.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    /// <summary>
    /// Rappresents a language, from a ebnf-grammar point of view.
    /// Includes all methods for ebnf grammar manipulation.
    /// </summary>
    /// <typeparam name="T">The type of the output that compiling a phrase of this language should produce.</typeparam>
    public interface ILanguage<T>
    {
        void AddRule(DerivationRule rule);

        void AddRule(string rule);

        void AddRule(DerivationRule rule, SDTranslator<T> t);

        void AddRule(string rule, SDTranslator<T> t);

        void AddRule(DerivationRule rule, RulePriority priority);

        void AddRule(string rule, RulePriority priority);

        void AddRule(string rule, SDTranslator<T> t, RulePriority priority);

        void AddRule(DerivationRule rule, SDTranslator<T> t, RulePriority priority);

        void SetTranslation(DerivationRule rule, SDTranslator<T> t);
		
    }
}
