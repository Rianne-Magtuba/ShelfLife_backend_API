using Business_Layer.DTOs.InventoryDTO;
using Microsoft.AspNetCore.Authorization;
using Business_Layer.Services;
using Microsoft.AspNetCore.Mvc;
namespace ShellLife_backend_API.Controllers
{
    [Authorize]
    [ApiController] 
    [Route("api/[controller]")]
    public class InventoryController : BaseController    // Must inherit from ControllerBase
    {
        private readonly inventoryLogicService _inventoryLogicService;

        
        public InventoryController(inventoryLogicService inventoryLogicService)
        {
            _inventoryLogicService = inventoryLogicService;
        }

        [HttpPost]
        public async Task<IActionResult> addInventoryItem([FromBody] AddInventoryItemRequestDto requestDto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();
            bool isSucess = await _inventoryLogicService.AddInventoryItemAsync(requestDto, userId);

            if (isSucess)
            {
                return Ok(new { Message = "Item succesfully added" });
            }
            return BadRequest("Failed to add item");
        }


        [HttpGet("pantry")]
        public async Task<IActionResult> GetUserPantry()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();


            List<InventoryItemResponseDto> pantryDtos = await _inventoryLogicService.GetUserPantryAsync(userId);

       
            if (pantryDtos == null || pantryDtos.Count == 0)
            {
               
                return Ok(new List<InventoryItemResponseDto>());
            }

         
            return Ok(pantryDtos);
        }


        [HttpDelete("{inventoryId}")]
        public async Task<IActionResult> discardInventoryItem(string inventoryId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            bool isSucess = await _inventoryLogicService.DiscardInventoryItemAsync(inventoryId, userId);

            if (isSucess)
            {
                return Ok(new { Message = "Item succesfully discarded" });
            }
            return BadRequest("Failed to discard item");
        }

        [HttpPut("{inventoryId}")]
        public async Task<IActionResult> UpdateInventoryItem(string inventoryId, [FromBody] AddInventoryItemRequestDto requestDto)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            bool isSuccess = await _inventoryLogicService
                .UpdateInventoryItemAsync(inventoryId, requestDto, userId);

            if (!isSuccess)
            {
                return NotFound(new
                {
                    Message = "Inventory item not found"
                });
            }

            return Ok(new
            {
                Message = "Inventory item updated successfully"
            });
        }

    }
}
