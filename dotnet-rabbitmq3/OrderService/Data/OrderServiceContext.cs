using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderServiceContext : DbContext
    {
        public OrderServiceContext (DbContextOptions<OrderServiceContext> options)
            : base(options)
        {
        }

        public DbSet<OrderService.Models.Product> Product { get; set; }
        public DbSet<OrderService.Models.Order> Order { get; set; }
    }
}
