using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoMoq.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectHekate.Grammar;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Scripting.Tests
{
    [TestClass]
    public class THekateScriptVisitor : AutoMoqTestFixture<HekateScriptVisitor>
    {
        protected const string WrappedProgramStringUnfmted = "function main(){{{0};}}";

        protected IVirtualMachine MockVirtualMachine => Mocker.GetMock<IVirtualMachine>().Object;

        protected IScopeManager MockScopeManager => Mocker.GetMock<IScopeManager>().Object;


        protected virtual TContextType GetFirstContext<TContextType>(string expression, bool wrappedInFunctionString = true) where TContextType : class, IParseTree
        {
            return GetNthContext<TContextType>(expression, 0, wrappedInFunctionString);
        }

        protected virtual TContextType GetNthContext<TContextType>(string expression, int n, bool wrappedInFunctionString) where TContextType : class, IParseTree
        {
            return GetNthContext<TContextType>(expression, n, wrappedInFunctionString, WrappedProgramStringUnfmted);
        }

        protected virtual TContextType GetNthContext<TContextType>(string expression, int n, bool wrappedFormatString, string formatString) where TContextType : class, IParseTree
        {
            var lexer = new HekateLexer(new AntlrInputStream(wrappedFormatString ? String.Format(formatString, expression) : expression));
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);

            var tree = parser.script();

            return tree.GetDescendantsOfType<TContextType>()
                .Skip(n)
                .FirstOrDefault();
        }

        protected virtual TContextType GetNthContext<TContextType>(string entireScript, int n) where TContextType : class, IParseTree
        {
            var lexer = new HekateLexer(new AntlrInputStream(entireScript));
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);

            var tree = parser.script();

            return tree.GetDescendantsOfType<TContextType>()
                .Skip(n)
                .FirstOrDefault();
        }
        
        protected virtual void SetUpGetCurrentScope(CodeScope scope)
        {
            Mocker.GetMock<IScopeManager>()
                .Setup(ism => ism.GetCurrentScope())
                .Returns(scope);
        }

        [TestInitialize]
        public void InitializeSubject()
        {
            ResetSubject();
        }

        [TestClass]
        public class VisitVariableDeclaration : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForVariableDeclaration()
            {
                // Setup: dummy data
                const string expression = "var someIdentifier = 1.35";

                SetUpGetCurrentScope(new CodeScope());

                // Act
                var result = Subject.VisitVariableDeclaration(GetFirstContext<HekateParser.VariableDeclarationContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(5);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1.35f);
                result.Code[2].Should().Be((byte)Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenVariableNameConflictsWithExistingSymbol()
            {
                // Setup: dummy data
                const string expression = @"var someIdentifier = 6;";

                var scope = new CodeScope();
                scope.AddSymbol("someIdentifier", SymbolType.Numerical);
                SetUpGetCurrentScope(scope);
                
                // Act+Verify
                Subject.Invoking(hsv => hsv.VisitVariableDeclaration(GetFirstContext<HekateParser.VariableDeclarationContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenVariableNameConflictsWithGlobalSymbol()
            {
                // Setup: dummy data
                const string expression = "var someIdentifier = 1.35";

                SetUpGetCurrentScope(new CodeScope());
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(igsc => igsc.HasGlobalSymbolDefined("someIdentifier"))
                    .Returns(true);

                // Act+Verify
                Subject.Invoking(hsv => hsv.VisitVariableDeclaration(GetFirstContext<HekateParser.VariableDeclarationContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }
        }

        [TestClass]
        public class VisitReturnStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForReturnStatement()
            {
                // Setup: dummy data
                const string expression = "return 3;";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitReturnStatement(GetFirstContext<HekateParser.ReturnStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(3);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(3);
                result.Code[2].Should().Be((byte) Instruction.Return);
            }
        }

        [TestClass]
        public class VisitIfStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForEmptyIfStatement()
            {
                // Setup: dummy data
                const string expression = "if(1) {}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitIfStatement(GetFirstContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(4);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(4);
            }

            [TestMethod]
            public void ShouldGenerateCodeForIfStatement()
            {
                // Setup: dummy data
                const string expression = @"if(1) {
    3;                                          
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitIfStatement(GetFirstContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(7);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
            }

            [TestMethod]
            public void ShouldGenerateCodeForIfWithElseStatement()
            {
                // Setup: dummy data
                const string expression = @"if(1) {
    3;                                          
}
else {
    4;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitIfStatement(GetFirstContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(12);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(9);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
                result.Code[7].Should().Be((byte) Instruction.Jump);
                result.Code[8].Should().Be(12);
                result.Code[9].Should().Be((byte) Instruction.Push);
                result.Code[10].Should().Be(4);
                result.Code[11].Should().Be((byte) Instruction.Pop);
            }

            [TestMethod]
            public void ShouldGenerateCodeForComplexIfWithElseStatement()
            {
                // Setup: dummy data
                const string expression = @"if(1 % 2 == 0) {
    3;                                          
}
else {
    4;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitIfStatement(GetFirstContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(18);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(2);
                result.Code[4].Should().Be((byte)Instruction.OpMod);
                result.Code[5].Should().Be((byte)Instruction.Push);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte)Instruction.OpEqual);
                result.Code[8].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[9].Should().Be(15);
                result.Code[10].Should().Be((byte)Instruction.Push);
                result.Code[11].Should().Be(3);
                result.Code[12].Should().Be((byte)Instruction.Pop);
                result.Code[13].Should().Be((byte)Instruction.Jump);
                result.Code[14].Should().Be(18);
                result.Code[15].Should().Be((byte)Instruction.Push);
                result.Code[16].Should().Be(4);
                result.Code[17].Should().Be((byte)Instruction.Pop);
            }

            [TestMethod]
            public void ShouldGenerateCodeForIfWithElseIfAndElseStatement()
            {
                // Setup: dummy data
                const string expression = @"if(1) {
    3;                                          
}
else if(2) {
    4;
}
else {
    5;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitIfStatement(GetFirstContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(21);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(9);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
                result.Code[7].Should().Be((byte) Instruction.Jump);
                result.Code[8].Should().Be(21);
                result.Code[9].Should().Be((byte) Instruction.Push);
                result.Code[10].Should().Be(2);
                result.Code[11].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[12].Should().Be(18);
                result.Code[13].Should().Be((byte) Instruction.Push);
                result.Code[14].Should().Be(4);
                result.Code[15].Should().Be((byte) Instruction.Pop);
                result.Code[16].Should().Be((byte) Instruction.Jump);
                result.Code[17].Should().Be(21);
                result.Code[18].Should().Be((byte) Instruction.Push);
                result.Code[19].Should().Be(5);
                result.Code[20].Should().Be((byte) Instruction.Pop);
            }
        }

        [TestClass]
        public class VisitForStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForEmptyForStatement()
            {
                // Setup: dummy data
                const string expression = @"for(;;) {}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte) Instruction.Jump);
                result.Code[1].Should().Be(0);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEmptyForStatementButWithInit()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 23;;) {}";
                SetUpGetCurrentScope(new CodeScope());

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(23);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte) Instruction.Jump);
                result.Code[6].Should().Be(5);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEmptyForStatementButWithInitAndExpression()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10;) {}";
                SetUpGetCurrentScope(new CodeScope());

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(14);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte) Instruction.GetVariable);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte) Instruction.Push);
                result.Code[8].Should().Be(10);
                result.Code[9].Should().Be((byte)Instruction.OpLessThan);
                result.Code[10].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[11].Should().Be(14);
                result.Code[12].Should().Be((byte) Instruction.Jump);
                result.Code[13].Should().Be(5);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEmptyForStatementButWithInitExpressionAndUpdate()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10; i++) {}";
                SetUpGetCurrentScope(new CodeScope());

                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(22);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte) Instruction.GetVariable);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte) Instruction.Push);
                result.Code[8].Should().Be(10);
                result.Code[9].Should().Be((byte)Instruction.OpLessThan);
                result.Code[10].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[11].Should().Be(22);
                result.Code[12].Should().Be((byte) Instruction.GetVariable);
                result.Code[13].Should().Be(0);
                result.Code[14].Should().Be((byte) Instruction.Push);
                result.Code[15].Should().Be(1);
                result.Code[16].Should().Be((byte) Instruction.OpAdd);
                result.Code[17].Should().Be((byte) Instruction.SetVariable);
                result.Code[18].Should().Be(0);
                result.Code[19].Should().Be((byte)Instruction.Pop);
                result.Code[20].Should().Be((byte) Instruction.Jump);
                result.Code[21].Should().Be(5);
            }

            [TestMethod]
            public void ShouldGenerateCodeForForStatementWithBreak()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10; i++) {
    break;
}";
                SetUpGetCurrentScope(new CodeScope());

                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(24);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte)Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte)Instruction.GetVariable);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte)Instruction.Push);
                result.Code[8].Should().Be(10);
                result.Code[9].Should().Be((byte)Instruction.OpLessThan);
                result.Code[10].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[11].Should().Be(24);
                result.Code[12].Should().Be((byte)Instruction.GetVariable);
                result.Code[13].Should().Be(0);
                result.Code[14].Should().Be((byte)Instruction.Push);
                result.Code[15].Should().Be(1);
                result.Code[16].Should().Be((byte)Instruction.OpAdd);
                result.Code[17].Should().Be((byte)Instruction.SetVariable);
                result.Code[18].Should().Be(0);
                result.Code[19].Should().Be((byte)Instruction.Pop);
                result.Code[20].Should().Be((byte)Instruction.Jump);
                result.Code[21].Should().Be(24);
                result.Code[22].Should().Be((byte)Instruction.Jump);
                result.Code[23].Should().Be(5);
            }

            [TestMethod]
            public void ShouldGenerateCodeForForStatementWithContinue()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10; i++) {
    continue;
}";
                SetUpGetCurrentScope(new CodeScope());

                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(24);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte)Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte)Instruction.GetVariable);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte)Instruction.Push);
                result.Code[8].Should().Be(10);
                result.Code[9].Should().Be((byte)Instruction.OpLessThan);
                result.Code[10].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[11].Should().Be(24);
                result.Code[12].Should().Be((byte)Instruction.GetVariable);
                result.Code[13].Should().Be(0);
                result.Code[14].Should().Be((byte)Instruction.Push);
                result.Code[15].Should().Be(1);
                result.Code[16].Should().Be((byte)Instruction.OpAdd);
                result.Code[17].Should().Be((byte)Instruction.SetVariable);
                result.Code[18].Should().Be(0);
                result.Code[19].Should().Be((byte)Instruction.Pop);
                result.Code[20].Should().Be((byte)Instruction.Jump);
                result.Code[21].Should().Be(5);
                result.Code[22].Should().Be((byte)Instruction.Jump);
                result.Code[23].Should().Be(5);
            }

            [TestMethod]
            public void ShouldGenerateCodeForCompleteForStatement()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10; i++) {
    3;
}";
                SetUpGetCurrentScope(new CodeScope());

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GetFirstContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(25);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte)Instruction.Pop);
                result.Code[5].Should().Be((byte) Instruction.GetVariable);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte) Instruction.Push);
                result.Code[8].Should().Be(10);
                result.Code[9].Should().Be((byte) Instruction.OpLessThan);
                result.Code[10].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[11].Should().Be(25);
                result.Code[12].Should().Be((byte) Instruction.GetVariable);
                result.Code[13].Should().Be(0);
                result.Code[14].Should().Be((byte) Instruction.Push);
                result.Code[15].Should().Be(1);
                result.Code[16].Should().Be((byte) Instruction.OpAdd);
                result.Code[17].Should().Be((byte) Instruction.SetVariable);
                result.Code[18].Should().Be(0);
                result.Code[19].Should().Be((byte)Instruction.Pop);
                result.Code[20].Should().Be((byte) Instruction.Push);
                result.Code[21].Should().Be(3);
                result.Code[22].Should().Be((byte) Instruction.Pop);
                result.Code[23].Should().Be((byte) Instruction.Jump);
                result.Code[24].Should().Be(5);
            }
        }

        [TestClass]
        public class VisitWhileStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForEmptyWhileStatement()
            {
                // Setup: dummy data
                const string expression = @"while(1) {}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitWhileStatement(GetFirstContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(6);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(6);
                result.Code[4].Should().Be((byte) Instruction.Jump);
                result.Code[5].Should().Be(0);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSimpleWhileStatement()
            {
                // Setup: dummy data
                const string expression = @"while(1) {
    3;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitWhileStatement(GetFirstContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(9);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.IfZeroBranch);
                result.Code[3].Should().Be(9);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
                result.Code[7].Should().Be((byte) Instruction.Jump);
                result.Code[8].Should().Be(0);
            }

            [TestMethod]
            public void ShouldGenerateCodeForWhileWithBreakStatement()
            {
                // Setup: dummy data
                const string expression = @"while(1) {
    3;
    break;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitWhileStatement(GetFirstContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(11);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[3].Should().Be(11);
                result.Code[4].Should().Be((byte)Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte)Instruction.Pop);
                result.Code[7].Should().Be((byte)Instruction.Jump);
                result.Code[8].Should().Be(11);
                result.Code[9].Should().Be((byte)Instruction.Jump);
                result.Code[10].Should().Be(0);
            }

            [TestMethod]
            public void ShouldGenerateCodeForWhileWithContinueStatement()
            {
                // Setup: dummy data
                const string expression = @"while(1) {
    3;
    continue;
}";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitWhileStatement(GetFirstContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(11);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.IfZeroBranch);
                result.Code[3].Should().Be(11);
                result.Code[4].Should().Be((byte)Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte)Instruction.Pop);
                result.Code[7].Should().Be((byte)Instruction.Jump);
                result.Code[8].Should().Be(0);
                result.Code[9].Should().Be((byte)Instruction.Jump);
                result.Code[10].Should().Be(0);
            }
        }

        [TestClass]
        public class VisitBreakStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForBreakStatementBecauseOfDummyJumpValue()
            {
                // Setup: dummy data
                const string expression = @"break;";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitBreakStatement(GetFirstContext<HekateParser.BreakStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte)Instruction.Jump);
                result.Code[1].Should().Be(0);
            }
        }

        [TestClass]
        public class VisitContinueStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForContinueStatementBecauseOfDummyJumpValue()
            {
                // Setup: dummy data
                const string expression = @"continue;";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitContinueStatement(GetFirstContext<HekateParser.ContinueStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte)Instruction.Jump);
                result.Code[1].Should().Be(0);
            }
        }

        [TestClass]
        public class VisitWaitStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForWaitStatement()
            {
                // Setup: dummy data
                const string expression = @"wait 1 frames;";

                // Act
                var result = new CodeBlock();
                Subject
                    .VisitWaitStatement(GetFirstContext<HekateParser.WaitStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(3);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.WaitFrames);
            }
        }

        [TestClass]
        public class VisitLiteralExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForIntegerLiteral()
            {
                // Setup
                const int literal = 3;
                string literalExpression = literal.ToString();

                // Act
                var result = Subject.VisitLiteralExpression(GetFirstContext<HekateParser.LiteralExpressionContext>(literalExpression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(literal);
            }

            [TestMethod]
            public void ShouldThrowExceptionForIntegerOverflow()
            {
                // Setup
                const string literalExpression = "9999999999999999999999999999999999999999999999999999999999999999";

                // Act+Verify
                Subject
                    .Invoking(hsv => hsv.VisitLiteralExpression(GetFirstContext<HekateParser.LiteralExpressionContext>(literalExpression))
                        .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<OverflowException>();
            }

            [TestMethod]
            public void ShouldGenerateCodeForFloatLiteral()
            {
                // Setup
                const float literal = 3.455f;
                string literalExpression = literal.ToString();

                // Act
                var result = Subject.VisitLiteralExpression(GetFirstContext<HekateParser.LiteralExpressionContext>(literalExpression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(literal);
            }

            [TestMethod]
            public void ShouldThrowExceptionForFloatOverflow()
            {
                // Setup
                const string literalExpression = "999999999999999999999999999999999999999999999.99999999999999999999999999999999";

                // Act+Verify
                Subject
                    .Invoking(hsv => hsv.VisitLiteralExpression(GetFirstContext<HekateParser.LiteralExpressionContext>(literalExpression))
                        .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<OverflowException>();
            }
        }

        [TestClass]
        public class VisitNormalIdentifierExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForMatchingVariable()
            {
                // Setup: create code scope with existing numerical variable, mock scope out
                const string identifier = "someIdentifier";
                var expression = String.Format("{0}", identifier);
                var codeScope = new CodeScope();
                var idx = codeScope.AddSymbol(identifier, SymbolType.Numerical);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitNormalIdentifierExpression(
                    GetFirstContext<HekateParser.NormalIdentifierExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code[0].Should().Be((byte) Instruction.GetVariable);
                result.Code[1].Should().Be(idx);
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionForNonMatchingVariable()
            {
                // Setup: create code scope, mock scope out
                const string identifier = "someIdentifier";
                var expression = String.Format("{0}", identifier);
                var codeScope = new CodeScope();
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                Subject.Invoking(
                    hsv => hsv.VisitNormalIdentifierExpression(GetFirstContext<HekateParser.NormalIdentifierExpressionContext>(expression))
                        .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }
        }

        [TestClass]
        public class VisitPropertyIdentifierExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForMatchingProperty()
            {
                // Setup: create code scope with existing property, mock vm out
                const string identifier = "$SomeIdentifier";
                var expression = String.Format("{0}", identifier);
                var dummyRecord = new IdentifierRecord(identifier, 0);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetPropertyIndex(identifier))
                    .Returns(dummyRecord.Index);

                // Act
                var result = Subject.VisitPropertyIdentifierExpression(
                    GetFirstContext<HekateParser.PropertyIdentifierExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code[0].Should().Be((byte) Instruction.GetProperty);
                result.Code[1].Should().Be(dummyRecord.Index);
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionForNonMatchingVariable()
            {
                // Setup: create code scope, mock vm out
                const string identifier = "SomeIdentifier";
                var expression = String.Format("${0}", identifier);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetPropertyIndex(identifier))
                    .Throws<ArgumentException>();

                // Act + Verify
                Subject.Invoking(
                    hsv =>
                        hsv.VisitPropertyIdentifierExpression(GetFirstContext<HekateParser.PropertyIdentifierExpressionContext>(expression))
                            .Generate(MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }
        }

        [TestClass]
        public class VisitUnaryExpression : THekateScriptVisitor
        {
            private void TestCodeGenerationForOperator(string operatorString, Instruction op)
            {
                // Setup: dummy data
                const int value = 1;
                var expression = String.Format("{0}{1}", operatorString, value);

                // Act
                var result = Subject.VisitUnaryExpression(GetFirstContext<HekateParser.UnaryExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(3);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(value);
                result.Code[2].Should().Be((byte) op);
            }

            [TestMethod]
            public void ShouldGenerateCodeForConditionalNot()
            {
                TestCodeGenerationForOperator("!", Instruction.OpNot);
            }

            [TestMethod]
            public void ShouldGenerateCodeForNegativeNumber()
            {
                TestCodeGenerationForOperator("-", Instruction.Negate);
            }
        }

        [TestClass]
        public class VisitBinaryExpression : THekateScriptVisitor
        {
            private void TestCodeGenerationForOperator(string operatorString, Instruction op)
            {
                // Setup: dummy data
                const int left = 1;
                const float right = 0;
                var expression = String.Format("{0}{1}{2}", left, operatorString, right);

                // Act
                var result = Subject.VisitBinaryExpression(GetFirstContext<HekateParser.BinaryExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(5);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(left);
                result.Code[2].Should().Be((byte) Instruction.Push);
                result.Code[3].Should().Be(right);
                result.Code[4].Should().Be((byte) op);
            }

            [TestMethod]
            public void ShouldGenerateCodeForMultiplication()
            {
                TestCodeGenerationForOperator("*", Instruction.OpMultiply);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDivision()
            {
                TestCodeGenerationForOperator("/", Instruction.OpDivide);
            }

            [TestMethod]
            public void ShouldGenerateCodeForModulus()
            {
                TestCodeGenerationForOperator("%", Instruction.OpMod);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAddition()
            {
                TestCodeGenerationForOperator("+", Instruction.OpAdd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSubtraction()
            {
                TestCodeGenerationForOperator("-", Instruction.OpSubtract);
            }

            [TestMethod]
            public void ShouldGenerateCodeForLessThan()
            {
                TestCodeGenerationForOperator("<", Instruction.OpLessThan);
            }

            [TestMethod]
            public void ShouldGenerateCodeForGreaterThan()
            {
                TestCodeGenerationForOperator(">", Instruction.OpGreaterThan);
            }

            [TestMethod]
            public void ShouldGenerateCodeForLessThanEqual()
            {
                TestCodeGenerationForOperator("<=", Instruction.OpLessThanEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForGreaterThanEqual()
            {
                TestCodeGenerationForOperator(">=", Instruction.OpGreaterThanEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEquality()
            {
                TestCodeGenerationForOperator("==", Instruction.OpEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForInequality()
            {
                TestCodeGenerationForOperator("!=", Instruction.OpNotEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForConditionalAnd()
            {
                TestCodeGenerationForOperator("&&", Instruction.OpAnd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForConditionalOr()
            {
                TestCodeGenerationForOperator("||", Instruction.OpOr);
            }
        }

        [TestClass]
        public class VisitTernaryOpExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForTernaryOpExpression()
            {
                // Setup: dummy data
                const int ifTrueVal = 1;
                const float ifFalseVal = 2;
                var expression = String.Format("1 % 2 == 0 ? {0} : {1}", ifTrueVal, ifFalseVal);

                // Act
                var result = Subject.VisitTernaryOpExpression(GetFirstContext<HekateParser.TernaryOpExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(16);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(2);
                result.Code[4].Should().Be((byte)Instruction.OpMod);
                result.Code[5].Should().Be((byte)Instruction.Push);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte)Instruction.OpEqual); 
                result.Code[8].Should().Be((byte)Instruction.IfZeroBranchOffset);
                result.Code[9].Should().Be(4);
                result.Code[10].Should().Be((byte)Instruction.Push);
                result.Code[11].Should().Be(ifTrueVal);
                result.Code[12].Should().Be((byte)Instruction.JumpOffset);
                result.Code[13].Should().Be(2);
                result.Code[14].Should().Be((byte)Instruction.Push);
                result.Code[15].Should().Be(ifFalseVal);
            }
            
            [TestMethod]
            public void ShouldGenerateCodeForChainedTernaryOpExpression()
            {                
                // Setup: dummy data
                var expression = String.Format("1 % 2 == 0 ? 3 : 4 % 5 == 0 ? 6 : 7");

                // Act
                var result = Subject.VisitTernaryOpExpression(GetFirstContext<HekateParser.TernaryOpExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(30);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(2);
                result.Code[4].Should().Be((byte)Instruction.OpMod);
                result.Code[5].Should().Be((byte)Instruction.Push);
                result.Code[6].Should().Be(0);
                result.Code[7].Should().Be((byte)Instruction.OpEqual);
                result.Code[8].Should().Be((byte)Instruction.IfZeroBranchOffset);
                result.Code[9].Should().Be(4);
                // true
                    result.Code[10].Should().Be((byte)Instruction.Push);
                    result.Code[11].Should().Be(3);
                    result.Code[12].Should().Be((byte)Instruction.JumpOffset);
                    result.Code[13].Should().Be(16);
                // false
                    result.Code[14].Should().Be((byte)Instruction.Push);
                    result.Code[15].Should().Be(4);
                    result.Code[16].Should().Be((byte)Instruction.Push);
                    result.Code[17].Should().Be(5);
                    result.Code[18].Should().Be((byte)Instruction.OpMod);
                    result.Code[19].Should().Be((byte)Instruction.Push);
                    result.Code[20].Should().Be(0);
                    result.Code[21].Should().Be((byte)Instruction.OpEqual);
                    result.Code[22].Should().Be((byte)Instruction.IfZeroBranchOffset);
                    result.Code[23].Should().Be(4);
                    // true
                        result.Code[24].Should().Be((byte)Instruction.Push);
                        result.Code[25].Should().Be(6);
                        result.Code[26].Should().Be((byte)Instruction.JumpOffset);
                        result.Code[27].Should().Be(2);
                    // false
                        result.Code[28].Should().Be((byte)Instruction.Push);
                        result.Code[29].Should().Be(7);
            }
        }

        [TestClass]
        public class VisitPostIncDecExpression : THekateScriptVisitor
        {
            private void TestIncDecWithExistingVariableOrProperty(bool isIncrementing, IdentifierType type)
            {
                // Setup: create code scope with existing numerical variable/property, mock scope out
                const string identifier = "someIdentifier";
                var expression = String.Format("{0}{1}{2}", type == IdentifierType.Property ? "$" : "", identifier,
                    isIncrementing ? "++" : "--");

                var idx = -1;
                if (type == IdentifierType.Variable) {
                    var codeScope = new CodeScope();
                    idx = codeScope.AddSymbol(identifier, SymbolType.Numerical);
                    SetUpGetCurrentScope(codeScope);
                }
                else if (type == IdentifierType.Property) {
                    var dummyRecord = new IdentifierRecord(identifier, 0);
                    idx = dummyRecord.Index;
                    Mocker.GetMock<IVirtualMachine>()
                        .Setup(ivm => ivm.GetPropertyIndex(identifier))
                        .Returns(dummyRecord.Index);
                }

                // Act
                var result = Subject.VisitPostIncDecExpression(GetFirstContext<HekateParser.PostIncDecExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[1].Should().Be(idx);
                result.Code[2].Should().Be((byte) Instruction.Push);
                result.Code[3].Should().Be(1);
                result.Code[4].Should().Be(isIncrementing ? (byte) Instruction.OpAdd : (byte) Instruction.OpSubtract);
                if (type == IdentifierType.Variable) {
                    result.Code[0].Should().Be((byte) Instruction.GetVariable);
                    result.Code[5].Should().Be((byte) Instruction.SetVariable);
                }
                else if (type == IdentifierType.Property) {
                    result.Code[0].Should().Be((byte) Instruction.GetProperty);
                    result.Code[5].Should().Be((byte) Instruction.SetProperty);
                }
                result.Code[6].Should().Be(idx);
            }

            [TestMethod]
            public void ShouldGenerateCodeForIncrementingExistingNumericalVariable()
            {
                TestIncDecWithExistingVariableOrProperty(true, IdentifierType.Variable);
            }

            [TestMethod]
            public void ShouldGenerateCodeForIncrementingExistingNumericalProperty()
            {
                TestIncDecWithExistingVariableOrProperty(true, IdentifierType.Property);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDecrementingExistingNumericalVariable()
            {
                TestIncDecWithExistingVariableOrProperty(false, IdentifierType.Variable);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDecrementingExistingNumericalProperty()
            {
                TestIncDecWithExistingVariableOrProperty(false, IdentifierType.Property);
            }
        }

        [TestClass]
        public class VisitAssignmentExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForAssigningToExistingNumericalVariable()
            {
                // Setup: create code scope with existing numerical variable, mock scope out
                const string variableName = "someNumericalVariable";
                var expression = String.Format("{0} = 3.5", variableName);
                var codeScope = new CodeScope();
                var idx = codeScope.AddSymbol(variableName, SymbolType.Numerical);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(4);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(3.5f);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(idx);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAssigningToExistingProperty()
            {
                // Setup: add property to vm
                const string propertyName = "$SomeProperty";
                var expression = String.Format("{0} = 3.5", propertyName);
                var dummyRecord = new IdentifierRecord(propertyName, 0);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetPropertyIndex(propertyName))
                    .Returns(dummyRecord.Index);

                // Act
                var result = Subject.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(4);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(3.5f);
                result.Code[2].Should().Be((byte) Instruction.SetProperty);
                result.Code[3].Should().Be(dummyRecord.Index);
            }

            [TestMethod]
            public void ShouldThrowExceptionForAssigningToNonexistentProperty()
            {
                // Setup: none
                const string propertyName = "SomeProperty";
                var expression = String.Format("${0} = 3.5", propertyName);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetPropertyIndex(propertyName))
                    .Throws<ArgumentException>();

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                                .Generate(MockVirtualMachine, MockScopeManager))
                        .ShouldThrow<ArgumentException>();
            }

            [TestMethod]
            public void ShouldThrowExceptionForAssigningToNonexistentNumericalVariable()
            {
                // Setup: create code scope, mock scope out
                const string variableName = "someNumericalVariable";
                var expression = String.Format("{0} = 3.5", variableName);
                var codeScope = new CodeScope();
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                                .Generate(MockVirtualMachine, MockScopeManager))
                        .ShouldThrow<ArgumentException>();
            }

            [TestMethod]
            public void ShouldThrowExceptionForAssigningToExistingEmitterVariable()
            {
                // Setup: create code scope with existing emitter variable, mock scope out
                const string variableName = "someEmitterVariable";
                var expression = String.Format("{0} = 3.5", variableName);
                var codeScope = new CodeScope();
                codeScope.AddSymbol(variableName, SymbolType.Emitter);
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                                .Generate(MockVirtualMachine, MockScopeManager))
                        .ShouldThrow<InvalidOperationException>();
            }



            private void TestCompoundAssignmentWithExistingNumericalVariable(string opStr, Instruction op)
            {
                // Setup: create code scope with existing numerical variable, mock scope out
                const string variableName = "someNumericalVariable";
                var expression = String.Format("{0} {1}= 3.5", variableName, opStr);
                var codeScope = new CodeScope();
                var idx = codeScope.AddSymbol(variableName, SymbolType.Numerical);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[0].Should().Be((byte) Instruction.GetVariable);
                result.Code[1].Should().Be(idx);
                result.Code[2].Should().Be((byte) Instruction.Push);
                result.Code[3].Should().Be(3.5f);
                result.Code[4].Should().Be((byte) op);
                result.Code[5].Should().Be((byte) Instruction.SetVariable);
                result.Code[6].Should().Be(idx);
            }

            [TestMethod]
            public void ShouldGenerateCodeForMultiplyAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("*", Instruction.OpMultiply);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDivideAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("/", Instruction.OpDivide);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAddAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("+", Instruction.OpAdd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSubtractAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("-", Instruction.OpSubtract);
            }
            
            [TestMethod]
            public void ShouldThrowExceptionForCompoundAssigningToExistingEmitterVariable()
            {
                // Setup: create code scope with existing emitter variable, mock scope out
                const string variableName = "someEmitterVariable";
                var expression = String.Format("{0} += 3.5", variableName);
                var codeScope = new CodeScope();
                codeScope.AddSymbol(variableName, SymbolType.Emitter);
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GetFirstContext<HekateParser.AssignmentExpressionContext>(expression))
                                .Generate(MockVirtualMachine, MockScopeManager))
                        .ShouldThrow<InvalidOperationException>();
            }
        }

        [TestClass]
        public class VisitFunctionCallExpression : THekateScriptVisitor
        {
            private void TestFunctionCallGenerationWithParameters(int numParameters)
            {
                // Setup: create function
                const string functionName = "SomeFunctionName";
                var variableValues = Enumerable.Range(1, numParameters).ToList();
                var expression = String.Format("{0}({1})", functionName, String.Join(",", variableValues.Select(v => v.ToString())));

                var parameterValues = variableValues.Select(v => "param" + v);
                var funcCodeBlock = new FunctionCodeScope(parameterValues) {Index = 0};
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetFunctionCodeScope(functionName))
                    .Returns(funcCodeBlock);

                // Act
                var result = Subject.VisitFunctionCallExpression(GetFirstContext<HekateParser.FunctionCallExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(numParameters*2 + 2);
                var i = 0;
                for (i = 0; i < numParameters*2;) {
                    result.Code[i++].Should().Be((byte) Instruction.Push);
                    result.Code[i++].Should().Be(variableValues[(i - 1)/2]);
                }
                result.Code[i++].Should().Be((byte) Instruction.FunctionCall);
                result.Code[i++].Should().Be(funcCodeBlock.Index);
            }

            [TestMethod]
            public void ShouldGenerateCodeForCallingAnExistingFunctionWithZeroParameters()
            {
                TestFunctionCallGenerationWithParameters(0);
            }

            [TestMethod]
            public void ShouldGenerateCodeForCallingAnExistingFunctionWithOneParameter()
            {
                TestFunctionCallGenerationWithParameters(1);
            }

            [TestMethod]
            public void ShouldGenerateCodeForCallingAnExistingFunctionWithThreeParameters()
            {
                TestFunctionCallGenerationWithParameters(3);
            }

            private ScriptStatus DummyFunc(ScriptState state) {  return ScriptStatus.Ok; }
            [TestMethod]
            public void ShouldGenerateCodeForExternalFunctionCall()
            {
                // Setup: create function
                const string functionName = "SomeFunctionName";
                var expression = String.Format("{0}()", functionName);

                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetExternalFunction(functionName))
                    .Returns(new FunctionDefinition());

                // Act
                var result = Subject.VisitFunctionCallExpression(GetFirstContext<HekateParser.FunctionCallExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Count.Should().Be(2);
                result.Code[0].Should().Be((byte)Instruction.ExternalFunctionCall);
                result.Code[1].Should().Be(0);
            }
        }
        
        [TestClass]
        public class VisitFireStatement : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateFiringFunctionCode()
            {
                // Setup: dummy data
                const string expression = "fire bullet(3,5);";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fire"))
                    .Returns(new FiringFunctionDefinition()
                             {
                                 Index = 0,
                                 Name = "fire",
                                 OwningType = new TypeDefinition("bullet", 0),
                                 NumParams = 2
                             });

                // Act
                var result = new CodeBlock();
                Subject.VisitFireStatement(GetFirstContext<HekateParser.FireStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(6);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(3);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(5);
                result.Code[4].Should().Be((byte)Instruction.Fire);
                result.Code[5].Should().Be(0);
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenNotPassingInEnoughArguments()
            {
                // Setup: dummy data
                const string expression = "fire bullet(3,5);";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fire"))
                    .Returns(new FiringFunctionDefinition()
                             {
                                 Index = 0,
                                 Name = "fire",
                                 OwningType = new TypeDefinition("bullet", 0),
                                 NumParams = 3
                             });

                // Act+Verify
                Subject
                    .Invoking(
                        hsv =>
                            hsv.VisitFireStatement(GetFirstContext<HekateParser.FireStatementContext>(expression))
                                .EmitTo(It.IsAny<CodeBlock>(), MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }
            
            [TestMethod]
            public void ShouldThrowExceptionWhenNoMatchingFiringFunctionCanBeFound()
            {
                // Setup: dummy data
                const string expression = "fire bullet(3,5);";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fireBLAH"))
                    .Returns(new FiringFunctionDefinition()
                    {
                        Index = 0,
                        Name = "fire",
                        OwningType = new TypeDefinition("bullet", 0),
                        NumParams = 3
                    });

                // Act+Verify
                Subject
                    .Invoking(
                        hsv =>
                            hsv.VisitFireStatement(GetFirstContext<HekateParser.FireStatementContext>(expression))
                                .EmitTo(It.IsAny<CodeBlock>(), MockVirtualMachine, MockScopeManager))
                    .ShouldThrow<ArgumentException>();
            }
            
            [TestMethod]
            public void ShouldGenerateFiringFunctionCodeWithUpdater()
            {
                // Setup: dummy data
                const string script = @"
action someAction(blah)
{
}

function main()
{
    fire bullet(3,5) with updater someAction(3);
}
";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fire"))
                    .Returns(new FiringFunctionDefinition()
                             {
                                 Index = 0,
                                 Name = "fire",
                                 OwningType = new TypeDefinition("bullet", 0),
                                 NumParams = 2
                             });
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetActionCodeScope("someAction"))
                    .Returns(new ActionCodeScope(new[] { "blah" })
                    {
                        Index = 0
                    });

                // Act
                var result = new CodeBlock();
                Subject.VisitFireStatement(GetNthContext<HekateParser.FireStatementContext>(script, 0))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(9);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(3);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(5);
                result.Code[4].Should().Be((byte)Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte)Instruction.FireWithUpdater);
                result.Code[7].Should().Be(0);
                result.Code[8].Should().Be(0);
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenFiringFunctionCodeWithUpdaterWithWrongArgumentCount()
            {
                // Setup: dummy data
                const string script = @"
action someAction(blah)
{
}

function main()
{
    fire bullet(3,5) with updater someAction();
}
";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fire"))
                    .Returns(new FiringFunctionDefinition()
                    {
                        Index = 0,
                        Name = "fire",
                        OwningType = new TypeDefinition("bullet", 0),
                        NumParams = 2
                    });
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetActionCodeScope("someAction"))
                    .Returns(new ActionCodeScope(new[]{"blah"})
                    {
                        Index = 0
                    });

                // Act+Verify
                var result = new CodeBlock();
                Subject
                    .Invoking(hsv => hsv.VisitFireStatement(GetNthContext<HekateParser.FireStatementContext>(script, 0))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager)).ShouldThrow<ArgumentException>();
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenFiringFunctionCodeWithUpdaterWithWrongName()
            {
                // Setup: dummy data
                const string script = @"
action someAction(blah)
{
}

function main()
{
    fire bullet(3,5) with updater someAcsdftion();
}
";
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(vm => vm.GetFiringFunction("bullet", "fire"))
                    .Returns(new FiringFunctionDefinition()
                    {
                        Index = 0,
                        Name = "fire",
                        OwningType = new TypeDefinition("bullet", 0),
                        NumParams = 2
                    });

                // Act+Verify
                var result = new CodeBlock();
                Subject
                    .Invoking(hsv => hsv.VisitFireStatement(GetNthContext<HekateParser.FireStatementContext>(script, 0))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager)).ShouldThrow<ArgumentException>();
            }
        }
    }
}