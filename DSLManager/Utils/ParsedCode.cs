using DSLManager.Parsing;

namespace DSLManager.Utils
{
    /// <summary>
    /// A string wrapped to be parsed as specified when used as SDT result, which then wraps a different value.
    /// </summary>
    public class ParsedCode<T> : IParsedCodeType
    {
        public T Value { get; set; }

        public string Code { get; protected set; }

        public ParsedCode(string code, T value)
        {
            Value = value;
            Code = code;
        }

        public string GetSrcCode()
        {
            return Code;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
