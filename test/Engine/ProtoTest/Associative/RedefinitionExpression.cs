using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class RedefinitionExpression
    {
        public TestFrameWork thisTest = new TestFrameWork();
        public ProtoCore.Core core;
        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }


        [Test]
        public void RedefineWithFunctions01()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1001);
        }

        [Test]
        [Category("ToFixYuKe")]
        public void RedefineWithFunctions02()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
        }

        [Test]
        public void RedefineWithFunctions03()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
        }
        //TestCase from Mark//

        [Test]
        public void RedefineWithFunctions04()
        {
            String code =
@"def f1(i : int, k : int)
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 20);
        }

        [Test]
        public void RedefineWithFunctions05()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 36);
        }

        [Test]
        public void RedefineWithExpressionLists01()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
        }

        [Test]
        public void RedefineWithExpressionLists02()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
        }
        //Mark TestCases//

        [Test]
        [Category("ToFixJun")]
        public void RedefineWithExpressionLists03()
        {
            String code =
@"
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 3);
        }

        [Test]
        public void RedefineWithExpressionLists04()
        {
            String code =
@"
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 12 });
        }
    }
}