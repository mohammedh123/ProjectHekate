using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHekate.Grammar;
using FluentAssertions;

namespace ProjectHekate.Grammar.Tests
{
    [TestClass]
    public class GrammarTests
    {
        [TestMethod]
        public void ShouldParseEmptyStringToEmptyTree()
        {
            var input = new AntlrInputStream("");
            var lexer = new HekateLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(0);
        }
        
        [TestMethod]
        public void ShouldParseSingleEmptyFunction()
        {
            var input = new AntlrInputStream("function Main() {}");
            var lexer = new HekateLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(1);
        }

        [TestMethod]
        public void ShouldParseTwoEmptyFunctions()
        {
            var input = new AntlrInputStream("function SomeOtherShit(){}\r\nfunction Main() {}");
            var lexer = new HekateLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(2);
        }

        [TestMethod]
        public void ShouldParseComplexScript()
        {
            var input = new AntlrInputStream(@"function MoveDown()
{
    $Y += 0.5;
    $Angle += TWO_PI/180.0;
}
        
function Main()
{
    if ($FramesAlive == 60) {
        var baseEmitterBuilder = create emitter($X, $Y, PI_OVER_2, true) with updater MoveDown();
        const var numShots = 3;
        const var diffAngle = TWO_PI/numShots;
        
        for (var i = 0; i < numShots; i++) {
            baseEmitterBuilder attach OrbitingEmitter(200, diffAngle*i, true) with updater SomeCrap1();
        }
        
        var finalEmitter = build baseEmitterBuilder;

        for (var i = 0; i < numShots; i++)
        {
            fire OrbitingCurvedLaser(200, diffAngle * i, 8, 50, 0, 0, 3) from finalEmitter;
            fire OrbitingCurvedLaser(100, diffAngle * i, 8, 50, 0, 0, 2) from finalEmitter;
        }
    };

    wait 5 frames;
}");
            var lexer = new HekateLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);
            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(2);
        }
    }
}