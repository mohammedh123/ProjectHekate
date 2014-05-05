using System;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHekate.Scripting.Tests
{
    [TestClass]
    public class GrammarTests
    {
        [TestMethod]
        public void ShouldParseEmptyString()
        {
            var input = new AntlrInputStream("");
            var lexer = new HekateGrammarLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateGrammarParser(tokens);
            var tree = parser.script();
        }
    }
}