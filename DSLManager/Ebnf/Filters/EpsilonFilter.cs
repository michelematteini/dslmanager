using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class EpsilonFilter : AbstractFilter, IEbnfExprVisitor
    {
        private HashSet<string> ng;
        private List<DerivationRule> genRules;
        private string epsilon;
        private List<Stack<EbnfExpression>> curExprs;

        public EpsilonFilter()
        {
            epsilon = EbnfToken.InstanceEpsilon.ToEbnf();
            curExprs = new List<Stack<EbnfExpression>>();
        }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            //TODO: expression that aggregate more nullable variables are currentry not associated to a nullable symbol
            //(in both ng set calculation and permutations gen)

            //compile nullable symbols list
            ng = new HashSet<string>();
            foreach (DerivationRule r in grammar)
            {
                if (r.Expression.ToEbnf() == epsilon)
                {
                    ng.Add(r.Variable.Name);
                }
            }

            //expand list
            bool listUpdated = true;
            while (listUpdated)
            {
                listUpdated = false;
                foreach (DerivationRule r in grammar)
                {
                    if (r.Expression.ExprType == EbnfExprType.Variable)
                    {
                        Variable v = (Variable)r.Expression;
                        if (ng.Contains(v.Name) && !ng.Contains(r.Variable.Name))
                        {
                            ng.Add(r.Variable.Name);
                            listUpdated = true;
                        }
                    }
                }
            }

            //filter grammar
            genRules = new List<DerivationRule>();
            HashSet<string> genRulesHash = new HashSet<string>();
            List<int> ruleIndices = new List<int>();

            for(int i = 0; i < grammar.Length; i++)
            {
                DerivationRule r = grammar[i];
                
                //remove all A -> #e or A -> ng symbol
                if (ng.Contains(r.Variable.Name) && r.Expression.ToEbnf() == epsilon) continue;

                //generate all permutations with or without the nullable variables (see VisitVariable())
                curExprs.Clear();
                curExprs.Add(new Stack<EbnfExpression>());
                r.Expression.Accept(this);

                foreach (Stack<EbnfExpression> estack in curExprs)
                {
                    if (estack.Count == 0) continue;

                    DerivationRule permRule = new DerivationRule(r.Variable, estack.Pop());
                    if (!genRulesHash.Contains(permRule.ToEbnf()))
                    {
                        genRules.Add(permRule);
                        genRulesHash.Add(permRule.ToEbnf());
                        ruleIndices.Add(i);
                    }
                }              
            }

            newRuleIndices = ruleIndices.ToArray();
            return genRules.ToArray();
        }

        public void VisitVariable(Variable v)
        {
            if (ng.Contains(v.Name))
            {
                int permCount = curExprs.Count;
                for(int i = 0; i < permCount; i++)
                {
                    curExprs.Add(new Stack<EbnfExpression>(curExprs[i].Reverse()));//make a copy of the stack
                    curExprs[i].Push(v);//add the nullable variable to only one of the two stacks.
                }
            }
            else
            {
                foreach (Stack<EbnfExpression> estack in curExprs)
                {
                    estack.Push(v);
                }
            }
        }

        public void VisitConcatenation(int exprCount)
        {
            foreach (Stack<EbnfExpression> estack in curExprs)
            {
                CatStack(estack);
            }
        }

        public void VisitToken(EbnfToken ebnfToken)
        {
            foreach (Stack<EbnfExpression> estack in curExprs)
            {
                estack.Push(ebnfToken);
            }
        }

        #region Invalid symbols

        public void VisitDefinitions(int exprCount)
        {
            throw new Exception("The grammar must be in BNF format before e-filtering.");
        }

        public void VisitOptional()
        {
            throw new Exception("The grammar must be in BNF format before e-filtering.");
        }

        public void VisitSequence(bool optional)
        {
            throw new Exception("The grammar must be in BNF format before e-filtering.");
        }

        public void VisitGroup()
        {
            throw new Exception("The grammar must be in BNF format before e-filtering.");
        }

        #endregion

    }
}
