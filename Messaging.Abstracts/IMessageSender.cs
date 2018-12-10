using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Abstracts
{
    public interface IMessageSender<T> where T: class
    {
        Task SendAsync(Message<T> message);
    }
}
