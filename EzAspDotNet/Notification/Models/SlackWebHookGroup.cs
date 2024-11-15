using System.Collections.Generic;

namespace EzAspDotNet.Notification.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SlackWebHookGroup
    {
        public List<string> GroupedIds { get; set; } = [];

        public Protocols.Request.SlackWebHook Data { get; set; }
    }
}
