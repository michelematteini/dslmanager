using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public class DerivationRule : IEquatable<DerivationRule>
    { 
        private const int VARIABLE = 0;
        private const int PRODUCTION = 1;
        private string ebnfCache;

        public Variable Variable { get; private set; }

        public EbnfExpression Expression { get; private set; }

        /// <summary>
        /// Create a Deravation from an ebnf string. 
        /// You can use '' for terminals insted of escape sequences.
        /// Rule separtion character ';' can be omitted.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DerivationRule FromString(string s)
        {
            return EbnfParser.ParseRule(s);
        }

        public DerivationRule(Variable v, EbnfExpression production)
        {
            this.Variable = v;
            this.Expression = production;
            ebnfCache = string.Empty;
        }

        public string ToEbnf()
        {
            if (ebnfCache == string.Empty)
            {
                ebnfCache = String.Format("{0} {1} {2};", Variable.Name, EbnfParser.OP_PRODUCE, Expression.ToEbnf());
            }

            return ebnfCache;
        }

        public override string ToString()
        {
            return this.ToEbnf();
        }

        public static bool operator ==(DerivationRule rule1, DerivationRule rule2)
        {
            if (object.ReferenceEquals(rule1, null) ^ object.ReferenceEquals(rule2, null))
                return false;
            if (object.ReferenceEquals(rule1, null) && object.ReferenceEquals(rule2, null))
                return true;

            return rule1.ToEbnf() == rule2.ToEbnf();
        }

        public static bool operator !=(DerivationRule rule1, DerivationRule rule2)
        {
            return !(rule1 == rule2);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as DerivationRule);
        }

        public override int GetHashCode()
        {
            return ToEbnf().GetHashCode();
        }

        public bool Equals(DerivationRule other)
        {
            return this == other;
        }

    }
}
