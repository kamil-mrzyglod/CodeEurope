using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace CodeEurope.Functions
{
    public static class BlobBuffer
    {
        [FunctionName("BlobBuffer")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // Get data passed to the function
            var payload = await req.Content.ReadAsStringAsync();
            log.Info($"Data received: {payload}");
            var message = JsonConvert.DeserializeObject<EventGridSender.EventGridMessage[]>(payload)[0];

            // We'd like to operate on an append blob
            var storageAccount = CloudStorageAccount.Parse("");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("data");
            container.CreateIfNotExists();
            var now = DateTime.UtcNow;
            var blob = container.GetAppendBlobReference($"{now.Year}{now.Month}{now.Day}");

            log.Info("Blob reference obtained!");

            if (blob.Exists() == false)
            {
                blob.CreateOrReplace(AccessCondition.GenerateIfNotExistsCondition());
            }

            // Because we'll be appending data in an environment,
            // where paralell executions are possible, there's only
            // one method which we can use - AppendBlockAsync
            using (var ms = new MemoryStream())
            using(var sw = new StreamWriter(ms))
            {
                await sw.WriteLineAsync(JsonConvert.SerializeObject(message.Data));
                await sw.FlushAsync();

                ms.Position = 0;

                await blob.AppendBlockAsync(ms);
            }

            return req.CreateResponse(HttpStatusCode.OK, "Data appended!!");
        }
    }
}
