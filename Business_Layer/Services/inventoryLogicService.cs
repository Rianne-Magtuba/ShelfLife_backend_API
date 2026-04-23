
using Common_Class.Entities;
using Common_Class.Interfaces;
using Business_Layer.DTOs;
namespace Business_Layer.Services
{
    public class inventoryLogicService
    {
        private readonly IInventoryDataService _dataService;

        // You inject the INTERFACE, not the Data Layer class!
        public inventoryLogicService(IInventoryDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<List<InventoryItemResponseDto>> GetUserPantryAsync(string userId)
        {
            // 1. Call the interface method to get raw database entities
            List<InventoryEntity> entities = await _dataService.GetUserInventoryAsync(userId);

            // 2. Map the Entities to DTOs
            var dtos = new List<InventoryItemResponseDto>();

            foreach (var entity in entities)
            {
                // Create the base DTO and map the direct fields
                var dto = new InventoryItemResponseDto
                {
                    InventoryId = entity.InventoryId,
                    IsCustomItem = entity.IsCustomItem,
                    BarcodeRef = entity.BarcodeRef,
                    Quantity = entity.Quantity,
                    Status = entity.Status,

                    // Convert Firestore Timestamps to standard C# DateTimes
                    ExpirationDate = entity.ExpirationDate.ToDateTime(),
                    DateRegistered = entity.DateRegistered.ToDateTime()
                };

                // Apply our Hybrid Business Logic to figure out what Name/Category to display
                if (entity.IsCustomItem)
                {
                    // It's a custom item (e.g., Wet Market Fish). Use the locally stored data.
                    dto.DisplayName = entity.CustomName ?? "Unknown Custom Item";
                    dto.DisplayCategory = entity.CustomCategory ?? "Uncategorized";
                    dto.WeightGrams = entity.CustomWeightGrams;
                }
                else
                {
                    // It's a barcoded item. 
                    // TODO: We need to look up the global product details using entity.BarcodeRef!

                    // For now, we put a placeholder until we wire up the Global Catalog fetch.
                    dto.DisplayName = "Pending Catalog Lookup";
                    dto.DisplayCategory = "Pending Catalog Lookup";
                }

                // Add the clean, translated DTO to our outgoing list
                dtos.Add(dto);
            }

            // 3. Return the clean DTOs to the Controller
            return dtos;
        }

    }

}

