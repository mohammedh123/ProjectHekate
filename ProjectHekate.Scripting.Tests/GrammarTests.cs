using System;
using System.Linq;
using Antlr4.Runtime;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHekate.Scripting.Tests
{
    [TestClass]
    public class GrammarTests
    {
        [TestMethod]
        public void ShouldParseEmptyStringToEmptyTree()
        {
            var input = new AntlrInputStream("");
            var lexer = new HekateGrammarLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateGrammarParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(0);
        }


        [TestMethod]
        public void ShouldParseSingleEmptyFunction()
        {
            var input = new AntlrInputStream("function Main() {}");
            var lexer = new HekateGrammarLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateGrammarParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(1);
        }
    }
}