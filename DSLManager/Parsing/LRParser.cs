using System;
using System.Collections.Generic;
using System.Linq;
using DSLManager.Ebnf;
using DSLManager.Languages;

namespace DSLManager.Parsing
{
    public static class LRParser
    {
        public static Dictionary<EbnfExpression, HashSet<EbnfToken>> ComputeFirstSets(DerivationRule[] grammar)
        {
            Dictionary<EbnfExpression, HashSet<EbnfToken>> first = new Dictionary<EbnfExpression, HashSet<EbnfToken>>();
            Dictionary<EbnfExpression, HashSet<Variable>> unprocFirst = new Dictionary<EbnfExpression, HashSet<Variable>>();

            //initialize first sets
            foreach (DerivationRule r in grammar)
            {
                //Initialize Terminals
                EbnfExpression[] ruleSymbols = r.Expression.ToSymbolArray();
                foreach (EbnfExpression symbol in ruleSymbols)
                {
                    if (!first.ContainsKey(symbol) && symbol is EbnfToken)
                    {
                        first[symbol] = new HashSet<EbnfToken>();
                        first[symbol].Add((EbnfToken)symbol);
                    }
                }

                //Initialize Variables
                EbnfExpression firstSym = ruleSymbols[0];

                if (!first.ContainsKey(r.Variable))
                {
                    first[r.Variable] = new HashSet<EbnfToken>();
                    unprocFirst[r.Variable] = new HashSet<Variable>();
                }

                if (firstSym is EbnfToken)
                {
                    first[r.Variable].Add((EbnfToken)firstSym);
                }
                else
                {
                    unprocFirst[r.Variable].Add((Variable)firstSym);
                }
            }

            //iterative computing of the first sets
            bool setsCompleted = false;
            while (!setsCompleted)
            {
                setsCompleted = true;
                EbnfExpression[] unprocSymbols = unprocFirst.Keys.ToArray();
                foreach (EbnfExpression symbol in unprocSymbols)
                //process all the symbols with unsolved first sets (for each of those symbols)
                {
                    HashSet<Variable> unprocVars = unprocFirst[symbol];
                    if (unprocVars.Count > 0)
                    {
                        unprocFirst[symbol] = new HashSet<Variable>();
                        foreach (Variable uvar in unprocVars)
                        //foreach "first" variable, complete the first set by looking at the variable firsts
                        {
                            int firstSetSize = first[symbol].Count;
                            first[symbol].UnionWith(first[uvar]);
                            setsCompleted = setsCompleted && !(firstSetSize < first[symbol].Count);
                            unprocFirst[symbol].UnionWith(unprocFirst[uvar]);

                        }
                    }
                }
            }

            return first;
        }

        public static Dictionary<string, HashSet<EbnfToken>> ComputeFollowSets(DerivationRule[] grammar, Dictionary<EbnfExpression, HashSet<EbnfToken>> first)
        {
            Dictionary<string, HashSet<EbnfToken>> follow = new Dictionary<string, HashSet<EbnfToken>>();
            follow[EbnfParser.DEF_INITIAL_SYM] = new HashSet<EbnfToken>();
            follow[EbnfParser.DEF_INITIAL_SYM].Add(EbnfToken.InstanceEndOfStream);
            bool followSetsChanged = true;
            int prevSetDim;
            
            //iterative computing of the follow sets
            while (followSetsChanged)
            {
                followSetsChanged = false;

                foreach (DerivationRule r in grammar)
                {
                    EbnfExpression[] rsList = r.Expression.ToSymbolArray();
                    string ekey1 = rsList[0].ToEbnf();

                    for (int i = 0; i < rsList.Length - 1; i++)
                    {
                        EbnfExpression ekey2 = rsList[i + 1];
                        if (!follow.ContainsKey(ekey1))
                        {
                            follow[ekey1] = new HashSet<EbnfToken>();
                        }

                        prevSetDim = follow[ekey1].Count;
                        follow[ekey1].UnionWith(first[ekey2]);
                        followSetsChanged = followSetsChanged || (follow[ekey1].Count > prevSetDim);                   

                        ekey1 = ekey2.ToEbnf();
                    }

                    //add to the last production symbol the follow set of the symbol to which the production refers.
                    if (follow.ContainsKey(r.Variable.Name))
                    {
                        if (!follow.ContainsKey(ekey1))
                        {
                            follow[ekey1] = new HashSet<EbnfToken>();
                        }

                        prevSetDim = follow[ekey1].Count;
                        follow[ekey1].UnionWith(follow[r.Variable.Name]);
                        followSetsChanged = followSetsChanged || (follow[ekey1].Count > prevSetDim);
                    }
                }
            }

            return follow;
        }

        public static LexerTable GenerateLexerTable(DerivationRule[] grammar)
        {
            LexerTable lt = new LexerTable();
            int nextFreeState = 0;//index of the next free state

            //add generic literal token states
            int INITAL_STATE = nextFreeState++;
            lt.SetStateType(INITAL_STATE, TokenType.Epsilon);
            int NAME_STATE = nextFreeState++;
            lt.SetStateType(NAME_STATE, TokenType.Name);
            int ID_STATE = nextFreeState++;
            lt.SetStateType(ID_STATE, TokenType.Id);
            int INT_STATE = nextFreeState++;
            lt.SetStateType(INT_STATE, TokenType.Int);
            int DOTTED_INT_STATE = nextFreeState++;
            lt.SetStateType(DOTTED_INT_STATE, TokenType.Invalid);
            int REAL_STATE = nextFreeState++;
            lt.SetStateType(REAL_STATE, TokenType.Real);
            int EXP_REAL_STATE = nextFreeState++;
            lt.SetStateType(EXP_REAL_STATE, TokenType.Invalid);
            int EXP_NEG_REAL_STATE = nextFreeState++;
            lt.SetStateType(EXP_NEG_REAL_STATE, TokenType.Invalid);

            // define transitions for literal token states: curState = actual state id to initialize, baseState = equivalent generic state id
            Action<int, int> initLiteralTransitions = (int curState, int baseState) =>
            {
                if (baseState == INITAL_STATE)
                {
                    lt.SetRange(CharRange.LowerCaseLetters, curState, NAME_STATE);
                    lt.SetRange(CharRange.UpperCaseLetters, curState, NAME_STATE);
                    lt.SetRange(CharRange.Digits, curState, INT_STATE);
                    lt['_', curState] = ID_STATE;
                }
                else if (baseState == NAME_STATE)
                {
                    lt.SetRange(CharRange.LowerCaseLetters, curState, NAME_STATE);
                    lt.SetRange(CharRange.UpperCaseLetters, curState, NAME_STATE);
                    lt.SetRange(CharRange.Digits, curState, ID_STATE);
                    lt['-', curState] = ID_STATE;
                    lt['_', curState] = ID_STATE;
                }
                else if (baseState == ID_STATE)
                {
                    lt.SetRange(CharRange.LowerCaseLetters, curState, ID_STATE);
                    lt.SetRange(CharRange.UpperCaseLetters, curState, ID_STATE);
                    lt.SetRange(CharRange.Digits, curState, ID_STATE);
                    lt['-', curState] = ID_STATE;
                    lt['_', curState] = ID_STATE;
                }
                else if (baseState == INT_STATE)
                {
                    lt.SetRange(CharRange.Digits, curState, INT_STATE);
                    lt['.', curState] = DOTTED_INT_STATE;
                    lt['e', curState] = EXP_REAL_STATE;
                }
                else if (baseState == DOTTED_INT_STATE)
                {
                    lt.SetRange(CharRange.Digits, curState, REAL_STATE);
                }
                else if (baseState == REAL_STATE)
                {
                    lt.SetRange(CharRange.Digits, curState, REAL_STATE);
                    lt['e', curState] = EXP_REAL_STATE;
                }
                else if (baseState == EXP_REAL_STATE)
                {
                    lt.SetRange(CharRange.Digits, curState, REAL_STATE);
                    lt['-', curState] = EXP_NEG_REAL_STATE;
                }
                else if (baseState == EXP_NEG_REAL_STATE)
                {
                    lt.SetRange(CharRange.Digits, curState, REAL_STATE);
                }
            };

            // assign literal transitions
            initLiteralTransitions(INITAL_STATE, INITAL_STATE);
            initLiteralTransitions(NAME_STATE, NAME_STATE);
            initLiteralTransitions(ID_STATE, ID_STATE);
            initLiteralTransitions(INT_STATE, INT_STATE);
            initLiteralTransitions(DOTTED_INT_STATE, DOTTED_INT_STATE);
            initLiteralTransitions(REAL_STATE, REAL_STATE);
            initLiteralTransitions(EXP_REAL_STATE, EXP_REAL_STATE);
            initLiteralTransitions(EXP_NEG_REAL_STATE, EXP_NEG_REAL_STATE);

            //search all terminal sequences
            int FIRST_TERM_STATE = nextFreeState;
            HashSet<string> terminals = new HashSet<string>();
            foreach (DerivationRule r in grammar)
            {
                EbnfExpression[] symbols = r.Expression.ToSymbolArray();
                foreach (EbnfExpression e in symbols)
                {
                    EbnfToken te = e as EbnfToken;
                    if (te != null && te.TokenType == TokenType.LangSyntax)
                    {
                        terminals.Add(te.StringValue);
                    }
                }
            }

            //compile lexer table with the language syntax         
            foreach (string term in terminals)
            {
                int curState = INITAL_STATE;//current state while compiling the table for this term
                int baseState = INITAL_STATE;//current state without this term
                for (int i = 0; i < term.Length; i++)
                // iterate through each char in the term
                // a parallel chain of states for the terminal characters will be created, which links back to literal states on fail
                {
                    int nextState = lt[term[i], curState];
                    if (nextState >= FIRST_TERM_STATE)
                    //transition and next state already exists (another terminal have this prefix)
                    {
                        curState = nextState;
                        baseState = nextState;
                    }
                    else
                    //compute transition and next state type
                    {
                        baseState = lt[term[i], baseState];

                        //set transition and next state type
                        lt[term[i], curState] = nextFreeState; // move to a new state for the terminal so far
                        TokenType curType = lt.GetStateType(baseState); // type that would be detected so far without this terminal
                        lt.SetStateType(nextFreeState, curType);
                        curState = nextFreeState++;

                        // initialize the new terminal state transitions, to the default ones from the base state type
                        switch (curType)
                        {
                            case TokenType.Name:
                                initLiteralTransitions(curState, NAME_STATE);
                                break;

                            case TokenType.Int:
                                initLiteralTransitions(curState, INT_STATE);
                                break;

                            case TokenType.Real:
                                initLiteralTransitions(curState, REAL_STATE);
                                break;

                            case TokenType.Id:
                                initLiteralTransitions(curState, ID_STATE);
                                break;

                            case TokenType.Invalid:
                                if(baseState == DOTTED_INT_STATE || baseState == EXP_REAL_STATE || baseState == EXP_NEG_REAL_STATE)
                                    initLiteralTransitions(curState, baseState);
                                break;
                        }
                    }

                }//end term

                lt.SetStateType(curState, TokenType.LangSyntax);

            } //end foreach

            //compile lexer table with high priority generic tokens
     
            /*-----------String-----------*/
            int STRING_STATE = nextFreeState++;
            lt['"', INITAL_STATE] = STRING_STATE;
            lt.SetStateDefaultNext(STRING_STATE, STRING_STATE);
            lt.SetStateType(STRING_STATE, TokenType.Invalid);

            int STRING_END_STATE = nextFreeState++;
            lt['"', STRING_STATE] = STRING_END_STATE;
            lt.SetStateType(STRING_END_STATE, TokenType.String);

            //TODO: other high-priority tokens here

            return lt;
        } //end GenerateLexerTable

        public static EbnfToken[] Tokenize(string code, LexerTable lexTable)
        {
            int tokenStartIndex = 0, lastValidEnd = -1, lexerState = 0;
            TokenType lastValidType = TokenType.Invalid;// this means: still no valid token classification.
            List<EbnfToken> tokens = new List<EbnfToken>();
            EbnfToken newLine = EbnfToken.InstanceNewLineAnnotation;
            int lineIndex = 1;// used for debug porposes

            for (int tokenEndIndex = 0; tokenEndIndex < code.Length; tokenEndIndex++)
            {
                if(tokenStartIndex == tokenEndIndex && Char.IsWhiteSpace(code[tokenEndIndex]))
                //ignore white spaces between tokens
                {
                    if (code[tokenEndIndex] == '\n')
                    //its a new line, save an annotation
                    {
                        tokens.Add(newLine);
                        lineIndex++;
                    }

                    tokenStartIndex++;    
                    continue;
                }

                if (lexTable.ValidateInput(code[tokenEndIndex], lexerState))
                //valid character for this token
                {
                    lexerState = lexTable[code[tokenEndIndex], lexerState];
                    TokenType curType = lexTable.GetStateType(lexerState);
                    if (curType != TokenType.Invalid)
                    //if its a valid token until now, save its type/index to eventually backtrace.
                    {
                        lastValidEnd = tokenEndIndex;
                        lastValidType = curType;
                    }
                }
                else
                //lexer is blocked, backtrace to last valid token
                {
                    if (lastValidType == TokenType.Invalid)
                    //no valid tokens till now, syntax error
                    {
                        string invalidToken = code.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex + 1);
                        throw new SyntaxError(lineIndex, "LRPAR0005", "", invalidToken, new List<EbnfExpression>());
                    }

                    //backtracing and token creation
                    EbnfToken t = new EbnfToken(lastValidType, code.Substring(tokenStartIndex, lastValidEnd - tokenStartIndex + 1));
                    tokens.Add(t);
                    tokenStartIndex = lastValidEnd + 1;
                    tokenEndIndex = lastValidEnd;
                    lastValidType = TokenType.Invalid;
                    lexerState = 0;
                }
            }

            //check for a last token
            if (tokenStartIndex != code.Length)
            //there is another token to be processed...
            {
                if (lastValidType == TokenType.Invalid)
                //...but its invalid
                {
                    throw new SyntaxError(lineIndex, "LRPAR0006", "", code.Substring(tokenStartIndex, code.Length - tokenStartIndex), new List<EbnfExpression>());
                }
                else
                //...and its of a valid type
                {
                    //add last token
                    EbnfToken lastToken = new EbnfToken(lastValidType, code.Substring(tokenStartIndex, code.Length - tokenStartIndex));
                    tokens.Add(lastToken);
                }
            }

            //add termination token
            tokens.Add(EbnfToken.InstanceEndOfStream);

            return tokens.ToArray();
        }

        public static EbnfToken[] Tokenize(string code, DerivationRule[] grammar)
        {
            LexerTable lexTable = GenerateLexerTable(grammar);
            return Tokenize(code, lexTable);
        }

        public static ParseTable GenerateParseTable(DerivationRule[] grammar, RulePriority[] priorities)
        {
            LRDiagram lrDiag = new LRDiagram(grammar, priorities);
            lrDiag.InitializeFromRule(grammar[0]);
            lrDiag.BuildDiagram();
            return lrDiag.ToParseTable();
        }

        /// <summary>
        /// Compile a program specified by a list of tokens. A list is filled with the indices of the applied derivations rules.
        /// </summary>
        public static void CompileCode<T>(ParseTable t, EbnfToken[] code, DerivationRule[] grammar, SDTranslator<T>[] translators, int[] tIndices, bool checkOnly, out List<int> reductions, out T result)
        {
            //filter code from lexer annotations
            List<EbnfToken> filteredCode = new List<EbnfToken>();
            List<int> newLineTokens = new List<int>();
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i].TokenType != TokenType.Annotation)
                //just copy everything that's not an annotation
                {
                    filteredCode.Add(code[i]);
                }
                else
                //process annotations
                {
                    switch (code[i].StringValue)
                    {
                        case "NewLine":
                            newLineTokens.Add(filteredCode.Count);
                            break;
                    }
                }
            }
            newLineTokens.Add(filteredCode.Count);//this should be never reached, added just for the purpose of simplifying code
            code = filteredCode.ToArray();

            //initialize outputs
            reductions = new List<int>();
            result = default(T);

            //initialize parser
            int lookAheadIndex = 0;
            int codeLine = 0;//current line in the code, used for throwing syntax-errors
            Stack<EbnfExpression> parsingStack = new Stack<EbnfExpression>(); // symbols stack
            Stack<int> lastState = new Stack<int>(); // state stack
            Stack<StdStackElem<T>> sdtStack = new Stack<StdStackElem<T>>(); // stack of translated symbols with their user-defined names
            Stack<EbnfToken> sdtTokenStack = new Stack<EbnfToken>(); // contains tokens reduced by generated rules, that should be passed to user sdt later.
            Stack<StdRuleSize> stdRuleSizeStack = new Stack<StdRuleSize>(); // number of translated symbols and tokens of the already reduced rules contained in the stdStack and sdtTokenStack that will be reduced by the next user rule.
            Stack<string> parsedCodeStack = new Stack<string>(); // contains the string version of currently reduced rules with symbol replaced with the ToString() of the sdt. These values will be passed to the user sdt.

            lastState.Push(0);//initial parser state

            SDTArgs<T> sdtArgs = null; // stores variables and tokens usable in a translator

            // Parsing loop
            LRAction action = new LRAction(LRActionType.Nop, 0);
            while (action.Type != LRActionType.Accept)
            {
                // update line number
                while (lookAheadIndex == newLineTokens[codeLine]) codeLine++;

                // retrieve current LR state
                int state = lastState.Peek();
                EbnfToken lookAhead = code[lookAheadIndex];
                action = t[state, lookAhead];


                switch (action.Type)
                {
                    case LRActionType.Shift:
                        parsingStack.Push(lookAhead);
                        lookAheadIndex++;
                        lastState.Push(action.Value);
                        if (!checkOnly) parsedCodeStack.Push(lookAhead.StringValue);
                        break;

                    case LRActionType.Reduce:
                        EbnfExpression curReduceExpression = grammar[action.Value].Expression;
                        int curRuleLength = curReduceExpression.ToSymbolArray().Length; // number of tokens to pop for this reduction
                        bool isRuleGenerated = tIndices != null && tIndices[action.Value] < 0;
                        if (!checkOnly && !isRuleGenerated)
                        {
                            sdtArgs = new SDTArgs<T>(); // initialize sdt maps 
                            sdtArgs.CodeLine = codeLine + 1;
                        }


                        // Update sdt stack and maps with the current rule
                        // if no SDT will be called, the single elemente length will be
                        // the sum of the reduced elements (see below #1)
                        {
                            string curRuleParsedCode = string.Empty; // parsed source code for the currently reduced rule
                            StdRuleSize stdResultSize = new StdRuleSize(0, isRuleGenerated ? 0 /* calculated next*/: 1 /* = one single symbol*/);

                            for (int i = 0; i < curRuleLength; i++)
                            {
                                lastState.Pop();
                                EbnfExpression e = parsingStack.Pop();

                                if (checkOnly) continue; // no sdt management needed to just perform a syntax-check

                                // merge parsed code for this rule
                                curRuleParsedCode = parsedCodeStack.Pop() + (curRuleParsedCode.Length == 0 ? "" : " " + curRuleParsedCode);

                                // process tokens
                                if(e is EbnfToken eToken)
                                {
                                    if (isRuleGenerated)
                                    {
                                        // save to sdt stacks and increment parent rule token size
                                        stdResultSize.TokenCount++;
                                        sdtTokenStack.Push(eToken);
                                    }
                                    else
                                    {
                                        // add token to sdt maps
                                        sdtArgs.Tokens.PushSymbol(eToken, eToken.TokenType == TokenType.LangSyntax ? eToken.StringValue : eToken.ToString());
                                    }
                                }
                                // process symbols
                                else
                                {
                                    if (isRuleGenerated)
                                    {
                                        // add the generated rule sdt size to the parent rule
                                        stdResultSize += stdRuleSizeStack.Pop();//#1
                                    }
                                    else
                                    {
                                        // add all the generated sub-rule tokens and translated symbols to sdt maps
                                        StdRuleSize curRuleSize = stdRuleSizeStack.Pop();
                                        for (int si = 0; si < curRuleSize.SymbolCount; si++)
                                        {
                                            StdStackElem<T> subSymbol = sdtStack.Pop();
                                            sdtArgs.Values.PushSymbol(subSymbol.Value, subSymbol.Name);
                                        }
                                        for (int ti = 0; ti < curRuleSize.TokenCount; ti++)
                                        {
                                            EbnfToken subToken = (EbnfToken)sdtTokenStack.Pop();
                                            sdtArgs.Tokens.PushSymbol(subToken, subToken.TokenType == TokenType.LangSyntax ? subToken.StringValue : subToken.ToString());
                                        }
                                    }
                                }                                
                            }

                            if (!checkOnly)
                            {
                                stdRuleSizeStack.Push(stdResultSize); // save this reduction sdt stack sizes
                                if (isRuleGenerated)
                                    parsedCodeStack.Push(curRuleParsedCode); // push parsed code as is for generated symbols
                                else
                                    sdtArgs.ParsedCode = curRuleParsedCode; // add parsed code as an user sdt param
                            }
                        }

                        // Update parsing stacks
                        parsingStack.Push(grammar[action.Value].Variable); // push the resulting symbol of reduction into the parsing stack
                        reductions.Add(action.Value); //list all applied reductions as output
                        lastState.Push(t[lastState.Peek(), grammar[action.Value].Variable].Value);//set as new state the one after the result of reduction is shifted

                        // Call SDT and push the result into sdt stack
                        if (!checkOnly && !isRuleGenerated)
                        {
                            try
                            {
                                T translatedSymbol = translators[tIndices[action.Value]].Invoke(sdtArgs);
                                sdtStack.Push(new StdStackElem<T>(grammar[action.Value].Variable.Name, translatedSymbol)); //push the translated symbol into the translation stack
                                string translatedCode = translatedSymbol.ToString();
                                if (translatedSymbol is IParsedCodeType parsedCode)
                                    translatedCode = parsedCode.GetSrcCode();
                                parsedCodeStack.Push(translatedCode);
                            }
                            catch (CompileError e)
                            {
                                e.Line = codeLine + 1;
                                throw e;
                            }
                            catch (Exception e)
                            {
                                throw new SDTError(-1, "LRPAR0007", "", e.Message + " ( While parsing '" + grammar[action.Value].Variable.ToString() + "')");
                            }
                        }
                        break;

                    case LRActionType.Accept:
                        if (parsingStack.Count != 1) // PARSING ERROR
                            throw new CompileError(codeLine + 1, "LRPAR0004", "End of file reached more than one time (your file is probably corrupted or has an invalid start", "");

                        Variable v = parsingStack.Pop() as Variable;

                        if(v == null || v.Name != EbnfParser.DEF_INITIAL_SYM) // PARSING ERROR
                            throw new CompileError(codeLine + 1, "LRPAR0003", "Invalid language.", "");

                        if (!checkOnly) result = sdtStack.Pop().Value;
                        break;
                        

                    case LRActionType.Nop:
                        //PARSING ERROR
                        List<EbnfExpression> expected = t.ExpectedTokens(state);
                        throw new SyntaxError(codeLine + 1, "LRPAR0002", "", lookAhead, expected);

                    default:
                        //PARSING ERROR
                        throw new CompileError(-1, "LRPAR0001", "Invalid parsing command.", "");
                }

            }//End While

        }
        
        /// <summary>
        /// Compile a program specified by a list of tokens, and returns a compiled result.
        /// </summary>
        public static void CompileCode<T>(ParseTable t, EbnfToken[] code, DerivationRule[] grammar, SDTranslator<T>[] translators, int[] tIndices, bool checkOnly, out T result)
        {
            List<int> reductions;
            CompileCode<T>(t, code, grammar, translators, tIndices, checkOnly, out reductions, out result);
        }


        /// <summary>
        /// Check the syntax of a program specified by a list of tokens. If an error is found its trown as an exception. A list is filled with the indices of the applied derivations rules.
        /// </summary>
        /// <param name="reductions"></param>
        public static void CheckSyntax(ParseTable t, EbnfToken[] code, DerivationRule[] grammar, out List<int> reductions)
        {
            bool result;
            CompileCode<bool>(t, code, grammar, null, null, true, out reductions, out result);
        }

        /// <summary>
        /// Check the syntax of a program specified by a list of tokens.
        /// </summary>
        public static void CheckSyntax(ParseTable t, EbnfToken[] code, DerivationRule[] grammar)
        {
            bool result;
            List<int> reductions;
            CompileCode<bool>(t, code, grammar, null, null, true, out reductions, out result);
        }

        private struct StdStackElem<T>
        {
            public string Name;
            public T Value;

            public StdStackElem(string name, T value)
            {
                Name = name;
                Value = value;
            }
        }

        private struct StdRuleSize
        {
            public int TokenCount, SymbolCount;

            public StdRuleSize(int tokenCount, int symbolCount)
            {
                TokenCount = tokenCount;
                SymbolCount = symbolCount;
            }

            public static StdRuleSize operator +(StdRuleSize size1, StdRuleSize size2)
            {
                return new StdRuleSize(size1.TokenCount + size2.TokenCount, size1.SymbolCount + size2.SymbolCount);
            }
        }

    }

}
