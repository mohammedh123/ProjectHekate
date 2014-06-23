using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoMoq.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectHekate.Grammar.Implementation;
using ProjectHekate.Scripting;
using ProjectHekate.Scripting.Interfaces;

namespace ProjectHekate.Grammar.Tests
{
    [TestClass]
    public class THekateScriptVisitor : AutoMoqTestFixture<HekateScriptVisitor>
    {
        protected const string WrappedProgramStringUnfmted = "function main(){{{0};}}";

        protected IVirtualMachine MockVirtualMachine
        {
            get { return Mocker.GetMock<IVirtualMachine>().Object; }
        }

        protected IScopeManager MockScopeManager
        {
            get { return Mocker.GetMock<IScopeManager>().Object; }
        }


        protected virtual TContextType GenerateContext<TContextType>(string expression) where TContextType : class, IParseTree
        {
            var lexer = new HekateLexer(new AntlrInputStream(String.Format(WrappedProgramStringUnfmted, expression)));
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);

            var tree = parser.script();

            return tree.GetFirstDescendantOfType<TContextType>();
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
                var result = Subject.VisitVariableDeclaration(GenerateContext<HekateParser.VariableDeclarationContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(4);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1.35f);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
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
                    .VisitReturnStatement(GenerateContext<HekateParser.ReturnStatementContext>(expression))
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
                    .VisitIfStatement(GenerateContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(4);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
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
                    .VisitIfStatement(GenerateContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
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
                    .VisitIfStatement(GenerateContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(12);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
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
                    .VisitIfStatement(GenerateContext<HekateParser.IfStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(21);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
                result.Code[3].Should().Be(9);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
                result.Code[7].Should().Be((byte) Instruction.Jump);
                result.Code[8].Should().Be(21);
                result.Code[9].Should().Be((byte) Instruction.Push);
                result.Code[10].Should().Be(2);
                result.Code[11].Should().Be((byte) Instruction.JumpIfZero);
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
                    .VisitForStatement(GenerateContext<HekateParser.ForStatementContext>(expression))
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
                    .VisitForStatement(GenerateContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(6);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(23);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte) Instruction.Jump);
                result.Code[5].Should().Be(4);
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
                    .VisitForStatement(GenerateContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(11);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte) Instruction.GetVariable);
                result.Code[5].Should().Be(0);
                result.Code[6].Should().Be((byte) Instruction.Push);
                result.Code[7].Should().Be(10);
                result.Code[8].Should().Be((byte) Instruction.OperatorLessThan);
                result.Code[9].Should().Be((byte) Instruction.Jump);
                result.Code[10].Should().Be(4);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEmptyForStatementButWithInitExpressionAndUpdate()
            {
                // Setup: dummy data
                const string expression = @"for(var i = 5; i < 10; i++) {}";
                SetUpGetCurrentScope(new CodeScope());

                var result = new CodeBlock();
                Subject
                    .VisitForStatement(GenerateContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(18);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte) Instruction.GetVariable);
                result.Code[5].Should().Be(0);
                result.Code[6].Should().Be((byte) Instruction.Push);
                result.Code[7].Should().Be(10);
                result.Code[8].Should().Be((byte) Instruction.OperatorLessThan);
                result.Code[9].Should().Be((byte) Instruction.GetVariable);
                result.Code[10].Should().Be(0);
                result.Code[11].Should().Be((byte) Instruction.Push);
                result.Code[12].Should().Be(1);
                result.Code[13].Should().Be((byte) Instruction.OperatorAdd);
                result.Code[14].Should().Be((byte) Instruction.SetVariable);
                result.Code[15].Should().Be(0);
                result.Code[16].Should().Be((byte) Instruction.Jump);
                result.Code[17].Should().Be(4);
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
                    .VisitForStatement(GenerateContext<HekateParser.ForStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(21);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(5);
                result.Code[2].Should().Be((byte) Instruction.SetVariable);
                result.Code[3].Should().Be(0);
                result.Code[4].Should().Be((byte) Instruction.GetVariable);
                result.Code[5].Should().Be(0);
                result.Code[6].Should().Be((byte) Instruction.Push);
                result.Code[7].Should().Be(10);
                result.Code[8].Should().Be((byte) Instruction.OperatorLessThan);
                result.Code[9].Should().Be((byte) Instruction.GetVariable);
                result.Code[10].Should().Be(0);
                result.Code[11].Should().Be((byte) Instruction.Push);
                result.Code[12].Should().Be(1);
                result.Code[13].Should().Be((byte) Instruction.OperatorAdd);
                result.Code[14].Should().Be((byte) Instruction.SetVariable);
                result.Code[15].Should().Be(0);
                result.Code[16].Should().Be((byte) Instruction.Push);
                result.Code[17].Should().Be(3);
                result.Code[18].Should().Be((byte) Instruction.Pop);
                result.Code[19].Should().Be((byte) Instruction.Jump);
                result.Code[20].Should().Be(4);
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
                    .VisitWhileStatement(GenerateContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(6);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
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
                    .VisitWhileStatement(GenerateContext<HekateParser.WhileStatementContext>(expression))
                    .EmitTo(result, MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(9);
                result.Code[0].Should().Be((byte) Instruction.Push);
                result.Code[1].Should().Be(1);
                result.Code[2].Should().Be((byte) Instruction.JumpIfZero);
                result.Code[3].Should().Be(9);
                result.Code[4].Should().Be((byte) Instruction.Push);
                result.Code[5].Should().Be(3);
                result.Code[6].Should().Be((byte) Instruction.Pop);
                result.Code[7].Should().Be((byte) Instruction.Jump);
                result.Code[8].Should().Be(0);
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
                var result = Subject.VisitBreakStatement(GenerateContext<HekateParser.BreakStatementContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte) Instruction.Jump);
                result.Code[1].Should().Be(0);
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
                var result = Subject.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression))
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
                    .Invoking(hsv => hsv.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression))
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
                var result = Subject.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression))
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
                    .Invoking(hsv => hsv.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression))
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
                var idx = codeScope.AddNumericalVariable(identifier);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitNormalIdentifierExpression(
                    GenerateContext<HekateParser.NormalIdentifierExpressionContext>(expression))
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
                    hsv => hsv.VisitNormalIdentifierExpression(GenerateContext<HekateParser.NormalIdentifierExpressionContext>(expression))
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
                    .Setup(ivm => ivm.GetProperty(identifier))
                    .Returns(dummyRecord);

                // Act
                var result = Subject.VisitPropertyIdentifierExpression(
                    GenerateContext<HekateParser.PropertyIdentifierExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code[0].Should().Be((byte) Instruction.GetProperty);
                result.Code[1].Should().Be(dummyRecord.Index);
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionForNonMatchingVariable()
            {
                // Setup: create code scope, mock vm out
                const string identifier = "$SomeIdentifier";
                var expression = String.Format("{0}", identifier);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetProperty(identifier))
                    .Throws<ArgumentException>();

                // Act + Verify
                Subject.Invoking(
                    hsv =>
                        hsv.VisitPropertyIdentifierExpression(GenerateContext<HekateParser.PropertyIdentifierExpressionContext>(expression))
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
                var result = Subject.VisitUnaryExpression(GenerateContext<HekateParser.UnaryExpressionContext>(expression))
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
                TestCodeGenerationForOperator("!", Instruction.OperatorNot);
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
                var result = Subject.VisitBinaryExpression(GenerateContext<HekateParser.BinaryExpressionContext>(expression))
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
                TestCodeGenerationForOperator("*", Instruction.OperatorMultiply);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDivision()
            {
                TestCodeGenerationForOperator("/", Instruction.OperatorDivide);
            }

            [TestMethod]
            public void ShouldGenerateCodeForModulus()
            {
                TestCodeGenerationForOperator("%", Instruction.OperatorMod);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAddition()
            {
                TestCodeGenerationForOperator("+", Instruction.OperatorAdd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSubtraction()
            {
                TestCodeGenerationForOperator("-", Instruction.OperatorSubtract);
            }

            [TestMethod]
            public void ShouldGenerateCodeForLessThan()
            {
                TestCodeGenerationForOperator("<", Instruction.OperatorLessThan);
            }

            [TestMethod]
            public void ShouldGenerateCodeForGreaterThan()
            {
                TestCodeGenerationForOperator(">", Instruction.OperatorGreaterThan);
            }

            [TestMethod]
            public void ShouldGenerateCodeForLessThanEqual()
            {
                TestCodeGenerationForOperator("<=", Instruction.OperatorLessThanEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForGreaterThanEqual()
            {
                TestCodeGenerationForOperator(">=", Instruction.OperatorGreaterThanEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForEquality()
            {
                TestCodeGenerationForOperator("==", Instruction.OperatorEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForInequality()
            {
                TestCodeGenerationForOperator("!=", Instruction.OperatorNotEqual);
            }

            [TestMethod]
            public void ShouldGenerateCodeForConditionalAnd()
            {
                TestCodeGenerationForOperator("&&", Instruction.OperatorAnd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForConditionalOr()
            {
                TestCodeGenerationForOperator("||", Instruction.OperatorOr);
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
                    idx = codeScope.AddNumericalVariable(identifier);
                    SetUpGetCurrentScope(codeScope);
                }
                else if (type == IdentifierType.Property) {
                    var dummyRecord = new IdentifierRecord(identifier, 0);
                    idx = dummyRecord.Index;
                    Mocker.GetMock<IVirtualMachine>()
                        .Setup(ivm => ivm.GetProperty(identifier))
                        .Returns(dummyRecord);
                }

                // Act
                var result = Subject.VisitPostIncDecExpression(GenerateContext<HekateParser.PostIncDecExpressionContext>(expression))
                    .Generate(MockVirtualMachine, MockScopeManager);

                // Verify
                result.Code.Should().HaveCount(7);
                result.Code[1].Should().Be(idx);
                result.Code[2].Should().Be((byte) Instruction.Push);
                result.Code[3].Should().Be(1);
                result.Code[4].Should().Be(isIncrementing ? (byte) Instruction.OperatorAdd : (byte) Instruction.OperatorSubtract);
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
                var idx = codeScope.AddNumericalVariable(variableName);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
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
                    .Setup(ivm => ivm.GetProperty(propertyName))
                    .Returns(dummyRecord);

                // Act
                var result = Subject.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
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
                const string propertyName = "$SomeProperty";
                var expression = String.Format("{0} = 3.5", propertyName);
                Mocker.GetMock<IVirtualMachine>()
                    .Setup(ivm => ivm.GetProperty(propertyName))
                    .Throws<ArgumentException>();

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
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
                codeScope.AddEmitterVariable(variableName);
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
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
                codeScope.AddEmitterVariable(variableName);
                SetUpGetCurrentScope(codeScope);

                // Act + Verify
                var result =
                    Subject.Invoking(
                        hsv =>
                            hsv.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
                                .Generate(MockVirtualMachine, MockScopeManager))
                        .ShouldThrow<ArgumentException>();
            }



            private void TestCompoundAssignmentWithExistingNumericalVariable(string opStr, Instruction op)
            {
                // Setup: create code scope with existing numerical variable, mock scope out
                const string variableName = "someNumericalVariable";
                var expression = String.Format("{0} {1}= 3.5", variableName, opStr);
                var codeScope = new CodeScope();
                var idx = codeScope.AddNumericalVariable(variableName);
                SetUpGetCurrentScope(codeScope);

                // Act
                var result = Subject.VisitAssignmentExpression(GenerateContext<HekateParser.AssignmentExpressionContext>(expression))
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
                TestCompoundAssignmentWithExistingNumericalVariable("*", Instruction.OperatorMultiply);
            }

            [TestMethod]
            public void ShouldGenerateCodeForDivideAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("/", Instruction.OperatorDivide);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAddAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("+", Instruction.OperatorAdd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSubtractAssignToExistingNumericalVariable()
            {
                TestCompoundAssignmentWithExistingNumericalVariable("-", Instruction.OperatorSubtract);
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
                var result = Subject.VisitFunctionCallExpression(GenerateContext<HekateParser.FunctionCallExpressionContext>(expression))
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
        }
    }
}