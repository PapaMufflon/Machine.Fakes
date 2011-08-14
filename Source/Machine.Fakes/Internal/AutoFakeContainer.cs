using System;
using System.Linq.Expressions;
using Machine.Fakes.Sdk;
using StructureMap;
using StructureMap.AutoMocking;

namespace Machine.Fakes.Internal
{
    sealed class AutoFakeContainer<TSubject> :
        ServiceLocator,
        IFakeEngine where TSubject : class
    {
        readonly StructureMapAutoMockerAdapter<TSubject> _autoMocker;
        readonly IFakeEngine _fakeEngine;

        public AutoFakeContainer(IFakeEngine fakeEngine)
        {
            Guard.AgainstArgumentNull(fakeEngine, "fakeEngine");

            _fakeEngine = fakeEngine;
            _autoMocker = new StructureMapAutoMockerAdapter<TSubject>(this);
        }

        public object CreateFake(Type interfaceType, params object[] args)
        {
            return _fakeEngine.CreateFake(interfaceType, args);
        }

        public IQueryOptions<TReturnValue> SetUpQueryBehaviorFor<TFake, TReturnValue>(
            TFake fake,
            Expression<Func<TFake, TReturnValue>> func) where TFake : class
        {
            return _fakeEngine.SetUpQueryBehaviorFor(fake, func);
        }

        public ICommandOptions SetUpCommandBehaviorFor<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) where TFake : class
        {
            return _fakeEngine.SetUpCommandBehaviorFor(fake, func);
        }

        public void VerifyBehaviorWasNotExecuted<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) where TFake : class
        {
            _fakeEngine.VerifyBehaviorWasNotExecuted(fake, func);
        }

        public IMethodCallOccurance VerifyBehaviorWasExecuted<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) where TFake : class
        {
            return _fakeEngine.VerifyBehaviorWasExecuted(fake, func);
        }

        public void RaiseEvent<TFake>(TFake fake, Action<TFake> registerEvent) where TFake : class
        {
            _fakeEngine.RaiseEvent(fake, registerEvent);
        }

        public EventHandler<EventArgs> WireItUp<TFake>(TFake fake, EventArgs e) where TFake : class
        {
            return _fakeEngine.WireItUp(fake, e);
        }

        public T PartialMock<T>(params object[] args) where T : class
        {
            return _fakeEngine.PartialMock<T>(args);
        }

        T ServiceLocator.Service<T>()
        {
            return _fakeEngine.Stub<T>();
        }

        object ServiceLocator.Service(Type serviceType)
        {
            return _fakeEngine.CreateFake(serviceType);
        }

        public TSubject CreateSubject()
        {
            try
            {
                return _autoMocker.ClassUnderTest;
            }
            catch (StructureMapException ex)
            {
                throw new SubjectCreationException(typeof(TSubject), ex);
            }
        }

        public TFakeSingleton Get<TFakeSingleton>() where TFakeSingleton : class
        {
            return _autoMocker.Get<TFakeSingleton>();
        }

        public void Inject(Type contract, object implementation)
        {
            _autoMocker.Inject(contract, implementation);
        }

        public void Register(Registrar registar)
        {
            _autoMocker.Register(registar);
        }
    }
}