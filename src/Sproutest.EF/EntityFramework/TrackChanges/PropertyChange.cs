using System;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.TrackChanges
{
  public class PropertyChange
  {
    public required string PropertyName { get; init; }

    public required Type PropertyType { get; init; }

    public required PropertyInfo PropertyInfo { get; init; }

    public required PropertyChangeStateType ChangeState { get; init; }

    public object OriginalValue { get; init; }

    public object CurrentValue { get; init; }
  }
}
