using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.AutoMock;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration
{
    public class DbContextConfigurationValidator
    {
        private readonly DbContextTestCollection _tests;

        public DbContextConfigurationValidator(IEnumerable<EntityTypeInfo> entities)
        {
            _tests = InitilizeEntityTests(entities);
        }

        public ICollection<IConfigurationResult> Run()
        {
            var results = new List<IConfigurationResult>();

            foreach (var (_, builder, configuration) in _tests)
            {
                configuration
                    .GetType()
                    .GetMethod(nameof(IEntityTypeConfiguration<object>.Configure))!
                    .Invoke(configuration, [builder]);

                results.Add(builder.Result);
            }

            return results;
        }

        private static DbContextTestCollection InitilizeEntityTests(IEnumerable<EntityTypeInfo> entities)
        {
            var tests = new DbContextTestCollection();
            var mocker = new AutoMocker(MockBehavior.Loose);

            foreach (var entity in entities)
            {
                var configuration = mocker.CreateInstance(entity.ConfigurationType);

                var builder = (ITestEntityTypeBuilder)typeof(TestEntityTypeBuilder<>)
                    .MakeGenericType(entity.EntityType)
                    .GetMethod(
                        nameof(TestEntityTypeBuilder<object>.CreateInstance),
                        BindingFlags.Public | BindingFlags.Static)!
                    .Invoke(null, new object[] { tests });

                tests.Add(entity.EntityType, builder, configuration);
            }

            return tests;
        }
    }
}
