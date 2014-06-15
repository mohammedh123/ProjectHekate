using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation.Interfaces
{
    public interface IScopeManager
    {
        void Add(CodeBlock scope);
        void Remove();
        CodeBlock GetCurrentScope();
    }
}