using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    public class RulePriority
    {
        public const int DEFAULT_PRIORITY = 1;
        public const int LOWEST_PRIORITY = -1000;

        #region static constructors

        public static RulePriority Default
        {
            get { return new RulePriority(DEFAULT_PRIORITY); }
        }

        public static RulePriority LowestPriority
        {
            get { return new RulePriority(LOWEST_PRIORITY); }
        }

        public static RulePriority ReduceOver(RulePriority other)
        {
            return new RulePriority(other.ShiftPriority, Math.Max(other.ReducePriority, other.ShiftPriority) + 1);
        }

        public static RulePriority ShiftOver(RulePriority other)
        {
            return new RulePriority(Math.Max(other.ReducePriority, other.ShiftPriority) + 1, other.ReducePriority);
        }

        public static RulePriority Override(RulePriority other)
        {
            return new RulePriority(Math.Max(other.ReducePriority, other.ShiftPriority) + 1);
        }

        #endregion

        /// <summary>
        /// On A reduce-reduce conflict, the rule in which this priority value is higher is chosen. 
        /// <para/> On a shift-reduce conflict in which this rule should be reduced, this value is compared with the ShiftPriority value from the other rule.
        /// </summary>
        public int ReducePriority { get; set; }

        /// <summary>
        /// On a shift-reduce conflict in which this rule should be shifted, this value is compared with the ReducePriority value from the other rule.
        /// </summary>
        public int ShiftPriority { get; set; }

        private RulePriority(int priority) : this(priority, priority)
        {
        }

        private RulePriority(int shift, int reduce)
        {
            ReducePriority = reduce;
            ShiftPriority = shift;
        }



    }
}
