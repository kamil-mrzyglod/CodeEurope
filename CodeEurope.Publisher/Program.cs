using System;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace CodeEurope.Publisher
{
    internal class Program
    {
        private const string EventHubConnectionString = "";
        private const string EventHubName = "";

        private static int _counter;

        private static void Main()
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();
            SendingRandomMessages();
        }

        private static void SendingRandomMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(EventHubConnectionString, EventHubName);
            while (true)
            {
                try
                {
                    var message = JsonConvert.SerializeObject(new Message(Guid.NewGuid(), DateTime.Now, "Message"));
                    Console.WriteLine("{2} | {0} > Sending message: {1}", DateTime.Now, message, _counter);
                    _counter++;
                    eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(10);
            }
        }

        public class Message
        {
            public Guid Id { get; }
            public DateTime DateSent { get; }
            public string Text { get; }

            public Message(Guid id, DateTime dateSent, string text)
            {
                Id = id;
                DateSent = dateSent;
                Text = text;
            }
        }
    }
}
