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
using EzAspDotNet.Code;
using System;
using EzAspDotNet.HttpClient;

namespace EzAspDotNet.Services
{
    public class WebHookService
    {
        private readonly MongoDbUtil<Notification.Models.Notification> _mongoDbNotification;

        private readonly IHttpClientFactory _httpClientFactory;


        private readonly List<Notification.Protocols.Request.DiscordWebHook> _discordWebHooks =
            new List<Notification.Protocols.Request.DiscordWebHook>();

        private readonly List<Notification.Protocols.Request.SlackWebHook> _slackWebHooks =
            new List<Notification.Protocols.Request.SlackWebHook>();

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
                            var origin = _slackWebHooks.FirstOrDefault(x => x.HookUrl == notification.HookUrl);
                            if (origin != null)
                            {
                                origin.Attachments.AddRange(webHooks.ConvertAll(x => Notification.Protocols.Request.SlackAttachment.Convert(x)));
                            }
                            else
                            {
                                _slackWebHooks.Add(SlackNotify(notification, webHooks));
                            }
                        }
                        break;
                    default:
                        throw new DeveloperException(ResultCode.NotImplementedYet);
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
            var processList = new ConcurrentBag<Notification.Protocols.Request.SlackWebHook>();
            Parallel.ForEach(_slackWebHooks.GroupBy(x => x.Channel), group =>
            {
                foreach (var webHook in group.Select(x => x))
                {
                    _ = _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook);
                    Thread.Sleep(1000); // 초당 한개...라고함.
                    processList.Add(webHook);
                }
            });

            foreach (var process in processList)
            {
                _slackWebHooks.Remove(process);
            }
        }

        private void ProcessDiscordWebHooks()
        {
            var processList = new ConcurrentBag<Notification.Protocols.Request.DiscordWebHook>();
            Parallel.ForEach(_discordWebHooks.GroupBy(x => x.HookUrl).Select(x => x.Select(y => y.Clone())).ToList(), group =>
            {
                foreach (var webHook in group)
                {
                    try
                    {
                        var response = _httpClientFactory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook).Result;
                        if (response == null || response.Headers == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        if(!response.Headers.Contains("x-ratelimit-remaining") ||
                            !response.Headers.Contains("x-ratelimit-reset-after"))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                        var rateLimitAfter = response.Headers.GetValues("x-ratelimit-reset-after").FirstOrDefault().ToInt();
                        if (response.IsSuccessStatusCode)
                        {
                            processList.Add(webHook);
                        }

                        if (rateLimitRemaining <= 1 || rateLimitAfter > 0)
                        {
                            Thread.Sleep((rateLimitAfter + 1) * 1000);
                            continue;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Log.Logger.Error($"Too Many Requests [{webHook.HookUrl}] [{rateLimitRemaining}, {rateLimitAfter}]");
                            break;
                        }
                    }
                    catch (System.Exception e)
                    {
                        e.ExceptionLog();
                        Thread.Sleep(1000);
                        break;
                    }
                }
            });

            foreach (var process in processList)
            {
                _discordWebHooks.Remove(process);
            }
        }

        public void HttpTaskRun()
        {
            ProcessDiscordWebHooks();
            ProcessSlackWebHooks();
        }
    }
}
