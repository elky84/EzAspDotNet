using System;
using System.Collections.Generic;
using System.Text;

namespace EzAspDotNet.Protocols.Page
{
    public class Pageable
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;

        public string Sort { get; set; }

        public bool Asc { get; set; }
    }
}
