namespace ProjectHekate.Scripting
{
    public struct IdentifierRecord
    {
        private readonly string _name;
        private readonly int _index;
        
        public string Name => _name;

        public int Index => _index;

        public IdentifierRecord(string name, int index)
        {
            _name = name;
            _index = index;
        }
    }
}