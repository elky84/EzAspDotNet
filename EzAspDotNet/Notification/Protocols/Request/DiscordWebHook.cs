using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class DiscordWebHook
    {

        public class EmbedImage
        {
            [JsonProperty("Image")]
            public Dictionary<string, string> image { get; set; } = new Dictionary<string, string>();
        }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("embeds")]
        public List<object> Embeds = new List<object>();


        [JsonIgnore]
        public string HookUrl { get; set; }

        public DiscordWebHook AddImage(List<string> imageUrls)
        {
            if (imageUrls == null)
            {
                return this;
            }

            foreach (var imageUrl in imageUrls)
            {
                var embedImage = new EmbedImage();
                embedImage.image.Add("url", imageUrl);
                Embeds.Add(embedImage);
            }

            return this;
        }

        public DiscordWebHook Clone()
        {
            return new DiscordWebHook
            {
                AvatarUrl = AvatarUrl,
                Content = Content,
                Embeds = Embeds,
                HookUrl = HookUrl,
                UserName = UserName
            };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordWebHook);
        }

        public bool Equals(DiscordWebHook other)
        {
            return other != null &&
                   Content == other.Content &&
                   UserName == other.UserName &&
                   HookUrl == other.HookUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Content, UserName, HookUrl);
        }
    }
}
