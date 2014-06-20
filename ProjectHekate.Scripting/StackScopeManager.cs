using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
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
