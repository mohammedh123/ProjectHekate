using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{

    public class HekateScriptVisitor : HekateBaseVisitor<AbstractScriptRecord>
    {
        private BytecodeCompiler _compiler;
        private VirtualMachine _virtualMachine;
        private ProgramScriptRecord _mainScriptRecord;
        private Stack<AbstractScriptRecord> _scopeStack; 

        public override AbstractScriptRecord VisitScript(HekateParser.ScriptContext context)
        {
            // beginning of the script, create a new compiler/vm that all children will use
            _compiler = new BytecodeCompiler();
            _virtualMachine = new VirtualMachine();
            _mainScriptRecord = new ProgramScriptRecord();
            _scopeStack = new Stack<AbstractScriptRecord>();

            foreach (var child in context.children)
            {
                // visit each child and append the code to the main record
                var childRecord = Visit(child);
                if(childRecord == null) throw new InvalidOperationException("A visit to a child resulted in a null return value; check the visitor and make sure it overrides " + child.GetType().Name + "\'s visit method.");
                _mainScriptRecord.AppendCodeFromRecord(Visit(child));
            }

            return _mainScriptRecord;
        }

        public override AbstractScriptRecord VisitEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.Identifier().GetText());
            var name = context.Identifier().GetText();


            var bUpdaterRecord = new BulletUpdaterScriptRecord(paramNames);
            _scopeStack.Push(bUpdaterRecord);
            foreach (var child in context.children) {
                bUpdaterRecord.AppendCodeFromRecord(Visit(child));
            }
            _scopeStack.Pop();

            // done, now add to the pool of bullet updater records
            _mainScriptRecord.AddBulletUpdaterScriptRecord(name, bUpdaterRecord);
            
            return bUpdaterRecord;
        }

        public override AbstractScriptRecord VisitBulletUpdaterDeclaration(HekateParser.BulletUpdaterDeclarationContext context)
        {
            return base.VisitBulletUpdaterDeclaration(context);
        }

        public override AbstractScriptRecord VisitFunctionDeclaration(HekateParser.FunctionDeclarationContext context)
        {
            return base.VisitFunctionDeclaration(context);
        }


        public override AbstractScriptRecord VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            return base.VisitExpressionStatement(context);
        }
    }
}
