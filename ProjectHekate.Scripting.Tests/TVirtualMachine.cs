using System;
using AutoMoq.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHekate.Scripting.Tests
{
    [TestClass]
    public class TVirtualMachine : AutoMoqTestFixture<VirtualMachine>
    {
        protected ScriptState State;

        [TestInitialize]
        public void Setup()
        {
            State = new ScriptState();
        }

        [TestClass]
        public class InterpetCode : TVirtualMachine
        {
            [TestClass]
            public class Push : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetPushCode()
                {
                    // Setup: set up state
                    const float expectedValue = 3.5f;
                    var code = new CodeBlock();
                    code.Add(Instruction.Push);
                    code.Add(expectedValue);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has something on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.Stack[0].Should().Be(expectedValue);
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenPushingOverStackLimit()
                {
                    // Setup: set up state
                    const float expectedValue = 3.5f;
                    var code = new CodeBlock();

                    for (var i = 0; i < VirtualMachine.MaxStackSize; i++)
                    {
                        code.Add(Instruction.Push);
                        code.Add(expectedValue);
                    }

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class Pop : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetPopCode()
                {
                    // Setup: set up state
                    State.StackHead = 1;
                    var code = new CodeBlock();
                    code.Add(Instruction.Pop);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has nothing on it
                    State.StackHead.Should().Be(0);
                    State.CurrentInstructionIndex.Should().Be(1);
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenPoppingBelowStackBase()
                {
                    // Setup: set up state
                    State.StackHead = 0;
                    var code = new CodeBlock();
                    code.Add(Instruction.Pop);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class Negate : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetNegateCode()
                {
                    // Setup: set up state
                    const float dummyValue = 3.5f;
                    State.Stack[0] = dummyValue;
                    State.StackHead = 1;
                    var code = new CodeBlock();
                    code.Add(Instruction.Negate);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(-dummyValue);
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStackIsEmpty()
                {
                    // Setup: set up state
                    State.StackHead = 0;
                    var code = new CodeBlock();
                    code.Add(Instruction.Negate);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class Not : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetNotCodeForTrue()
                {
                    // Setup: set up state
                    const float dummyValue = 3.5f;
                    State.Stack[0] = dummyValue;
                    State.StackHead = 1;
                    var code = new CodeBlock();
                    code.Add(Instruction.OperatorNot);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(0);
                }

                [TestMethod]
                public void ShouldInterpetNotCodeForFalse()
                {
                    // Setup: set up state
                    const float dummyValue = 0;
                    State.Stack[0] = dummyValue;
                    State.StackHead = 1;
                    var code = new CodeBlock();
                    code.Add(Instruction.OperatorNot);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(1);
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStackIsEmpty()
                {
                    // Setup: set up state
                    State.StackHead = 0;
                    var code = new CodeBlock();
                    code.Add(Instruction.OperatorNot);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }
        }
    }
}