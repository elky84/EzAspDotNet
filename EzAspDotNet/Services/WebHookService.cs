using EzAspDotNet.Exception;
using EzAspDotNet.HttpClient;
using EzAspDotNet.Notification.Types;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EzAspDotNet.Services
{
    public class WebHookService
    {
        private readonly MongoDbUtil<Notification.Models.Notification> _mongoDbNotification;

        private readonly HttpClientService _httpClientService;

        private readonly ConcurrentBag<Notification.Protocols.Request.DiscordWebHook> _discordWebHooks = new();

        private readonly ConcurrentBag<Notification.Protocols.Request.SlackWebHook> _slackWebHooks = new();

        public WebHookService(MongoDbService mongoDbService,
            HttpClientService httpClientService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification.Models.Notification>(mongoDbService.Database);
            _httpClientService = httpClientService;

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification.Models.Notification>(
                Builders<Notification.Models.Notification>.IndexKeys.Ascending(x => x.SourceId)
                .Ascending(x => x.Type)));
        }

        public async Task<List<Notification.Models.Notification>> Get(FilterDefinition<Notification.Models.Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task Execute(FilterDefinition<Notification.Models.Notification> filter,
                   Notification.Data.WebHook webHook)
        {
            await Execute(filter, new List<Notification.Data.WebHook> { webHook });
        }

        public async Task Execute(FilterDefinition<Notification.Models.Notification> filter,
                   List<Notification.Data.WebHook> webHooks)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications)
            {
                if (webHooks.Any(x => !notification.ContainsFilterKeyword(x.Title)))
                {
                    continue;
                }

                if (notification.FilteredTime(DateTime.Now))
                {
                    continue;
                }

                webHooks.ForEach(x =>
                {
                    if (notification.ContainsKeyword(x.Title))
                    {
                        if (!string.IsNullOrEmpty(notification.Prefix))
                            x.Title = notification.Prefix + x.Title;

                        if (!string.IsNullOrEmpty(notification.Postfix))
                            x.Title += notification.Postfix;
                    }
                });

                switch (notification.Type)
                {
                    case NotificationType.Discord:
                        {
                            var origin = _discordWebHooks.LastOrDefault(x => x.HookUrl == notification.HookUrl);
                            if (origin != null)
                            {
                                lock (origin.Embeds)
                                {
                                    if (origin.Embeds.Count > 50)
                                        _discordWebHooks.Add(DiscordNotify(notification, webHooks));
                                    else
                                        origin.Embeds.AddRange(webHooks.ConvertAll(x => Notification.Protocols.Request.DiscordWebHook.Convert(x)));
                                }
                            }
                            else
                            {
                                _discordWebHooks.Add(DiscordNotify(notification, webHooks));
                            }
                        }
                        break;
                    case NotificationType.Slack:
                        {
                            var origin = _slackWebHooks.LastOrDefault(x => x.HookUrl == notification.HookUrl &&
                                                                            x.Channel == notification.Channel);
                            if (origin != null)
                            {
                                lock (origin.Attachments)
                                {
                                    if (origin.Attachments.Count > 50)
                                        _slackWebHooks.Add(SlackNotify(notification, webHooks));
                                    else
                                        origin.Attachments.AddRange(webHooks.ConvertAll(x => Notification.Protocols.Request.SlackAttachment.Convert(x)));
                                }
                            }
                            else
                            {
                                _slackWebHooks.Add(SlackNotify(notification, webHooks));
                            }
                        }
                        break;
                    default:
                        throw new DeveloperException(Protocols.Code.ResultCode.NotImplementedYet);
                }
            }
        }

        private static Notification.Protocols.Request.SlackWebHook SlackNotify(Notification.Models.Notification notification,
            List<Notification.Data.WebHook> webHooks)
        {
            return new Notification.Protocols.Request.SlackWebHook
            {
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                HookUrl = notification.HookUrl,
                UserName = notification.Name,
                Attachments = webHooks.ConvertAll(x => Notification.Protocols.Request.SlackAttachment.Convert(x))
            };
        }

        private static Notification.Protocols.Request.DiscordWebHook DiscordNotify(Notification.Models.Notification notification,
            List<Notification.Data.WebHook> webHooks)
        {
            return new Notification.Protocols.Request.DiscordWebHook
            {
                UserName = notification.Name,
                AvatarUrl = notification.IconUrl,
                HookUrl = notification.HookUrl,
                Embeds = webHooks.ConvertAll(x => Notification.Protocols.Request.DiscordWebHook.Convert(x))
            };
        }

        private void ProcessSlackWebHooks()
        {
            var cloneList = _slackWebHooks.ToList();
            _slackWebHooks.Clear();

            Parallel.ForEach(cloneList, async webHook =>
            {
                var response = await _httpClientService.Factory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Logger.Error($"SlackWebHook Failed. <WebHookUrl:{webHook.HookUrl}> <Response:{response.StatusCode}>");
                    _slackWebHooks.Add(webHook);
                }
            });
        }

        private void ProcessDiscordWebHooks()
        {
            var cloneList = _discordWebHooks.ToList();
            _discordWebHooks.Clear();

            Parallel.ForEach(cloneList, webHook =>
            {
                try
                {
                    var response = _httpClientService.Factory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook).Result;
                    if (response == null || response.Headers == null)
                    {
                        _discordWebHooks.Add(webHook);
                        Thread.Sleep(1000);
                        return;
                    }

                    if (!response.Headers.Contains("x-ratelimit-remaining") ||
                        !response.Headers.Contains("x-ratelimit-reset-after"))
                    {
                        _discordWebHooks.Add(webHook);
                        Thread.Sleep(1000);
                        return;
                    }

                    var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                    var rateLimitAfter = response.Headers.GetValues("x-ratelimit-reset-after").FirstOrDefault().ToInt();
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Log.Logger.Error($"Too Many Requests [{webHook.HookUrl}] [{rateLimitRemaining}, {rateLimitAfter}]");
                        }

                        _discordWebHooks.Add(webHook);
                    }

                    if (rateLimitRemaining <= 1 || rateLimitAfter > 0)
                    {
                        Thread.Sleep((rateLimitAfter + 1) * 1000);
                    }
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                    Thread.Sleep(1000);
                }
            });
        }

        public void HttpTaskRun()
        {
            ProcessDiscordWebHooks();
            ProcessSlackWebHooks();
        }
    }
}
