using EzAspDotNet.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [JsonProperty("embeds")] public List<Embed> Embeds { get; set; } = [];

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
            var embed = new Embed
            {
                Title = webHook.Title,
                Url = NormalizeUrl(webHook.TitleLink),
                Description = string.IsNullOrWhiteSpace(webHook.Text)
                    ? webHook.Title
                    : webHook.Text,
                TimeStamp = webHook.TimeStamp?.ToDateTime().ToUniversalTime().ToString("o"),
                Color = ParseDiscordColor(webHook.Color),
                Fields = webHook.Fields?.ConvertAll(EmbedField.Convert) ?? []
            };

            if (!string.IsNullOrWhiteSpace(webHook.Author))
            {
                embed.Author = new EmbedAuthor
                {
                    Name = webHook.Author,
                    Url = NormalizeUrl(webHook.AuthorLink),
                    IconUrl = NormalizeUrl(webHook.AuthorIcon)
                };
            }

            if (!string.IsNullOrWhiteSpace(webHook.Footer))
            {
                embed.Footer = new EmbedFooter
                {
                    Text = webHook.Footer,
                    IconUrl = NormalizeUrl(webHook.FooterIcon)
                };
            }

            var imageUrl = NormalizeUrl(webHook.ImageUrl);
            if (imageUrl != null)
            {
                embed.Image = new EmbedImage
                {
                    Url = imageUrl
                };
            }

            return embed;

            string NormalizeUrl(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    return null;

                try
                {
                    var decoded = Uri.UnescapeDataString(url);
                    if (Uri.TryCreate(decoded, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        return decoded;
                    }
                }
                catch
                {
                    // ignored
                }

                return null;
            }
        }


        private static int ParseDiscordColor(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return 0;

            hex = hex.Trim();

            if ("#".StartsWith(hex))
                hex = hex[1..];

            if (hex.Length == 3)
            {
                hex = string.Concat(hex.Select(c => $"{c}{c}"));
            }

            if (hex.Length != 6)
                return 0;

            return int.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture,
                out var color) ? color : 0;
        }

    }
}
