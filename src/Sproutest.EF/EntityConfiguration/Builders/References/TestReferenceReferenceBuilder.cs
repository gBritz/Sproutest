using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sencinet.DigitalJourney.TestingLibrary.Utils;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.References
{
    internal class TestReferenceReferenceBuilder<TEntity, TRelatedEntity> : ReferenceReferenceBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        private ITestEntityTypeBuilder _builder;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestReferenceReferenceBuilder()
            : base(default, default, default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public static TestReferenceReferenceBuilder<TEntity, TRelatedEntity> CreateInstance(ITestEntityTypeBuilder builder)
        {
            var instance = ActivatorHelper.CreateUninitialized<TestReferenceReferenceBuilder<TEntity, TRelatedEntity>>();
            instance._builder = builder;
            return instance;
        }

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
            => this;

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(params string[] foreignKeyPropertyNames)
            => this;

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool required = true)
            => this;

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => this;
    }
}
