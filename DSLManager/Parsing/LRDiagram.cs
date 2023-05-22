using DSLManager.Ebnf;
using DSLManager.Languages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSLManager.Parsing
{
    public partial class LRDiagram
    {
        private int nextStateId;
        private Grammar grammar;
        private RulePriority[] priorities;
        private Dictionary<int, LRNode> nodes;
        private LRNode root;
        private Dictionary<EbnfExpression, HashSet<EbnfToken>> firstSet;
        private int conflictCount;//counts conflicts during parsing-table generation

        public LRDiagram(DerivationRule[] rules, RulePriority[] priorities)
        {
            nodes = new Dictionary<int, LRNode>();
            nextStateId = 1;
            this.grammar = new Grammar(rules);
            this.priorities = priorities;
            this.firstSet = LRParser.ComputeFirstSets(grammar.Rules);
        }

        public LRDiagram(DerivationRule[] rules) : this(rules, Enumerable.Repeat(RulePriority.Default, rules.Length).ToArray()) { }

        public void InitializeFromRule(DerivationRule initialProduction)
        {
            ItemSet initSet = new ItemSet();
            LR1Item initProdItem = new LR1Item(initialProduction);
            initProdItem.AddFollow(EbnfToken.InstanceEndOfStream);
            initSet.AddItem(initProdItem);
            initSet.AddClosureItems(grammar, firstSet);
            root = new LRNode(initSet);
            root.ID = 0;
            nextStateId = 1;
            nodes.Clear();
            nodes[root.GetHashCode()] = root;
        }

        /// <summary>
        /// Add a node to the diagram, created from the given Itemset, 
        /// then add to the parent node a transition to that state on a given input expression.
        /// </summary>
        /// <param name="items">Itemset for the new node.</param>
        /// <param name="parent">Parent node, this must be already contained in the diagram.</param>
        /// <param name="onInput">Input expression that will activate the 'parent -> newNode' transition.</param>
        /// <returns>
        /// A reference to the new node. If another equivalent node had alrady been created, 
        /// no nodes will be added and the returned value will be 'null'.
        /// </returns>
        public ILRNode AddNode(ItemSet items, ILRNode parent, EbnfExpression onInput)
        {     
            if(!nodes.ContainsKey(parent.GetHashCode()))
            {
                throw new InvalidOperationException("The parent node must have already been added to the diagram.");
            }

            LRNode parentNode = nodes[parent.GetHashCode()];
            items.AddClosureItems(grammar, firstSet);
            if (nodes.ContainsKey(items.GetHashCode()))
            {
                parentNode[onInput] = nodes[items.GetHashCode()];
                return null;
            }
       
            LRNode s = new LRNode(items);
            s.ID = nextStateId;
            nodes[s.GetHashCode()] = s;
            nextStateId++;
            parentNode[onInput] = s;
            return s;
        }

        public int StateCount
        {
            get
            {
                return nextStateId;
            }
        }
		
        public ILRNode Root
        {
            get
            {
                return root;
            }
        }

        public void BuildDiagram()
        {
            buildSubTree(root);
        }

        public void CompressToLARL()
		{
            //1 - organize nodes in set of LR(0) equivalence
            Dictionary<int, int> lr0Equivalents = new Dictionary<int, int>();
            Dictionary<int, LRNode> lr0Nodes = new Dictionary<int, LRNode>();         
 
            foreach(int lr1Hash in nodes.Keys) 
            {
                int lr0Hash = nodes[lr1Hash].Items.ToEbnfNoFollows().GetHashCode();
                lr0Equivalents.Add(lr1Hash, lr0Hash);
                lr0Nodes.Add(lr0Hash, null);
            }
			
            //2 - generate LALR nodes for each set
            //TODO
			
			
			
            //3 - build new LALR node list generating LALR hash
            Dictionary<int, LRNode> lalrNodes = new Dictionary<int, LRNode>();
            foreach(LRNode lalrNode in lr0Nodes.Values)
            {
                lalrNodes.Add(lalrNode.GetHashCode(), lalrNode);
            }
			
            //4 - build LALR diagram with new LALR nodes, using the old diagram
            //TODO
			
			
            //save new nodes
            this.nodes = lalrNodes;
		}

        
        public ParseTable ToParseTable()
        {
            ParseTable pt = new ParseTable();
            HashSet<ILRNode> computedNodes = new HashSet<ILRNode>();
            conflictCount = 0;
            addNodeToTable(root, pt, computedNodes);
            //add final accept action
            pt[0, EbnfToken.InstanceEndOfStream] = new LRAction(LRActionType.Accept, 0);
            if(conflictCount > 0)
                DSLDebug.Log("Found " + conflictCount + " conflicts.", DSLDebug.MsgType.Warning);

            return pt;
        }

        private void addNodeToTable(ILRNode node, ParseTable pt, HashSet<ILRNode> computedNodes)
        {
            computedNodes.Add(node);//add current node to the computed ones

            // compute reductions
            Dictionary<EbnfToken, int> reductions;
            bool rrConflict;
            reductions = node.Items.MatchReductions(grammar.Rules, priorities, out rrConflict);
            if (rrConflict)
            {
                DSLDebug.Log("Reduce/Reduce conflict detected:", DSLDebug.MsgType.Warning);
                DSLDebug.Log(node.ToString(), DSLDebug.MsgType.Warning);
                conflictCount++;
            }

            // compile shift and GOTO
            ICollection<EbnfExpression> transitions = node.AllTransitions;
            bool conflictPrinted = false;
            foreach (EbnfExpression next in transitions)
            {
                if (next is Variable)
                {
                    pt[node.ID, next] = new LRAction(LRActionType.Goto, node[next].ID);
                }
                else if (next is EbnfToken nextToken)
                {               
                    if (reductions.ContainsKey((EbnfToken)next))
					//shift/reduce conflict, how to behave?
                    {
                        // retrieve shift and reduce priorities
                        int reducePriority = priorities[reductions[nextToken]].ReducePriority;
                        int maxShiftPriority = RulePriority.LOWEST_PRIORITY;
                        int maxShiftRule = -1;
                        foreach (int shiftableRuleIndex in node.Items.MatchShiftOn(next, grammar.Rules))
                        {
                            if (maxShiftRule < 0 || maxShiftPriority < priorities[shiftableRuleIndex].ShiftPriority)
                            {
                                maxShiftPriority = priorities[shiftableRuleIndex].ShiftPriority;
                                maxShiftRule = shiftableRuleIndex;
                            }
                        }

                        // check for conflicts
                        if (!conflictPrinted && reducePriority == maxShiftPriority)
                        {
                            conflictPrinted = true;
                            conflictCount++;
                            DSLDebug.Log(DSLDebug.MsgType.Warning, "Shift/Reduce conflict detected(Reduce: {0} and Shift for rule: {1} have the same priority ({2})):", grammar.Rules[reductions[nextToken]].Variable, grammar.Rules[maxShiftRule].Variable, reducePriority);
                            DSLDebug.Log(node.ToString(), DSLDebug.MsgType.Warning);
                        }

                        // remove reductions that don't reach enough priority against shifts (or they will override the shifts by default)
                        if(reducePriority <= maxShiftPriority)
                            reductions.Remove((EbnfToken)next);                       
                    }

                    pt[node.ID, next] = new LRAction(LRActionType.Shift, node[next].ID);
                }

                // propagate to other nodes in the diagram
                if (!computedNodes.Contains(node[next]))
                {
                    addNodeToTable(node[next], pt, computedNodes);
                }
            }

            // compile reductions
            foreach (EbnfToken follow in reductions.Keys)
            {
                pt[node.ID, follow] = new LRAction(LRActionType.Reduce, reductions[follow]);
            }
        }

        private const int maxToStringNodeLen = 16;
        public override string ToString()
        {
            string result = "";
            int i = 0;
            Dictionary<int, LRNode>.ValueCollection allNodes = nodes.Values;
            foreach (LRNode node in allNodes)
            {
                result += node.ToString() + "\n";
                i++;
                if (i >= maxToStringNodeLen)
                {
                    result += "[and other " + (this.StateCount - maxToStringNodeLen) + " states] \n";
                    break;
                }
            }
            return result;
        }

        public string ToFullString()
        {
            string result = "";
            Dictionary<int, LRNode>.ValueCollection allNodes = nodes.Values;
            foreach (LRNode node in allNodes)
            {
                result += node.ToString() + "\n\n";
            }
            return result;
        }
        
        private void buildSubTree(ILRNode root)
        {
            //add all shift branches
            HashSet<EbnfExpression> shiftSymbols = root.Items.GetShiftSymbols();
            foreach (EbnfExpression se in shiftSymbols)
            {
                ILRNode nextNode = AddNode(root.Items.ShiftOn(se), root, se);
                if (nextNode != null)//nextNode == null -> node already exist
                {
                    buildSubTree(nextNode);
                }
            }
        }
        
    }

}
