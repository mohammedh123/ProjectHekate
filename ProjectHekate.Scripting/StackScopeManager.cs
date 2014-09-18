using System.Collections.Generic;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    class StackScopeManager : IScopeManager
    {
        private readonly Stack<CodeScope> _scopeStack;


        public StackScopeManager()
        {
            _scopeStack = new Stack<CodeScope>();
        }

        public void Add(CodeScope scope)
        {
            _scopeStack.Push(scope);
        }

        public void Remove()
        {
            _scopeStack.Pop();
        }

        public CodeScope GetCurrentScope()
        {
            return _scopeStack.Peek();
        }
    }
}
