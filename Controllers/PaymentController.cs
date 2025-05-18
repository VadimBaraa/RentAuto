using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using RentAutoWeb.Models;

public class PaymentController : Controller
{
    private readonly StripeSettings _stripeSettings;
    private readonly AppDbContext _context;


    public PaymentController(IOptions<StripeSettings> stripeOptions, AppDbContext context)
    {
        _stripeSettings = stripeOptions.Value;
        _context = context;
    }


    [HttpPost]
    public IActionResult CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
    {
        try
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == request.CarId);

            if (car == null)
            {
                return NotFound("Автомобиль не найден.");
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "rub",
                            UnitAmount = (long)(car.Price * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Аренда автомобиля {car.Brand} {car.Model} ({car.Year})"
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme),
                CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme),
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id, publishableKey = _stripeSettings.PublishableKey });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка Stripe: " + ex.Message);
            return StatusCode(500, $"Ошибка: {ex.Message}");
        }
    }




    [HttpGet]
    public IActionResult Success()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Cancel()
    {
        return View();
    }
}
