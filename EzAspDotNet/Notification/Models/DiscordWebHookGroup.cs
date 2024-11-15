using System.Collections.Generic;

namespace EzAspDotNet.Notification.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DiscordWebHookGroup
    {
        public List<string> GroupedIds { get; set; } = [];

        public Protocols.Request.DiscordWebHook Data { get; set; }
    }
}
