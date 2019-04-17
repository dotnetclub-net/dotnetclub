using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace Discussion.Core.ETag
{
    public class ETagBuilder : ITagBuilder
    {
        public EntityTagHeaderValue EntityTagBuild(DateTime fileModifiedAtUtc, long fileSize)
        {
            var entityTag = new EntityTagHeaderValue("\"CalculatedEtagValue\"");
            var lastOffset = ToDateTimeOffset(DateTime.SpecifyKind(fileModifiedAtUtc, DateTimeKind.Utc));
            var _lastModified = new DateTimeOffset(lastOffset.Year, lastOffset.Month, lastOffset.Day,
                            lastOffset.Hour, lastOffset.Minute, lastOffset.Second, lastOffset.Offset);
            var etagHash = _lastModified.ToFileTime() ^ fileSize;
            entityTag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');
            return entityTag;
        }
        private static DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        {
            return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
                       ? DateTimeOffset.MinValue
                       : new DateTimeOffset(dateTime);
        }
    }
}
