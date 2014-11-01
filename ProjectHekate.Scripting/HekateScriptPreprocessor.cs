using System.Collections.Generic;
using System.Linq;
using ProjectHekate.Grammar;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting
{
    public class HekateScriptPreprocessor : HekateBaseListener
    {
        private readonly IVirtualMachine _vm;

        public HekateScriptPreprocessor(IVirtualMachine vm)
        {
            _vm = vm;
        }

        public override void EnterEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var paramNames = new List<string>();

            var paramList = context.formalParameters().formalParameterList();
            if (paramList != null) {
                var paramContexts = paramList.formalParameter();

                paramNames.AddRange(paramContexts.Select(fpc => fpc.NormalIdentifier().GetText()));
            }

            var name = context.NormalIdentifier().GetText();
            var eUpdaterCodeBlock = new EmitterUpdaterCodeScope(paramNames);

            _vm.AddEmitterUpdaterCodeScope(name, eUpdaterCodeBlock);
        }

        public override void EnterActionDeclaration(HekateParser.ActionDeclarationContext context)
        {
            var paramNames = new List<string>();

            var paramList = context.formalParameters().formalParameterList();
            if (paramList != null)
            {
                var paramContexts = paramList.formalParameter();

                paramNames.AddRange(paramContexts.Select(fpc => fpc.NormalIdentifier().GetText()));
            }

            var name = context.NormalIdentifier().GetText();
            var actionCodeScope = new ActionCodeScope(paramNames);

            _vm.AddActionCodeScope(name, actionCodeScope);
        }
    }
}
