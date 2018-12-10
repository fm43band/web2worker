using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Messaging.Abstracts;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusMessageForwarder
{
    
    public class SbMessageReceiver<T> : IMessageReceiver<T> where T: class
    {
        private QueueClient queueClient;

        public SbMessageReceiver(string serviceBusConnectionString, string queueName)
        {
            queueClient = new QueueClient(connectionString: serviceBusConnectionString,
                entityPath: queueName);
        }

        public void Receive(
            Func<Message<T>, MessageProcessingStatus> onProcess,
            Action<Exception> onError,
            Action onWait)
        {
            var options = new MessageHandlerOptions(e =>
            {
                onError(e.Exception);
                return Task.CompletedTask;
            })
            {
                AutoComplete = false,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(1)
            };

            queueClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    try
                    {
                        // Get message
                        var data = Encoding.UTF8.GetString(message.Body);
                        T payload = JsonConvert.DeserializeObject<T>(data);
                        var id = message.MessageId;

                        // Process message
                        var result = onProcess(new Message<T>{ Id = id, Payload = payload});

                        if (result == MessageProcessingStatus.Complete)
                            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                        else if (result == MessageProcessingStatus.Abandon)
                            await queueClient.AbandonAsync(message.SystemProperties.LockToken);
                        else if (result == MessageProcessingStatus.Dead)
                            await queueClient.DeadLetterAsync(message.SystemProperties.LockToken);

                        // Wait for next message
                        onWait();
                    }
                    catch (Exception ex)
                    {
                        await queueClient.DeadLetterAsync(message.SystemProperties.LockToken);
                        onError(ex);
                    }
                }, options);
        }
    }
}
