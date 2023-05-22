using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Ebnf;
using DSLManager.Ebnf.Filters;
using DSLManager.Parsing;

namespace DSLManager.Test.Tests
{
    public class LRDiagTest : ITest
    {
        public string TestName
        {
            get
            {
                return "LR Diagram Test";
            }
        }

        public void Run()
        {
            //input della grammatica
            DerivationRule[] parsedRules = TestUtils.GetGrammarFromConsole();
                
            //normalizzazione della grammatica
            DerivationRule[] normalizedRules = new FullNormalizingFilter().ApplyFilter(parsedRules);
            if (normalizedRules.Length == 0)
            {
                Console.WriteLine("This grammar is non-generative!");
                return;
            }

            //creazione del diagramma lr
            LRDiagram lrDiag = new LRDiagram(normalizedRules);
            lrDiag.InitializeFromRule(normalizedRules[0]);
            lrDiag.BuildDiagram();

            //stampa del diagramma
            if (TestUtils.Ask("Do you want to write the diagram to file?"))
            {
                Console.WriteLine("Write the ouput file path:");
                string filePath = Console.ReadLine();
                File.WriteAllText(filePath, lrDiag.ToFullString());
            }
            else
            {
                Console.WriteLine("Diagramma LR:");
                Console.WriteLine();
                Console.WriteLine(lrDiag.ToFullString());
                Console.WriteLine();
            }

            // force conflicts printout
            Console.ForegroundColor = ConsoleColor.DarkRed;
            DSLDebug.Output = (msg, msgType) => Console.WriteLine(msg);
            lrDiag.ToParseTable();
            Console.ResetColor();
            DSLDebug.Output = null;
        }
    }
}
