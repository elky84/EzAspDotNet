using EzAspDotNet.Exception;
using EzAspDotNet.HttpClient;
using EzAspDotNet.Notification.Data;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Notification.Protocols.Request;
using EzAspDotNet.Notification.Types;
using EzAspDotNet.Protocols.Code;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordWebHook = EzAspDotNet.Notification.Models.DiscordWebHook;
using SlackWebHook = EzAspDotNet.Notification.Models.SlackWebHook;

namespace EzAspDotNet.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WebHookService
    {
        private readonly HttpClientService _httpClientService;

        private readonly MongoDbUtil<DiscordWebHook> _mongoDbDiscordWebHook;
        private readonly MongoDbUtil<Notification.Models.Notification> _mongoDbNotification;
        private readonly MongoDbUtil<SlackWebHook> _mongoDbSlackWebHook;

        public WebHookService(MongoDbService mongoDbService,
            HttpClientService httpClientService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification.Models.Notification>(mongoDbService.Database);
            _mongoDbDiscordWebHook = new MongoDbUtil<DiscordWebHook>(mongoDbService.Database);
            _mongoDbSlackWebHook = new MongoDbUtil<SlackWebHook>(mongoDbService.Database);

            _httpClientService = httpClientService;

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification.Models.Notification>(
                Builders<Notification.Models.Notification>.IndexKeys.Ascending(x => x.SourceId)
                    .Ascending(x => x.Type)));
        }

        private async Task<List<Notification.Models.Notification>> Get(
            FilterDefinition<Notification.Models.Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task Execute(FilterDefinition<Notification.Models.Notification> filter,
            WebHook webHook)
        {
            await Execute(filter, [webHook]);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public async Task Execute(FilterDefinition<Notification.Models.Notification> filter,
            List<WebHook> webHooks)
        {
            var notifications = await Get(filter);
            foreach (var notification in notifications.Where(notification => !notification.FilteredTime(DateTime.Now)))
            {
                var filteredWebHooks = webHooks.Where(x => notification.CheckFilterKeyword(x.Title) &&
                                                           notification.ContainsKeyword(x.Title))
                    .Select(x =>
                    {
                        if (!string.IsNullOrEmpty(notification.Prefix))
                            x.Title = notification.Prefix + x.Title;

                        if (!string.IsNullOrEmpty(notification.Postfix))
                            x.Title += notification.Postfix;

                        return x;
                    }).ToList();

                if (filteredWebHooks.Count <= 0)
                    continue;

                switch (notification.Type)
                {
                    case NotificationType.Discord:
                    {
                        await _mongoDbDiscordWebHook.CreateAsync(new DiscordWebHook
                        {
                            Data = DiscordNotify(notification, filteredWebHooks)
                        });
                    }
                        break;
                    case NotificationType.Slack:
                    {
                        await _mongoDbSlackWebHook.CreateAsync(new SlackWebHook
                        {
                            Data = SlackNotify(notification, filteredWebHooks)
                        });
                    }
                        break;
                    default:
                        throw new DeveloperException(ResultCode.NotImplementedYet);
                }
            }
        }

        private static Notification.Protocols.Request.SlackWebHook SlackNotify(
            Notification.Models.Notification notification,
            List<WebHook> webHooks)
        {
            return new Notification.Protocols.Request.SlackWebHook
            {
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                HookUrl = notification.HookUrl,
                UserName = notification.Name,
                Attachments = webHooks.ConvertAll(SlackAttachment.Convert)
            };
        }

        private static Notification.Protocols.Request.DiscordWebHook DiscordNotify(
            Notification.Models.Notification notification,
            List<WebHook> webHooks)
        {
            return new Notification.Protocols.Request.DiscordWebHook
            {
                UserName = notification.Name,
                AvatarUrl = notification.IconUrl,
                HookUrl = notification.HookUrl,
                Embeds = webHooks.ConvertAll(Notification.Protocols.Request.DiscordWebHook.Convert)
            };
        }

        private async Task ProcessSlackWebHooks()
        {
            var webHooks = await _mongoDbSlackWebHook.FindAsync();
            var webHookGroups = webHooks
                .GroupBy(w => new { w.Data.Channel, w.Data.UserName })
                .SelectMany(g =>
                {
                    var batches = g.Select((webhook, index) => new
                        {
                            webhook,
                            index
                        })
                        .GroupBy(x => x.index / 50, x => x.webhook);

                    var processedBatches = new List<SlackWebHookGroup>();

                    foreach (var batch in batches)
                    {
                        var firstWebhookWithEmbedsAndIds = new SlackWebHookGroup
                        {
                            Data = batch.First().Data,
                            GroupedIds = batch.Select(b => b.Id).ToList()
                        };

                        foreach (var webhook in batch.Skip(1))
                        {
                            firstWebhookWithEmbedsAndIds.Data.Attachments.AddRange(webhook.Data.Attachments);
                        }

                        processedBatches.Add(firstWebhookWithEmbedsAndIds);
                    }

                    return processedBatches;
                })
                .ToList();

            foreach (var webHookGroup in webHookGroups)
            {
                try
                {
                    var response =
                        await _httpClientService.Factory.RequestJson(HttpMethod.Post, webHookGroup.Data.HookUrl,
                            webHookGroup.Data);
                    if (!response.IsSuccessStatusCode)
                    {
                        Log.Logger.Error("SlackWebHook Failed. <WebHookUrl:{WebHookHookUrl}> <Response:{ResponseStatusCode}>",
                            webHookGroup.Data.HookUrl, response.StatusCode);
                        return;
                    }

                    foreach (var id in webHookGroup.GroupedIds)
                        await _mongoDbSlackWebHook.RemoveAsync(id);
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                    Thread.Sleep(1000);
                }
            }
        }

        private async Task ProcessDiscordWebHooks()
        {
            var webHooks = await _mongoDbDiscordWebHook.FindAsync();
            var webHookGroups = webHooks
                .GroupBy(w => new { w.Data.HookUrl, w.Data.UserName })
                .SelectMany(g =>
                {
                    var batches = g.Select((webhook, index) => new
                        {
                            webhook,
                            index
                        })
                        .GroupBy(x => x.index / 50, x => x.webhook);

                    var processedBatches = new List<DiscordWebHookGroup>();

                    foreach (var batch in batches)
                    {
                        var firstWebhookWithEmbedsAndIds = new DiscordWebHookGroup
                        {
                            Data = batch.First().Data,
                            GroupedIds = batch.Select(b => b.Id).ToList()
                        };

                        foreach (var webhook in batch.Skip(1))
                        {
                            firstWebhookWithEmbedsAndIds.Data.Embeds.AddRange(webhook.Data.Embeds);
                        }

                        processedBatches.Add(firstWebhookWithEmbedsAndIds);
                    }

                    return processedBatches;
                })
                .ToList();

            foreach (var webHook in webHooks)
            {
                try
                {
                    var response = _httpClientService.Factory
                        .RequestJson(HttpMethod.Post, webHook.Data.HookUrl, webHook.Data).Result;
                    if (response?.Headers == null)
                    {
                        Thread.Sleep(1000);
                        return;
                    }

                    if (!response.Headers.Contains("x-ratelimit-remaining") ||
                        !response.Headers.Contains("x-ratelimit-reset-after"))
                    {
                        Thread.Sleep(1000);
                        return;
                    }

                    var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                    var rateLimitAfter = response.Headers.GetValues("x-ratelimit-reset-after").FirstOrDefault().ToInt();
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                            Log.Logger.Error(
                                "Too Many Requests [{WebHookHookUrl}] [{RateLimitRemaining}, {RateLimitAfter}]",
                                webHook.Data.HookUrl, rateLimitRemaining, rateLimitAfter);

                        return;
                    }

                    if (rateLimitRemaining <= 1 || rateLimitAfter > 0)
                        Thread.Sleep((rateLimitAfter + 1) * 1000);

                    await _mongoDbDiscordWebHook.RemoveAsync(webHook.Id);
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                    Thread.Sleep(1000);
                }
            }

        }

        public async Task HttpTaskRun()
        {
            await ProcessDiscordWebHooks();
            await ProcessSlackWebHooks();
        }
    }
}
