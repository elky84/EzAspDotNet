using Newtonsoft.Json;

namespace EzAspDotNet.Notification.Protocols.Request
{
    public class SlackWebHook
    {
#pragma warning disable IDE1006 // Naming Styles
        public string text { get; set; }

        public string username { get; set; }

        public string icon_url { get; set; }

        public string channel { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        [JsonIgnore]
        public string HookUrl { get; set; }
    }
}
