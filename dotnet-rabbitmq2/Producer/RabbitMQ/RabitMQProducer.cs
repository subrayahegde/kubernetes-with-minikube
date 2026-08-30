using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Producer.RabitMQ {
    public class RabitMQProducer: IRabitMQProducer {
        public Task SendProductMessage<T>(T message) 
        {
            // Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
            var factory = new ConnectionFactory {
                HostName = "rabbitmq-service", 
                UserName = "guest", 
                Password = "guest"
            };

            // Create the RabbitMQ connection
            using var connection = factory.CreateConnection();
            
            // Here we create channel with session and model
            using var channel = connection.CreateModel();
            
            // declare the queue after mentioning name and a few property related to that
            channel.QueueDeclare("product", exclusive: false, durable: true, autoDelete: false, arguments: null);
            
            // Serialize the message
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            // put the data on to the product queue
            channel.BasicPublish(exchange: "", routingKey: "product", basicProperties: null, body: body);

            return Task.CompletedTask; // Resolves the "Cannot await void" compiler error!
        }
    }
}
