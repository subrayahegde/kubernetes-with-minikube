using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

// Establish connection
var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "guest", Password = "guest" };

string queueName = "locationSampleQueue";
using var rabbitMqConnection = factory.CreateConnection();
using var rabbitMqChannel = rabbitMqConnection.CreateModel();

rabbitMqChannel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

int messageCount = Convert.ToInt32(rabbitMqChannel.MessageCount(queueName));
Console.WriteLine(" Listening to the queue. This channel currently has {0} cached messages.", messageCount);

var consumer = new EventingBasicConsumer(rabbitMqChannel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(" Location received: " + message);
    
    // Acknowledge the message to remove it from RabbitMQ
    rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

rabbitMqChannel.BasicConsume(queue: queueName,
                             autoAck: false,
                             consumer: consumer);

Console.WriteLine(" Press Ctrl+C in your terminal to exit. Listening for messages...");

// CRITICAL FIX: This keeps the thread alive continuously inside the Docker container
var keepAliveEvent = new ManualResetEvent(false);

// Gracefully handle container shutdown when Docker stops it
AppDomain.CurrentDomain.ProcessExit += (sender, e) => 
{
    Console.WriteLine(" Shutting down consumer container...");
    keepAliveEvent.Set();
};

// This blocks the Main method execution path from reaching the end
keepAliveEvent.WaitOne();

Console.WriteLine(" Connection closed safely.");

