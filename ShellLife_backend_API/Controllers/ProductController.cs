using Microsoft.AspNetCore.Mvc;
using Common_Class.Entities;
using Business_Layer.Services;
using Business_Layer.DTOs.ProductDTO;
namespace ShellLife_backend_API.Controllers
{
    [ApiController]
    [Route("/api[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductLogicService _productLogicService;

        public ProductController(ProductLogicService productLogicService)
        {
            _productLogicService = productLogicService;
        }

        [HttpPost]
        public async Task<IActionResult> registerProduct([FromBody] ProductRequestDTO requestDto) {

           bool isSucess = await _productLogicService.RegisterProductAsync(requestDto);

            if (isSucess)
            {
                return Ok(new { Message = "Product succesfully registered" });
            }
            return BadRequest("Failed to register product");
        }

        [HttpGet]
        public async Task<IActionResult> getProduct(string barcode)
        {

            if (barcode == null || barcode.Trim() == "")
            {
                return BadRequest("Barcode cannot be null or empty");
            }
            ProductRequestDTO prod = await _productLogicService.GetProductAsync(barcode);

            {
                return Ok(prod);


            }
        }

    }


}
