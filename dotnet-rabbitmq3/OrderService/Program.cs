using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using OrderService.Data;
using OrderService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

var rabbitMqHost = "rabbitmq-service"; // Default RabbitMQ host";
var mysqlHost = "mysql-service";
var mysqlConnectionString = $"server={mysqlHost};port=3306;database=orderservice;user=root;password=Password123#;";

Console.WriteLine($"Connecting to RabbitMQ at host: {rabbitMqHost}");
var factory = new ConnectionFactory {
    HostName = rabbitMqHost, UserName="guest", Password="guest"
};
var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "user", type: ExchangeType.Topic, durable: true, autoDelete: false);
channel.QueueDeclare(queue: "user.postservice", durable: true, exclusive: false, autoDelete: false);
channel.QueueDeclare(queue: "user.otherservice", durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(queue: "user.postservice", exchange: "user", routingKey: "user.add");
channel.QueueBind(queue: "user.otherservice", exchange: "user", routingKey: "user.update");

var consumer = new EventingBasicConsumer(channel);

                
    consumer.Received += (model, ea) =>  { 
    var contextOptions = new DbContextOptionsBuilder<OrderServiceContext>()
            .UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString))
            .Options;
            
     var dbContext = new OrderServiceContext(contextOptions);        

        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}", message);
        Console.WriteLine("RoutingKey = " + ea.RoutingKey + " | Queue = " + ea.RoutingKey);
        var data = JObject.Parse(message);
        var type = ea.RoutingKey;
         if (type == "user.add")
         {
            dbContext.Product.Add(new Product()
            {
                Id= data["id"].Value<int>(),
                Name = data["name"].Value<string>(),
                Description = data["description"].Value<string>(),
                Price = data["price"].Value<decimal>()
            });
            dbContext.SaveChanges();
         }
else if (type == "user.update")
{            
    // 1. Fix the Console typo (was Console.Log which doesn't exist)
    Console.WriteLine("Order table to be UPDATED"); 
    
    var cartObject = JsonConvert.DeserializeObject<CartItems>(message);
    List<CartItem> items = cartObject.cartItems;            
   
    // 2. Safe Max ID selection (defaults to 0 if the table is empty)
    int maxOrder = dbContext.Order.Any() ? dbContext.Order.Max(p => p.OrderId) : 0;        
    decimal TotalPrice = 0;          
   
    // Calculate the next Order group ID safely
    int nextGroupId = maxOrder + 1;

    foreach (CartItem item in items) 
    { 
        Console.WriteLine("ProductId: " + item.ProductId + ", Qty: " + item.Qty);
        
        // Use SingleOrDefault or FirstOrDefault to prevent crashes if a product is missing
        Product pr = dbContext.Product.FirstOrDefault(x => x.Id == item.ProductId);
        if (pr == null)
        {
            Console.WriteLine($"Error: Product with ID {item.ProductId} not found in consumer database!");
            continue; // Skip this item safely
        }
       
        decimal price = pr.Price;
        TotalPrice += item.Qty * price; 

        Console.WriteLine("nextGroupId: " + nextGroupId + ", TotalPrice: " + TotalPrice);

        Order or = new Order()
        {
            // NOTE: Remove the OrderId assignment completely if your database uses AUTO_INCREMENT
            OrderId = nextGroupId, 
            ProductId = item.ProductId,
            Qty = item.Qty,
            CreatedOn = DateTime.UtcNow // Best practice: Use UtcNow for databases
        };           
        Console.WriteLine("READY TO SAVE Order: " + JsonConvert.SerializeObject(or));
        dbContext.Order.Add(or);        
    } 
    
    // 3. Move SaveChanges outside the loop to save everything efficiently as a single transaction
    dbContext.SaveChanges();

    Console.WriteLine("Total Order Cost is: $" + TotalPrice);            
}
    };
             
        Console.WriteLine("Consumer started for user.postservice");
        channel.BasicConsume(queue: "user.postservice",
                                     autoAck: true,
                                     consumer: consumer);
 
         Console.WriteLine("Consumer started for user.otherservice");
         channel.BasicConsume(queue: "user.otherservice",
                                     autoAck: true,
                                     consumer: consumer);
                                     
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? mysqlConnectionString;

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderService", Version = "v1" }); 
        c.SwaggerDoc("v2", new OpenApiInfo { Title = "UserService", Version = "v1" });
    })
);

builder.Services.AddDbContext<OrderServiceContext>(options =>
         options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDbContext<ProductServiceContext>(
 options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
