
using Common_Class.Entities;
using Common_Class.Interfaces;
using Business_Layer.DTOs.InventoryDTO;
using Business_Layer.DTOs.ProductDTO;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
namespace Business_Layer.Services
{
    public class inventoryLogicService
    {
        private readonly IInventoryDataService _dataService;
        private readonly IProductDataService _productDataService;

        // You inject the INTERFACE, not the Data Layer class!
        public inventoryLogicService(IInventoryDataService dataService, IProductDataService productDataService)
        {
            _dataService = dataService;
            _productDataService = productDataService;
        }

        public async Task<List<InventoryItemResponseDto>> GetUserPantryAsync(string userId)
        {
            // 1. Call the interface method to get raw database entities
            List<Food> entities = await _dataService.GetUserInventoryAsync(userId);

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
   
                    dto = await getBarcodedItemForRequest(dto);


                }

                // Add the clean, translated DTO to our outgoing list
                dtos.Add(dto);
            }

            // 3. Return the clean DTOs to the Controller
            return dtos;
        }
        public async Task<InventoryItemResponseDto> getBarcodedItemForRequest(InventoryItemResponseDto reponseDto)
        {
            Product prod = await _productDataService.GetProductAsync(reponseDto.BarcodeRef);
            reponseDto.DisplayName = prod.Name;
            reponseDto.DisplayCategory = prod.Category;
            reponseDto.WeightGrams = prod.WeightGrams;
            return reponseDto;
        }



        public async Task<bool> AddInventoryItemAsync(AddInventoryItemRequestDto requestDto, string userId)
        {
            var processedDTO = await isCustomItemHelper(requestDto);
            var inventoryEntity = new Food { 
             InventoryId = "",
             IsCustomItem = processedDTO.IsCustomItem,
             BarcodeRef = processedDTO.BarcodeRef,
             CustomName = processedDTO.CustomName,
             CustomCategory = processedDTO.CustomCategory,
             CustomWeightGrams = processedDTO.CustomWeightGrams,
             Quantity = processedDTO.Quantity,
             ExpirationDate = Timestamp.FromDateTime(processedDTO.ExpirationDate.ToUniversalTime()),
             DateRegistered = Timestamp.FromDateTime(DateTime.UtcNow),
             Status = "Active"

            };
   
            return await _dataService.AddInventoryItemAsync(inventoryEntity, userId);
        }

        public async Task<AddInventoryItemRequestDto>isCustomItemHelper(AddInventoryItemRequestDto requestDto)
        {
            if (!requestDto.IsCustomItem)
            {
                if (string.IsNullOrEmpty(requestDto.BarcodeRef))
                {
                    throw new ArgumentException("Barcoded items must have a barcode reference.");

                }

                requestDto = await getBarcodedItemToAdd(requestDto);
            }
            else
            {
                if (string.IsNullOrEmpty(requestDto.CustomName) || string.IsNullOrEmpty(requestDto.CustomCategory) || requestDto.CustomWeightGrams == null)
                {
                    throw new ArgumentException("Custom items must have a name, category, and weight.");
                }
                requestDto.BarcodeRef = null; // Ensure barcode is null for custom items
                requestDto.CustomName = requestDto.CustomName.Trim();
                requestDto.CustomWeightGrams = Math.Round(requestDto.CustomWeightGrams.Value, 2); // Round weight to 2 decimal places   
                requestDto.CustomCategory = requestDto.CustomCategory.Trim();

            }
              
            return requestDto;
        }

        public async Task<AddInventoryItemRequestDto> getBarcodedItemToAdd(AddInventoryItemRequestDto requestDto)
        {
           Product prod = await _productDataService.GetProductAsync(requestDto.BarcodeRef);
            requestDto.CustomName = prod.Name;
            requestDto.CustomCategory = prod.Category;
            requestDto.CustomWeightGrams = prod.WeightGrams;
            return requestDto;
        }

    



    }
}

