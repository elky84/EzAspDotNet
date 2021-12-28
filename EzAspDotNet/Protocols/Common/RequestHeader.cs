using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EzAspDotNet.Protocols.Id;
using EzAspDotNet.Swagger;

namespace EzAspDotNet.Protocols
{
    public class RequestHeader
    {
        public virtual ProtocolId ProtocolId { get; set; }

        [SwaggerExcludeAttribute]
        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
