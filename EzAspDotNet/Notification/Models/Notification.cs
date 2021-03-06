using EzAspDotNet.Notification.Types;
using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzAspDotNet.Notification.Models
{
    public class Notification : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
        public NotificationType Type { get; set; }

        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public string SourceId { get; set; }

        public string CrawlingType { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<DayOfWeek> FilterDayOfWeeks { get; set; } = new List<DayOfWeek>();
        public string Keyword { get; set; }

        public string Prefix { get; set; }

        public string Postfix { get; set; }

        public string FilterKeyword { get; set; }

        public string FilterStartTime { get; set; }

        public string FilterEndTime { get; set; }

        public bool ContainsKeyword(string check)
        {
            if (string.IsNullOrEmpty(Keyword))
            {
                return false;
            }

            return Keyword.Split("|").Any(x => check.Contains(x));
        }

        public bool ContainsFilterKeyword(string check)
        {
            if (string.IsNullOrEmpty(FilterKeyword))
            {
                return true;
            }

            return FilterKeyword.Split("|").Any(x => check.Contains(x));
        }

        public bool FilteredTime(DateTime dateTime)
        {
            if (FilterDayOfWeeks == null || !FilterDayOfWeeks.Contains(dateTime.DayOfWeek))
            {
                return false;
            }

            if (string.IsNullOrEmpty(FilterStartTime) || string.IsNullOrEmpty(FilterEndTime))
            {
                return false;
            }

            var startTime = DateTime.Parse(FilterStartTime);
            var endTime = DateTime.Parse(FilterEndTime);

            return startTime < dateTime && endTime > dateTime;
        }
    }
}
