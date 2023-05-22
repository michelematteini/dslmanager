using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Utils
{
    /// <summary>
    /// A logical read-only indexeable list that can be used to aggregate other lists without materializing the actual elements.
    /// </summary>
    public class RepeatList : IList
    {
        private static RepeatListCompiler listCompiler;

        private List<IList> lists;
        private RepeatMode mode;       
        private int start, count;//used in sublists

        #region Operators

        public static RepeatList operator &(RepeatList l1, RepeatList l2) 
		{
            return mergeRList(l1, l2, RepeatMode.Concatenate);
		}
		
		public static RepeatList operator |(RepeatList l1, RepeatList l2) 
		{
            return mergeRList(l1, l2, RepeatMode.Alternate);
		}

		private static RepeatList mergeRList(RepeatList l1, RepeatList l2, RepeatMode defaultMode)
		{
			bool compatibleLists = true;
			compatibleLists = compatibleLists && (l1.Mergeable && l2.Mergeable);
			compatibleLists = compatibleLists && ((l1.Mode == defaultMode) || (l1.Mode == RepeatMode.List));
            compatibleLists = compatibleLists && ((l2.Mode == defaultMode) || (l2.Mode == RepeatMode.List));

            List<IList> mergeList = new List<IList>();
			if (compatibleLists)
			//merge
			{			
				mergeList.AddRange(l1.lists);
				mergeList.AddRange(l2.lists);	
			}
			else
			//just combine
			{
                mergeList.Add(l1);
                mergeList.Add(l2);
			}
            return new RepeatList(defaultMode, 0, 0, mergeList);
		}

        #endregion

        #region Constructors

        public RepeatList(IList list)
            : this(RepeatMode.List, 0, 0, new List<IList>(new IList[] { list }))
        {
        }

        public static RepeatList FromFormat(string format, object[] args)
        {
            int index = 0;
            if (int.TryParse(format, out index))
            {
                return new RepeatList((IList)args[index]);
            }

            if (listCompiler == null)
            {
                listCompiler = new RepeatListCompiler();
                listCompiler.Initialize();
            }
            listCompiler.Args = args;

            listCompiler.CompileProgram(format);
            Exception compileError = null;
            RepeatList result = (RepeatList)listCompiler.GetCompiledResults(out compileError);
            if (compileError != null) throw compileError;
            return result;
        }

        public static RepeatList Concatenate(params IList[] lists)
        {
            return new RepeatList(RepeatMode.Concatenate, 0, 0, new List<IList>(lists));
        }

        public static RepeatList Alternate(params IList[] lists)
        {
            return new RepeatList(RepeatMode.Alternate, 0, 0, new List<IList>(lists));
        }

        public static RepeatList SubList(IList list, int startIndex, int count)
        {
            return new RepeatList(RepeatMode.Sublist, startIndex, count, new List<IList>(new IList[] { list }));
        }

        public static RepeatList SubList(IList list, int startIndex)
        {
            return new RepeatList(RepeatMode.Sublist, startIndex, list.Count - startIndex, new List<IList>(new IList[] { list }));
        }

        public static RepeatList Repeat(object value, int times)
        {
            return new RepeatList(RepeatMode.Repeat, 0, times, new List<IList>(new IList[] { new object[] { value } }));
        }
		
		private RepeatList(RepeatMode mode, int start, int count, List<IList> lists)
        {
            this.lists = lists;
            this.mode = mode;
			Mergeable = true;
            this.start = start;
            this.count = count;
        }
		
        #endregion

        /// <summary>
		/// Gets or sets whether when a new RepeatList is created from this one, the result will just combine
		/// the two or more list arrays or create a new list that uses this as a normal list. If this flag is 
		/// true in all the combined list and these are of the same type, the resulting list will behave with the specified
		/// mode using all the underlying lists.
		/// </summary>
		public bool Mergeable
		{
			get;
			set;
		}

        public RepeatMode Mode
		{
			get { return this.mode; }
		}

        public object this[int index]
        {
            get
            {
                switch (mode)
                {
                    case RepeatMode.Concatenate:
                        calcSequentialIndex(index);
                        return lists[li][lindex];

                    case RepeatMode.Alternate:
                        return lists[index % lists.Count][index / lists.Count];

                    case RepeatMode.List:
                        return lists[0][index];

                    case RepeatMode.Sublist:
                        return lists[0][index + start];

                    case RepeatMode.Repeat:
                        return lists[0][0];

                    default:
                        throw new NotImplementedException();
                }
            }
            set
            {
                switch (mode)
                {
                    case RepeatMode.Concatenate:
                        calcSequentialIndex(index);
                        lists[li][lindex] = value;
                        break;
                    case RepeatMode.Alternate:
                        lists[index % lists.Count][index / lists.Count] = value;
                        break;
                    case RepeatMode.List:
                        lists[0][index] = value;
                        break;
                    case RepeatMode.Sublist:
                        lists[0][index + start] = value;
                        break;
                    case RepeatMode.Repeat:
                        lists[0][0] = value;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public int Count
        {
            get
            {
                if (mode == RepeatMode.List) return lists[0].Count;
                if (mode == RepeatMode.Repeat || mode == RepeatMode.Sublist) return this.count;
                if (lists.Count == 0) return 0;

                int minCount = lists[0].Count;
                int totCount = lists[0].Count;
                for (int i = 1; i < lists.Count; i++)
                {
                    minCount = minCount > lists[i].Count ? lists[i].Count : minCount;
                    totCount += lists[i].Count;
                }

                if (mode == RepeatMode.Alternate)
                {
                    return minCount * lists.Count;
                }
                else if (mode == RepeatMode.Concatenate)
                {
                    return totCount;
                }

                throw new NotImplementedException();
            }
        }

        //sequential access optimization
        private int li = 0, lzero = 0, lindex = 0;
        private void calcSequentialIndex(int index)
        {
            int lcount;
            lindex = index - lzero;
            if (lindex < 0)//move backward
            {
                while (lindex < 0)
                {
                    li--;
                    lcount = this.lists[li].Count;
                    lzero -= lcount;
                    lindex += lcount;
                }
            }
            else//move forward
            {
                lcount = this.lists[li].Count;
                while (lindex >= lcount)
                {
                    lzero += lcount;
                    lindex -= lcount;
                    li++;
                    lcount = this.lists[li].Count;
                }
            }
        }

        #region Not Implemented

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    public enum RepeatMode
    {
        //sequentialize all the arrays
        Concatenate,
        //pick an item from each array and loops
        Alternate,
		//used to specify that a single list has been encampsulated by the repeat list
		List,
        //Part of the original list with a start index and lenght
        Sublist,
        //Same value repeated N times
        Repeat
    }
}
