# mutalk
C# UDP multicast message library inspired by [this repo](https://github.com/reddec/mutalk/).

## main idea
In the world of IoT we often face with the problem how to organize communication between peers.\
We often need a protocol that:

1. Can work **without dedicated broker**. 
2. Reliable but rare messages losses are acceptable
2. Based on a pub/sub design. 
3. Requires minimum getting started efforts

It looks like udp multicast messaging is an ideal candidate for such kind of protocol.

## the essence of the library
1. This is c# micro-library that implements pub/sub udp multicast messaging mechanism
2. Each message is a valid json based string
2. Each message consists of "topic" and "body" parts. Example\
`{"t":"test123", "m":"SGVsbG8gd29ybGQgIQ=="}`
3. Body of message is base64 encoded string (so you may send an arbitrary bytes)
3. Sender/publisher need to know only about topic to send messages.
4. Receiver/subscriber need to know only about topic for consuming messages.
5. Library hashes "topics" and maps them to ip multicast range "224.0.0.1-224.255.255.254"


## from sender/publisher perspective

```csharp
using var mutalk = new Mutalk("test123"); // "test123" is topic name where messages will be sent
mutalk.SendMessage(Encoding.UTF8.GetBytes("Hello")); // send first message to topic
Task.Delay(1000).Wait();
mutalk.SendMessage(Encoding.UTF8.GetBytes("World !")); // send second message to topic
```

## from receiver/subscriber perspective
```csharp
using var mutalk = new Mutalk("test123");

mutalk.OnMessage += (_, eventArgs) =>
{
  if (eventArgs.Topic == "test123") // double check topic name due to possible hash collisions
  {
	Console.WriteLine(Encoding.UTF8.GetString(eventArgs.Message));
  }
};

mutalk.ReceiveMessages(cancellationToken); // this is a blocking call until cancellationToken.Cancel()
```
