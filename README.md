[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
![Made with](https://img.shields.io/badge/made%20with-.NET6-blue.svg)

![GitHub forks](https://img.shields.io/github/forks/elky84/EzAspDotNet.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/EzAspDotNet.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/EzAspDotNet.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/EzAspDotNet.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/EzAspDotNet.svg)


# EzAspDotNet

## introduce

Easily usable with ASP.NET (Core or later).

Implemented by C# .NET 6.

The purpose of this project is to easily build and operate a ASP.NET web server.

## features
* ElasticSearch usage to easy.
* RabbitMQ usage to easy.
* WebSocket usage to easy.
* ASP.NET Web API usage to easy.
  * Common Protocol (with Common Header), Common Spec, Easy Setup, Exception Handling, Validation
* Swagger usage to easy.
* MongoDB  usage to easy.
  * use [MongoDbWebUtil](https://github.com/elky84/MongoDbWebUtil)
* And more included C# utility codes

## nuget

<https://www.nuget.org/packages/EzAspDotNet/>

## version history

### v1.0.20
Improve Protocol header. (Removed similar variables)

### v1.0.19
Typo correction (ToIntRegx() -> ToIntRegex())

### v1.0.18
Support Alert optional field. (Notification.Data.WebHook)

### v1.0.17
Improved Slack Alarm Readability 

### v1.0.16

added general utility codes (from MongoDbWebUtil)
- LoopingService
- RepeatedService
- ClassUtil
- CollectionUtil
- HttpClientUtil (integrate to HttpClient.Extend)
- JsonUtil
- StringUtil (integrate)
- TupleUtil
- TypesUtil

### v1.0.14

features added WebHook (Slack, Discord)