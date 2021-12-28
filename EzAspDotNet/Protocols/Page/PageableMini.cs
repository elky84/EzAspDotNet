using System;
using System.Collections.Generic;
using System.Text;

namespace EzAspDotNet.Protocols.Page
{
    public class PageableMini
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;
    }
}
