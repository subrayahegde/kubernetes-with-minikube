using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public DateTime CreatedOn { get; set; }                
    }
}
