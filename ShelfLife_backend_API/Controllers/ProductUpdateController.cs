using Business_Layer.DTOs.ProductDTO;
using Business_Layer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShellLife_backend_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize(Policy = "CustomAuth")] // Both endpoints require authentication
    public class ProductUpdateController : ControllerBase
    {
        private readonly ProductUpdateLogicService _updateLogicService;

        public ProductUpdateController(ProductUpdateLogicService updateLogicService)
        {
            _updateLogicService = updateLogicService;
        }

        [HttpPost]
        public async Task<IActionResult> RequestProductUpdate([FromBody] CreateProductUpdateRequestDTO requestDto)
        {
            // Extract the user's ID directly from their JWT Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("Id");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Could not identify the user from the token.");
            }

            var success = await _updateLogicService.RequestUpdateAsync(requestDto, userId);

            if (success)
            {
                return Ok(new { Message = "Product update request submitted to admins successfully." });
            }

            return BadRequest("Failed to submit request. Ensure the barcode is correct.");
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetRequestProductUpdates()
        {
            // Verify if the token belongs to an Admin
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin")
            {
                return StatusCode(403, new { Message = "Forbidden: Only admins can view pending requests." });
            }

            var requests = await _updateLogicService.GetPendingUpdatesAsync();
            return Ok(requests);
        }
    }
}