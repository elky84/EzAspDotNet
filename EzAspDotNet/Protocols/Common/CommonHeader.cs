using System;

namespace EzAspDotNet.Protocols.Common
{
    public class CommonHeader
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }
    }
}
