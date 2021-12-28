using System.Collections.Generic;

namespace EzAspDotNet.Protocols.Page
{
    public class PageResponse<T> : ResponseHeader
    {
        public long Total { get; set; }

        public List<T> Contents { get; set; }
    }
}
