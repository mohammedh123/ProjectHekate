using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Interpreter;
using Irony.Parsing;

namespace ProjectHekate.Scripting
{
    public class ScriptEngine
    {
        public void Run()
        {
            var grammar = new BulletGrammar();

            var filename = "example script.txt";
            var parser = new Parser(grammar);
            var text = File.ReadAllText(filename);
            var tree = parser.Parse(text, filename);
        }
    }
}
