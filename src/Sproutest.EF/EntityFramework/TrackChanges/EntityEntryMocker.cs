using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sproutest.EF.EntityFramework.TrackChanges
{
  public class EntityEntryMocker
  {
    public static ICollection<EntityEntry> MockFor(Dictionary<Type, EntityChange> types) =>
      types.Select(type => MockEntityEntryByProperties(type.Key, type.Value)).ToArray();

    private static EntityEntry MockEntityEntryByProperties(Type entityType, EntityChange entityChange)
    {
      ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
      ArgumentNullException.ThrowIfNull(entityChange, nameof(entityChange));

      var runtimeEntity = new RuntimeEntityType(
        entityType.FullName,
        entityType,
        false,
        null,
        null,
        null,
        ChangeTrackingStrategy.Snapshot,
        null,
        false,
        null);

      List<(PropertyChange Change, RuntimeProperty Runtime)> addedProperties = [];
      foreach (var propertyChange in entityChange.Properties)
      {
        var property = runtimeEntity.AddProperty(
          propertyChange.PropertyName,
          propertyChange.PropertyType,
          propertyChange.PropertyInfo,
          null,
          PropertyAccessMode.PreferProperty,
          propertyChange.PropertyType.IsGenericType && propertyChange.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>));
        addedProperties.Add((propertyChange, property));
      }

      var internalEntityEntry = new InternalEntityEntry(
        new Mock<IStateManager>().Object,
        runtimeEntity,
        Activator.CreateInstance(entityType));

      List<PropertyEntry> propertyEntries = [];
      foreach (var addedProperty in addedProperties)
      {
        var mockPropertyEntry = new Mock<PropertyEntry>(internalEntityEntry, addedProperty.Runtime)
        {
          CallBase = true,
        };

        mockPropertyEntry.Setup(m => m.IsModified)
          .Returns(addedProperty.Change.ChangeState == PropertyChangeStateType.Modified);
        mockPropertyEntry.Setup(m => m.IsTemporary)
          .Returns(addedProperty.Change.ChangeState == PropertyChangeStateType.Temporary);
        mockPropertyEntry.Setup(m => m.OriginalValue)
          .Returns(addedProperty.Change.OriginalValue);
        mockPropertyEntry.Setup(m => m.CurrentValue)
          .Returns(addedProperty.Change.CurrentValue);

        propertyEntries.Add(mockPropertyEntry.Object);
      }

      var mockEntityEntry = new Mock<EntityEntry>(internalEntityEntry)
      {
        CallBase = true,
      };

      mockEntityEntry.Setup(m => m.State)
        .Returns(entityChange.State);

      mockEntityEntry.Setup(m => m.Properties)
        .Returns(propertyEntries);

      return mockEntityEntry.Object;
    }
  }
}
