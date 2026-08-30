using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Producer.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ProducerController: ControllerBase {
        private readonly string
        bootstrapServers = Environment.GetEnvironmentVariable("Kafka__BootstrapServers") ?? "kafka-broker:29092";
        private readonly string topic = "test";

        [HttpPost]
        public async Task <IActionResult>
        Post([FromBody] OrderRequest orderRequest) {
            string message = JsonSerializer.Serialize(orderRequest);
            return Ok(await SendOrderRequest(topic, message));
        }
        private async Task < bool > SendOrderRequest
        (string topic, string message) {
            ProducerConfig config = new ProducerConfig {
                BootstrapServers = bootstrapServers,
                ClientId = Dns.GetHostName()
            };

            try {
                using(var producer = new ProducerBuilder
                <Null, string> (config).Build()) {
                    var result = await producer.ProduceAsync
                    (topic, new Message <Null, string> {
                        Value = message
                });

                Debug.WriteLine($"Delivery Timestamp: {result.Timestamp.UtcDateTime}");
                return await Task.FromResult(true);
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error occured: {ex.Message}");
        }

        return await Task.FromResult(false);
    }
    }
}
