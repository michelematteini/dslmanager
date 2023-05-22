using DSLManager.Ebnf;
using System;
using System.IO;

namespace DSLManager.Test
{
    public static class TestUtils
    {
        /// <summary>
        /// Read a grammar or a grammar file from console
        /// </summary>
        /// <returns></returns>
        public static DerivationRule[] GetGrammarFromConsole()
        {
            string grammarStr = string.Empty;

            if(Ask("A grammar is needed, do you want to manually insert one?"))
            { 
                Console.WriteLine("Insert grammar rules, one for each line, terminated with a semi-color or the path of a grammar file:");
                string ruleStr;
                do
                {
                    ruleStr = Console.ReadLine();
                    grammarStr += "\n" + ruleStr;
                } while (ruleStr != string.Empty);
            }
            else
            {
                Console.WriteLine("Choose a grammar:");
                TestGrammar[] grammars = TestGrammars.GetList();
                for (int i = 0; i < grammars.Length; i++)
                {
                    Console.WriteLine($"{i}- {grammars[i].Name}");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(grammars[i].Rules);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                int grammarID = int.Parse(Console.ReadLine());
                grammarStr = grammars[grammarID].Rules;
            }

            return EbnfParser.ParseGrammar(grammarStr);
        }

        public static string GetCodeFromConsole()
        {
            Console.WriteLine("Write source code line by line, or the path of a source code file:");
            string codeLine, code = string.Empty;
            do
            {
                codeLine = Console.ReadLine();
                if (File.Exists(codeLine))
                {
                    code = File.ReadAllText(codeLine);
                    break;
                }
                code += codeLine;
            } while (codeLine != string.Empty);
            return code;
        }

        public static bool Ask(string question)
        {
            Console.WriteLine(question + " (Y/N)");
            string answer = Console.ReadLine();
            return answer == "y" || answer == "Y" || answer.ToLower().Trim() == "yes";
        }

    }
}
