using System;
using System.Linq.Expressions;
using Machine.Fakes.Sdk;
using Rhino.Mocks;

namespace Machine.Fakes.Adapters.Rhinomocks
{
    public class RhinoFakeEngine : RewritingFakeEngine
    {
        public RhinoFakeEngine() : base(new RhinoMocksExpressionRewriter())
        {
        }

        public override object CreateFake(Type interfaceType, params object[] args)
        {
            var stub = MockRepository.GenerateStub(interfaceType, args);
            stub.Replay();
            return stub;
        }

        public override T PartialMock<T>(params object[] args)
        {
            var mock = MockRepository.GenerateMock<T>(args);
            mock.Replay();
            return mock;
        }

        protected override IQueryOptions<TReturnValue> OnSetUpQueryBehaviorFor<TDependency, TReturnValue>(
            TDependency fake,
            Expression<Func<TDependency, TReturnValue>> func)
        {
            var compiledFunction = func.Compile();

            return new RhinoQueryOptions<TReturnValue>(fake.Stub(f => compiledFunction(f)));
        }

        protected override ICommandOptions OnSetUpCommandBehaviorFor<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) 
        {
            var compiledFunction = func.Compile();

            return new RhinoCommandOptions(fake.Stub(compiledFunction));
        }

        protected override void OnVerifyBehaviorWasNotExecuted<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) 
        {
            var compiledFunction =  func.Compile();

            fake.AssertWasNotCalled(compiledFunction);
        }

        public override void RaiseEvent<TFake>(TFake fake, Action<TFake> registerEvent)
        {
            fake.Raise(registerEvent, fake, _eventArgs);
        }

        EventArgs _eventArgs;
        public override EventHandler<EventArgs> WireItUp<TFake>(TFake fake, EventArgs e)
        {
            _eventArgs = e;
            return null;
        }

        protected override IMethodCallOccurance OnVerifyBehaviorWasExecuted<TFake>(
            TFake fake, 
            Expression<Action<TFake>> func)
        {
            var compiledFunction = func.Compile();

            return new RhinoMethodCallOccurance<TFake>(fake, compiledFunction);
        }
    }
}