using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// See https://aka.ms/new-console-template for more information
//Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
var factory = new ConnectionFactory {
    HostName = "rabbitmq-service", UserName="guest", Password="guest"
};
//Create the RabbitMQ connection using connection factory details as i mentioned above
var connection = factory.CreateConnection();
//Here we create channel with session and model
using
var channel = connection.CreateModel();
//declare the queue after mentioning name and a few property related to that
channel.QueueDeclare("product", exclusive: false, durable: true, autoDelete: false, arguments: null);
//Set Event object which listen message from chanel which is sent by producer
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, eventArgs) => {
    var body = eventArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine("Product message received:" + message);
};
//read the message
channel.BasicConsume(queue: "product", autoAck: true, consumer: consumer);
await Task.Delay(Timeout.Infinite); 

