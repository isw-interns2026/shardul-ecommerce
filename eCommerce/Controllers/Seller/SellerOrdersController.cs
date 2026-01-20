using ECommerce.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Seller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public OrdersController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
