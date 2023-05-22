using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public class EbnfToBnfFilter : AbstractFilter, IEbnfExprVisitor
    {
        private List<DerivationRule> genRules;
        private Stack<EbnfExpression> curExpr;
        private List<int> ruleIndices;

        public EbnfToBnfFilter() { }

        public override DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices)
        {
            genRules = new List<DerivationRule>();
            curExpr = new Stack<EbnfExpression>();
            ruleIndices = new List<int>();

            for (int i = 0; i < grammar.Length; i++)
            {
                grammar[i].Expression.Accept(this);
                genRules.Insert(i, new DerivationRule(grammar[i].Variable, curExpr.Pop()));
                ruleIndices.Insert(i, i);
            }

            newRuleIndices = ruleIndices.ToArray();
            return genRules.ToArray();
        }

        public void VisitVariable(Variable v)
        {
            Push(v);
        }

        public void VisitDefinitions(int exprCount)
        {
            MakeNewVariable();
            for (int i = 0; i < exprCount; i++)
            {
                MakeRule(Pop());
            }
            Push(GetLastVariable());
        }

        public void VisitConcatenation(int exprCount)
        {
            EbnfExpression[] es = new EbnfExpression[exprCount];
            for (int i = 0; i < exprCount; i++)
            {
                es[exprCount - i - 1] = Pop();
            }
            Push(ComposedEbnfExpression.Cat(es));
        }

        public void VisitOptional()
        {
            MakeNewVariable();
            MakeRule(Pop());
            MakeRule(EbnfToken.InstanceEpsilon);
            Push(GetLastVariable());
        }

        public void VisitSequence(bool optional)
        {
            EbnfExpression e = Pop();
            MakeNewVariable();
            MakeRule(e);
            MakeRule(ComposedEbnfExpression.Cat(e, GetLastVariable()));
            if (optional) MakeRule(EbnfToken.InstanceEpsilon);
            Push(GetLastVariable());
        }

        public void VisitGroup()
        {
			MakeNewVariable();
            MakeRule(Pop());
            Push(GetLastVariable());
        }

        public void VisitToken(EbnfToken ebnfToken)
        {
            Push(ebnfToken);
        }

        #region Simplified Grammar Build

        private void Push(EbnfExpression e)
        {
            curExpr.Push(e);
        }

        private EbnfExpression Pop()
        {
            return curExpr.Pop();
        }

        private void MakeRule(EbnfExpression e)
        {
            genRules.Add(new DerivationRule(GetLastVariable(), e));
            ruleIndices.Add(-1);
        }

        #endregion

    }
}
