using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CodeEurope.Functions
{
    public static class DataAggregator
    {
        [FunctionName("DataAggregator_Start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {
            var data = await req.Content.ReadAsStringAsync();
            log.Info(data);

            var @event = JsonConvert.DeserializeObject<Event[]>(data)[0];
            log.Info($"Processing {@event.Id} event.");

            // Since we didn't set filter, it's possible that we'll get
            // events we are not interested in
            if (@event.EventType != "Microsoft.EventHub.CaptureFileCreated")
            {
                return req.CreateResponse(HttpStatusCode.OK);
            }

            // It's important to know, that Event Hub's capture feature
            // injects en empty file if there were no events during the most
            // recent window. In that case we can skip processing
            if (@event.Data.SizeInBytes == 0)
            {
                return req.CreateResponse(HttpStatusCode.OK);
            }

            var instanceId = await starter.StartNewAsync("DataAggregator", @event);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("DataAggregator")]
        public static async Task RunImpl([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            // Orchestration code. Here we can schedule all activities
            // in any order and just await 'til each one is finished.
            var ctx = JsonConvert.DeserializeObject<Event>(context.GetInputAsJson().ToString());
            var msg = await context.CallActivityAsync<List<Message>>("AvroDeserializer", ctx.Data.FileUrl);
            await context.CallActivityAsync("EventGridSender", msg);

        }

        public class Event
        {
            public string Topic { get; set; }
            public string Subject { get; set; }
            public string EventType { get; set; }
            public DateTime EventTime { get; set; }
            public Guid Id { get; set; }

            public EventData Data { get; set; }
        }

        public class EventData
        {
            public string FileUrl { get; set; }
            public string FileType { get; set; }
            public string PartitionId { get; set; }
            public int SizeInBytes { get; set; }
            public int EventCount { get; set; }
            public int FirstSequenceNumber { get; set; }
            public int LastSequenceNumber { get; set; }
            public DateTime FirstEqueueTime { get; set; }
            public DateTime LastEqueueTime { get; set; }
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
