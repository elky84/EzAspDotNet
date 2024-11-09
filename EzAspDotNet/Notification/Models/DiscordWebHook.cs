using EzMongoDb.Models;

namespace EzAspDotNet.Notification.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class DiscordWebHook : MongoDbHeader
{
    public Protocols.Request.DiscordWebHook Data { get; set; }
}