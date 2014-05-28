using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoMoq.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectHekate.Grammar.Implementation;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Tests
{
    [TestClass]
    public class THekateScriptVisitor : AutoMoqTestFixture<HekateScriptVisitor>
    {
        protected const string WrappedProgramStringUnfmted = "function main(){{{0};}}";

        protected virtual TContextType GenerateContext<TContextType>(string expression) where TContextType : class, IParseTree
        {
            var lexer = new HekateLexer(new AntlrInputStream(String.Format(WrappedProgramStringUnfmted, expression)));
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);

            var tree = parser.script();

            return tree.GetFirstDescendantOfType<TContextType>();
        }

        [TestClass]
        public class VisitUnaryExpression : THekateScriptVisitor
        {
            [TestMethod]
            public void ShouldGenerateCodeForLogicalNot()
            {
                // Setup: dummy data + mock vm out
                const int value = 1;
                var expression = String.Format("!{0}", value);

                ResetSubject();

                // Act
                var result = Subject.VisitUnaryExpression(GenerateContext<HekateParser.UnaryExpressionContext>(expression));

                // Verify
                result.Code.Should().HaveCount(3);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(value);
                result.Code[2].Should().Be((byte)Instruction.OperatorNot);
            }

            [TestMethod]
            public void ShouldGenerateCodeForNegativeNumber()
            {
                // Setup: dummy data + mock vm out
                const int value = 5;
                var expression = String.Format("-{0}", value);

                ResetSubject();

                // Act
                var result = Subject.VisitUnaryExpression(GenerateContext<HekateParser.UnaryExpressionContext>(expression));

                // Verify
                result.Code.Should().HaveCount(3);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(value);
                result.Code[2].Should().Be((byte)Instruction.Negate);
            }
        }

        [TestClass]
        public class VisitBinaryExpression : THekateScriptVisitor
        {
            private void TestCodeGenerationForOperator(string operatorString, Instruction op)
            {
                // Setup: dummy data + mock vm out
                const int left = 1;
                const float right = 0;
                var expression = String.Format("{0}{1}{2}", left, operatorString, right);

                ResetSubject();

                // Act
                var result = Subject.VisitBinaryExpression(GenerateContext<HekateParser.BinaryExpressionContext>(expression));

                // Verify
                result.Code.Should().HaveCount(5);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(left);
                result.Code[2].Should().Be((byte)Instruction.Push);
                result.Code[3].Should().Be(right);
                result.Code[4].Should().Be((byte)op);
            }

            [TestMethod]
            public void ShouldGenerateCodeForAddition()
            {
                TestCodeGenerationForOperator("+", Instruction.OperatorAdd);
            }

            [TestMethod]
            public void ShouldGenerateCodeForSubtraction()
            {
                TestCodeGenerationForOperator("+", Instruction.OperatorSubtract);
            }

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
                var result = Subject.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression));

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(literal);
            }

            [TestMethod]
            public void ShouldGenerateCodeForFloatLiteral()
            {
                // Setup
                const float literal = 3.455f;
                string literalExpression = literal.ToString();

                // Act
                var result = Subject.VisitLiteralExpression(GenerateContext<HekateParser.LiteralExpressionContext>(literalExpression));

                // Verify
                result.Code.Should().HaveCount(2);
                result.Code[0].Should().Be((byte)Instruction.Push);
                result.Code[1].Should().Be(literal);
            }
        }
    }
}