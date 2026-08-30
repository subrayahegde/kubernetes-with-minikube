using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrderService.Data;
using OrderService.Models;


namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductServiceContext _context;

        public ProductController(ProductServiceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            return await _context.Product.ToListAsync();
        }

        private void PublishToMessageQueue(string integrationEvent, string eventData)
        {
            // TOOO: Reuse and close connections and channel, etc, 
            var rabbitMqHost = "rabbitmq-service";
            Console.WriteLine($"Publishing message to RabbitMQ host: {rabbitMqHost} | exchange=user | routingKey={integrationEvent}");
            var factory = new ConnectionFactory {
               HostName = rabbitMqHost, UserName="guest", Password="guest"
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var body = Encoding.UTF8.GetBytes(eventData);
            channel.BasicPublish(exchange: "user",
                                             routingKey: integrationEvent,
                                             basicProperties: null,
                                             body: body);
        }
       

        [HttpPost ("AddProduct")]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            var integrationEventData = JsonConvert.SerializeObject(new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price
            });
           
            PublishToMessageQueue("user.add", integrationEventData);

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        
        [HttpPost ("BuyProducts")]
        public async Task<ActionResult<String>> BuyProducts(CartItems cartItems)
        {          
              
              List<CartItem> items = cartItems.cartItems;

              foreach (CartItem item in items) {
                //Product pr = _context.Product.First(x => x.ID == item.ProductId);
                Product pr = _context.Product.Find(item.ProductId);   
 
                if (pr == null) {                  
                    return ("Invalid Product Id");
                };           
              }
        
            var integrationEventData = JsonConvert.SerializeObject(cartItems);           
            PublishToMessageQueue("user.update", integrationEventData);        
            return ("Product Ordered");
        }
    }
}
