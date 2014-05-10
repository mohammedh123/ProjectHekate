using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    public class ScriptRecord
    {
        public List<byte> Code { get; set; }

        public ScriptRecord()
        {
            Code = new List<byte>();
        }
    }

    public class BulletUpdaterScriptRecord : ScriptRecord
    {
    }

    public class EmitterUpdaterScriptRecord : ScriptRecord
    {
    }

    public class VirtualMachine
    {
        private readonly List<BulletUpdaterScriptRecord> _bulletUpdaterScriptRecords;


        public VirtualMachine()
        {
            _bulletUpdaterScriptRecords = new List<BulletUpdaterScriptRecord>();
        }
    }
}
