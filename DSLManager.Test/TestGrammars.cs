using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Test
{
    internal struct TestGrammar
    {
        public string Name;
        public string Rules;
    }


    internal static class TestGrammars
    {
        public static TestGrammar[] GetList()
        {
            return new TestGrammar[]
            {
                new TestGrammar()
                {
                    Name = "Simple Grammar",
                    Rules = string.Join("\n",
                        "A ::= [B], ''end'';",
                        "B ::= ''start'';"
                    )
                },

                new TestGrammar()
                { 
                    Name = "Expression Grammar",
                    Rules = string.Join("\n",
                        "Expr ::= Product | Sum | Division | Difference | #int | Brackets;",
                        "Product ::= Expr, ''*'', Expr;",
                        "Sum ::= Expr, '' + '', Expr;",
                        "Division ::= Expr, '' / '', Expr;",
                        "Difference ::= Expr, '' - '', Expr;",
                        "Brackets ::= ''('', Expr, '')'';"
                    )
                },

                new TestGrammar()
                {
                    Name = "Prefix Nightmare",
                    Rules = string.Join("\n",
                        "Statement ::= Expr1 | Expr2 | Expr3;",
                        "Expr1 ::= [''tokenA''], {''tokenB''}*, ''common'', ''end1'';",
                        "Expr2 ::= ''tokenA'', ''common'', ''end2'';",
                        "Expr3 ::= ''tokenB'', ''common'', ''end3'';"
                    )
                },

                new TestGrammar()
                {
                    Name = "Literal Test",
                    Rules = string.Join("\n",
                        "Statement ::= #int | #real | #string | #name | #id;"
                    )
                }
            };
        }

    }


}
