using MongoDbWebUtil.Util;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
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

        public async Task Execute(FilterDefinition<Notification.Models.Notification> filter, string title, string content, List<string> imageUrls = null)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications)
            {
                if (!notification.ContainsKeyword(title))
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
                        _discordWebHooks.Add(DiscordNotify(notification, content, imageUrls));
                        break;
                    case NotificationType.Slack:
                        _slackWebHooks.Add(SlackNotify(notification, content, imageUrls));
                        break;
                    default:
                        throw new DeveloperException(ResultCode.NotImplementedYet);
                }
            }
        }

        private Notification.Protocols.Request.SlackWebHook SlackNotify(Notification.Models.Notification notification, string content, List<string> imageUrls = null)
        {
            return new Notification.Protocols.Request.SlackWebHook
            {
                UserName = notification.Name,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Text = content,
                HookUrl = notification.HookUrl
            }.AddImage(imageUrls);
        }

        private Notification.Protocols.Request.DiscordWebHook DiscordNotify(Notification.Models.Notification notification, string content, List<string> imageUrls = null)
        {
            return new Notification.Protocols.Request.DiscordWebHook
            {
                UserName = notification.Name,
                AvatarUrl = notification.IconUrl,
                Content = content,
                HookUrl = notification.HookUrl
            }.AddImage(imageUrls);
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
