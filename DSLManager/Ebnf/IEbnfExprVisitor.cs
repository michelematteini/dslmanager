using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Ebnf
{
    public interface IEbnfExprVisitor
    {
        void VisitVariable(Variable v);

        void VisitDefinitions(int exprCount);

        void VisitConcatenation(int exprCount);

        void VisitOptional();

        void VisitSequence(bool optional);

        void VisitGroup(); 

        void VisitToken(EbnfToken ebnfToken);
    }
}
