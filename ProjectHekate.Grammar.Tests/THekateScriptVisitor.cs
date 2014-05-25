using System;
using System.Linq;
using AutoMoq.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectHekate.Grammar.Implementation;
using ProjectHekate.Scripting;

namespace ProjectHekate.Grammar.Tests
{
    [TestClass]
    public class THekateScriptVisitor : AutoMoqTestFixture<HekateScriptVisitor>
    {
        [TestClass]
        public class GenerateConstantExpression : THekateScriptVisitor
        {
            [TestClass]
            public class Addition : THekateScriptVisitor
            {
                [TestMethod]
                public void ShouldGenerateCodeForBasicExpression()
                {
                    // Setup: dummy data + mock vm out
                    const string expression = "3+5";

                    ResetSubject();

                    // Act

                    // Verify

                    Assert.Inconclusive("Test not written yet.");
                }

                
            }
        }
    }
}