using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sencinet.DigitalJourney.TestingLibrary.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders
{
    internal class TestPropertyBuilder : PropertyBuilder
    {
        private EntityProperty _property;
        private IMutableProperty _metadata;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        public TestPropertyBuilder() : base(default)
        {
        }

        public override IMutableProperty Metadata => _metadata;

        public static TestPropertyBuilder CreateInstance(EntityProperty property)
        {
            var instance = ActivatorHelper.CreateUninitialized<TestPropertyBuilder>();
            instance._property = property;
            instance._metadata = instance.CreateMetadataMock();
            return instance;
        }

        public override PropertyBuilder IsRequired(bool required = true)
            => this;

        private IMutableProperty CreateMetadataMock()
        {
            var mock = new Mock<IMutableProperty>();

            mock.Setup(x => x.SetOrRemoveAnnotation(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string name, object value) =>
                {
                    if (name == RelationalAnnotationNames.ColumnName)
                    {
                        _property.ColumnName = value as string;
                    }
                });

            mock.SetupGet(x => x.ClrType)
                .Returns(_property.PropertyType);

            return mock.Object;
        }
    }

    internal class TestPropertyBuilder<TProperty> : PropertyBuilder<TProperty>
    {
        private EntityProperty _property;
        private IMutableProperty _metadata;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        public TestPropertyBuilder()
            : base(default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public override IMutableProperty Metadata => _metadata;

        public static TestPropertyBuilder<TProperty> CreateInstance(EntityProperty property)
        {
            var instance = ActivatorHelper.CreateUninitialized<TestPropertyBuilder<TProperty>>();
            instance._property = property;
            instance._metadata = instance.CreateMetadataMock();
            return instance;
        }

        public override PropertyBuilder<TProperty> HasConversion<TProvider>()
            => this;

        public override PropertyBuilder<TProperty> HasConversion<TProvider>(Expression<Func<TProperty, TProvider>> convertToProviderExpression, Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => this;

        public override PropertyBuilder<TProperty> HasMaxLength(int maxLength)
            => this;

        public override PropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            => this;

        public override PropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            => this;

        public override PropertyBuilder<TProperty> HasPrecision(int precision)
            => this;

        public override PropertyBuilder<TProperty> HasPrecision(int precision, int scale)
            => this;

        public override PropertyBuilder<TProperty> IsRequired(bool required = true)
            => this;

        public override PropertyBuilder<TProperty> ValueGeneratedOnAdd()
            => this;

        private IMutableProperty CreateMetadataMock()
        {
            var mock = new Mock<IMutableProperty>();

            mock.Setup(x => x.SetOrRemoveAnnotation(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string name, object value) =>
                {
                    if (name == RelationalAnnotationNames.ColumnName)
                    {
                        _property.ColumnName = value as string;
                    }
                });

            mock.SetupGet(x => x.ClrType)
                .Returns(typeof(TProperty));

            return mock.Object;
        }
    }
}
