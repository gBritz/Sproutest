using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.References;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sencinet.DigitalJourney.TestingLibrary.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.Navigation
{
    internal class TestReferenceNavigationBuilder<TEntity, TRelatedEntity> : ReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        private DbContextTestCollection _tests;
        private EntityProperty _entityProperty;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestReferenceNavigationBuilder()
            : base(default, default, default(string), default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public static TestReferenceNavigationBuilder<TEntity, TRelatedEntity> CreateInstance(DbContextTestCollection tests, EntityProperty entityProperty)
        {
            ArgumentNullException.ThrowIfNull(tests, nameof(tests));
            ArgumentNullException.ThrowIfNull(entityProperty, nameof(entityProperty));

            var instance = ActivatorHelper.CreateUninitialized<TestReferenceNavigationBuilder<TEntity, TRelatedEntity>>();
            instance._tests = tests;
            instance._entityProperty = entityProperty;

            return instance;
        }

        public override ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(string navigationName = null)
            => TestReferenceCollectionBuilder<TRelatedEntity, TEntity>.CreateInstance(_entityProperty, _tests.GetBuilder<TEntity>().EntityConfiguration);

        public override ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression)
        {
            var principalBuilder = _tests.GetBuilder<TEntity>();
            var builder = _tests.GetBuilder<TRelatedEntity>();
            builder.Configure(navigationExpression, c => c.IsReference = true);
            return TestReferenceCollectionBuilder<TRelatedEntity, TEntity>.CreateInstance(_entityProperty, principalBuilder.EntityConfiguration);
        }

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(string navigationName = null)
        {
            var builder = _tests.GetBuilder<TRelatedEntity>();
            return TestReferenceReferenceBuilder<TEntity, TRelatedEntity>.CreateInstance(builder);
        }

        public override ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> navigationExpression)
        {
            var builder = _tests.GetBuilder<TRelatedEntity>();
            builder.Configure(navigationExpression, c => c.IsReference = true);
            return TestReferenceReferenceBuilder<TEntity, TRelatedEntity>.CreateInstance(builder);
        }
    }
}
