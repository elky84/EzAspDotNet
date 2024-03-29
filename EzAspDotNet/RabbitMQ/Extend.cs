﻿using EzAspDotNet.Exception;
using EzAspDotNet.Settings;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EzAspDotNet.RabbitMQ
{
    public static class Extend
    {
        public static IConnection CreateConnection(string hostName, int port, string userName, string password)
        {
            var factory = new ConnectionFactory() { HostName = hostName };

            if (port != 0)
            {
                factory.Port = port;
            }

            if (userName != null)
            {
                factory.UserName = userName;
            }

            if (password != null)
            {
                factory.Password = password;
            }

            return factory.CreateConnection();
        }

        public static IConnection CreateConnectionFromConfiguration(IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetRabbitMqSettings();
            return CreateConnectionWithTryCatch(rabbitMqSettings.Url, rabbitMqSettings.Port,
                rabbitMqSettings.UserName, rabbitMqSettings.Password);
        }

        public static IConnection CreateConnectionWithTryCatch(string hostName, int port, string userName, string password)
        {
            try
            {
                return CreateConnection(hostName, port, userName, password);
            }
            catch (System.Exception e)
            {
                throw new DeveloperException(Protocols.Code.ResultCode.NotConnectedMQ, System.Net.HttpStatusCode.InternalServerError, e.Message);
            }
        }


        public static IModel CreateChannel(this IConnection connection)
        {
            return connection.CreateModel();
        }


        public static void Publish(this IModel channel, string routingKey, byte[] bytes)
        {
            channel.BasicPublish(exchange: "",
                     routingKey: routingKey,
                     basicProperties: null,
                     body: bytes);
        }

        public static void Publish(this IModel channel, string routingKey, string message)
        {
            channel.BasicPublish(exchange: "",
                     routingKey: routingKey,
                     basicProperties: null,
                     body: Encoding.UTF8.GetBytes(message));
        }

        public static void Consume(this IModel channel, string queueName, EventingBasicConsumer consumer)
        {
            channel.QueueDeclare(queue: queueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

            channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);
        }

        public static void UnConsume(this IModel channel)
        {
            channel.Close();
        }

    }
}
