using Business_Layer.DTOs.ProductDTO;
using Business_Layer.Services;
using Common_Class.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ShellLife_backend_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductLogicService _productLogicService;

        public ProductController(ProductLogicService productLogicService)
        {
            _productLogicService = productLogicService;
        }

        [Authorize(Policy = "CustomAuth")]
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            List<ProductRequestDTO> prodList = await _productLogicService.GetProductsAsync();
            if (prodList == null || prodList.Count == 0)
            {
                return Ok(new List<ProductRequestDTO>());
            }
            return Ok(prodList);
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

        [HttpGet("{barcode}")]
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

        [HttpPut("{barcode}")]
        public async Task<IActionResult> UpdateProduct(string barcode,[FromBody] ProductRequestDTO requestDto)
        {
            if (barcode != requestDto.Barcode)
            {
                return BadRequest("Barcode mismatch");
            }

            var success = await _productLogicService.UpdateProductAsync(requestDto);

            if (!success)
            {
                return NotFound(new
                {
                    Message = "Product not found"
                });
            }

            return Ok(new
            {
                Message = "Product updated successfully"
            });
        }

    }

}


    
