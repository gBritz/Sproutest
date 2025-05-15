using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moq;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.Navigation;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sencinet.DigitalJourney.TestingLibrary.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders
{
    internal class TestIndexBuilder<T> : IndexBuilder<T>
    {
        private IMutableIndex _metadata;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        public TestIndexBuilder() : base(default)
        {
        }

        public override IMutableIndex Metadata => _metadata;

        public static TestIndexBuilder<T> CreateInstance()
        {
            var instance = ActivatorHelper.CreateUninitialized<TestIndexBuilder<T>>();
            instance._metadata = instance.CreateMetadataMock();
            return instance;
        }

        public override IndexBuilder<T> IsUnique(bool unique = true)
            => this;

        private IMutableIndex CreateMetadataMock()
        {
            var mock = new Mock<IMutableIndex>();

            mock.Setup(x => x.FindAnnotation(It.IsAny<string>()))
                .Returns(() => Mock.Of<IAnnotation>());

            mock.Setup(x => x.SetAnnotation(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string name, object value) =>
                {
                    if (name == RelationalAnnotationNames.TableName)
                    {
                        //_result.SetTableName(value as string);
                    }
                });

            return mock.Object;
        }
    }

    internal class TestEntityTypeBuilder<T> : EntityTypeBuilder<T>, ITestEntityTypeBuilder
        where T : class
    {
        private ConfigurationResult<T> _result;
        private DbContextTestCollection _tests;
        private IMutableEntityType _metadata;

        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestEntityTypeBuilder()
            : base(default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        internal ConfigurationResult<T> EntityConfiguration => _result;

        public IConfigurationResult Result => _result;

        public override IMutableEntityType Metadata => _metadata;

        public static TestEntityTypeBuilder<T> CreateInstance(DbContextTestCollection tests)
        {
            var instance = ActivatorHelper.CreateUninitialized<TestEntityTypeBuilder<T>>();
            instance._result = new(typeof(T));
            instance._tests = tests;
            instance._metadata = instance.CreateMetadataMock();
            return instance;
        }

        public override KeyBuilder HasKey(Expression<Func<T, object>> keyExpression)
        {
            _result.AddKey(keyExpression);
            return null;
        }

        public override KeyBuilder<T> HasKey(params string[] propertyNames)
        {
            _result.AddKey(propertyNames);
            return null;
        }

        public override CollectionNavigationBuilder<T, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<T, IEnumerable<TRelatedEntity>>> navigationExpression = null)
            where TRelatedEntity : class
        {
            var property = _result.Configure(navigationExpression, isReference: true);
            return TestCollectionNavigationBuilder<T, TRelatedEntity>.CreateInstance(_tests, property);
        }

        public override EntityTypeBuilder<T> HasNoKey()
        {
            _result.HasNoKey();
            return this;
        }

        public override ReferenceNavigationBuilder<T, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<T, TRelatedEntity>> navigationExpression = null)
            where TRelatedEntity : class
        {
            var property = _result.Configure(navigationExpression, isReference: true);
            return TestReferenceNavigationBuilder<T, TRelatedEntity>.CreateInstance(_tests, property);
        }

        public override EntityTypeBuilder<T> Ignore(Expression<Func<T, object>> propertyExpression)
        {
            _result.Ignore(propertyExpression);
            return this;
        }

        public override PropertyBuilder Property(Type propertyType, string propertyName)
        {
            EntityProperty property = _result.Configure(propertyType, propertyName);
            return TestPropertyBuilder.CreateInstance(property);
            /*
            return typeof(TestPropertyBuilder<>)
                .MakeGenericType(propertyType)
                .GetMethod("CreateInstance", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, [property]) as PropertyBuilder;
            */
        }

        public override PropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => TestPropertyBuilder<TProperty>.CreateInstance(null);

        public override PropertyBuilder<TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var property = _result.Configure(propertyExpression);
            return TestPropertyBuilder<TProperty>.CreateInstance(property);
        }

        public override IndexBuilder<T> HasIndex(Expression<Func<T, object?>> indexExpression)
            => TestIndexBuilder<T>.CreateInstance();

        public override EntityTypeBuilder<T> OwnsOne<TRelatedEntity>(
            Expression<Func<T, TRelatedEntity?>> navigationExpression,
            Action<OwnedNavigationBuilder<T, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => this; // TODO: trabalhar na propriedade buildAction para capturar relacionamento.

        public override DataBuilder<T> HasData(params T[] data)
            => new(); // TODO: trabalhar para capturar o dado e utilizar junto no seed

        internal void Configure<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Action<EntityProperty> action)
        {
            var property = _result.Configure(propertyExpression);
            action(property);
        }

        internal void Configure<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> propertyExpression, Action<EntityProperty> action)
        {
            var property = _result.Configure(propertyExpression);
            action(property);
        }

        private IMutableEntityType CreateMetadataMock()
        {
            var mock = new Mock<IMutableEntityType>();

            mock.Setup(x => x.FindNavigation(It.IsAny<string>()))
                .Returns(() => Mock.Of<IMutableNavigation>());

            mock.Setup(x => x.SetAnnotation(It.IsAny<string>(), It.IsAny<object>()))
                .Callback((string name, object value) =>
                {
                    if (name == RelationalAnnotationNames.TableName)
                    {
                        _result.SetTableName(value as string);
                    }
                });

            return mock.Object;
        }
    }
}
