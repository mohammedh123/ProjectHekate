using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHekate.Scripting
{
    public abstract class AbstractScriptRecord
    {
        public List<byte> Code { get; set; }

        public int Index { get; set; }

        protected AbstractScriptRecord()
        {
            Code = new List<byte>();
        }

        public void AppendCodeFromRecord(AbstractScriptRecord record)
        {
            Code.AddRange(record.Code);
        }
    }

    public class ProgramScriptRecord : AbstractScriptRecord
    {
        public IReadOnlyList<BulletUpdaterScriptRecord> BulletUpdaterScriptRecords { get; private set; }
        public IReadOnlyList<EmitterUpdaterScriptRecord> EmitterUpdaterScriptRecords { get; private set; }

        private readonly List<BulletUpdaterScriptRecord> _bulletUpdaterScriptRecords;
        private readonly Dictionary<string, int> _bulletUpdaterScriptRecordNameToIndex;
        private readonly List<EmitterUpdaterScriptRecord> _emitterUpdaterScriptRecords;
        private readonly Dictionary<string, int> _emitterUpdaterScriptRecordNameToIndex;

        public ProgramScriptRecord()
        {
            _bulletUpdaterScriptRecords = new List<BulletUpdaterScriptRecord>();
            _bulletUpdaterScriptRecordNameToIndex = new Dictionary<string, int>();

            _emitterUpdaterScriptRecords = new List<EmitterUpdaterScriptRecord>();
            _emitterUpdaterScriptRecordNameToIndex = new Dictionary<string, int>();

            BulletUpdaterScriptRecords = _bulletUpdaterScriptRecords.AsReadOnly();
            EmitterUpdaterScriptRecords = _emitterUpdaterScriptRecords.AsReadOnly();
        }

        /// <summary>
        /// Adds a bullet updater script record to the program script record.
        /// </summary>
        /// <param name="name">The name of the bullet updater</param>
        /// <param name="newRecord">The script record for the bullet updater</param>
        /// <returns>Returns the index of the bullet updater script record (also populates the Index property of the newRecord)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a bullet updater with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the bullet updater has already been added, but with a different name</exception>
        public int AddBulletUpdaterScriptRecord(string name, BulletUpdaterScriptRecord newRecord)
        {
            // TODO: make method thread-safe
            if(_bulletUpdaterScriptRecordNameToIndex.ContainsKey(name)) 
                throw new ArgumentException("A bullet updater with the name \"" + name + "\" already exists in this script.", "name");
            if(_bulletUpdaterScriptRecords.Contains(newRecord))
                throw new ArgumentException("This bullet updater script record has already been added, but with a different name.", "newRecord");

            _bulletUpdaterScriptRecords.Add(newRecord);
            newRecord.Index = _bulletUpdaterScriptRecords.Count;
            _bulletUpdaterScriptRecordNameToIndex[name] = newRecord.Index;

            return newRecord.Index;
        }

        /// <summary>
        /// Adds an emitter updater script record to the program script record.
        /// </summary>
        /// <param name="name">The name of the emitter updater</param>
        /// <param name="newRecord">The script record for the emitter updater</param>
        /// <returns>Returns the index of the emitter updater script record (also populates the Index property of the newRecord)</returns>
        /// <exception cref="System.ArgumentException">Thrown when a emitter updater with that name already exists</exception>
        /// <exception cref="System.ArgumentException">Thrown when the emitter updater has already been added, but with a different name</exception>
        public int AddEmitterUpdaterScriptRecord(string name, EmitterUpdaterScriptRecord newRecord)
        {
            // TODO: make method thread-safe
            if (_emitterUpdaterScriptRecordNameToIndex.ContainsKey(name))
                throw new ArgumentException("An emitter updater with the name \"" + name + "\" already exists in this script.", "name");
            if (_emitterUpdaterScriptRecords.Contains(newRecord))
                throw new ArgumentException("This emitter updater script record has already been added, but with a different name.", "newRecord");

            _emitterUpdaterScriptRecords.Add(newRecord);
            newRecord.Index = _emitterUpdaterScriptRecords.Count;
            _emitterUpdaterScriptRecordNameToIndex[name] = newRecord.Index;

            return newRecord.Index;
        }
    }

    public class BulletUpdaterScriptRecord : AbstractScriptRecord
    {
    }

    public class EmitterUpdaterScriptRecord : AbstractScriptRecord
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
