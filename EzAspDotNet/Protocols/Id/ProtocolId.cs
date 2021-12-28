using EzAspDotNet.Common;

namespace EzAspDotNet.Protocols.Id
{
    public abstract class ProtocolId
    : Enumeration
    {
        protected ProtocolId(int id, string name) : base(id, name)
        {
        }
    }
}
