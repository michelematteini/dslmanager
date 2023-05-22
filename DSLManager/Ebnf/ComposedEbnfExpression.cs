using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DSLManager.Ebnf
{
    public class ComposedEbnfExpression : EbnfExpression
    {
        #region Static Creation Methods
       
        public static ComposedEbnfExpression Or(params EbnfExpression[] exprs)
        {
            return new ComposedEbnfExpression(EbnfExprType.Definitions, false, exprs);
        }

        public static ComposedEbnfExpression Cat(params EbnfExpression[] exprs)
        {
            return new ComposedEbnfExpression(EbnfExprType.Concatenation, false, exprs);
        }

        public static ComposedEbnfExpression Group(EbnfExpression e)
        {
            return new ComposedEbnfExpression(EbnfExprType.Group, false, new EbnfExpression[] { e });
        }

        public static ComposedEbnfExpression Optional(EbnfExpression e)
        {
            return new ComposedEbnfExpression(EbnfExprType.Optional, true, new EbnfExpression[] { e });
        }

        public static ComposedEbnfExpression Sequence(EbnfExpression e, bool optional)
        {
            return new ComposedEbnfExpression(EbnfExprType.Sequence, optional, new EbnfExpression[] { e });
        }

        #endregion

        private EbnfExprType type;
        private bool optional;
        private EbnfExpression[] exprs;

        protected ComposedEbnfExpression(EbnfExprType type, bool optional, params EbnfExpression[] exprs)
        {
            this.type = type;
            this.optional = optional;
            this.exprs = exprs;
        }

        protected override string getEbnf()
        {
            string ebnf = "";

            switch (type)
            {
                case EbnfExprType.Definitions:
                    ebnf = exprs[0].ToEbnf();
                    for (int i = 1; i < exprs.Length; i++)
                    {
                        ebnf += String.Format(" {0} {1}", EbnfParser.OP_OR, exprs[i].ToEbnf());
                    }
                    return ebnf;
                case EbnfExprType.Concatenation:
                    ebnf = exprs[0].ToEbnf();
                    for (int i = 1; i < exprs.Length; i++)
                    {
                        ebnf += String.Format(" {0} {1}", EbnfParser.OP_CAT, exprs[i].ToEbnf());
                    }
                    return ebnf;
                case EbnfExprType.Optional:
                    return EbnfParser.START_OPTIONAL + exprs[0].ToEbnf() + EbnfParser.END_OPTIONAL;
                case EbnfExprType.Sequence:
                    string endSeqStr = optional ? EbnfParser.END_OPT_SEQ : EbnfParser.END_SEQ;
                    return EbnfParser.START_SEQ + exprs[0].ToEbnf() + endSeqStr;
                case EbnfExprType.Group:
                    return EbnfParser.START_GROUP + exprs[0].ToEbnf() + EbnfParser.END_GROUP;
                case EbnfExprType.Undefined:
                default:
                    return string.Empty;
            }
        }

        public override void Accept(IEbnfExprVisitor visitor)
        {
            foreach (EbnfExpression e in this.exprs)
            {
                e.Accept(visitor);
            }

            switch (type)
            {
                case EbnfExprType.Definitions:
                    visitor.VisitDefinitions(this.exprs.Length);
                    break;
                case EbnfExprType.Concatenation:
                    visitor.VisitConcatenation(this.exprs.Length);
                    break;
                case EbnfExprType.Optional:
                    visitor.VisitOptional();
                    break;
                case EbnfExprType.Sequence:
                    visitor.VisitSequence(this.optional);
                    break;
                case EbnfExprType.Group:
                    visitor.VisitGroup();
                    break;
            }
        }

        public override EbnfExprType ExprType
        {
            get 
            {
                return this.type;
            }
        }

        public override EbnfExpression[] ToSymbolArray()
        {
            List<EbnfExpression> symbols = new List<EbnfExpression>();
            foreach (EbnfExpression e in this.exprs)
            {
                symbols.AddRange(e.ToSymbolArray());
            }

            return symbols.ToArray();
        }


    }

}
