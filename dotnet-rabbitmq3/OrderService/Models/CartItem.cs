using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class CartItem
    {     
        public int ProductId { get; set; }
        public int Qty { get; set; }
    }
}
