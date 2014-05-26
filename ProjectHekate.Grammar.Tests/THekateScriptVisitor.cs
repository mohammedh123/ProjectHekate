using System;
using System.Linq;
using Antlr4.Runtime;
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
        [TestClass]
        public class GenerateConstantExpression : THekateScriptVisitor
        {
            private HekateParser.BinaryExpressionContext GenerateBinaryExpressionContext(string expression)
            {
                var lexer = new HekateLexer(new AntlrInputStream(String.Format(WrappedProgramStringUnfmted, expression)));
                var tokens = new CommonTokenStream(lexer);
                var parser = new HekateParser(tokens);

                var tree = parser.script();

                return tree.GetFirstDescendantOfType<HekateParser.BinaryExpressionContext>();
            }

            [TestClass]
            public class Addition : GenerateConstantExpression
            {
                [TestMethod]
                public void ShouldGenerateCodeForBasicExpression()
                {
                    // Setup: dummy data + mock vm out
                    const string expression = "3+5";

                    ResetSubject();

                    // Act
                    var result = Subject.VisitBinaryExpression(GenerateBinaryExpressionContext(expression));

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
    }
}