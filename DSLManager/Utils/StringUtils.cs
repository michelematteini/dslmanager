using DSLManager.Ebnf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DSLManager.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// Format a strings with the given format and parameters multiple times, shifting the arg index by argRepeatCount and 
        /// concatenates all the results.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="argRepeatCount">Number of args used at each repetition.</param>
        /// <param name="args">List of all the args.</param>
        /// <returns></returns>
        public static string Repeat(this string format, int argRepeatCount, IList args)
        {
            return Repeat(format, argRepeatCount, args, -1);
        }

        /// <summary>
        /// Format a strings with the given format and parameters multiple times, shifting the arg index by argRepeatCount and 
        /// concatenates all the results.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="argRepeatCount">Number of args used at each repetition.</param>
        /// <param name="args">List of all the args.</param>
        /// <param name="condtionIndex">
        /// Index of the condition used to choose the format string.
        /// If used, the format string must be composed of two parts separated by 'R_END_ARGS' where the first if used if the condition is true.
        /// </param>
        /// <returns></returns>
        public static string Repeat(this string format, int argRepeatCount, IList args, int condtionIndex)
        {
            object[] fargs = new object[argRepeatCount];
            StringBuilder s = new StringBuilder();
            int nArgs = args.Count;
            string[] splitFormat = null;
            if (condtionIndex >= 0) splitFormat = format.Split(R_END_ARGS);

            for (int i = 0; i < nArgs / argRepeatCount; i++)
            {
                //prepare current instance args
                for (int ci = 0; ci < argRepeatCount; ci++)
                {
                    fargs[ci] = args[i * argRepeatCount + ci];
                }

                //format
                if (condtionIndex >= 0)
                {
                    if ((bool)fargs[condtionIndex])
                    {
                        s.AppendFormat(splitFormat[0], fargs);
                    }
                    else
                    {
                        s.AppendFormat(splitFormat[1], fargs);
                    }
                }
                else
                {
                    s.AppendFormat(format, fargs);
                }
            }

            return s.ToString();
        }


        private static string R_TOKEN = "#repeat";
		private static char R_END_ARGS = '#';
        private static int R_TOKEN_LEN = R_TOKEN.Length;
        private static string R_END_TOKEN = "#endrepeat";
        private static int R_END_TOKEN_LEN = R_END_TOKEN.Length;	

		/// <summary>
        /// Format a strings with the given format and parameters, in the same way as String.Format().
		/// Also include support to a 'repeat' processing where a block of the string is repeated and re-formatted
		/// multiple times, using a list of args specified as an object[] parameter. Syntax:
        /// #repeat &lt;list_index&gt; [, join] [, condition&lt;cond_index&gt;]
        /// &lt;string_to_repeat&gt;
		/// #endrepeat
        /// Specifying'join' concatenates all args using &lt;string_to_repeat&gt; as delimiter, instead of using it as a format.
        /// &lt;list_index&gt; is a list of objects specified with an Integer that identifies the parameter index to be used (must be an IList)
        /// or a combibation of many list where '&amp;' means concatenate and '|' means alternate between. 
        /// E.g. (3 | 4 | 5) &amp; 6 means alternate lists at index 3, 4, 5 and use list 6 when over.
        /// Using the condition arg (e.g. 'condition5') enables condition mode.
        /// When in condition mode, &lt;string_to_repeat&gt; must contain an '#' that split the format in two parts.
        /// If the specified &lt;cond_index&gt; is true the first part of the format is used, the last part is used otherwise.	
        /// Usage example: #repeat 8,condition3#format1{0}#format2{2}{1}#endrepeat
        /// Usage example (comma separation): #repeat 2,join#,#endrepeat
        /// </summary>
        /// <param name="format"></param>
        /// <param name="argRepeatCount"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string RFormat(this string format, params object[] args)
        {
            StringBuilder repFormat = new StringBuilder();
            string[] formatElems = format.Split(new string[]{R_TOKEN, R_END_TOKEN}, StringSplitOptions.None);

            for(int i = 0; i < formatElems.Length; i++)
            {
                if(i % 2 == 0)//format block
                {
                    repFormat.Append(formatElems[i]);
                }
                else// repeat block
                {
					//get repeat args and code
					int rblockIndex = formatElems[i].IndexOf(R_END_ARGS);
                    if (rblockIndex < 0) throw new ArgumentException(string.Format("Missing {0} after {1} command.", R_END_ARGS, R_TOKEN));
					string[] rArgs = formatElems[i].Substring(0, rblockIndex).Split(',');
					string rcode = formatElems[i].Substring(rblockIndex + 1).TrimStart();

					//process args
					bool modeJoin = false, condRepeat = false;
					IList argList = null;
                    int condIndex = -1;
					foreach(string repeatArg in rArgs)
					{
                        if(repeatArg.StartsWith("condition"))
                        {
                            condRepeat = true;
                            condIndex = int.Parse(repeatArg.Substring(9));
                            continue;
                        }
                        else if (repeatArg == "join")
                        {
                            modeJoin = true;
                        }
                        else // list index
                        {
                            argList = RepeatList.FromFormat(repeatArg, args);
                        }
					}
					
					if(argList.Count == 0) continue;
					
					/*process repeat block*/
                    if(modeJoin)
                    //join
                    {			
                        repFormat.Append(argList[0]);
                        for(int ai = 1; ai < argList.Count; ai++)
                        {
                            repFormat.Append(rcode);
                            repFormat.Append(argList[ai]);
                        }
                    }
                    else
                    //repeat format
                    {           
						int argCount = formatArgCount(rcode);
						rcode = rcode.Replace("{{", "{{{{");//formatted in 2 steps, double escapes
						rcode = rcode.Replace("}}", "}}}}");//formatted in 2 steps, double escapes
                        string repeatedCode = string.Empty;
                        if (condRepeat)
                        {
                            argCount = argCount < condIndex + 1 ? condIndex + 1 : argCount;
                            repeatedCode = rcode.Repeat(argCount, argList, condIndex);
                        }
                        else
                        {
                            repeatedCode = rcode.Repeat(argCount, argList);
                        }
                        repFormat.Append(repeatedCode.TrimEnd());
                    }
                }
            }

            return string.Format(repFormat.ToString(), args);
        }

		/// <summary>
		/// Load a template file (.tpl) from the project, apply RFormat and returns the string result.
		/// </summary>
		/// <param name="name">Physical name of the template file</param>
		public static string RTemplate(string name, params object[] args)
		{
			string code;
			if(templates == null) templates = new Dictionary<string, string>();
			if(templates.ContainsKey(name))
			{
				code = templates[name];
			}
			else
			{
				string[] tpl = Directory.GetFiles(DSLDir.Instance.SolutionRoot, name + ".*", SearchOption.AllDirectories);
				if (tpl.Length == 0) throw new FileNotFoundException();
				code = File.ReadAllText(tpl[0]);
				templates[name] = code;
			}
            return code.RFormat(args);
		}
		
		private static Dictionary<string, string> templates;
		public static void PreloadTemplates(string extension)
		{
			if(templates == null) templates = new Dictionary<string, string>();
            string[] tplPath = Directory.GetFiles(DSLDir.Instance.SolutionRoot, "*." + extension, SearchOption.AllDirectories);
			for(int i = 0; i < tplPath.Length; i++)
			{
				string tplName = Path.GetFileNameWithoutExtension(tplPath[i]);
				string tplCode = File.ReadAllText(tplPath[i]);
				templates[tplName] = tplCode;
			}
		}
		
        private static int formatArgCount(string format)
        {
            int i = 0;       
            for(int argIndex = 0; argIndex >= 0; i++) argIndex = format.IndexOf("{" + i + "}");
            return i - 1;
        }

        /// <summary>
        /// Delimits a string with ebnf string delimiters.
        /// </summary>
        /// <returns></returns>
        public static string ToStr(string s)
        {
            return EbnfParser.START_TERMINAL + s + EbnfParser.END_TERMINAL;
        }

        /// <summary>
        /// Split a string to lines, breaking at all possible line separators.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] ToLines(this string s)
        {
            return s.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Return the line index of a given position in a string.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="strIndex">The index of the character in the string for which the line index should be retrived.</param>
        /// <returns></returns>
        public static int LineIndexOf(this string s, int strIndex)
        {
            int line, lastNlCharIndex = s.IndexOf('\n', 0);
            for (line = 0; lastNlCharIndex >= 0 && lastNlCharIndex < strIndex; line++)
            {
                lastNlCharIndex = s.IndexOf('\n', ++lastNlCharIndex);
            }
            return line;
        }

        /// <summary>
        /// Removes comments from a source code string.
        /// </summary>
        /// <param name="code">The source code.</param>
        /// <param name="ilcToken">Inline comment prefix.</param>
        /// <param name="mlcStart">Multiline comment prefix.</param>
        /// <param name="mlcEnd">Multiline comment terminator.</param>
        /// <returns></returns>
        public static string RemoveComments(this string code, string ilcToken, string mlcStart, string mlcEnd)
        {
            int ilcIndex = ilcToken == string.Empty || ilcToken == null ? -1 : 0;
            int mlcIndex = mlcStart == string.Empty || mlcEnd == string.Empty || mlcStart == null || mlcEnd == null ? -1 : 0;

            if (ilcIndex < 0 && mlcIndex < 0) return code;

            int nextStart = 0;
            StringBuilder cleanCode = new StringBuilder();
            char[] lineTerm = new char[] { '\n', '\r' };

            while (nextStart < code.Length && (ilcIndex >= 0 || mlcIndex >= 0))
            {
                ilcIndex = ilcIndex < 0 ? -1 : code.IndexOf(ilcToken, nextStart);
                mlcIndex = mlcIndex < 0 ? -1 : code.IndexOf(mlcStart, nextStart);

                if (ilcIndex < 0 && mlcIndex < 0)
                //no more comments
                {
                    cleanCode.Append(code.Substring(nextStart, code.Length - nextStart));
                    break;
                }

                if (ilcIndex >= 0 && (mlcIndex < 0 || ilcIndex < mlcIndex))
                //remove inline comment
                {
                    cleanCode.Append(code.Substring(nextStart, ilcIndex - nextStart));
                    nextStart = code.IndexOfAny(lineTerm, ilcIndex + ilcToken.Length);
                }
                else
                //remove multiline comment
                {
                    cleanCode.Append(code.Substring(nextStart, mlcIndex - nextStart));
                    nextStart = code.IndexOf(mlcEnd, mlcIndex + mlcStart.Length) + mlcEnd.Length;
                }

                if (nextStart < 0) nextStart = code.Length;
            }

            return cleanCode.ToString();
        }

        public static string RemoveComments(this string code, string ilcToken)
        {
            return RemoveComments(code, ilcToken, string.Empty, string.Empty);
        }

        public static string RemoveComments(this string code, string mlcStart, string mlcEnd)
        {
            return RemoveComments(code, string.Empty, mlcStart, mlcEnd);
        }
    }

}
