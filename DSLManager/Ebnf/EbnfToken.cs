using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public class EbnfToken : EbnfExpression
    {
        private TokenType name;
        private string value;

        public static EbnfToken FromName(string name)
        {
            //shortcuts
            if(name == "e") name = "epsilon";
            if (name == "$") name = "EOS";

            //parsing
            TokenType tokenName = (TokenType)Enum.Parse(typeof(TokenType), name, true);
            return new EbnfToken(tokenName);
        }

        public static EbnfToken InstanceEpsilon { get { return new EbnfToken(TokenType.Epsilon); } }
        public static EbnfToken InstanceId { get { return new EbnfToken(TokenType.Id); } }
        public static EbnfToken InstanceInt { get { return new EbnfToken(TokenType.Int); } }
        public static EbnfToken InstanceInvalid { get { return new EbnfToken(TokenType.Invalid); } }
        public static EbnfToken InstanceName { get { return new EbnfToken(TokenType.Name); } }
        public static EbnfToken InstanceReal { get { return new EbnfToken(TokenType.Real); } }
        public static EbnfToken InstanceString { get { return new EbnfToken(TokenType.String); } }
        public static EbnfToken InstanceEndOfStream { get { return new EbnfToken(TokenType.EOS); } }
        public static EbnfToken InstanceNewLineAnnotation { get { return new EbnfToken(TokenType.Annotation, "NewLine"); } }

        public TokenType TokenType
        {
            get
            {
                return name;
            }
        }

        public string StringValue
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public int IntValue
        {
            get
            {
                return int.Parse(value);
            }
            set
            {
                this.value = value.ToString();
            }
        }

        public double RealValue
        {
            get
            {
                return double.Parse(value, CultureInfo.InvariantCulture);
            }
            set
            {
                this.value = value.ToString();
            }
        }

        public EbnfToken(TokenType name)
        {
            this.name = name;
        }

        public EbnfToken(TokenType name, string value) : this(name)
        {
            this.value = value;
        }

        protected override string getEbnf()
        {
            switch (name)
            {
                default:
                case TokenType.Invalid:        
                    return EbnfParser.DF_BNF_PREPROC + "invalid";
                case TokenType.Name:
                    return EbnfParser.DF_BNF_PREPROC + "name"; 
                case TokenType.Int:
                    return EbnfParser.DF_BNF_PREPROC + "int";
                case TokenType.Real:
                    return EbnfParser.DF_BNF_PREPROC + "real";
                case TokenType.Id:
                    return EbnfParser.DF_BNF_PREPROC + "id";
                case TokenType.String:
                    return EbnfParser.DF_BNF_PREPROC + "string";
                case TokenType.LangSyntax:
                    return EbnfParser.START_TERMINAL + value + EbnfParser.END_TERMINAL;
                case TokenType.Epsilon:
                    return EbnfParser.DF_BNF_PREPROC + "e";
                case TokenType.Annotation:
                    return EbnfParser.DF_BNF_ANNOTATION + value;
                case TokenType.EOS:
                    return EbnfParser.DF_BNF_PREPROC + EbnfParser.END_GRAMMAR;
            }

        }

        public override EbnfExprType ExprType
        {
            get 
            {
                return EbnfExprType.Token;
            }
        }

        public override void Accept(IEbnfExprVisitor visitor)
        {
            visitor.VisitToken(this);
        }

        public string ToTokenString()
        {
            return this.name.ToString() + "(\"" + this.value + "\")";
        }

        public override EbnfExpression[] ToSymbolArray()
        {
            return new EbnfExpression[] { this };
        }
    }

    public enum TokenType
    {
        Invalid,
        Name,
        Int,
        Real,   
        Id,
        String,     
        LangSyntax,
        Epsilon,
        Annotation,
        EOS
    }
}
