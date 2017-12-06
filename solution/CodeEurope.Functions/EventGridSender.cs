using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CodeEurope.Functions
{
    public static class EventGridSender
    {
        private const string EventGridEndpoint =
            "";
        private const string Key = "";

        private static Lazy<HttpClient> HttpClient = new Lazy<HttpClient>(() => new HttpClient());

        [FunctionName("EventGridSender")]
        public static async Task Run([ActivityTrigger] List<DataAggregator.Message> messages)
        {
            var content = JsonConvert.SerializeObject(messages.Select(message => new EventGridMessage(message)));
            var result = await HttpClient.Value.PostAsync(EventGridEndpoint,
                new StringContent($"{content}") {Headers = {{"aeg-sas-key", Key}}});

            // Throw if something happens
            result.EnsureSuccessStatusCode();
        }

        public class EventGridMessage
        {
            public EventGridMessage()
            {
            }

            public EventGridMessage(DataAggregator.Message message)
            {
                EventType = "codeeurope.common";
                Subject = "codeeurope/common";
                EventTime = message.DateSent;
                Data = message;
            }

            public Guid Id => Guid.NewGuid();
            public string EventType { get; set; }
            public string Subject { get; set; }
            public DateTime EventTime { get; set; }
            public DataAggregator.Message Data { get; set; }
        }
    }
}
