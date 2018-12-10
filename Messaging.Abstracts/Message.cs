using System;

namespace Messaging.Abstracts
{
    public class Message<T> where T: class
    {
        public string Id { get; set; }
        public T Payload { get; set; }
    }
}
