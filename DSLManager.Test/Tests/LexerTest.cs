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
    public class LexerTest : ITest
    {
        public string TestName
        {
            get
            {
                return "Lexer test";
            }
        }

        public void Run()
        {
            // grammar input
            DerivationRule[] parsedRules = TestUtils.GetGrammarFromConsole();

            //normalizzazione della grammatica
            DerivationRule[] normalizedRules = new FullNormalizingFilter().ApplyFilter(parsedRules);

            //input del codice
            Console.WriteLine("Inserisci del codice da dividere in tokens:");
            string code = TestUtils.GetCodeFromConsole();

            //calcolo e stampa dei token
            EbnfToken[] tokens = LRParser.Tokenize(code, normalizedRules);
            Console.WriteLine();
            Console.WriteLine("TOKENS:");
            foreach (EbnfToken t in tokens)
            {
                Console.WriteLine(t.ToTokenString());
            }
            Console.WriteLine();
        }
    }
}
