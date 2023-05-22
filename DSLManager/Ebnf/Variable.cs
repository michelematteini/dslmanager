using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public class Variable : EbnfExpression
    {
        public string Name { get; private set; }

        public Variable(string name)
        {
            this.Name = name;
        }

        protected override string getEbnf()
        {
            return Name;
        }

        public override EbnfExprType ExprType
        {
            get
            {
                return EbnfExprType.Variable;
            }
        }

        public override void Accept(IEbnfExprVisitor visitor)
        {
            visitor.VisitVariable(this);
        }


        public override EbnfExpression[] ToSymbolArray()
        {
            return new EbnfExpression[] { this };
        }

    }
}
