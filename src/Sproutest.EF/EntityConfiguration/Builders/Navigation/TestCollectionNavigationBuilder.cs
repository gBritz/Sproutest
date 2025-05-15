using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.References;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sencinet.DigitalJourney.TestingLibrary.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.Navigation
{
    internal class TestCollectionNavigationBuilder<T, TRelatedEntity> : CollectionNavigationBuilder<T, TRelatedEntity>
        where T : class
        where TRelatedEntity : class
    {
        private DbContextTestCollection _tests;
        private EntityProperty _entityProperty;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestCollectionNavigationBuilder()
            : base(default, default, default, default, default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public static TestCollectionNavigationBuilder<T, TRelatedEntity> CreateInstance(DbContextTestCollection tests, EntityProperty entityProperty)
        {
            ArgumentNullException.ThrowIfNull(tests, nameof(tests));
            ArgumentNullException.ThrowIfNull(entityProperty, nameof(entityProperty));

            var instance = ActivatorHelper.CreateUninitialized<TestCollectionNavigationBuilder<T, TRelatedEntity>>();
            instance._tests = tests;
            instance._entityProperty = entityProperty;

            return instance;
        }

        public override CollectionCollectionBuilder<TRelatedEntity, T> WithMany(string navigationName = null)
            => TestCollectionCollectionBuilder<TRelatedEntity, T>.CreateInstance();

        public override CollectionCollectionBuilder<TRelatedEntity, T> WithMany(Expression<Func<TRelatedEntity, IEnumerable<T>>> navigationExpression)
        {
            var builder = _tests.GetBuilder<TRelatedEntity>();
            builder.Configure(navigationExpression, c => c.IsReference = true);
            return TestCollectionCollectionBuilder<TRelatedEntity, T>.CreateInstance();
        }

        public override ReferenceCollectionBuilder<T, TRelatedEntity> WithOne(string navigationName = null)
            => TestReferenceCollectionBuilder<T, TRelatedEntity>.CreateInstance(_entityProperty, _tests.GetBuilder<TRelatedEntity>().EntityConfiguration);

        public override ReferenceCollectionBuilder<T, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, T>> navigationExpression)
        {
            var relatedBuilder = _tests.GetBuilder<TRelatedEntity>();
            relatedBuilder.Configure(navigationExpression, c => c.IsReference = true);

            return TestReferenceCollectionBuilder<T, TRelatedEntity>.CreateInstance(_entityProperty, relatedBuilder.EntityConfiguration);
        }
    }
}
