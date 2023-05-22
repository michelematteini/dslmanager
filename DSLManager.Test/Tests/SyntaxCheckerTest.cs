using DSLManager.Ebnf;
using DSLManager.Ebnf.Filters;
using DSLManager.Parsing;
using System;
using System.Collections.Generic;

namespace DSLManager.Test.Tests
{
    public class SyntaxCheckerTest : ITest
    {
        public string TestName
        {
            get
            {
                return "Syntax Checker Test";
            } 
        }

        public void Run()
        {
            // grammar input
            DerivationRule[] parsedRules = TestUtils.GetGrammarFromConsole();

            DerivationRule[] normalizedRules = new FullNormalizingFilter().ApplyFilter(parsedRules);

            //creazione della tabella lr
            LRDiagram lrDiag = new LRDiagram(normalizedRules);
            lrDiag.InitializeFromRule(normalizedRules[0]);
            lrDiag.BuildDiagram();
            ParseTable ptable = lrDiag.ToParseTable();


            //input del codice
            Console.WriteLine("Inserisci del codice da interpretare:");
            string code = TestUtils.GetCodeFromConsole();

            //syntax checker
            List<int> reductions;

            try
            {
                LRParser.CheckSyntax(ptable, LRParser.Tokenize(code, normalizedRules), normalizedRules, out reductions);
            }
            catch (CompileError)
            {
                //compiler error
                throw;
            }


            //stampa delle riduzioni in ordine
            Console.WriteLine("\nRiduzioni Effettuate:\n");
            foreach (int ri in reductions)
            {
                Console.WriteLine(normalizedRules[ri]);
            }
            Console.WriteLine();
        }
    }
}
