using System;

namespace Discussion.Core.Time
{
    public class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}