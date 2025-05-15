using Microsoft.EntityFrameworkCore;

namespace Sproutest.EF.EntityFramework.TrackChanges;

public class DbTrackEntities(Dictionary<Type, EntityChange> types)
{
    private readonly Dictionary<Type, EntityChange> _typedProperties = types ?? throw new ArgumentNullException(nameof(types));

    public DbTrackEntity<T> OfType<T>(EntityState? state = null)
        where T : class
    {
        List<PropertyChange> properties = [];
        _typedProperties.Add(typeof(T), new()
        {
            Properties = properties,
            State = state ?? EntityState.Modified,
        });
        return new(properties);
    }
}