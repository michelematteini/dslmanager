using DSLManager.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public static class EbnfParser
    {       
        #region EBNF Tokens

        public const string OP_PRODUCE = "::=";
        public const string OP_CAT = ",";
        public const string OP_OR = "|";
        public const string END_RULE = ";";
        public const string START_TERMINAL = "\"";
        public const string END_TERMINAL = "\"";
        public const string START_COMMENT = "(*";
        public const string END_COMMENT = "*)";
        public const string START_GROUP = "(";
        public const string END_GROUP = ")";
        public const string START_OPTIONAL = "[";
        public const string END_OPTIONAL = "]";
        public const string START_SEQ = "{";
        public const string END_SEQ = "}+";
        public const string END_OPT_SEQ = "}*";
        public const string DF_BNF_PREPROC = "#";
        public const string DF_BNF_ANNOTATION = "@";
        public const string END_GRAMMAR = "$";
        public const string DEF_INITIAL_SYM = "START";

        #endregion
        
        public const char TOKEN_SEPARATOR = '~';

        public static string[] Tokenize(string ebnf)
        {
            if (ebnf.Contains(TOKEN_SEPARATOR))
            {
                throw new Exception(String.Format("Invalid character {0} in ebnf (\"{1}\").", TOKEN_SEPARATOR, ebnf));
            }

            // '' in ebnf will be interpreted as terminal separator
            ebnf = ebnf.Replace("''", START_TERMINAL); 

            //first approximated split
            string[] ebnfTokens = removeWhiteSpace(ebnf)
                .Replace(OP_PRODUCE, separate(OP_PRODUCE))
                .Replace(OP_CAT, separate(OP_CAT))
                .Replace(OP_OR, separate(OP_OR))
                .Replace(OP_CAT, separate(OP_CAT))
                .Replace(END_RULE, separate(END_RULE))
                .Replace(START_TERMINAL, separate(START_TERMINAL))
                .Replace(END_TERMINAL, separate(END_TERMINAL))
                .Replace(START_COMMENT, separate(START_COMMENT))
                .Replace(END_COMMENT, separate(END_COMMENT))
                .Replace(START_GROUP, separate(START_GROUP))
                .Replace(END_GROUP, separate(END_GROUP))
                .Replace(START_OPTIONAL, separate(START_OPTIONAL))
                .Replace(END_OPTIONAL, separate(END_OPTIONAL))
                .Replace(START_SEQ, separate(START_SEQ))
                .Replace(END_SEQ, separate(END_SEQ))
                .Replace(END_OPT_SEQ, separate(END_OPT_SEQ))
                .Replace(DF_BNF_PREPROC, separate(DF_BNF_PREPROC))
                .Split(new char[] { TOKEN_SEPARATOR });

            //correct literal spliting
            List<string> correctedTokens = new List<string>();
            bool composing = false;
            string composedLiteral = string.Empty;
            foreach (string token in ebnfTokens)
            {
                if (composing)
                {
                    if (token == END_TERMINAL)
                    {
                        composedLiteral.Replace("``", "\"");// `` in a terminal will be interpreted as "
                        correctedTokens.Add(composedLiteral);
                        composedLiteral = string.Empty;
                        composing = false;
                    }
                    else
                    {
                        composedLiteral += token;
                        continue;
                    }
                }
                else if (token == START_TERMINAL)
                {
                    composing = true;
                }

                if (token == string.Empty)
                {
                    continue;
                }

                correctedTokens.Add(token);
            }

            //return cleaned tokens
            return correctedTokens.ToArray();
        }

        public static DerivationRule ParseRule(string ebnfRule)
        {
            // add termination of omitted
            if (!ebnfRule.EndsWith(END_RULE))
            {
                ebnfRule += END_RULE;
            }

            string[] tokens = Tokenize(ebnfRule);
            int i = 0;
            return parseRuleFrom(tokens, ref i);
        }

        public static DerivationRule[] ParseGrammar(string ebnfGrammar)
        {
            int i = 0;
            string[] tokens = Tokenize(ebnfGrammar);   
            List<DerivationRule> grammarRules = new List<DerivationRule>();

            while (i < tokens.Length)
            {
                grammarRules.Add(parseRuleFrom(tokens, ref i));
            }

            return grammarRules.ToArray();
        }

        private static DerivationRule parseRuleFrom(string[] tokens, ref int startIndex)
        {
            Variable v = new Variable(tokens[startIndex++]);

            if (tokens[startIndex] != OP_PRODUCE)
            {
                throw new SyntaxError(-1, "EBNFP0001",  "", tokens[startIndex], new EbnfToken(TokenType.LangSyntax, OP_PRODUCE));
            }

            startIndex++;
            Stack<EbnfExpression> shiftStack = new Stack<EbnfExpression>();
            parseExpression(tokens, ref startIndex, shiftStack, EbnfExprType.Undefined);

            if (tokens[startIndex] != EbnfParser.END_RULE)
            {
                throw new CompileError(-1, "EBNFP0002", String.Format("'{0}' expected.", EbnfParser.END_RULE), "");
            }
            startIndex++;
            return new DerivationRule(v, shiftStack.Pop());
        }

        private static void parseExpression(string[] tokens, ref int i, Stack<EbnfExpression> shiftStack, EbnfExprType type)
        {
            //activation record pointer for shiftStack
            int ar = shiftStack.Count;
            //accumulation array for defs/cat popping
            EbnfExpression[] defs;
            //token state dependence flag
            bool isStateDependent;

            while (i < tokens.Length)
            {
                isStateDependent = false;

                /*
                 * Tokens with state-independent actions
                 */
                switch (tokens[i])
                {
                    case START_COMMENT:
                        while (i < tokens.Length && tokens[i] != END_COMMENT) i++;
                        i++;
                        break;

                    case START_TERMINAL: // EbnfTokens.END_TERMINAL
                        if (i + 2 >= tokens.Length || tokens[i + 2] != END_TERMINAL)
                            throw new CompileError(-1, "EBNFP0003", "String termination character not found in rule \"" + tokens[0] + "\"...", "");
                        shiftStack.Push(new EbnfToken(TokenType.LangSyntax, tokens[i + 1]));
                        i += 3;
                        break;

                    case DF_BNF_PREPROC:
                        if (i + 1 >= tokens.Length)
                            throw new CompileError(-1, "EBNFP0004", "Invalid use of preprocessor commands in rule \"" + tokens[0] + "\".", "");
                        shiftStack.Push(EbnfToken.FromName(tokens[i + 1]));
                        i += 2;
                        break;

                    case START_GROUP:
                        i++;
                        parseExpression(tokens, ref i, shiftStack, EbnfExprType.Group);
                        break;

                    case START_OPTIONAL:
                        i++;
                        parseExpression(tokens, ref i, shiftStack, EbnfExprType.Optional);
                        break;

                    case START_SEQ:
                        i++;
                        parseExpression(tokens, ref i, shiftStack, EbnfExprType.Sequence);
                        break;

                    default:
                        isStateDependent = true;
                        break;
                }

                if (!isStateDependent) continue;

                /*
                 * Tokens with state-dependent actions
                 */
                switch (type)
                {
                    case EbnfExprType.Definitions:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                            case END_RULE:
                            case END_GROUP:
                            case END_OPTIONAL:
                            case END_SEQ:
                            case END_OPT_SEQ:
                                defs = new EbnfExpression[shiftStack.Count - ar + 1];
                                for (int k = defs.Length - 1; k >= 0; k--)
                                {
                                    defs[k] = shiftStack.Pop();
                                }
                                shiftStack.Push(ComposedEbnfExpression.Or(defs));
                                return;
                            case OP_OR:
                                i++;
                                break;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;

                    case EbnfExprType.Concatenation:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                                i++;
                                break;
                            case OP_OR:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Definitions);
                                break;
                            case END_RULE:
                            case END_GROUP:
                            case END_OPTIONAL:
                            case END_SEQ:
                            case END_OPT_SEQ:
                                defs = new EbnfExpression[shiftStack.Count - ar + 1];
                                for (int k = defs.Length - 1; k >= 0; k--)
                                {
                                    defs[k] = shiftStack.Pop();
                                }
                                shiftStack.Push(ComposedEbnfExpression.Cat(defs));
                                return;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;

                    case EbnfExprType.Optional:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Concatenation);
                                break;
                            case OP_OR:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Definitions);
                                break;
                            case END_RULE:
                            case END_GROUP:
                            case END_SEQ:
                            case END_OPT_SEQ:
                                throw new CompileError(-1, "EBNFP0005", "Unexpected symbol: " + tokens[i] + " of rule \"" + tokens[0] + "\"...", "");
                            case END_OPTIONAL:
                                shiftStack.Push(ComposedEbnfExpression.Optional(shiftStack.Pop()));
                                i++;
                                return;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;

                    case EbnfExprType.Sequence:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Concatenation);
                                break;
                            case OP_OR:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Definitions);
                                break;
                            case END_RULE:
                            case END_GROUP:
                            case END_OPTIONAL:
                                throw new CompileError(-1, "EBNFP0006", "Unexpected symbol: " + tokens[i] + " of rule \"" + tokens[0] + "\", sequence repetition expected! (* or +)", "");
                            case END_SEQ:
                                shiftStack.Push(ComposedEbnfExpression.Sequence(shiftStack.Pop(), false));
                                i++;
                                return;
                            case END_OPT_SEQ:
                                shiftStack.Push(ComposedEbnfExpression.Sequence(shiftStack.Pop(), true));
                                i++;
                                return;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;

                    case EbnfExprType.Group:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Concatenation);
                                break;
                            case OP_OR:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Definitions);
                                break;
                            case END_RULE:
                                throw new CompileError(-1, "EBNFP0007", "Unexpected end of rule \"" + tokens[0] + "\"! Are you missing a " + END_GROUP + " ?", "");
                            case END_OPTIONAL:
                            case END_SEQ:
                            case END_OPT_SEQ:
                                throw new CompileError(-1, "EBNFP0007", "Unexpected symbol: " + tokens[i] + " of rule \"" + tokens[0] + "\", " + END_GROUP + "expected.", "");
                            case END_GROUP:
                                shiftStack.Push(ComposedEbnfExpression.Group(shiftStack.Pop()));
                                i++;
                                return;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;

                    case EbnfExprType.Undefined:
                        switch (tokens[i])
                        {
                            case OP_CAT:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Concatenation);
                                break;
                            case OP_OR:
                                i++;
                                parseExpression(tokens, ref i, shiftStack, EbnfExprType.Definitions);
                                break;
                            case END_OPTIONAL:
                            case END_SEQ:
                            case END_OPT_SEQ:
                            case END_GROUP:
                                throw new CompileError(-1, "EBNFP0008", "Unexpected symbol: " + tokens[i] + " of rule \"" + tokens[0] + "\"...", "");
                            case END_RULE:
                                //end of expression parsing
                                return;
                            default://variable
                                shiftStack.Push(new Variable(tokens[i]));
                                i++;
                                break;
                        }
                        break;
                    default:
                        throw new CompileError(-1, "EBNFP0010", "Unknown ebnf error.", "");
                }
            }

            throw new CompileError(-1, "EBNFP0011", "End of grammar reached before parsing termination! Are you missing a ';'?", "");
        }

        private static string separate(string token)
        {
            return TOKEN_SEPARATOR + token + TOKEN_SEPARATOR;
        }

        private static string removeWhiteSpace(string input)
        {
            StringBuilder output = new StringBuilder(input.Length);
            int ssi = 0, sse = 0;
            bool filterSuspended = false;

            for (int index = 0; index < input.Length; index++)
            {
                if (filterSuspended)
                    sse = END_TERMINAL[ssi] == input[index] ? sse + 1 : 0;
                else
                    ssi = START_TERMINAL[ssi] == input[index] ? ssi + 1 : 0;

                if (ssi == START_TERMINAL.Length)
                {
                    filterSuspended = true;
                    ssi = 0;
                }
                if (sse == END_TERMINAL.Length)
                {
                    filterSuspended = false;
                    sse = 0;
                }

                if (filterSuspended || !Char.IsWhiteSpace(input, index))
                {
                    output.Append(input[index]);
                }
            }

            return output.ToString();
        }

    }
}
