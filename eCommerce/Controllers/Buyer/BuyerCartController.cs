using ECommerce.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{   
    
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerCartController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public BuyerCartController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
