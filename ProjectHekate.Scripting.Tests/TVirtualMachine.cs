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

                    for (var i = 0; i <= VirtualMachine.MaxStackSize; i++)
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
                    code.Add(Instruction.OpNot);

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
                    code.Add(Instruction.OpNot);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(1);
                }

                [TestMethod]
                public void ShouldInterpetNotCodeForNegativeNumbersToo()
                {
                    // Setup: set up state
                    const float dummyValue = -1;
                    State.Stack[0] = dummyValue;
                    State.StackHead = 1;
                    var code = new CodeBlock();
                    code.Add(Instruction.OpNot);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(0);
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStackIsEmpty()
                {
                    // Setup: set up state
                    State.StackHead = 0;
                    var code = new CodeBlock();
                    code.Add(Instruction.OpNot);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class BinaryOperators : TVirtualMachine
            {
                private void TestBinaryOperator(Instruction binOp, Func<float, float, float> func)
                {
                    const float left = 3.5f;
                    const float right = 3;

                    // Setup: set up state
                    State.StackHead = 2;
                    State.Stack[0] = left;
                    State.Stack[1] = right;
                    var code = new CodeBlock();
                    code.Add(binOp);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has the right value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(1);
                    State.Stack[0].Should().Be(func(left, right));
                }

                [TestMethod]
                public void ShouldInterpretAddCode()
                {
                    TestBinaryOperator(Instruction.OpAdd, (l, r) => l + r);
                }

                [TestMethod]
                public void ShouldInterpretSubtractCode()
                {
                    TestBinaryOperator(Instruction.OpSubtract, (l, r) => l - r);
                }

                [TestMethod]
                public void ShouldInterpretMultiplyCode()
                {
                    TestBinaryOperator(Instruction.OpMultiply, (l, r) => l * r);
                }

                [TestMethod]
                public void ShouldInterpretDivideCode()
                {
                    TestBinaryOperator(Instruction.OpDivide, (l, r) => l / r);
                }

                [TestMethod]
                public void ShouldThrowInvalidOperationForDivideByZero()
                {
                    // Setup: set up state
                    const float left = 3.5f;
                    const float right = 0;
                    State.StackHead = 2;
                    State.Stack[0] = left;
                    State.Stack[1] = right;
                    var code = new CodeBlock();
                    code.Add(Instruction.OpDivide);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<InvalidOperationException>();
                }

                [TestMethod]
                public void ShouldInterpretModCode()
                {
                    TestBinaryOperator(Instruction.OpMod, (l, r) => l % r);
                }

                [TestMethod]
                public void ShouldInterpretLessThanCode()
                {
                    TestBinaryOperator(Instruction.OpLessThan, (l, r) => l < r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretLessThanEqualCode()
                {
                    TestBinaryOperator(Instruction.OpLessThanEqual, (l, r) => l <= r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretGreaterThanCode()
                {
                    TestBinaryOperator(Instruction.OpGreaterThan, (l, r) => l > r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretGreaterThanEqualCode()
                {
                    TestBinaryOperator(Instruction.OpGreaterThanEqual, (l, r) => l >= r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretEqualCode()
                {
                    TestBinaryOperator(Instruction.OpEqual, (l, r) => l == r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretNotEqualCode()
                {
                    TestBinaryOperator(Instruction.OpNotEqual, (l, r) => l != r ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretAndCode()
                {
                    TestBinaryOperator(Instruction.OpAnd, (l, r) => l > 0 && r > 0 ? 1.0f : 0.0f);
                }

                [TestMethod]
                public void ShouldInterpretOrCode()
                {
                    TestBinaryOperator(Instruction.OpOr, (l, r) => l > 0 || r > 0 ? 1.0f : 0.0f);
                }
            }

            [TestClass]
            public class Jump : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetJumpCode()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    code.Add(Instruction.Jump);
                    code.Add(4);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(1);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(100);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: instruction index changes to beginning
                    State.CurrentInstructionIndex.Should().Be(6);
                    State.StackHead.Should().Be(1);
                    State.Stack[0].Should().Be(100);
                }

                [TestMethod]
                public void ShouldThrowOutOfRangeExceptionForInvalidJumpAddress()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    code.Add(Instruction.Jump);
                    code.Add(-10);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<IndexOutOfRangeException>();

                    code = new CodeBlock();
                    code.Add(Instruction.Jump);
                    code.Add(2); // out of range

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<IndexOutOfRangeException>();
                }
            }

            [TestClass]
            public class JumpIfZero : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetJumpIfZeroCodeForZero()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 0;
                    code.Add(Instruction.JumpIfZero);
                    code.Add(4);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(1);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(100);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: instruction index changes to beginning
                    State.CurrentInstructionIndex.Should().Be(6);
                    State.StackHead.Should().Be(2);
                    State.Stack[1].Should().Be(100);
                }

                [TestMethod]
                public void ShouldNotJumpForNonZero()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 5;
                    code.Add(Instruction.JumpIfZero);
                    code.Add(4);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(1);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(100);

                    // Act: call method
                    Subject.InterpretCode(code, State, false);

                    // Verify: stack has all 3 values on it
                    State.CurrentInstructionIndex.Should().Be(6);
                    State.StackHead.Should().Be(3);
                    State.Stack[2].Should().Be(100);
                }

                [TestMethod]
                public void ShouldThrowOutOfRangeExceptionForInvalidJumpAddress()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 0;
                    code.Add(Instruction.JumpIfZero);
                    code.Add(-10);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<IndexOutOfRangeException>();

                    code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 0;
                    code.Add(Instruction.Jump);
                    code.Add(2); // out of range

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, false))
                        .ShouldThrow<IndexOutOfRangeException>();
                }
            }
        }
    }
}