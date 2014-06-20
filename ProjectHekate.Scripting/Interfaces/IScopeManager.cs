namespace ProjectHekate.Scripting.Interfaces
{
    public interface IScopeManager
    {
        void Add(CodeBlock scope);
        void Remove();
        CodeBlock GetCurrentScope();
    }
}