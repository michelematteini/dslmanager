using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class TokenPFilter : AbstractFilter, IEbnfExprVisitor
    {
        private Stack<EbnfExpression> curExpr;
        private Variable curFindVar;
        private EbnfToken curReplaceToken;

        public TokenPFilter() 
        {
            curExpr = new Stack<EbnfExpression>();
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            List<DerivationRule> normGrammar = new List<DerivationRule>(grammar);
            List<int> ruleIndices = new List<int>();
            for (int i = 0; i < grammar.Length; i++) { ruleIndices.Add(i); }
            bool unitRuleRemoved = true;

            while (unitRuleRemoved)
            {
                unitRuleRemoved = false;
                for (int i = 0; i < normGrammar.Count; i++)
                //search grammar for token rules A -> token
                {
                    if (normGrammar[i].Expression.ExprType == EbnfExprType.Token)
                    // token rule?
                    {
                        //could be an token rule, but it's explicitly declared by user.
                        if (!IsGenerated(normGrammar[i].Variable)) continue;

                        //search for multiple definitions
                        int defCount = 0;
                        for (int j = 0; j < normGrammar.Count; j++)
                        {
                            if (normGrammar[i].Variable == normGrammar[j].Variable)
                                defCount++;//definition found
                        }

                        if (defCount > 1) continue;//multiple definitions, not a unit token rule.


                        //vvvvvvvvvvvv- token rule removal -vvvvvvvvvvvvvv
                        curFindVar = normGrammar[i].Variable;
                        curReplaceToken = normGrammar[i].Expression as EbnfToken;

                        for (int j = 0; j < normGrammar.Count; j++)
                        // visit all rules in grammar, and replace A with the token
                        {
                            
                            normGrammar[j].Expression.Accept(this);
                            normGrammar[j] = new DerivationRule(normGrammar[j].Variable, curExpr.Pop());
                        }

                        // remove unit token rule
                        normGrammar.RemoveAt(i);
                        ruleIndices.RemoveAt(i);
                        unitRuleRemoved = true;
                        break;
                    }
                }
            }

            newRuleIndices = ruleIndices.ToArray();
            return normGrammar.ToArray();
        }

        public void VisitConcatenation(int exprCount)
        {
            CatStack(curExpr, exprCount);
        }

        public void VisitDefinitions(int exprCount)
        {
            DefStack(curExpr, exprCount);
        }

        public void VisitGroup()
        {
            curExpr.Push(ComposedEbnfExpression.Group(curExpr.Pop()));
        }

        public void VisitOptional()
        {
            curExpr.Push(ComposedEbnfExpression.Optional(curExpr.Pop()));
        }

        public void VisitSequence(bool optional)
        {
            curExpr.Push(ComposedEbnfExpression.Sequence(curExpr.Pop(), optional));
        }

        public void VisitToken(EbnfToken ebnfToken)
        {
            curExpr.Push(ebnfToken);
        }

        public void VisitVariable(Variable v)
        {
            if (v == curFindVar)
            {
                curExpr.Push(curReplaceToken);
            }
            else
            {
                curExpr.Push(v);
            }
        }
    }
}
