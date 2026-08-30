using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Consumer
{
public class ApacheKafkaConsumerService:IHostedService {
        private readonly string topic = "test";
        private readonly string groupId = "test_group_v3"; 
        private readonly string bootstrapServers = Environment.GetEnvironmentVariable("Kafka__BootstrapServers") ?? "kafka-broker:29092";

        public Task StartAsync(CancellationToken cancellationToken) {
            var config = new ConsumerConfig {
            GroupId = groupId,
            BootstrapServers = bootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            ReconnectBackoffMs = 1000, 
            ReconnectBackoffMaxMs = 5000,
            EnableAutoCommit = true
            };

            try {
                using(var consumerBuilder = new ConsumerBuilder 
                <Ignore, string> (config).Build()) {
                    consumerBuilder.Subscribe(topic);
                    var cancelToken = new CancellationTokenSource();

                try {
                    while (true) {
                        var consumer = consumerBuilder.Consume (cancelToken.Token);
                        Console.WriteLine($"[RAW KAFKA DATA RECEIVED]: {consumer.Message.Value}");
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive =  true };
                        var orderRequest = JsonSerializer.Deserialize 
                            <OrderProcessingRequest> 
                                (consumer.Message.Value, options);
                       Console.WriteLine($"Processing Order Id: {orderRequest.OrderId}");                 
                    }
                } catch (OperationCanceledException) {
                    consumerBuilder.Close();
                }
                  catch (ConsumeException e)
                {
                    Console.WriteLine($"❌ Consume error: {e.Error.Reason}");
                }
                  catch (JsonException ex)
                {
                    Console.WriteLine($"❌ JSON Parsing failed: {ex.Message}");
                }
            }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

       
    }
}
