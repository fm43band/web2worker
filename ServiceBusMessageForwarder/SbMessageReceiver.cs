using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusMessageForwarder
{
    public enum MessageProcessResponse
    {
        Complete,
        Abandon,
        Dead
    }
    public class SbMessageReceiver<T> where T: class
    {
        private QueueClient queueClient;

        public SbMessageReceiver(string serviceBusConnectionString, string queueName)
        {
            queueClient = new QueueClient(connectionString: serviceBusConnectionString,
                entityPath: queueName);
        }

        public void Receive(
            Func<T, MessageProcessResponse> onProcess,
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
                        T item = JsonConvert.DeserializeObject<T>(data);

                        // Process message
                        var result = onProcess(item);

                        if (result == MessageProcessResponse.Complete)
                            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                        else if (result == MessageProcessResponse.Abandon)
                            await queueClient.AbandonAsync(message.SystemProperties.LockToken);
                        else if (result == MessageProcessResponse.Dead)
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
