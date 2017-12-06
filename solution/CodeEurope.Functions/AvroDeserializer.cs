using System;
using System.Collections.Generic;
using System.Text;
using Avro.File;
using Avro.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace CodeEurope.Functions
{
    public static class AvroDeserializer
    {
        [FunctionName("AvroDeserializer")]
        public static List<DataAggregator.Message> Run(
            [ActivityTrigger] string blobName
        )
        {
            // Here we have to get a reference to a blob. Note that
            // it'd be great to just use a binding to encapsulate
            // this functionality. Unfortunately for now it's not possible,
            // mostly because of the fact, that this requires general-purpose
            // storage account, while for our platform we selected blob-only.
            var storageAccount = CloudStorageAccount.Parse("");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blob = blobClient.GetBlobReferenceFromServer(new Uri(blobName));

            using (var avroReader = DataFileReader<GenericRecord>.OpenReader(blob.OpenRead()))
            {
                // It's important to rememember, that Event Hub may capture
                // multiple events in one file(and this is mainly true for
                // heavy loads, where we have at least several hundred events
                // per second).
                var messages = new List<DataAggregator.Message>();
                while (avroReader.HasNext())
                {
                    var record = avroReader.Next();

                    // Here we have a complete `EventData` type, which we can
                    // access in any way. Since for now we're only interested 
                    // in the payload, there's no need to deserialize other
                    // properties also
                    var body = (byte[])record["Body"];
                    var payload = Encoding.ASCII.GetString(body);
                    var message = JsonConvert.DeserializeObject<DataAggregator.Message>(payload);

                    messages.Add(message);
                }

                return messages;
            }
        }
    }
}
