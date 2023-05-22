using System;

namespace DSLManager
{
    public static class DSLDebug
    {
        public enum MsgType
        {
            Log = 0,
            Success,
            Error,
            Failure,
            Warning,
            Info,
            Question,
            InfoProgress
        }

        /// <summary>
        /// Optionally set externally to retrieve the debug log of various compiling processes provided by the library.
        /// </summary>
        public static Action<string, MsgType> Output { get; set; }

        public static void Log(string msg, MsgType msgType)
        {
            if (Output != null) Output(msg, msgType);
        }

        public static void Log(string msg)
        {
            Log(msg, MsgType.Log);
        }

        public static void Log(MsgType msgType, string msg, params object[] args)
        {
            Log(string.Format(msg, args), msgType);
        }

    }
}
