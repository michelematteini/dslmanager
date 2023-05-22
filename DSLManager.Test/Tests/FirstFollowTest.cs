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
    public class FirstFollowTest : ITest
    {
        public string TestName
        {
            get
            {
                return "First follow test";
            }
        }

        public void Run()
        {
            // grammar input
            DerivationRule[] parsedRules = TestUtils.GetGrammarFromConsole();

            //normalizzazione della grammatica
            DerivationRule[] normalizedRules = new FullNormalizingFilter().ApplyFilter(parsedRules);

            //calcolo e stampa dei first sets
            Dictionary<EbnfExpression, HashSet<EbnfToken>> firstSets = LRParser.ComputeFirstSets(normalizedRules);
            Console.WriteLine();
            Console.WriteLine("FIRST SETS:");
            foreach (KeyValuePair<EbnfExpression, HashSet<EbnfToken>> fSet in firstSets)
            {
                EbnfToken[] firsts = fSet.Value.ToArray();
                string fsRow = fSet.Key + ": {" + firsts[0];
                for (int i = 1; i < firsts.Length; i++)
                {
                    fsRow += ", " + firsts[i];
                }
                fsRow += "}";
                Console.WriteLine(fsRow);
            }

            //calcolo e stampa dei follow sets
            Dictionary<string, HashSet<EbnfToken>> followSets = LRParser.ComputeFollowSets(normalizedRules, firstSets);
            Console.WriteLine();
            Console.WriteLine("FOLLOW SETS:");
            foreach (KeyValuePair<string, HashSet<EbnfToken>> fSet in followSets)
            {
                EbnfToken[] follow = fSet.Value.ToArray();
                string fsRow = fSet.Key + ": {" + follow[0];
                for (int i = 1; i < follow.Length; i++)
                {
                    fsRow += ", " + follow[i];
                }
                fsRow += "}";
                Console.WriteLine(fsRow);
            }
        }
    }
}
