using System;
using System.Text;
using System.Threading.Tasks;
using Messaging.Abstracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace ServiceBusMessageForwarder
{
    public class SbMessageSender<T>: IMessageSender<T> where T: class
    {
        private QueueClient queueClient;

        public SbMessageSender(string sbConnectionString, string queueName)
        {
            queueClient = new QueueClient(connectionString: sbConnectionString, entityPath: queueName);
        }

        public async Task SendAsync(Message<T> message)
        {
            var json = JsonConvert.SerializeObject(message.Payload);
            var sbMsg = new Message(Encoding.UTF8.GetBytes(json));
            sbMsg.MessageId = $"{Guid.NewGuid()}-{message.Id}" .ToString();
            await queueClient.SendAsync(sbMsg);
        }
    }
}
 