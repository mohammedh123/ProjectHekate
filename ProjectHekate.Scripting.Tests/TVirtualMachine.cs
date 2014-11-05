using System;
using AutoMoq.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectHekate.Core;

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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
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
                    Subject.InterpretCode(code, State, null, false);

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
                    Subject.InterpretCode(code, State, null, false);

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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
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
                    Subject.InterpretCode(code, State, null, false);

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
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<IndexOutOfRangeException>();

                    code = new CodeBlock();
                    code.Add(Instruction.Jump);
                    code.Add(3); // out of range

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<IndexOutOfRangeException>();
                }
            }

            [TestClass]
            public class IfZeroBranch : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetIfZeroBranchCodeForZero()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 0;
                    code.Add(Instruction.IfZeroBranch);
                    code.Add(4);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(1);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(100);

                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

                    // Verify: instruction index changes to end
                    State.CurrentInstructionIndex.Should().Be(6);
                    State.StackHead.Should().Be(2);
                    State.Stack[1].Should().Be(100);
                }

                [TestMethod]
                public void ShouldNotBranchForNonZero()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 5;
                    code.Add(Instruction.IfZeroBranch);
                    code.Add(4);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(1);
                    code.Add(Instruction.Push); // just some dummy code
                    code.Add(100);

                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

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
                    code.Add(Instruction.IfZeroBranch);
                    code.Add(-10);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<IndexOutOfRangeException>();

                    code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 0;
                    code.Add(Instruction.Jump);
                    code.Add(3); // out of range

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<IndexOutOfRangeException>();
                }
            }

            [TestClass]
            public class GetProperty : TVirtualMachine
            {
                private class TestBullet : AbstractScriptObject
                {
                    public float Speed { get; set; }    
                }

                private class TestBullet2 : AbstractScriptObject
                {
                }

                [TestMethod]
                public void ShouldInterpetGetPropertyWithMultipleTypes()
                {
                    // Setup: set up state + setup mappings
                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddType<TestBullet2>("bullet2");
                    Subject.UpdatePropertyMappings();

                    var dummyBullet = new TestBullet();
                    dummyBullet.X = 100;
                    dummyBullet.EmitTypeIndex = 0;

                    var code = new CodeBlock();
                    State.StackHead = 0;
                    code.Add(Instruction.GetProperty);
                    code.Add((byte)0);

                    // Act: call method
                    Subject.InterpretCode(code, State, dummyBullet, false);

                    // Verify: instruction index changes to end, stack has value of property
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.StackHead.Should().Be(1);
                    State.Stack[0].Should().Be(100);
                }

                [TestMethod]
                public void ShouldInterpetGetPropertyWithMultipleProperties()
                {
                    // Setup: set up state + setup mappings
                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddProperty<TestBullet, float>("bullet", t => t.Speed);
                    Subject.UpdatePropertyMappings();

                    var dummyBullet = new TestBullet();
                    dummyBullet.Speed = 100;
                    dummyBullet.EmitTypeIndex = 0;

                    var code = new CodeBlock();
                    State.StackHead = 0;
                    code.Add(Instruction.GetProperty);
                    code.Add(3); // 3 should be speed

                    // Act: call method
                    Subject.InterpretCode(code, State, dummyBullet, false);

                    // Verify: instruction index changes to end, stack has value of property
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.StackHead.Should().Be(1);
                    State.Stack[0].Should().Be(100);
                }
            }

            [TestClass]
            public class SetProperty : TVirtualMachine
            {
                private class TestBullet : AbstractScriptObject
                {
                    public float Speed { get; set; }
                }

                private class TestBullet2 : AbstractScriptObject
                {
                }

                [TestMethod]
                public void ShouldInterpetSetPropertyWithMultipleTypes()
                {
                    // Setup: set up state + setup mappings
                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddType<TestBullet2>("bullet2");
                    Subject.UpdatePropertyMappings();

                    var dummyBullet = new TestBullet();
                    dummyBullet.X = 100;
                    dummyBullet.EmitTypeIndex = 0;

                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 35.0f;
                    code.Add(Instruction.SetProperty);
                    code.Add((byte)0);

                    // Act: call method
                    Subject.InterpretCode(code, State, dummyBullet, false);

                    // Verify: instruction index changes to end, property on object changes
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.StackHead.Should().Be(1);
                    dummyBullet.X.Should().Be(35.0f);
                }

                [TestMethod]
                public void ShouldInterpetSetPropertyWithSingleType()
                {
                    // Setup: set up state + setup mappings
                    Subject.AddType<TestBullet>("bullet");
                    Subject.UpdatePropertyMappings();

                    var dummyBullet = new TestBullet();
                    dummyBullet.X = 100;
                    dummyBullet.EmitTypeIndex = 0;

                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 35.0f;
                    code.Add(Instruction.SetProperty);
                    code.Add((byte)0);

                    // Act: call method
                    Subject.InterpretCode(code, State, dummyBullet, false);

                    // Verify: instruction index changes to end, property on object changes
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.StackHead.Should().Be(1);
                    dummyBullet.X.Should().Be(35.0f);
                }

                [TestMethod]
                public void ShouldInterpetGetPropertyWithMultipleProperties()
                {
                    // Setup: set up state + setup mappings
                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddProperty<TestBullet, float>("bullet", b => b.Speed);
                    Subject.UpdatePropertyMappings();

                    var dummyBullet = new TestBullet();
                    dummyBullet.Speed = 100;
                    dummyBullet.EmitTypeIndex = 0;

                    var code = new CodeBlock();
                    State.StackHead = 1;
                    State.Stack[0] = 35.0f;
                    code.Add(Instruction.SetProperty);
                    code.Add(3); // 3 should be speed

                    // Act: call method
                    Subject.InterpretCode(code, State, dummyBullet, false);

                    // Verify: instruction index changes to end, property on object changes
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.StackHead.Should().Be(1);
                    dummyBullet.Speed.Should().Be(35.0f);
                }

                [TestMethod]
                public void ShouldThrowIfStackIsEmpty()
                {
                    var code = new CodeBlock();
                    code.Add(Instruction.SetProperty);
                    code.Add((byte)0);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class GetVariable : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetGetVariable()
                {
                    State.NumericalVariables[0] = 5f;

                    var code = new CodeBlock();
                    code.Add(Instruction.GetVariable);
                    code.Add((byte)0);
                    
                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

                    // Verify: stack has nothing on it
                    State.StackHead.Should().Be(1);
                    State.Stack[0].Should().Be(5f);
                    State.CurrentInstructionIndex.Should().Be(2);
                }
            }

            [TestClass]
            public class SetVariable : TVirtualMachine
            {
                [TestMethod]
                public void ShouldInterpetSetPropertyWithMultipleTypes()
                {
                    State.StackHead = 1;
                    State.Stack[0] = 35.5f;
                    var code = new CodeBlock();
                    code.Add(Instruction.SetVariable);
                    code.Add((byte)0);

                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

                    // Verify: stack has nothing on it
                    State.StackHead.Should().Be(1);
                    State.NumericalVariables[0].Should().Be(35.5f);
                    State.CurrentInstructionIndex.Should().Be(2);
                }

                [TestMethod]
                public void ShouldThrowIfStackIsEmpty()
                {
                    var code = new CodeBlock();
                    code.Add(Instruction.SetVariable);
                    code.Add((byte)0);

                    // Act+Verify
                    Subject
                        .Invoking(vm => vm.InterpretCode(code, State, null, false))
                        .ShouldThrow<InvalidOperationException>();
                }
            }

            [TestClass]
            public class ExternalFunctionCall : TVirtualMachine
            {
                private ScriptStatus DummyFunc(ScriptState state)
                {
                    // no arguments, returns value
                    state.Stack[state.StackHead] = 3.5f;
                    state.StackHead++;

                    return ScriptStatus.Ok;
                }

                [TestMethod]
                public void ShouldInterpetExternalFunctionCall()
                {
                    // Setup: set up state
                    const float dummyValue = 3.5f;
                    var code = new CodeBlock();
                    code.Add(Instruction.ExternalFunctionCall);
                    code.Add((byte)0);
                    Subject.AddExternalFunction("blah", DummyFunc);

                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(1);
                    State.CurrentInstructionIndex.Should().Be(2);
                    State.Stack[0].Should().Be(3.5f);
                }
            }

            [TestClass]
            public class Fire : TVirtualMachine
            {
                private class TestBullet : AbstractScriptObject
                {
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenNotPassingInEnoughArguments()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    code.Add(Instruction.Push);
                    code.Add(1);
                    code.Add(Instruction.Push);
                    code.Add(2);
                    code.Add(Instruction.Push);
                    code.Add(3);
                    code.Add(Instruction.Push);
                    code.Add(4);
                    code.Add(Instruction.Fire);
                    code.Add((byte)0);

                    var bus = new BulletSystem(Subject);

                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddFiringFunction("bullet", "fire", bus, bs => bs.FireBasicBullet(0, 0, 0, 0, 0));

                    // Act+Verify
                    Subject
                        .Invoking(s => s.InterpretCode(code, State, null, false))
                        .ShouldThrow<ArgumentException>();
                }
                
                [TestMethod]
                public void ShouldInterpetFire()
                {
                    // Setup: set up state
                    var code = new CodeBlock();
                    code.Add(Instruction.Push);
                    code.Add(1);
                    code.Add(Instruction.Push);
                    code.Add(2);
                    code.Add(Instruction.Push);
                    code.Add(3);
                    code.Add(Instruction.Push);
                    code.Add(4);
                    code.Add(Instruction.Push);
                    code.Add(5);
                    code.Add(Instruction.Fire);
                    code.Add((byte)0);

                    var bus = new BulletSystem(Subject);
                    
                    Subject.AddType<TestBullet>("bullet");
                    Subject.AddFiringFunction("bullet", "fire", bus, bs => bs.FireBasicBullet(0, 0, 0, 0, 0));

                    // Act: call method
                    Subject.InterpretCode(code, State, null, false);

                    // Verify: stack has negated dummy value on it
                    State.StackHead.Should().Be(0);
                    State.CurrentInstructionIndex.Should().Be(12);
                    bus.Bullets[0].IsActive.Should().BeTrue();
                    bus.Bullets[0].X.Should().Be(1);
                    bus.Bullets[0].Y.Should().Be(2);
                    bus.Bullets[0].Angle.Should().Be(3);
                    bus.Bullets[0].Speed.Should().Be(4);
                    bus.Bullets[0].SpriteIndex.Should().Be(5);
                }
            }
        }
    }
}