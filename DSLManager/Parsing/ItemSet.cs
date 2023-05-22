using System;
using System.Collections.Generic;
using System.Linq;
using DSLManager.Ebnf;
using DSLManager.Languages;

namespace DSLManager.Parsing
{
    public class ItemSet : IEquatable<ItemSet>
    {
        private List<LR1Item> items;
        private string ebnfCache;

        public ItemSet()
        {
            this.items = new List<LR1Item>();
            ebnfCache = string.Empty;
        }

        public void AddRule(DerivationRule rule)
        {
            AddItem(new LR1Item(rule));
        }

        public void AddItem(LR1Item item)
        {
            items.Add(item);
            ebnfCache = string.Empty;
        }

        public void AddClosureItems(Grammar grammar, Dictionary<EbnfExpression, HashSet<EbnfToken>> firstSet)
        {
            Stack<LR1Item> unprocItems = new Stack<LR1Item>(items);//non-terminal symbols on which closure is not complete
            Dictionary<string, LR1Item> itemsMap = new Dictionary<string, LR1Item>();//hash set used to check that items are unique

            //algorithm initialization (add all existing items to the map)
            foreach (LR1Item item in items)
            {
                itemsMap[item.ToEbnfNoFollows()] = item;
            }

            //compute closure
            while (unprocItems.Count > 0)
            {
                LR1Item curItem = unprocItems.Pop();

                //skip items that doesnt extends the set
                if (!curItem.CanShift || !(curItem.CurrentSymbol is Variable))
                {
                    continue;
                }

                foreach(DerivationRule rule in grammar.GetRulesStartingWith(curItem.CurrentSymbol))
                //search the grammar for a matching item
                {
                    LR1Item newItem = new LR1Item(rule);
                    string itemHash = newItem.ToEbnfNoFollows();
                    bool isNewItem = !itemsMap.ContainsKey(itemHash);
                    if (!isNewItem)
                    //if already exists, just add follows to that one
                    {
                        newItem = itemsMap[itemHash];
                    }
                    else
                    //add the new item to this set otherwise 
                    {
                        AddItem(newItem);
                        itemsMap[itemHash] = newItem;
                    }

                    //compute follows
                    bool followsUpdated = false;
                    LR1Item nextItem = curItem.Shift();
                    if(nextItem.CanShift)
                    //pick last symbol first's as follows
                    {
                        //TODO: is this working only on normalized grammars?
                        followsUpdated = newItem.AddFollowsFrom(firstSet[nextItem.CurrentSymbol]);
                    }
                    else
                    //use the same follows of this set
                    {
                        followsUpdated = newItem.AddFollowsFrom(curItem);
                    }

                    if (isNewItem || followsUpdated)
                    //reprocessing for closure needed
                    {
                        unprocItems.Push(newItem);
                    }
                    
                }
            }//end while

        }

        public HashSet<EbnfExpression> GetShiftSymbols()
        {
            HashSet<EbnfExpression> shiftSym = new HashSet<EbnfExpression>();
            foreach (LR1Item item in items)
            {
                if(item.CanShift)
                {
                    shiftSym.Add(item.CurrentSymbol);
                }
            }

            return shiftSym;
        }

        public bool IsReducible
        {
            get
            {
                foreach (LR1Item item in items)
                {
                    if (!item.CanShift)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Match the possible reductions on this set with their position in the grammar, indicating for each follow which rule should be applied.
        /// </summary>
        /// <param name="matchingBehaviour">Behaviour if a Reduce-Reduce conflict is found</param>
        /// <returns>A [Terminal, int] dictionary in which each pair rappresents a follow terminal and the index of the reduce rule to apply.</returns>       
        public Dictionary<EbnfToken, int> MatchReductions(DerivationRule[] grammar, RulePriority[] priorities, out bool rrConflict)
        {
            Dictionary<EbnfToken, int> reduceSym = new Dictionary<EbnfToken, int>();
            Dictionary<EbnfToken, bool> conflictualSym = new Dictionary<EbnfToken, bool>();

            foreach (LR1Item item in items)
            {
                if (item.CanShift)
                    continue; // only reduceable rules are considered
                
                // search the rule in the given grammar
                int ruleIndex = Array.IndexOf(grammar, item.Rule);
                int rulePriority = priorities[ruleIndex].ReducePriority;

                // match follows to the found rule
                foreach (EbnfToken follow in item.Follows)
                {
                    int curReducePriority = RulePriority.LOWEST_PRIORITY;
                    if (reduceSym.ContainsKey(follow))
                        curReducePriority = priorities[reduceSym[follow]].ReducePriority;

                    // check and save reduce-reduce conflicts
                    conflictualSym[follow] = rulePriority == curReducePriority;

                    // override rule for this follow if an higher priority rule is found
                    if(priorities[ruleIndex].ReducePriority > curReducePriority)
                        reduceSym[follow] = ruleIndex;
                }
            }

            rrConflict = conflictualSym.Values.Any(rrConflictFound => rrConflictFound);
            return reduceSym;
        }

        /// <summary>
        /// Search this set for the items that could shift a given symbol and then find the rule indices corresponding to those items in a given grammar.
        /// </summary>
        public int[] MatchShiftOn(EbnfExpression shiftSym, DerivationRule[] grammar)
        {
            HashSet<int> shiftableRulesIndices = new HashSet<int>();
            foreach (LR1Item item in items)
            {
                if (!item.CanShift) continue;
                if (item.CurrentSymbol != shiftSym) continue;

                int ruleIndex = Array.IndexOf(grammar, item.Rule);
                shiftableRulesIndices.Add(ruleIndex);
            }

            return shiftableRulesIndices.ToArray();
        }

        public ItemSet ShiftOn(EbnfExpression shiftSym)
        {
            ItemSet shiftedSet = new ItemSet();
            foreach (LR1Item lri in items)
            {
                if (lri.CanShift && lri.CurrentSymbol == shiftSym)
                {
                    shiftedSet.AddItem(lri.Shift());
                }
            }

            return shiftedSet;
        }

        public string ToEbnf()
        {
            if (ebnfCache == string.Empty)
            {
                foreach (LR1Item item in items)
                {
                    if (ebnfCache != string.Empty) ebnfCache += "\n";
                    ebnfCache += item.ToEbnf();
                }
            }

            return ebnfCache;
        }
		
		public string ToEbnfNoFollows()
		{
			string ebnfnf = string.Empty;
			foreach (LR1Item item in items)
			{
				if (ebnfnf != string.Empty) ebnfnf += "\n";
				ebnfnf += item.ToEbnfNoFollows();
			}

            return ebnfnf;
		}

        public static bool operator ==(ItemSet set1, ItemSet set2)
        {
            if (object.ReferenceEquals(set1, null) ^ object.ReferenceEquals(set2, null))
                return false;
            if (object.ReferenceEquals(set1, null) && object.ReferenceEquals(set2, null))
                return true;

            return set1.ToEbnf() == set2.ToEbnf();
        }

        public static bool operator !=(ItemSet set1, ItemSet set2)
        {
            return !(set1 == set2);
        }

        public override string ToString()
        {
            return this.ToEbnf();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as ItemSet);
        }

        public override int GetHashCode()
        {
            return ToEbnf().GetHashCode();
        }

        public bool Equals(ItemSet other)
        {
            return this == other;
        }
    }
}
