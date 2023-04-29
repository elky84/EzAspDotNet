using System;
using System.Collections.Generic;
using System.Linq;
using EzAspDotNet.Notification.Types;
using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EzAspDotNet.Notification.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class Notification : MongoDbHeader
{
    [BsonRepresentation(BsonType.String)] public NotificationType Type { get; init; }

    public string Name { get; init; }

    public string HookUrl { get; init; }

    public string Channel { get; init; }

    public string IconUrl { get; init; }

    public string SourceId { get; set; }


    public string CrawlingType { get; init; }

    [BsonRepresentation(BsonType.String)] private List<DayOfWeek> FilterDayOfWeeks { get; } = new();

    public string Prefix { get; init; }

    public string Postfix { get; init; }


    public string Keyword { get; init; }

    public string FilterKeyword { get; init; }

    public string FilterStartTime { get; init; }

    public string FilterEndTime { get; init; }

    public bool ContainsKeyword(string check)
    {
        return string.IsNullOrEmpty(Keyword) || Keyword.Split("|").Any(check.Contains);
    }

    public bool CheckFilterKeyword(string check)
    {
        return string.IsNullOrEmpty(FilterKeyword) || !FilterKeyword.Split("|").Any(check.Contains);
    }

    public bool FilteredTime(DateTime dateTime)
    {
        if (FilterDayOfWeeks == null || !FilterDayOfWeeks.Contains(dateTime.DayOfWeek)) return false;

        if (string.IsNullOrEmpty(FilterStartTime) || string.IsNullOrEmpty(FilterEndTime)) return false;

        var startTime = DateTime.Parse(FilterStartTime);
        var endTime = DateTime.Parse(FilterEndTime);

        return startTime < dateTime && endTime > dateTime;
    }
}