using EzAspDotNet.Protocols;

namespace EzAspDotNet.Exception
{
    public class ErrorDetails : ResponseHeader
    {
        public string Detail { get; set; }
    }
}
