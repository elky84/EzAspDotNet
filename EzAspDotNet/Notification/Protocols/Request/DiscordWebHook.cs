using EzAspDotNet.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class DiscordWebHook
    {

        public class EmbedImage
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public class EmbedAuthor
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }
        }

        public class EmbedFooter
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }
        }

        public class EmbedField
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("inline")]
            public bool Inline { get; set; }

            public static EmbedField Convert(Data.Field field)
            {
                return new EmbedField
                {
                    Name = field.Title,
                    Value = field.Value,
                    Inline = field.Short,
                };
            }
        }

        public class Embed
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("color")]
            public int Color { get; set; }

            [JsonProperty("author")]
            public EmbedAuthor Author { get; set; }

            [JsonProperty("image")]
            public EmbedImage Image { get; set; }

            [JsonProperty("timestamp")]
            public string TimeStamp { get; set; }

            [JsonProperty("footer")]
            public EmbedFooter Footer { get; set; }

            [JsonProperty("fields")]
            public List<EmbedField> Fields { get; set; } = new();
        }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("embeds")]
        public List<Embed> Embeds { get; set; }

        [JsonIgnore]
        public string HookUrl { get; set; }

        public DiscordWebHook Clone()
        {
            return new DiscordWebHook
            {
                AvatarUrl = AvatarUrl,
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
                   Embeds == other.Embeds &&
                   UserName == other.UserName &&
                   HookUrl == other.HookUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Embeds, UserName, HookUrl);
        }

        public static Embed Convert(Data.WebHook webHook)
        {
            return new Embed
            {
                Title = webHook.Title,
                Url = webHook.TitleLink,
                Description = webHook.Text,
                Author = new EmbedAuthor
                {
                    IconUrl = webHook.AuthorIcon,
                    Name = webHook.Author,
                    Url = webHook.AuthorLink,
                },
                TimeStamp = webHook.TimeStamp.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.000Z"),
                Footer = new EmbedFooter
                {
                    IconUrl = webHook.FooterIcon,
                    Text = webHook.Footer
                },
                Image = new EmbedImage
                {
                    Url = webHook.ImageUrl,
                },
                Color = int.Parse(webHook.Color.Substring(1), System.Globalization.NumberStyles.HexNumber),
                Fields = webHook.Fields.ConvertAll(x => EmbedField.Convert(x))
            };
        }
    }
}
