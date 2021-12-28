using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using EzAspDotNet.Code;
using EzAspDotNet.Protocols.Id;
using EzAspDotNet.Swagger;

namespace EzAspDotNet.Protocols
{
    public class ResponseHeader
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultCode ResultCode { get; set; } = ResultCode.Success;

        public string ErrorMessage { get; set; }

        public virtual ProtocolId ProtocolId { get; set; }

        [SwaggerExcludeAttribute]
        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
