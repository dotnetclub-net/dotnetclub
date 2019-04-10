using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discussion.Core.ETag
{
    public interface ITagBuilder
    {
        EntityTagHeaderValue EntityTagBuild(DateTime fileModifiedAtUtc, long fileSize);
    }
}
