using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{
    public interface IScopeManager
    {
        void Add(CodeBlock scope);
        void Remove();
        CodeBlock GetCurrentScope();
    }

    class StackScopeManager : IScopeManager
    {
        private readonly Stack<CodeBlock> _scopeStack;


        public StackScopeManager()
        {
            _scopeStack = new Stack<CodeBlock>();
        }

        public void Add(CodeBlock scope)
        {
            _scopeStack.Push(scope);
        }

        public void Remove()
        {
            _scopeStack.Pop();
        }

        public CodeBlock GetCurrentScope()
        {
            return _scopeStack.Peek();
        }
    }
}
