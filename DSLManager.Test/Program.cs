using System;
using System.Collections.Generic;
using System.Linq;
using DSLManager.Ebnf;
using DSLManager.Ebnf.Filters;
using DSLManager.Parsing;
using DSLManager.Test.Tests;

namespace DSLManager.Test
{
    class Program
    {
        private static List<ITest> tests;

        private static void Main(string[] args)
        {
            initTests();

            Console.WriteLine("============================================");
            Console.WriteLine("DSLManager tests.");

            int choiceNum;
            bool quitApplication = false;

            while (!quitApplication)
            {
                Console.WriteLine("============================================");
                Console.WriteLine();
                for (int i = 0; i < tests.Count; i++)
                {
                    Console.WriteLine(string.Format("{0}- {1}", i, tests[i].TestName));
                }

                Console.WriteLine();
                Console.WriteLine("Choose a test: ");

                choiceNum = -1;
                bool isValidChoice = false;
                while (choiceNum < 0 || !isValidChoice)
                {
                    string choice = Console.ReadLine();
                    isValidChoice = int.TryParse(choice, out choiceNum);
                    if (!isValidChoice) Console.WriteLine("Insert only the number of the choosen test:");
                    if (choiceNum < 0) Console.WriteLine("Negative numbers are invalid, choose a valid test:");
                }
                Console.WriteLine();
                try
                {
                    if (choiceNum >= tests.Count)
                    {
                        Console.WriteLine(String.Format("Test #{0} not available.", choiceNum));
                    }
                    else
                    {
                        tests[choiceNum].Run();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                    Console.WriteLine();
                }
            }

        }

        private static void initTests()
        {
            tests = new List<ITest>();
            tests.Add(new EbnfParserTest());
            tests.Add(new FirstFollowTest());
            tests.Add(new LexerTest());
            tests.Add(new LRDiagTest());
            tests.Add(new SyntaxCheckerTest());
            tests.Add(new ExpressionTest());
            // TODO: add more tests here
        }


    }
}
