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
        public class VisitBinaryExpression : THekateScriptVisitor
        {
            [TestClass]
            public class Addition : VisitBinaryExpression
            {
                [TestMethod]
                public void ShouldGenerateCodeForBasicExpression()
                {
                    // Setup: dummy data + mock vm out
                    const string expression = "3+5";

                    ResetSubject();

                    // Act
                    var result = Subject.VisitBinaryExpression(GenerateContext<HekateParser.BinaryExpressionContext>(expression));

                    // Verify
                    result.Code.Should().HaveCount(5);
                    result.Code[0].Should().Be((byte)Instruction.Push);
                    result.Code[1].Should().Be(3);
                    result.Code[2].Should().Be((byte)Instruction.Push);
                    result.Code[3].Should().Be(5);
                    result.Code[4].Should().Be((byte)Instruction.OperatorAdd);
                }

                [TestMethod]
                public void ShouldGenerateCodeForExpressionIncludingSingleParenthesizedExpression()
                {
                    // Setup: dummy data + mock vm out
                    const string expression = "(3+5)+7";

                    ResetSubject();

                    // Act
                    var result = Subject.VisitBinaryExpression(GenerateContext<HekateParser.BinaryExpressionContext>(expression));

                    // Verify
                    result.Code.Should().HaveCount(8);
                    result.Code[0].Should().Be((byte)Instruction.Push);
                    result.Code[1].Should().Be(3);
                    result.Code[2].Should().Be((byte)Instruction.Push);
                    result.Code[3].Should().Be(5);
                    result.Code[4].Should().Be((byte)Instruction.OperatorAdd);
                    result.Code[5].Should().Be((byte)Instruction.Push);
                    result.Code[6].Should().Be(7);
                    result.Code[7].Should().Be((byte)Instruction.OperatorAdd);
                }


                [TestMethod]
                public void ShouldGenerateCodeForComplexExpression()
                {
                    // Setup: dummy data + mock vm out
                    const string expression = "-35+((3+5)+7)";

                    ResetSubject();

                    // Act
                    var result = Subject.VisitBinaryExpression(GenerateContext<HekateParser.BinaryExpressionContext>(expression));

                    // Verify
                    result.Code.Should().HaveCount(11);
                    result.Code[0].Should().Be((byte)Instruction.Push);
                    result.Code[1].Should().Be(-35);
                    result.Code[2].Should().Be((byte)Instruction.Push);
                    result.Code[3].Should().Be(3);
                    result.Code[4].Should().Be((byte)Instruction.Push);
                    result.Code[5].Should().Be(5);
                    result.Code[6].Should().Be((byte)Instruction.OperatorAdd);
                    result.Code[7].Should().Be((byte)Instruction.Push);
                    result.Code[8].Should().Be(7);
                    result.Code[9].Should().Be((byte)Instruction.OperatorAdd);
                    result.Code[10].Should().Be((byte)Instruction.OperatorAdd);
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