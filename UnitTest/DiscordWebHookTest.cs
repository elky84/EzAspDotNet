using EzAspDotNet.HttpClient;
using EzAspDotNet.Notification.Protocols.Request;
using EzAspDotNet.Util;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnitTest
{
    public class DiscordWebHookTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test()
        {
            HttpClient httpClient = new();

            var color = int.Parse("#2eb886".Substring(1), System.Globalization.NumberStyles.HexNumber);

            var response = await httpClient.Request<string>(HttpMethod.Post, "https://discord.com/api/webhooks/757093882747158622/m2mrsgUWaPIzi8K7DjuRTLmUZet5VuNllhKRkekhcx_MSNHSRTdmuSISq67HlwN1ehJ6",
                new DiscordWebHook
                {
                    UserName = "elky",
                    AvatarUrl = "https://platform.slack-edge.com/img/default_application_icon.png",
                    Embeds = new()
                    {
                        new DiscordWebHook.Embed
                        {
                            Title = "title test",
                            Url = "https://op.gg",
                            Description = "test desc",
                            Author = new()
                            {
                                Name = "elky_author",
                                IconUrl = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
                                Url = "https://op.gg"
                            },
                            Color = color,
                            Fields = new()
                            {
                                new()
                                {
                                    Name = "elky_name",
                                    Value = "value~~~"
                                },
                                new()
                                {
                                    Name = "elky_name2",
                                    Value = "value~~~2"
                                }
                            },
                            Footer = new ()
                            {
                                Text = "footer_text",
                                IconUrl = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png"
                            },
                            Image = new() { Url = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png" },
                            TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000Z")
                        }
                    }
                });
        }

        [Test]
        public async Task UseConverter()
        {
            HttpClient httpClient = new();

            var response = await httpClient.Request<string>(HttpMethod.Post, "https://discord.com/api/webhooks/757093882747158622/m2mrsgUWaPIzi8K7DjuRTLmUZet5VuNllhKRkekhcx_MSNHSRTdmuSISq67HlwN1ehJ6",
                new DiscordWebHook
                {
                    UserName = "elky",
                    AvatarUrl = "https://platform.slack-edge.com/img/default_application_icon.png",
                    Embeds = new()
                    {
                        DiscordWebHook.Convert(new EzAspDotNet.Notification.Data.WebHook
                        {
                            Title = "title test",
                            TitleLink = "https://op.gg",
                            Text = "test desc",
                            Author = "elky_author",
                            AuthorIcon = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
                            AuthorLink = "https://op.gg",
                            Color = "#2eb886",
                            Fields = new()
                            {
                                new()
                                {
                                    Title = "elky_name",
                                    Value = "value~~~"
                                },
                                new()
                                {
                                    Title = "elky_name2",
                                    Value = "value~~~2"
                                }
                            },
                            Footer = "footer_text",
                            FooterIcon = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
                            ImageUrl = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
                            TimeStamp = DateTime.UtcNow.ToTimeStamp()
                        })
                    }
                });
        }
    }
}