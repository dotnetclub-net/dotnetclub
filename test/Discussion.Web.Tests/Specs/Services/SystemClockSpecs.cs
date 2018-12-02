using System;
using System.Threading;
using Discussion.Core.Time;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class SystemClockSpecs
    {
        [Fact]
        public void should_get_system_utc_time()
        {
            var utcTime = DateTime.UtcNow;
            Thread.Sleep(new Random().Next(10, 50));
            var got = new SystemClock().Now;

            var span = (got.UtcDateTime - utcTime).TotalMilliseconds;
            Assert.True(span > 0);
            Assert.True(span < 100);
        }
        
        [Fact]
        public void should_get_correct_offset_time()
        {
            var localTime = DateTime.Now;
            var got = new SystemClock().Now;

            var span = localTime - localTime.ToUniversalTime();
            Assert.Equal(span, got.Offset);
        }
    }
}