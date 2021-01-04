using System;
using System.Text;
using System.Threading;
using MutalkLib;

namespace MutalkConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using var mutalk = new Mutalk("test123");

            mutalk.OnMessage += (_, eventArgs) =>
            {
                if (eventArgs.Topic == "test123") // double check topic name due to possible hash collisions
                {
                    Console.WriteLine(Encoding.UTF8.GetString(eventArgs.Message));
                }
            };

            mutalk.ReceiveMessages(new CancellationToken());
        }
    }
}
