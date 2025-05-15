using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moq;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sencinet.DigitalJourney.TestingLibrary.Utils;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.References
{
    internal class TestReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> : ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity>
        where TPrincipalEntity : class
        where TDependentEntity : class
    {
        private IMutableForeignKey _metadata;
        private EntityProperty _entityProperty;
        private ConfigurationResult<TDependentEntity> _dependentEntityConfiguration;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestReferenceCollectionBuilder()
            : base(default, default, default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public override IMutableForeignKey Metadata => _metadata;

        public static TestReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> CreateInstance(
            EntityProperty entityProperty,
            ConfigurationResult<TDependentEntity> dependentEntityConfiguration)
        {
            ArgumentNullException.ThrowIfNull(entityProperty, nameof(entityProperty));
            ArgumentNullException.ThrowIfNull(dependentEntityConfiguration, nameof(dependentEntityConfiguration));

            var instance = ActivatorHelper.CreateUninitialized<TestReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity>>();
            instance._metadata = CreateMetadataMock();
            instance._entityProperty = entityProperty;
            instance._dependentEntityConfiguration = dependentEntityConfiguration;

            return instance;
        }

        public override ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey(params string[] foreignKeyPropertyNames)
        {
            _entityProperty.PropertyForeignKey = _dependentEntityConfiguration.GetProperty(foreignKeyPropertyNames[0]);
            return this;
        }

        public override ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
            => this;

        public override ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> IsRequired(bool required = true)
            => this;

        public override ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> OnDelete(DeleteBehavior deleteBehavior)
            => this;

        private static IMutableForeignKey CreateMetadataMock()
            => Mock.Of<IMutableForeignKey>();
    }
}
