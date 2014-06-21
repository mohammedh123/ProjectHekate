namespace ProjectHekate.Scripting
{
    public struct IdentifierRecord
    {
        private readonly string _name;
        private readonly int _index;
        
        public string Name
        {
            get { return _name; }
        }

        public int Index
        {
            get { return _index; }
        }

        public IdentifierRecord(string name, int index)
        {
            _name = name;
            _index = index;
        }
    }
}