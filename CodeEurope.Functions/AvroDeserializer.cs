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
            return new List<DataAggregator.Message>();
        }
    }
}
