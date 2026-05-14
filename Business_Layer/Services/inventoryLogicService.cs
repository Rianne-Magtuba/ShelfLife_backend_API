
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
                    Notes = entity.Notes,
                    DisplayPrice = entity.CustomPrice ?? 0, // Assuming custom price is the only price for now
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
            var inventoryEntity = new Food
            {
                InventoryId = "",
                IsCustomItem = processedDTO.IsCustomItem,
                BarcodeRef = processedDTO.BarcodeRef,
                CustomName = processedDTO.CustomName,
                CustomCategory = processedDTO.CustomCategory,
                CustomWeightGrams = processedDTO.CustomWeightGrams,
                CustomPrice = processedDTO.CustomPrice,
                Quantity = processedDTO.Quantity,
                Notes = processedDTO.Notes,
                ExpirationDate = Timestamp.FromDateTime(processedDTO.ExpirationDate.ToUniversalTime()),
                DateRegistered = Timestamp.FromDateTime(DateTime.UtcNow),
                isDiscarded = false,
                Quality = processedDTO.Quality

            };

            return await _dataService.AddInventoryItemAsync(inventoryEntity, userId);
        }

        public async Task<AddInventoryItemRequestDto> isCustomItemHelper(AddInventoryItemRequestDto requestDto)
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
                requestDto.CustomPrice = requestDto.CustomPrice != null ? Math.Round(requestDto.CustomPrice.Value, 2) : null; // Round price to 2 decimal places if provided

            }

            return requestDto;
        }

        public async Task<AddInventoryItemRequestDto> getBarcodedItemToAdd(AddInventoryItemRequestDto requestDto)
        {
            Product prod = await _productDataService.GetProductAsync(requestDto.BarcodeRef);
            requestDto.CustomName = prod.Name;
            requestDto.CustomCategory = prod.Category;
            requestDto.CustomWeightGrams = prod.WeightGrams;
            requestDto.CustomPrice = prod.Price;
            return requestDto;
        }

        public async Task<bool> DiscardInventoryItemAsync(string inventoryId, string userId)
        {
            if(inventoryId == null)
            {
                throw new ArgumentNullException("Error No Inventory Id Provided");
            }
            if (userId == null)
            {
                throw new ArgumentNullException("Error No User Id Provided");
            }
            return await _dataService.DiscardFoodItemAsync(inventoryId, userId);
        }

        public async Task<bool> UpdateInventoryItemAsync(string inventoryId, AddInventoryItemRequestDto requestDto, string userId)
        {
            if (string.IsNullOrEmpty(inventoryId))
            {
                throw new ArgumentNullException(nameof(inventoryId));
            }

            var existingItems = await _dataService.GetUserInventoryAsync(userId);

            var existingItem = existingItems
                .FirstOrDefault(x => x.InventoryId == inventoryId);

            if (existingItem == null)
            {
                return false;
            }

            var processedDTO = await isCustomItemHelper(requestDto);

            var updatedEntity = new Food
            {
                InventoryId = inventoryId,

                IsCustomItem = processedDTO.IsCustomItem,
                BarcodeRef = processedDTO.BarcodeRef,

                CustomName = processedDTO.CustomName,
                CustomCategory = processedDTO.CustomCategory,
                CustomWeightGrams = processedDTO.CustomWeightGrams,
                CustomPrice = processedDTO.CustomPrice,

                Quantity = processedDTO.Quantity,
                Notes = processedDTO.Notes,
                Quality = processedDTO.Quality,

                ExpirationDate = Timestamp.FromDateTime(
                    processedDTO.ExpirationDate.ToUniversalTime()
                ),

                // preserve original
                DateRegistered = existingItem.DateRegistered,
                isDiscarded = existingItem.isDiscarded
            };

            return await _dataService.UpdateInventoryItemAsync(updatedEntity, userId);
        }
    }
}

