using System;

namespace Discussion.Core.Time
{
    public interface IClock
    {
        DateTimeOffset Now { get; }
    }
}