using System;
namespace MutalkLib
{
    public class MessageEvent: EventArgs
    {
        public MessageEvent(string topic, byte[] message)
        {
            Topic = topic;
            Message = message;
        }

        public string Topic { get; }
        public byte[] Message { get; }
    }
}
