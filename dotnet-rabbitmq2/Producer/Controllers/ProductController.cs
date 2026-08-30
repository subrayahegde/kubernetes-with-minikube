using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using Producer.RabitMQ;
using Producer.Services;

namespace Producer.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController: ControllerBase {
        private readonly IProductService productService;
        private readonly IRabitMQProducer _rabitMQProducer;
        public ProductController(IProductService _productService, IRabitMQProducer rabitMQProducer) {
                productService = _productService;
                _rabitMQProducer = rabitMQProducer;
            }
            [HttpGet("productlist")]
        public IEnumerable < Product > ProductList() {
                var productList = productService.GetProductList();
                return productList;
            }
            [HttpGet("getproductbyid")]
        public Product GetProductById(int Id) {
                return productService.GetProductById(Id);
            }

[HttpPost("addproduct")]
public async Task<Product> AddProduct([FromBody] Product product) 
{
    var productData = productService.AddProduct(product);
    
    // Explicitly awaiting the Task ensures the connection is held until the message is flushed
    await _rabitMQProducer.SendProductMessage(productData);
    
    return productData;
}

            [HttpPut("updateproduct")]
        public Product UpdateProduct(Product product) {
                return productService.UpdateProduct(product);
            }
            [HttpDelete("deleteproduct")]
        public bool DeleteProduct(int Id) {
            return productService.DeleteProduct(Id);
        }
    }
}
