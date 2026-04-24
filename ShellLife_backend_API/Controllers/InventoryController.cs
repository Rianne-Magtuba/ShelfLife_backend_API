using Business_Layer.DTOs.InventoryDTO;

using Business_Layer.Services;
using Microsoft.AspNetCore.Mvc;
namespace ShellLife_backend_API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase // Must inherit from ControllerBase
    {
        private readonly inventoryLogicService _inventoryLogicService;

        
        public InventoryController(inventoryLogicService inventoryLogicService)
        {
            _inventoryLogicService = inventoryLogicService;
        }

        [HttpPost]
        public async Task<IActionResult> addInventoryItem([FromBody] AddInventoryItemRequestDto requestDto, string userid)
        {

            bool isSucess = await _inventoryLogicService.AddInventoryItemAsync(requestDto, userid);

            if (isSucess)
            {
                return Ok(new { Message = "Item succesfully added" });
            }
            return BadRequest("Failed to add item");
        }


        [HttpGet("{userId}/pantry")]
        public async Task<IActionResult> GetUserPantry(string userId)
        {
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID cannot be empty."); // Returns a 400 status code
            }

            
            List<InventoryItemResponseDto> pantryDtos = await _inventoryLogicService.GetUserPantryAsync(userId);

       
            if (pantryDtos == null || pantryDtos.Count == 0)
            {
               
                return Ok(new List<InventoryItemResponseDto>());
            }

         
            return Ok(pantryDtos);
        }
    }
}
