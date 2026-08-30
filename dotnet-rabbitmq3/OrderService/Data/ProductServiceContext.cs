using Microsoft.EntityFrameworkCore;

namespace OrderService.Data
{
    public class ProductServiceContext : DbContext
    {
        public ProductServiceContext (DbContextOptions<ProductServiceContext> options)
            : base(options)
        {
        }

        public DbSet<OrderService.Models.Product> Product { get; set; }
    }
}
