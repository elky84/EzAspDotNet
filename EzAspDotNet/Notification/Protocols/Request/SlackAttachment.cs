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

        [JsonProperty("author_name")]
        public string Author { get; set; }

        [JsonProperty("author_link")]
        public string AuthorLink { get; set; }

        [JsonProperty("author_icon")]
        public string AuthorIcon { get; set; }

        [JsonProperty("ts")]
        public long TimeStamp { get; set; }

        [JsonProperty("footer")]
        public string Footer { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; set; }

        [JsonProperty("footer_icon")]
        public string FooterIcon { get; set; } = "https://platform.slack-edge.com/img/default_application_icon.png";

        [JsonProperty("color")]
        public string Color { get; set; } = "#2eb886";

        [JsonProperty("fields")]
        public List<SlackField> Fields { get; set; } = new();

        public static SlackAttachment Convert(Data.WebHook webHook)
        {
            return new SlackAttachment
            {
                Text = webHook.Text,
                Title = webHook.Title,
                TitleLink = webHook.TitleLink,
                Author = webHook.Author,
                AuthorLink = webHook.AuthorLink,
                AuthorIcon = webHook.AuthorIcon,
                TimeStamp = webHook.TimeStamp,
                Footer = webHook.Footer,
                ImageUrl = webHook.ImageUrl,
                ThumbUrl = webHook.ThumbUrl,
                FooterIcon = webHook.FooterIcon,
                Color = webHook.Color,
                Fields = webHook.Fields.ConvertAll(x => SlackField.Convert(x))
            };
        }
    }
}
