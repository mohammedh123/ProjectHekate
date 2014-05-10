using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{

    public class HekateScriptVisitor : HekateBaseVisitor<BytecodeCompiler>
    {
        private BytecodeCompiler _compiler;

        public override BytecodeCompiler VisitScript(HekateParser.ScriptContext context)
        {
            // beginning of the script, create a new compiler that all children will use
            _compiler = new BytecodeCompiler();

            foreach (var child in context.children)
            {
                Visit(child);
            }

            return _compiler;
        }

        public override BytecodeCompiler VisitEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var bUpdaterRecord = new BulletUpdaterScriptRecord();

            var name = context.Identifier().GetText();

            foreach (var child in context.children) {
                Visit(child);
            }

            return 1;
        }

        public override BytecodeCompiler VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            var str = context.GetText();

            return str;
        }
    }
}
