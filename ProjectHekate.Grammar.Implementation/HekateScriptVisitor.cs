﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Implementation
{
    public class HekateScriptVisitor : HekateBaseVisitor<CodeBlock>
    {
        private IVirtualMachine _virtualMachine;
        private Stack<CodeBlock> _scopeStack;

        #region Top-level constructs


        public override CodeBlock VisitScript(HekateParser.ScriptContext context)
        {
            // beginning of the script, create a new compiler/vm that all children will use
            _virtualMachine = new VirtualMachine();
            _scopeStack = new Stack<CodeBlock>();

            foreach (var child in context.children)
            {
                // visit each child and append the code to the main record
                var childRecord = Visit(child);
                if(childRecord == null) throw new InvalidOperationException("A visit to a child resulted in a null return value; check the visitor and make sure it overrides " + child.GetType().Name + "\'s visit method.");
            }

            return null; // TODO: WHAT THE FUCK?
        }

        public override CodeBlock VisitEmitterUpdaterDeclaration(HekateParser.EmitterUpdaterDeclarationContext context)
        {
            var paramContexts = context.formalParameters().formalParameterList().formalParameter();

            var paramNames = paramContexts.Select(fpc => fpc.Identifier().GetText());
            var name = context.Identifier().GetText();


            var bUpdaterCodeBlock = new BulletUpdaterCodeBlock(paramNames);
            _scopeStack.Push(bUpdaterCodeBlock);
            foreach (var child in context.children) {
                bUpdaterCodeBlock.Add(Visit(child));
            }
            _scopeStack.Pop();

            // done, now add to the pool of bullet updater records
            _virtualMachine.AddBulletUpdaterCodeBlock(name, bUpdaterCodeBlock);
            
            return bUpdaterCodeBlock;
        }

        public override CodeBlock VisitBulletUpdaterDeclaration(HekateParser.BulletUpdaterDeclarationContext context)
        {
            return base.VisitBulletUpdaterDeclaration(context);
        }

        public override CodeBlock VisitFunctionDeclaration(HekateParser.FunctionDeclarationContext context)
        {
            return base.VisitFunctionDeclaration(context);
        }


        #endregion

        public override CodeBlock VisitExpressionStatement(HekateParser.ExpressionStatementContext context)
        {
            return base.VisitExpressionStatement(context);
        }

        #region Expression constructs


        public override CodeBlock VisitAddAssignmentExpression(HekateParser.AddAssignmentExpressionContext context)
        {
            var code = new CodeBlock();
            var identifierName = context.Identifier().GetText();

            var currentScope = _scopeStack.Peek();
            // TODO: FINISH THE REST OF THIS
            code.Add(Instructions.Assign);
            
                    
            return null;
        }


        #endregion

        private bool IsIdentifierProperty(string identifierName)
        {
            return identifierName.Any() && identifierName[0] == '$';
        }
    }
}