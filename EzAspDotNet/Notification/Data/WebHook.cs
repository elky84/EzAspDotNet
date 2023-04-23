using System.Collections.Generic;

namespace EzAspDotNet.Notification.Data;

public class WebHook
{
    public string Text { get; init; }

    public string Title { get; set; }

    public string TitleLink { get; init; }

    public string Author { get; init; }

    public string AuthorLink { get; init; }

    public string AuthorIcon { get; init; }

    public long? TimeStamp { get; init; }

    public string Footer { get; init; }

    public string ImageUrl { get; init; }

    public string ThumbUrl { get; init; }

    public string FooterIcon { get; init; } = "https://platform.slack-edge.com/img/default_application_icon.png";

    public string Color { get; init; } = "#2eb886";

    public List<Field> Fields { get; init; } = new();
}