using Microsoft.AspNetCore.Mvc;
using Business_Layer.Services;
using Business_Layer.DTOs;
namespace ShellLife_backend_API.Controllers
{
    [ApiController] // Tells ASP.NET this class handles web API requests
    [Route("api/[controller]")] // Sets the base URL to /api/inventory
    public class InventoryController : ControllerBase // Must inherit from ControllerBase
    {
        private readonly inventoryLogicService _inventoryLogicService;

        // 1. Dependency Injection: ASP.NET hands the Controller your Business Logic Service
        public InventoryController(inventoryLogicService inventoryLogicService)
        {
            _inventoryLogicService = inventoryLogicService;
        }

        // 2. HTTP Route: This handles GET requests to /api/inventory/{userId}/pantry
        [HttpGet("{userId}/pantry")]
        public async Task<IActionResult> GetUserPantry(string userId)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID cannot be empty."); // Returns a 400 status code
            }

            // 3. Call the Business Layer method
            List<InventoryItemResponseDto> pantryDtos = await _inventoryLogicService.GetUserPantryAsync(userId);

            // 4. Return the result
            if (pantryDtos == null || pantryDtos.Count == 0)
            {
                // Returns a 200 OK with an empty array (good practice if they just have an empty pantry)
                return Ok(new List<InventoryItemResponseDto>());
            }

            // Wrap the DTOs in a 200 OK response. 
            // ASP.NET automatically converts your C# List into a JSON array for Flutter!
            return Ok(pantryDtos);
        }
    }
}
