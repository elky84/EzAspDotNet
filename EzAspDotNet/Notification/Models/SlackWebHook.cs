using System;
using System.Collections.Generic;
using System.Linq;
using EzAspDotNet.Notification.Protocols.Request;
using EzAspDotNet.Notification.Types;
using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EzAspDotNet.Notification.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class SlackWebHook : MongoDbHeader
{
    public Protocols.Request.SlackWebHook Data { get; set; }
}