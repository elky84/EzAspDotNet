using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EzAspDotNet.Exception;
using EzAspDotNet.HttpClient;
using EzAspDotNet.Notification.Data;
using EzAspDotNet.Notification.Protocols.Request;
using EzAspDotNet.Notification.Types;
using EzAspDotNet.Protocols.Code;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;

namespace EzAspDotNet.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class WebHookService
{
    private readonly ConcurrentBag<DiscordWebHook> _discordWebHooks = new();

    private readonly HttpClientService _httpClientService;
    private readonly MongoDbUtil<Notification.Models.Notification> _mongoDbNotification;

    private readonly ConcurrentBag<SlackWebHook> _slackWebHooks = new();

    public WebHookService(MongoDbService mongoDbService,
        HttpClientService httpClientService)
    {
        _mongoDbNotification = new MongoDbUtil<Notification.Models.Notification>(mongoDbService.Database);
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
        await Execute(filter, new List<WebHook> { webHook });
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task Execute(FilterDefinition<Notification.Models.Notification> filter,
        List<WebHook> webHooks)
    {
        var notifications = await Get(filter);
        foreach (var notification in notifications.Where(notification =>
                         webHooks.All(x => notification.ContainsFilterKeyword(x.Title)))
                     .Where(notification => !notification.FilteredTime(DateTime.Now)))
        {
            webHooks.ForEach(x =>
            {
                if (!notification.ContainsKeyword(x.Title)) return;
                if (!string.IsNullOrEmpty(notification.Prefix))
                    x.Title = notification.Prefix + x.Title;

                if (!string.IsNullOrEmpty(notification.Postfix))
                    x.Title += notification.Postfix;
            });

            switch (notification.Type)
            {
                case NotificationType.Discord:
                {
                    var origin = _discordWebHooks.LastOrDefault(x => x.HookUrl == notification.HookUrl);
                    if (origin != null)
                        lock (origin.Embeds)
                        {
                            if (origin.Embeds.Count > 50)
                                _discordWebHooks.Add(DiscordNotify(notification, webHooks));
                            else
                                origin.Embeds.AddRange(webHooks.ConvertAll(DiscordWebHook.Convert));
                        }
                    else
                        _discordWebHooks.Add(DiscordNotify(notification, webHooks));
                }
                    break;
                case NotificationType.Slack:
                {
                    var origin = _slackWebHooks.LastOrDefault(x => x.HookUrl == notification.HookUrl &&
                                                                   x.Channel == notification.Channel);
                    if (origin != null)
                        lock (origin.Attachments)
                        {
                            if (origin.Attachments.Count > 50)
                                _slackWebHooks.Add(SlackNotify(notification, webHooks));
                            else
                                origin.Attachments.AddRange(webHooks.ConvertAll(SlackAttachment.Convert));
                        }
                    else
                        _slackWebHooks.Add(SlackNotify(notification, webHooks));
                }
                    break;
                default:
                    throw new DeveloperException(ResultCode.NotImplementedYet);
            }
        }
    }

    private static SlackWebHook SlackNotify(Notification.Models.Notification notification,
        List<WebHook> webHooks)
    {
        return new SlackWebHook
        {
            Channel = notification.Channel,
            IconUrl = notification.IconUrl,
            HookUrl = notification.HookUrl,
            UserName = notification.Name,
            Attachments = webHooks.ConvertAll(SlackAttachment.Convert)
        };
    }

    private static DiscordWebHook DiscordNotify(Notification.Models.Notification notification,
        List<WebHook> webHooks)
    {
        return new DiscordWebHook
        {
            UserName = notification.Name,
            AvatarUrl = notification.IconUrl,
            HookUrl = notification.HookUrl,
            Embeds = webHooks.ConvertAll(DiscordWebHook.Convert)
        };
    }

    private void ProcessSlackWebHooks()
    {
        var cloneList = _slackWebHooks.ToList();
        _slackWebHooks.Clear();

        Parallel.ForEach(cloneList, async webHook =>
        {
            var response = await _httpClientService.Factory.RequestJson(HttpMethod.Post, webHook.HookUrl, webHook);
            if (response.IsSuccessStatusCode) return;

            Log.Logger.Error("SlackWebHook Failed. <WebHookUrl:{WebHookHookUrl}> <Response:{ResponseStatusCode}>",
                webHook.HookUrl, response.StatusCode);
            _slackWebHooks.Add(webHook);
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
                if (response?.Headers == null)
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
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        Log.Logger.Error(
                            "Too Many Requests [{WebHookHookUrl}] [{RateLimitRemaining}, {RateLimitAfter}]",
                            webHook.HookUrl, rateLimitRemaining, rateLimitAfter);

                    _discordWebHooks.Add(webHook);
                }

                if (rateLimitRemaining <= 1 || rateLimitAfter > 0) Thread.Sleep((rateLimitAfter + 1) * 1000);
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