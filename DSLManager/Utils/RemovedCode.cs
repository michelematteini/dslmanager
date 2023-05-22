using DSLManager.Parsing;

namespace DSLManager.Utils
{
    /// <summary>
    /// A string wrapped to not be parsed automatically when used as SDT result.
    /// <para/> Can be casted from and to string.
    /// </summary>
    public class RemovedCode : ParsedCode<string>
    {
        public RemovedCode(string value) : base(string.Empty, value) { }

        public static explicit operator string(RemovedCode rs)
        {
            return rs.Value;
        }

        public static explicit operator RemovedCode(string s)
        {
            return new RemovedCode(s);
        }
    }
}
