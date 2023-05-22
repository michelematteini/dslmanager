using DSLManager.Ebnf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public interface ISymbolMapper<T>
    {
        T this[string name] { get; }

        T this[string name, int index] { get; }

        int GetInstanceCount(string name);

        bool Contains(string name);

        T GetInstanceValue();

        List<T> GetList(string name);

        int Count { get; }
    }

    public class SymbolMapper<T> : ISymbolMapper<T>
    {
        private Dictionary<string, List<T>> symMap;

        public T this[string name, int index] 
        {
            get
            {
                return symMap[name][index];
            }
        }

        public T this[string name]
        {
            get
            {
                return this[name, 0];
            }
        }

        public int Count
        {
            get
            {
                return symMap.Count;
            }
        }

        public int GetInstanceCount(string name)
        {
            if (!symMap.ContainsKey(name))
            {
                return 0;
            }

            return symMap[name].Count;
        }

        public bool Contains(string name)
        {
            return symMap.ContainsKey(name);
        }

        public SymbolMapper()
        {
            symMap = new Dictionary<string, List<T>>();
        }

        public void PushSymbol(T symbol)
        {
            PushSymbol(symbol, symbol.ToString());
        }

        public void PushSymbol(T symbol, string name)
        {
            if (!symMap.ContainsKey(name))
            {
                symMap[name] = new List<T>();
            }
            symMap[name].Insert(0, symbol);
        }

        /// <summary>
        /// If this map contains only one element, its value is returned, an exception is thrown otherwise.
        /// </summary>
        /// <returns></returns>
        public T GetInstanceValue()
        {
            if (symMap.Keys.Count == 1)
            {
                return this[symMap.Keys.First()];
            }
            else
            {
                throw new InvalidOperationException("To get a single value from this instance, this has to contain a single element.");
            }        
        }

        public List<T> GetList(string name)
        {
            if (!symMap.ContainsKey(name))
                return new List<T>();

            return symMap[name];
        }
   
    }


    public interface ISDTArgs<T>
    {
        ISymbolMapper<T> Values { get; }

        ISymbolMapper<EbnfToken> Tokens { get; }

        string ParsedCode { get; }

        int CodeLine { get; }
    }

    internal class SDTArgs<T> : ISDTArgs<T>
    {
        public SymbolMapper<T> Values { get; }

        public SymbolMapper<EbnfToken> Tokens { get; }

        public string ParsedCode { get; internal set; }

        ISymbolMapper<T> ISDTArgs<T>.Values => Values;

        ISymbolMapper<EbnfToken> ISDTArgs<T>.Tokens => Tokens;

        public int CodeLine { get; set; }

        public SDTArgs(SymbolMapper<T> values, SymbolMapper<EbnfToken> tokens, string resolvedExpression)
        {
            this.Values = values;
            this.Tokens = tokens;
            this.ParsedCode = resolvedExpression;
        }

        public SDTArgs()
        {
            this.Values = new SymbolMapper<T>();
            this.Tokens = new SymbolMapper<EbnfToken>();
            this.ParsedCode = string.Empty;
        }
    }

    public delegate T SDTranslator<T>(ISDTArgs<T> args);

    public static class SDT
    {
        public static SDTranslator<T> Null<T>()
        {
            return delegate(ISDTArgs<T> args) { return default(T); };
        }

    }
    
    
}
