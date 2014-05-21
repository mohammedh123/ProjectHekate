using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHekate.Grammar;
using FluentAssertions;
using ProjectHekate.Grammar.Implementation;

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
            var input = new AntlrInputStream(@"function CalculateSomething(x)
{
    var d = 0;
    d += 39;
    return x + 3;
}

emitterUpdater MoveDown(delta)
{
    $Y += (2*delta)/2;
    $Angle += TWO_PI/180.0;
    wait 1 frames;
}
        
function Main()
{
    if ($FramesAlive == 60) {
        var baseEmitterBuilder = create emitter($X, $Y, PI_OVER_2, true) with updater MoveDown(0.5);
        var numShots = 3;
        var diffAngle = TWO_PI/numShots;
        
        for (var i = 0; i < numShots; i++) {
            baseEmitterBuilder attach OrbitingEmitter(200, diffAngle*i, true) with updater SomeCrap1();
        }
        
        var finalEmitter = build baseEmitterBuilder;

        for (var i = 0; i < numShots; i++)
        {
            fire OrbitingCurvedLaser(200, diffAngle * i, 8, 50, 0, 0, 3) from finalEmitter;
            fire OrbitingCurvedLaser(100, diffAngle * i, 8, 50, 0, 0, 2) from finalEmitter;
        }
    }

    wait 5 frames;
}");
            var lexer = new HekateLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new HekateParser(tokens);
            parser.BuildParseTree = true;

            var tree = parser.script();

            parser.NumberOfSyntaxErrors.Should().Be(0);
            tree.ChildCount.Should().Be(3);

            new HekateScriptVisitor().Visit(tree);
        }
    }
}