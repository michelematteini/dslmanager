using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf.Filters
{
    public abstract class AbstractFilter : IGrammarFilter
    {
		private static int genVarID = 0;
		private static readonly string GEN_VAR_PREFIX = "VAR";
		
        public DerivationRule[] ApplyFilter(DerivationRule[] grammar)
        {
            int[] newRuleIndices;
            return ApplyFilter(grammar, out newRuleIndices);
        }

        protected Variable MakeNewVariable()
		{
			genVarID++;
			return GetLastVariable();
		}
		
		protected Variable GetLastVariable()
		{
			return new Variable(GEN_VAR_PREFIX + genVarID);
		}
		
		public bool IsGenerated(Variable v)
		{
			int varID;
			bool isGen = v.Name.StartsWith(GEN_VAR_PREFIX);
			isGen = isGen && int.TryParse(v.Name.Substring(GEN_VAR_PREFIX.Length), out varID);
			return isGen;
		}

		protected void CatStack(Stack<EbnfExpression> exprStack, int exprCount = 0)
		{
			if(exprCount == 0)
				exprCount = exprStack.Count;

			EbnfExpression[] exprArray = new EbnfExpression[exprCount];
			for (int i = 0; exprStack.Count > 0; i++)
			{
				exprArray[exprCount - i - 1] = exprStack.Pop();
			}
			exprStack.Push(ComposedEbnfExpression.Cat(exprArray));
		}

		protected void DefStack(Stack<EbnfExpression> exprStack, int exprCount = 0)
		{
			if (exprCount == 0)
				exprCount = exprStack.Count;

			EbnfExpression[] exprArray = new EbnfExpression[exprCount];
			for (int i = 0; exprStack.Count > 0; i++)
			{
				exprArray[exprCount - i - 1] = exprStack.Pop();
			}
			exprStack.Push(ComposedEbnfExpression.Or(exprArray));
		}

		public abstract DerivationRule[] ApplyFilter(DerivationRule[] grammar, out int[] newRuleIndices);
    }
}
