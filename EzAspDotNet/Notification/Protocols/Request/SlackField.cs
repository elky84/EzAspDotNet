using Newtonsoft.Json;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class SlackField
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("short")]
        public bool Short { get; set; }

        public static SlackField Convert(Data.Field field)
        {
            return new SlackField
            {
                Title = field.Title,
                Value = field.Value,
                Short = field.Short,
            };
        }
    }
}