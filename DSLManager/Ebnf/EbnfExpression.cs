using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public abstract class EbnfExpression : /*IEbnfExpression*/IEquatable<EbnfExpression>
    {
        private string ebnfCache;

        protected EbnfExpression()
        {
            ebnfCache = string.Empty;
        }

        public string ToEbnf()
        {
            if (ebnfCache == string.Empty)
            {
                ebnfCache = getEbnf();
            }

            return ebnfCache;
        }

        protected abstract string getEbnf();

        public abstract EbnfExprType ExprType { get; }

        public abstract void Accept(IEbnfExprVisitor visitor);

        public abstract EbnfExpression[] ToSymbolArray();

        public static bool operator ==(EbnfExpression expr1, EbnfExpression expr2)
        {
            if (object.ReferenceEquals(expr1, null) ^ object.ReferenceEquals(expr2,null))
                return false;
            if (object.ReferenceEquals(expr1, null) && object.ReferenceEquals(expr2, null))
                return true;

            return expr1.ToEbnf() == expr2.ToEbnf();
        }

        public static bool operator !=(EbnfExpression expr1, EbnfExpression expr2)
        {
            return !(expr1 == expr2);
        }

        public override string ToString()
        {
            return this.ToEbnf();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as EbnfExpression);
        }

        public override int GetHashCode()
        {
            return ToEbnf().GetHashCode();
        }

        public bool Equals(EbnfExpression other)
        {
            return this == other;
        }
    }

    public enum EbnfExprType
    {
        Variable,
        Definitions,
        Concatenation,
        Optional,
        Sequence,
        Group,
        Undefined,
        Token
    }
}
