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
                    result.Code.Should().HaveElementAt(0, Instruction.Push);
                    result.Code.Should().HaveElementAt(1, 3);
                    result.Code.Should().HaveElementAt(2, Instruction.Push);
                    result.Code.Should().HaveElementAt(3, 5);
                    result.Code.Should().HaveElementAt(4, Instruction.OperatorAdd);
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
                result.Code.Should().HaveElementAt(0, Instruction.Push);
                result.Code.Should().HaveElementAt(1, literal);
            }
        }
    }
}