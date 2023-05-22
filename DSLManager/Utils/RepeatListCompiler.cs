using DSLManager.Ebnf;
using DSLManager.Languages;
using DSLManager.Parsing;
using System.Collections;

namespace DSLManager.Utils
{
    public class RepeatListCompiler : SinglePassCompiler<RepeatList>
    {
        public object[] Args
        {
            get;
            set;
        }

        public RepeatListCompiler()
        {
            AddRule("Program ::= List", SDT_Program);
            AddRule("List ::= LIndex | LCat | LAlter | LPar", SDT_List);
            AddRule("LPar ::= ''('', List, '')''", SDT_LPar);
            AddRule("LIndex ::= #int", SDT_LIndex);
            AddRule("LCat ::= List, ''&'', List", SDT_LCat);
            AddRule("LAlter ::= List, ''|'', List", SDT_LAlter);
        }

        #region STDs

        private RepeatList SDT_Program(ISDTArgs<RepeatList> args)
        {
            return args.Values.GetInstanceValue();
        }

        private RepeatList SDT_List(ISDTArgs<RepeatList> args)
        {
            return args.Values.GetInstanceValue();
        }

        private RepeatList SDT_LPar(ISDTArgs<RepeatList> args)
        {
			RepeatList l = args.Values["List"];
			l.Mergeable = false;
            return l;
        }

        private RepeatList SDT_LIndex(ISDTArgs<RepeatList> args)
        {
            return new RepeatList((IList)Args[args.Tokens["#int"].IntValue]);
        }

        private RepeatList SDT_LCat(ISDTArgs<RepeatList> args)
        {
            RepeatList l1 = args.Values["List", 0], l2 = args.Values["List", 1];
            return l1 & l2;
        }

        private RepeatList SDT_LAlter(ISDTArgs<RepeatList> args)
        {
            RepeatList l1 = args.Values["List", 0], l2 = args.Values["List", 1];
            return l1 | l2;
        }

        #endregion

    }
}
