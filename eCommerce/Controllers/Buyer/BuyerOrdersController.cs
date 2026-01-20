using ECommerce.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerOrdersController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public BuyerOrdersController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
