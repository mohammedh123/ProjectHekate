namespace ProjectHekate.Scripting.Interfaces
{
    public interface IScopeManager
    {
        void Add(CodeScope scope);
        void Remove();
        CodeScope GetCurrentScope();
    }
}