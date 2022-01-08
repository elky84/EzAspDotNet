using Newtonsoft.Json;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class SlackAttachment
    {
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}
