using System;
using System.Collections.Generic;
using DSLManager.Ebnf;
using DSLManager.Languages;
using DSLManager.Parsing;

namespace DSLManager.Test.Tests
{
    public class ExpressionTest : ITest
    {
        public string TestName
        {
            get
            {
                return "Expression language test";
            }
        }

        public void Run()
        {
            ExpressionCompiler exprCompiler = new ExpressionCompiler();
            exprCompiler.Initialize();

            Console.Write("Insert an expression to be parsed: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("e.g. (5 + 11) / 4 + 8 - 1 should output 11");
            Console.ResetColor();
            string expressionCode = Console.ReadLine();
            if(expressionCode != string.Empty)
            {
                exprCompiler.CompileProgram(expressionCode);
                Exception compileErrors = null;
                Console.WriteLine("Result = " + exprCompiler.GetCompiledResults(out compileErrors));
            }
        }
    }

    public class ExpressionCompiler : SinglePassCompiler<int>
    {
        public ExpressionCompiler()
        {
            AddRule("Expr ::= Product | Sum | Division | Difference | #int | Brackets", SDT_Expr);
            AddRule("Product ::= Expr, ''*'', Expr", SDT_Product, RulePriority.ReduceOver(RulePriority.Default));
            AddRule("Sum ::= Expr, ''+'', Expr", SDT_Sum);
            AddRule("Division ::= Expr, ''/'', Expr", SDT_Division, RulePriority.ReduceOver(RulePriority.Default));
            AddRule("Difference ::= Expr, ''-'', Expr", SDT_Difference);
            AddRule("Brackets ::= ''('', Expr, '')''", SDT_Brackets);
        }

        private int SDT_Brackets(ISDTArgs<int> args)
        {
            return args.Values.GetInstanceValue();
        }

        private int SDT_Difference(ISDTArgs<int> args)
        {
            return args.Values["Expr", 0] - args.Values["Expr", 1];
        }

        private int SDT_Division(ISDTArgs<int> args)
        {
            return args.Values["Expr", 0] / args.Values["Expr", 1];
        }

        private int SDT_Sum(ISDTArgs<int> args)
        {
            return args.Values["Expr", 0] + args.Values["Expr", 1];
        }

        private int SDT_Product(ISDTArgs<int> args)
        {
            return args.Values["Expr", 0] * args.Values["Expr", 1];
        }

        private int SDT_Expr(ISDTArgs<int> args)
        {
            if (args.Tokens.Count > 0) return args.Tokens.GetInstanceValue().IntValue;
            return args.Values.GetInstanceValue();
        }
    }

}
