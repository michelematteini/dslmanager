using DSLManager.Ebnf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public class LexerTable
    {
        private Dictionary<ulong, uint> lexTable;
        private Dictionary<uint, StateInfo> stateInfos;

        private struct StateInfo
        {
            public TokenType stateType;
            public int defaultNext;

            public StateInfo(TokenType stateType, int defaultNext)
            {
                this.stateType = stateType;
                this.defaultNext = defaultNext;
            }

        }

        public LexerTable()
        {
            lexTable = new Dictionary<ulong, uint>();
            stateInfos = new Dictionary<uint, StateInfo>();
        }

        public int this[char input, int state]
        {
            get
            {
                //if exists, return the next state for this input
                ulong hashKey = hash(input, (uint)state);
                if (lexTable.ContainsKey(hashKey))
                {
                    return (int)lexTable[hashKey];
                }

                //if state/input is undefined, return the default 'next' for this state
                if (stateInfos.ContainsKey((uint)state))
                {
                    return stateInfos[(uint)state].defaultNext;
                }

                //if no default is available...
                return -1;       
            }
            set
            {
                ulong hashKey = hash(input, (uint)state);
                lexTable[hashKey] = (uint)value;
            }
        }

        public bool ValidateInput(char input, int state)
        {
            ulong hashKey = hash(input, (uint)state);
            return lexTable.ContainsKey(hashKey) || (stateInfos.ContainsKey((uint)state) && stateInfos[(uint)state].defaultNext >= 0);
        }

        /// <summary>
        /// Set all the transitions in a given char range, if other transitions exist, they will be overridden.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="state"></param>
        /// <param name="nextState"></param>
        public void SetRange(CharRange chars, int state, int nextState)
        {
            for (int c = chars.first; c <= chars.last; c++)
            {
                this[(char)c, state] = nextState;
            }
        }

        /// <summary>
        /// Set all the transitions in a given char range, without overriding the existing ones.
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="state"></param>
        /// <param name="nextState"></param>
        public void FillRange(CharRange chars, int state, int nextState)
        {
            for (int c = chars.first; c <= chars.last; c++)
            {
                ulong hashKey = hash((char)c, (uint)state);
                if (!lexTable.ContainsKey(hashKey))
                    lexTable[hashKey] = (uint)nextState;
            }
        }

        private ulong hash(char c, uint i)
        {
            return (((ulong)c) << 0x20) + i;
        }

        public int StateCount
        {
            get
            {
                return stateInfos.Count;
            }
        }

        public void SetStateType(int state, TokenType type)
        {
            StateInfo si = new StateInfo();
            if (stateInfos.ContainsKey((uint)state))
            {
                si = stateInfos[(uint)state];
            }
            else
            {
                si.defaultNext = -1;
            }
            si.stateType = type;
            stateInfos[(uint)state] = si;
        }

        public TokenType GetStateType(int state)
        {
            if (state >= 0 && stateInfos.ContainsKey((uint)state))
            {
                return stateInfos[(uint)state].stateType;
            }

            return TokenType.Invalid;
        }

        public void SetStateDefaultNext(int state, int defaultNext)
        {
            StateInfo si = new StateInfo();
            if (stateInfos.ContainsKey((uint)state))
            {
                si = stateInfos[(uint)state];
            }
            else
            {
                si.stateType = TokenType.Invalid;
            }
            si.defaultNext = defaultNext;
            stateInfos[(uint)state] = si;
        }
        
    }

    public struct CharRange
    {
        public char first, last;

        public CharRange(char first, char last)
        {
            this.first = first;
            this.last = last;
        }

        public bool Contains(char c)
        {
            return first <= c && c <= last;
        }

        public bool Contains(CharRange cr)
        {
            return first <= cr.first && cr.last <= last;
        }

        public static readonly CharRange AllChars = new CharRange(Char.MinValue, Char.MaxValue);
        public static readonly CharRange LowerCaseLetters = new CharRange('a', 'z');
        public static readonly CharRange UpperCaseLetters = new CharRange('A', 'Z');
        public static readonly CharRange Digits = new CharRange('0', '9');
    }

}
