[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
![Made with](https://img.shields.io/badge/made%20with-.NET8-blue.svg)

[![Publish Nuget Github Package](https://github.com/elky84/EzAspDotNet/actions/workflows/publish_github.yml/badge.svg)](https://github.com/elky84/EzAspDotNet/actions/workflows/publish_github.yml)
[![Publish Nuget Package](https://github.com/elky84/EzAspDotNet/actions/workflows/publish_nuget.yml/badge.svg)](https://github.com/elky84/EzAspDotNet/actions/workflows/publish_nuget.yml)

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

Implemented by C# .NET 8

The purpose of this project is to easily build and operate a ASP.NET web server.

## features

* ElasticSearch usage to easy.
* RabbitMQ usage to easy.
* WebSocket usage to easy.
* ASP.NET Web API usage to easy.
    * Common Protocol (with Common Header), Common Spec, Easy Setup, Exception Handling, Validation
* Swagger usage to easy.
* MongoDB usage to easy.
* And more included C# utility codes

## ENVIRONMENT Variable (high priority option)

* MONGODB_CONNECTION
    * MongoDB Connection String
* MONGODB_DATABASE
    * MongoDB Database Name

## nuget

<https://www.nuget.org/packages/EzAspDotNet/>

## version history

### v1.0.59

fixed WebHookLoopingService HttpTaskRun to Async (await)

### v1.0.58

webhook use in-memory to mongo db.

### v1.0.57

use .NET 8

### v1.0.55, v1.0.56

Recovery of the ability to blow web hooks only for data containing specific keywords

### v1.0.53, v1.0.54

Fixed not operation filterKeyword errors.
And remove keyword variable in Notification.

### v1.0.52

Added error message on SlackWebHook failed.

### v1.0.51

Support forcing mongodb connection string and database settings via environment variables

### v1.0.50

Improve exception mandling & message.

### v1.0.49

Remove JsonUtil.ConvertTo method.

### v1.0.48

change string.IsNullOrEmpty return false to true on ContainsKeyword in Notification (webhook)

### v1.0.47

added notification prefix and postfix.
changed notification keyword to notification filter keyword.

### v1.0.46

Moved MongoDb function To EzMongoDb (<https://github.com/elky84/EzMongoDb>)

### v1.0.45

Added AutoMapper and MapperUtil. (AutoMapper Util)

### v1.0.44

Disable cookie security configration. (for http)
Added port to RabbitMq Configuration.

### v1.0.43

Added file logging configuration from serilog.json.

### v1.0.42

Added file logging to default option.

### v1.0.41

Added InternalServer header to missing HttpClient method.

### v1.0.40

Upgrade version EzAspDotNet.Protocols. (1.0.3 -> 1.0.4)

### v1.0.39

Upgrade version EzAspDotNet.Protocols. (1.0.0 -> 1.0.3)

### v1.0.38

Added HttpClientService.

### v1.0.37

Maximum webhook attachment size 50

### v1.0.36

Seperate Protocol function to EzAspDotNet.Protocols package.

### v1.0.35

Fixed webhook error. (cause by v1.0.34)

### v1.0.34

Fixed multithreading issues in webhook process. (List to ConcurrentBag)

### v1.0.33

Fixed multithreading issues in webhook process.

### v1.0.32

Change use to Slack webhooks group by HookUrl and Channel (Discord webhook not used Channel)

### v1.0.31

Integrate MongoDbWebUtil. <https://github.com/elky84/MongoDbWebUtil>
Remove namespace MongoDbWebUtil. (move to EzAspDotNet)

### v1.0.30

Support webhook groupping.
Avoid having to pass a webhook timestamp value.

### v1.0.29

Fixed discord time stamp issue.

### v1.0.28

Support webhook data single or multi embed. (support twice)

### v1.0.27

Support webhook data multi embed.

### v1.0.26

Improve discord webhook formatting. (More information)

### v1.0.25

Improve discord webhook formatting.
Added discord webhook Unit Test.

### v1.0.23

Fix discord webhook image embeds error.

### v1.0.22

Fix discord webhook formatting.

### v1.0.20

Improve Protocol header. (Removed similar variables)

### v1.0.19

Typo correction (ToIntRegx() -> ToIntRegex())

### v1.0.18

Support Alert optional field. (Notification.Data.WebHook)

### v1.0.17

Improved Slack Alarm Readability

### v1.0.16

added general utility codes (from EzAspDotNet)

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
