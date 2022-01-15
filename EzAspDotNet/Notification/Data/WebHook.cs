using System.Collections.Generic;

namespace EzAspDotNet.Notification.Data
{
    public class WebHook
    {
        public string Text { get; set; }

        public string Title { get; set; }

        public string TitleLink { get; set; }

        public string Author { get; set; }

        public string AuthorLink { get; set; }

        public string AuthorIcon { get; set; }

        public long TimeStamp { get; set; }

        public string Footer { get; set; }

        public string ImageUrl { get; set; }

        public string ThumbUrl { get; set; }

        public string FooterIcon { get; set; } = "https://platform.slack-edge.com/img/default_application_icon.png";

        public string Color { get; set; } = "#2eb886";

        public List<Field> Fields { get; set; } = new();
    }
}
