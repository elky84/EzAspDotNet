using EzAspDotNet.Util;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using Serilog;
using EzAspDotNet.Exception;
using EzAspDotNet.Notification.Types;
using System;
using EzAspDotNet.HttpClient;

namespace EzAspDotNet.Services
{
    public class WebHookService
    {
        private readonly MongoDbUtil<Notification.Models.Notification> _mongoDbNotification;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ConcurrentBag<Notification.Protocols.Request.DiscordWebHook> _discordWebHooks = new();

        private readonly ConcurrentBag<Notification.Protocols.Request.SlackWebHook> _slackWebHooks = new();

        public WebHookService(MongoDbService mongoDbService,
            IHttpClientFactory httpClientFactory)
        {
            _mongoDbNotification = new MongoDbUtil<Notification.Models.Notification>(mongoDbService.Database);
            _httpClientFactory = httpClientFactory;

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
                if (webHooks.Any(x => !notification.ContainsKeyword(x.Title)))
                {
                    continue;
                }

                if (notification.FilteredTime(DateTime.Now))
                {
                    continue;
                }

                switch (notification.Type)
                {
                    case NotificationType.Discord:
                        {
                            var origin = _discordWebHooks.FirstOrDefault(x => x.HookUrl == notification.HookUrl);
                            if(origin != null)
                            {
                                lock(origin.Embeds)
                                    origin.Embeds.AddRange(webHooks.ConvertAll(x => Notification.Protocols.Request.DiscordWebHook.Convert(x)));
                            }
                            else
                            {
                                _discordWebHooks.Add(DiscordNotify(notification, webHooks));
                            }
                        }
                        break;
                    case NotificationType.Slack:
                        {
                            var origin = _slackWebHooks.FirstOrDefault(x => x.HookUrl == notification.HookUrl &&
                                                                            x.Channel == notification.Channel);
                            if (origin != null)
                            {
                                lock (origin.Attachments)
                                    origin.Attachments.AddRange(webHooks.ConvertAll(x => Notification.Protocols.Request.SlackAttachment.Convert(x)));
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

        private Notification.Protocols.Request.SlackWebHook SlackNotify(Notification.Models.Notification notification,
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

        private Notification.Protocols.Request.DiscordWebHook DiscordNotify(Notification.Models.Notification notification,
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

            Parallel.ForEach(cloneList, webHook =>
            {
                var response = _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook).Result;
                if(!response.IsSuccessStatusCode)
                {
                    _slackWebHooks.Add(webHook);
                }

                Thread.Sleep(1000); // 초당 한개...라고함.
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
                    var response = _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook).Result;
                    if (response == null || response.Headers == null)
                    {
                        _discordWebHooks.Add(webHook);
                        Thread.Sleep(1000);
                        return;
                    }

                    if(!response.Headers.Contains("x-ratelimit-remaining") ||
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
