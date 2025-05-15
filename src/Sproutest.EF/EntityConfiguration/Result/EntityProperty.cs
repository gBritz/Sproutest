using System.Diagnostics;
using System.Reflection;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result
{
    [DebuggerDisplay("{DisplayName}")]
    public class EntityProperty : IRelatedPropertyInfo
    {
        private readonly Type _propertyType;

        public EntityProperty(Type propertyType)
        {
            ArgumentNullException.ThrowIfNull(propertyType, nameof(propertyType));

            _propertyType = propertyType;
        }

        public EntityProperty(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property, nameof(property));

            Info = property;
        }

        public string DisplayName => Info is null ? ColumnName : $"{Info.ReflectedType?.Name}.{Info.Name}";

        public PropertyInfo? Info { get; internal set; }

        public Type PropertyType => Info?.PropertyType ?? _propertyType;

        public bool IsConfigured { get; internal set; }

        public bool IsIgnored { get; internal set; }

        public bool IsKey { get; internal set; }

        public bool IsReference { get; internal set; }

        public string ColumnName { get; internal set; }

        public bool IsShadow => Info is null;

        public PropertyInfo? PropertyForeignKey { get; set; }
    }
}
