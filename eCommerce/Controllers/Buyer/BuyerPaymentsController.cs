using ECommerce.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerPaymentsController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public BuyerPaymentsController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
