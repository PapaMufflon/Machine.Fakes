using System;
using System.Linq.Expressions;
using FakeItEasy;
using Machine.Fakes.Sdk;

namespace Machine.Fakes.Adapters.FakeItEasy
{
    public class FakeItEasyEngine : RewritingFakeEngine
    {
        public FakeItEasyEngine() : base(new FakeItEasyExpressionRewriter())
        {
        }

        public override object CreateFake(Type interfaceType, params object[] args)
        {
            var closedFakeType = typeof(Fake<>).MakeGenericType(interfaceType);
            var objectProperty = closedFakeType.GetProperty("FakedObject", interfaceType);

            var options = args != null && args.Length > 0 
                ? FakeItEasyHelper.CreateForType(interfaceType, args) : null;

            var instance = args != null && args.Length > 0 
                ? Activator.CreateInstance(closedFakeType, new object[] {options}) 
                : Activator.CreateInstance(closedFakeType);

            return objectProperty.GetValue(instance, null);
        }

        public override T PartialMock<T>(params object[] args) 
        {
            return A.Fake<T>(f => f.WithArgumentsForConstructor(args));
        }

        protected override IQueryOptions<TReturnValue> OnSetUpQueryBehaviorFor<TFake, TReturnValue>(
            TFake fake, 
            Expression<Func<TFake, TReturnValue>> func) 
        {
            var queryExpression = func.WrapExpression(fake);

            var configuration = A.CallTo(queryExpression);

            return new FakeItEasyQueryOptions<TReturnValue>(configuration);
        }

        protected override ICommandOptions OnSetUpCommandBehaviorFor<TFake>(
            TFake fake, 
            Expression<Action<TFake>> func) 
        {
            var callExpression = func.WrapExpression(fake);
            var configuration = A.CallTo(callExpression);

            return new FakeItEasyCommandOptions(configuration);
        }

        public override void RaiseEvent<TFake>(TFake fake, Action<TFake> registerEvent)
        {
            registerEvent.Invoke(fake);
        }

        public override EventHandler<EventArgs> WireItUp<TFake>(TFake fake, EventArgs e)
        {
            return Raise.With(e).Now;
        }

        protected override IMethodCallOccurance OnVerifyBehaviorWasExecuted<TFake>(
            TFake fake, 
            Expression<Action<TFake>> func) 
        {
            var callExpression = func.WrapExpression(fake);
            var configuration = A.CallTo(callExpression);

            return new FakeItEasyMethodCallOccurance(configuration);
        }

        protected override void OnVerifyBehaviorWasNotExecuted<TFake>(
            TFake fake, 
            Expression<Action<TFake>> func) 
        {
            var callExpression = func.WrapExpression(fake);
            
            A.CallTo(callExpression).MustNotHaveHappened();
        }
    }
}