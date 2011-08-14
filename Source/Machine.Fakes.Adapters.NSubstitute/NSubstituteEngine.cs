﻿using System;
using System.Linq.Expressions;
using Machine.Fakes.Sdk;
using NSubstitute;

namespace Machine.Fakes.Adapters.NSubstitute
{
    /// <summary>
    ///   An implementation of <see cref = "IFakeEngine" />
    ///   using the NSubstitute framework.
    /// </summary>
    public class NSubstituteEngine : RewritingFakeEngine
    {
        public NSubstituteEngine() : base(new NSubstituteExpressionRewriter())
        {
        }

        public override object CreateFake(Type interfaceType, params object[] args)
        {
            return Substitute.For(new[] { interfaceType }, args);
        }

        public override T PartialMock<T>(params object[] args) 
        {
            return Substitute.For<T>(args);
        }

        protected override IQueryOptions<TReturnValue> OnSetUpQueryBehaviorFor<TFake, TReturnValue>(
            TFake fake,
            Expression<Func<TFake, TReturnValue>> func) 
        {
            return new NSubstituteQueryOptions<TFake, TReturnValue>(fake, func);
        }

        protected override ICommandOptions OnSetUpCommandBehaviorFor<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) 
        {
            return new NSubstituteCommandOptions<TFake>(fake, func);
        }

        protected override void OnVerifyBehaviorWasNotExecuted<TFake>(
            TFake fake,
            Expression<Action<TFake>> func)
        {
            func.Compile().Invoke(fake.DidNotReceive());
        }

        public override void RaiseEvent<TFake>(TFake fake, Action<TFake> registerEvent)
        {
            registerEvent.Invoke(fake);
        }

        public override EventHandler<EventArgs> WireItUp<TFake>(TFake fake, EventArgs e)
        {
            return Raise.EventWith(fake, e);
        }

        protected override IMethodCallOccurance OnVerifyBehaviorWasExecuted<TFake>(
            TFake fake,
            Expression<Action<TFake>> func) 
        {
            return new NSubstituteMethodCallOccurance<TFake>(fake, func);
        }
    }
}