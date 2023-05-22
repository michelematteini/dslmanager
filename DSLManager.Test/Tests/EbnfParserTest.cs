using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Ebnf;
using DSLManager.Ebnf.Filters;
using DSLManager.Parsing;

namespace DSLManager.Test.Tests
{
    public class EbnfParserTest : ITest
    {
        public string TestName
        {
            get
            {
                return "EBNF Parser Test";
            }
        }

        public void Run()
        {
            // grammar input
            DerivationRule[] parsedRules = TestUtils.GetGrammarFromConsole();
            string startSimName = parsedRules[0].Variable.Name;

            //interpretazione
            Console.WriteLine("Interpretazione della grammatica:");
            foreach (DerivationRule rule in parsedRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro EBNF -> BNF
            Console.WriteLine("Semplificazione a BNF:");
            IGrammarFilter bnfFilter = new EbnfToBnfFilter();
            DerivationRule[] filteredRules = bnfFilter.ApplyFilter(parsedRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro Epsilon
            Console.WriteLine("Rimozione produzioni epsilon:");
            IGrammarFilter epsilonFilter = new EpsilonFilter();
            filteredRules = epsilonFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro prod. unita'
            Console.WriteLine("Rimozione produzioni unita':");
            IGrammarFilter unitRuleFilter = new UnitPFilter();
            filteredRules = unitRuleFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            // filtro prod. singolo token
            Console.WriteLine("Rimozione produzioni token:");
            IGrammarFilter tokenRuleFilter = new TokenPFilter();
            filteredRules = tokenRuleFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro start
            Console.WriteLine("Filtro Start:");
            IGrammarFilter startFilter = new StartFilter(startSimName);
            filteredRules = startFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro simboli generativi
            Console.WriteLine("Simboli generativi:");
            IGrammarFilter generativeSymFilter = new GenerativeSymFilter();
            filteredRules = generativeSymFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();

            //filtro simboli raggiungibili
            Console.WriteLine("Simboli raggiungibili:");
            IGrammarFilter reachableSymFilter = new ReachableSymFilter();
            filteredRules = reachableSymFilter.ApplyFilter(filteredRules);
            foreach (DerivationRule rule in filteredRules)
            {
                Console.WriteLine(rule.ToEbnf());
            }
            Console.WriteLine();
        }
    }
}
