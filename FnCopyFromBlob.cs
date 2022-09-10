using System;
using System.IO;
using Microsoft.Azure.Documents;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace FnBlobCopy
{
    [StorageAccount("BlobConnection")]
    public class FnCopyFromBlob
    {
        private readonly IConfiguration _configuration;

        public FnCopyFromBlob(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [FunctionName("CopyFromBlob")]
        public  async Task Run([BlobTrigger("sourceblobmanpower/{name}")]Stream myBlob,
            //[Blob("destblobmanpower/{name}",FileAccess.Write)] Stream outputStream,
            string name, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
                var jsonData=DeserializeFromStream(myBlob);
                //var result = JsonConvert.DeserializeObject<string>(jsonData);
                var client = new ServiceBusClient(_configuration["AzureServiceBus"].ToString());
                var sender = client.CreateSender("manpowersourcequeue");
                var msg = new ServiceBusMessage(jsonData.ToString());
                await sender.SendMessageAsync(msg);
                //IQueueClient queueClient = new QueueClient()_configuration["AzureServiceBus"].ToString(), _configuration["QueueName"].ToString());
                //var orderJSON = JsonConvert.SerializeObject(myBlob);
                //var orderMessage = new Message(Encoding.UTF8.GetBytes(orderJSON))
                //{
                //    MessageId = Guid.NewGuid().ToString(),
                //    ContentType = "application/json"
                //};
                // await queueClient.SendAsync(orderMessage).ConfigureAwait(false);
                //outputStream = myBlob;
            }
            catch (Exception ex)
            {

                log.LogError(ex.Message);
            }
            
        }
        public static object DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonTextReader);
            }
        }
    }
}
