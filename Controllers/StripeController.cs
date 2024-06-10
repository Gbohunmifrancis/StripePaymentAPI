using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using StripeAPI.Models;
using Session = Stripe.Checkout.Session;

namespace StripeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {

        private readonly StripeModel model;
        private readonly TokenService _tokenService;
        private readonly CustomerService _customerService;
        private readonly ChargeService _chargeService;
        private readonly ProductService _productService;

        public StripeController(IOptions<StripeModel> _model,

           TokenService tokenService,
           CustomerService customerService,
           ChargeService chargeService,
           ProductService productService)


        {
            model = _model.Value;
            _tokenService = new TokenService();
            _customerService = new CustomerService();
            _chargeService = new ChargeService();
            _productService = new ProductService();
        }

        [HttpPost("Pay")]
        public IActionResult Pay([FromBody] string priceId)
        {
            StripeConfiguration.ApiKey = model.SecretKey;

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    }
                },
                    Mode = "payment",
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"

            };
            var service = new Stripe.Checkout.SessionService();

            Session session = service.Create(options);
            return Ok(session.Url);


        }

        [HttpPost("CreateCustomer")]
        public async Task<dynamic>  CreateCustomer([FromBody] StripeCustomer customerInfo)
        {
            StripeConfiguration.ApiKey = model.SecretKey;

            var customerOptions = new CustomerCreateOptions
            {
                Email = customerInfo.Email,
                Name = customerInfo.Name
            };
            var customer = await _customerService.CreateAsync(customerOptions);

            return new { customer };
        }


        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts()
        {
            StripeConfiguration.ApiKey = model.SecretKey;
            var options = new ProductListOptions
            {
                Expand = new List<string>() { "data.default_price" }
            };
            
           var products = _productService.List(options);
            return Ok(products);
        }


    }
}
