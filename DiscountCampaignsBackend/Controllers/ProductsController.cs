using DiscountCampaignsBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiscountCampaignsBackend.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ProductsController : ControllerBase
{
    private static readonly Product[] Products = new Product[]
    {
        new Product { Sku= "SKU-TS" , Name = "T-Shirt" , Category = "Clothing" , Price = 500 },
        new Product { Sku= "SKU-HD" , Name = "Hoodie" , Category = "Clothing" , Price = 750 },
        new Product { Sku= "SKU-LT" , Name = "Laptop" , Category = "Electronics" , Price = 5000 },
        new Product { Sku= "SKU-SP" , Name = "Smart Phone" , Category = "Electronics" , Price = 3500 },
        new Product { Sku= "SKU-W" , Name = "Watch" , Category = "Accessories" , Price = 2500 },
        new Product { Sku= "SKU-ER" , Name = "Earring" , Category = "Accessories" , Price = 1500 },
    };
    private readonly ILogger<ProductsController> _logger;
    private readonly DiscountCalculator _discountCalculator;

    public ProductsController(ILogger<ProductsController> logger, DiscountCalculator discountCalculator)
    {
        _logger = logger;
        _discountCalculator = discountCalculator;
    }

    [HttpGet]
    public IEnumerable<Product> GetProducts()
    {
        return Products;
    }

    // [HttpPost]
    // public IActionResult CalculateTotalSum(DiscountRequestDto discountRequestDto)
    // {
    //     var total = _discountCalculator.CalculateTotal(discountRequestDto);
    //     return Ok(total);
    // }

    [HttpPost]
    public IActionResult CalculateTotalSum([FromBody] DiscountRequestDto req)
    {
        var result = _discountCalculator.Calculate(req);
        return Ok(new
        {
            original = req.SelectedProduct.Sum(i => i.Product.Price * i.Quantity),
            final = result.finalTotal,
            applied = result.applied.Select(x => new { code = x.promoCode, amount = x.amount })
        });
    }
}
