using Newtonsoft.Json;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class SlackWebHook
    {
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("attachments")]
        public List<SlackAttachment> Attachments { get; set; } = new();

        [JsonIgnore]
        public string HookUrl { get; set; }

        public SlackWebHook AddMessage(SlackAttachment slackAttachment)
        {
            Attachments.Add(slackAttachment);
            return this;
        }

        public SlackWebHook AddImage(List<string> imageUrls)
        {
            if (imageUrls == null)
            {
                return this;
            }

            foreach (var imageUrl in imageUrls)
            {
                Attachments.Add(new SlackAttachment
                {
                    ImageUrl = imageUrl
                });
            }

            return this;
        }
    }
}
