using System;
using System.Diagnostics;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration
{
    [DebuggerDisplay("{EntityType.Name}")]
    public class EntityTypeInfo
    {
        public Type EntityType { get; init; }

        public Type ConfigurationType { get; init; }
    }
}
