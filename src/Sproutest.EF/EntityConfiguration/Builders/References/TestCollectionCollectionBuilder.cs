using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sencinet.DigitalJourney.TestingLibrary.Utils;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders.References
{
    internal class TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
        : CollectionCollectionBuilder<TLeftEntity, TRightEntity>
        where TLeftEntity : class
        where TRightEntity : class
    {
        [SuppressMessage(
            "Usage",
            "EF1001:Internal EF Core API usage.",
            Justification = "This constructor is not used.")]
        private TestCollectionCollectionBuilder()
            : base(default, default, default, default)
        {
            // This class should be instantiated using the CreateInstance() method.
        }

        public static TestCollectionCollectionBuilder<TLeftEntity, TRightEntity> CreateInstance()
            => ActivatorHelper.CreateUninitialized<TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>>();
    }
}
