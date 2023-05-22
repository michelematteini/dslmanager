using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Ebnf;
using DSLManager.Parsing;

namespace DSLManager.Languages
{
    public abstract class Language<T> : ILanguage<T>
    {
        protected class LanguageRule
        {
            public DerivationRule Derivation { get; set; }

            public SDTranslator<T> Translator { get; set; }

            public RulePriority Priority { get; set; }
        }

        protected List<LanguageRule> Rules { get; private set; }

        public List<DerivationRule> DerivationRules
        {
            get
            {
                List<DerivationRule> derivations = new List<DerivationRule>();
                foreach (LanguageRule r in Rules)
                    derivations.Add(r.Derivation);
                return derivations;
            }
        }

        public List<SDTranslator<T>> Translators
        {
            get
            {
                List<SDTranslator<T>> translators = new List<SDTranslator<T>>();
                foreach (LanguageRule r in Rules)
                    translators.Add(r.Translator);
                return translators;
            }
        }

        public Language()
        {
            Rules = new List<LanguageRule>();
        }

        public void AddRule(DerivationRule rule)
        {
            AddRule(rule, null, RulePriority.Default);
        }

        public void AddRule(string rule)
        {
            AddRule(DerivationRule.FromString(rule), null, RulePriority.Default);
        }

        public void AddRule(DerivationRule rule, SDTranslator<T> t)
        {
            AddRule(rule, t, RulePriority.Default);
        }

        public void AddRule(string rule, SDTranslator<T> t)
        {
            AddRule(DerivationRule.FromString(rule), t, RulePriority.Default);
        }

        public void AddRule(DerivationRule rule, RulePriority priority)
        {
            AddRule(rule, null, priority);
        }

        public void AddRule(string rule, RulePriority priority)
        {
            AddRule(DerivationRule.FromString(rule), null, priority);
        }

        public void AddRule(string rule, SDTranslator<T> t, RulePriority priority)
        {
            AddRule(DerivationRule.FromString(rule), t, priority);
        }

        public void AddRule(DerivationRule rule, SDTranslator<T> t, RulePriority priority)
        {
            LanguageRule r = new LanguageRule();
            r.Derivation = rule;
            r.Translator = t;
            r.Priority = priority;
            Rules.Add(r);
            OnRuleModified(r);
        }

        public void SetTranslation(DerivationRule rule, SDTranslator<T> t)
        {
            int ruleIndex = DerivationRules.IndexOf(rule);

            if (ruleIndex < 0)
                throw new InvalidOperationException("You can only set a translation for an already existing rule.");

            Rules[ruleIndex].Translator = t;
            OnRuleModified(Rules[ruleIndex]);
        }

        protected abstract void OnRuleModified(LanguageRule rule);

    }
}
