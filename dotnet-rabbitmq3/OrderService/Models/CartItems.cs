using Microsoft.EntityFrameworkCore;

namespace OrderService.Models
{
    public class CartItems
    {     
        public List<CartItem> cartItems { get; set; }
       // public int Count { get; set; }
    }
}
