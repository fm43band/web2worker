using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Abstracts
{
    public interface IMessageReceiver<T> where T: class 
    {
        void Receive(
            Func<Message<T>, MessageProcessingStatus> onProcess,
            Action<Exception> onError,
            Action onWait);
    }
}
