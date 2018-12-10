using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Messaging.Abstracts;
using ServiceBusMessageForwarder;

namespace TestMessageReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = GetConfig();
            Console.WriteLine("Started");
            var sbConnectionString =
                config.GetSection("ServiceBus_ConnectionString").Value;
            var queueName = config.GetSection("ServiceBus_QueueName").Value;

            var sbMsgReceiver = new SbMessageReceiver<string>(sbConnectionString, queueName);

            sbMsgReceiver.Receive(msg =>
            {
                Console.WriteLine($"Id: {msg.Id}. Payload: {msg.Payload}");
                return MessageProcessingStatus.Complete;
            },
            ex => Console.WriteLine(ex.Message),
            () => Console.WriteLine("Waiting ..."));

            Console.ReadLine();
        }

        private static IConfigurationRoot GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
