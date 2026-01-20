using ECommerce.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Seller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public ProductsController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
