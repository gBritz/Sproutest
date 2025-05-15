using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration
{
    internal class DbContextTestCollection : IEnumerable<EntityTestEntry>
    {
        private readonly Dictionary<Type, EntityTestEntry> _entities = new();

        public void Add(Type type, ITestEntityTypeBuilder builder, object configuration)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            if (builder.GetType().GetGenericArguments().FirstOrDefault() != type)
            {
                throw new ArgumentException($"Unable to cast builder to {nameof(TestEntityTypeBuilder<object>)}<{type.Name}>.", nameof(builder));
            }

            var configurationType = typeof(IEntityTypeConfiguration<>).MakeGenericType(type);

            if (!configuration.GetType().IsAssignableTo(configurationType))
            {
                throw new ArgumentException($"Unable to cast configuration to {nameof(IEntityTypeConfiguration<object>)}<{type.Name}>.", nameof(configuration));
            }

            _entities[type] = new EntityTestEntry(type, builder, configuration);
        }

        public TestEntityTypeBuilder<T> GetBuilder<T>()
            where T : class
        {
            var entry = _entities.GetValueOrDefault(typeof(T));
            return (TestEntityTypeBuilder<T>)entry?.Builder
                ?? throw new Exception($"Builder found for entity type \"{typeof(T).FullName}\"");
        }

        public IEnumerator<EntityTestEntry> GetEnumerator() => _entities.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
