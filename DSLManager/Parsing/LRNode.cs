using DSLManager.Ebnf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public interface ILRNode : IEquatable<ILRNode>
    {
        ItemSet Items { get; }

        int ID { get; }

        ILRNode this[EbnfExpression e] { get; }

        ICollection<EbnfExpression> AllTransitions { get; }
    }

    internal class LRNode : ILRNode
        {
            protected ItemSet items;
            protected int id;
            protected Dictionary<EbnfExpression, ILRNode> transitions;

            public LRNode(ItemSet items)
            {
                this.items = items;
                transitions = new Dictionary<EbnfExpression, ILRNode>();
            }

            public ItemSet Items
            {
                get
                {
                    return items;
                }
            }

            public int ID
            {
                get
                {
                    return id;
                }
                set
                {
                    this.id = value;
                }
            }

            public ILRNode this[EbnfExpression e]
            {
                get
                {
                    if (transitions.ContainsKey(e))
                    {
                        return transitions[e];
                    }
                    return null;
                }
                set
                {
                    AddTransition(e, value);
                }
            }

            public ICollection<EbnfExpression> AllTransitions
            {
                get
                {
                    return transitions.Keys;
                }
            }

            protected void AddTransition(EbnfExpression e, ILRNode nextState)
            {
                transitions[e] = nextState;
            }


            public override int GetHashCode()
            {
                return this.items.GetHashCode();
            }

            public string ToEbnf()
            {
                return "LR STATE " + id + ":\n" + this.items.ToString();
            }

            public static bool operator ==(LRNode s1, ILRNode s2)
            {
                return s1.Items == s2.Items;
            }

            public static bool operator !=(LRNode s1, ILRNode s2)
            {
                return s1.Items != s2.Items;
            }

            public override string ToString()
            {
                return this.ToEbnf();
            }

            public override bool Equals(object obj)
            {
                ILRNode other = obj as ILRNode;
                if (other == null) return false;
                return Equals(other);
            }

            public bool Equals(ILRNode other)
            {
                return this.items == other.Items;
            }
        }

}
