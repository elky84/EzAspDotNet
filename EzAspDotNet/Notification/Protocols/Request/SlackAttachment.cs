using Newtonsoft.Json;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class SlackAttachment
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_link")]
        public string TitleLink { get; set; }

        [JsonProperty("ts")]
        public long TimeStamp { get; set; }

        [JsonProperty("footer")]
        public string Footer { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("footer_icon")]
        public string FooterIcon { get; set; } = "https://platform.slack-edge.com/img/default_application_icon.png";

        [JsonProperty("color")]
        public string Color { get; set; } = "#2eb886";
    }
}
