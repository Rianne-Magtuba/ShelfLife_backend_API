
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



        public async Task<InventoryItemResponseDto?> AddInventoryItemAsync(AddInventoryItemRequestDto requestDto, string userId)
        {
            // 1. Map Request DTO to Entity
            var inventoryEntity = new Food
            {
                InventoryId = "",
                IsCustomItem = requestDto.IsCustomItem,
                BarcodeRef = requestDto.BarcodeRef,
                CustomName = requestDto.CustomName,
                CustomCategory = requestDto.CustomCategory,
                CustomWeightGrams = requestDto.CustomWeightGrams,
                // (Note: Double check this line in your original code, it maps weight to price)
                CustomPrice = requestDto.CustomPrice,
                Quantity = requestDto.Quantity,
                Notes = requestDto.Notes,
                ExpirationDate = Timestamp.FromDateTime(requestDto.ExpirationDate.ToUniversalTime()),
                DateRegistered = Timestamp.FromDateTime(DateTime.UtcNow),
                isDiscarded = false,
                Quality = requestDto.Quality
            };

            // 2. Call Data Layer and get the saved entity (which now includes docRef.Id)
            Food? savedFood = await _dataService.AddInventoryItemAsync(inventoryEntity, userId);

            if (savedFood == null)
            {
                return null; // Database operation failed
            }

            // 3. Map the saved Entity back to the Response DTO
            var responseDto = new InventoryItemResponseDto
            {
                InventoryId = savedFood.InventoryId, // The real Firestore ID
                IsCustomItem = savedFood.IsCustomItem,
                BarcodeRef = savedFood.BarcodeRef,

                // Map your entity fields to your unified display fields
                DisplayName = savedFood.CustomName ?? string.Empty,
                DisplayCategory = savedFood.CustomCategory ?? string.Empty,
                WeightGrams = savedFood.CustomWeightGrams,
                DisplayPrice = savedFood.CustomPrice ?? 0.0,
                Quantity = savedFood.Quantity,
                Quality = savedFood.Quality,
                Notes = savedFood.Notes ?? string.Empty,

                // Convert Firestore Timestamps back to C# DateTime for the JSON response
                ExpirationDate = savedFood.ExpirationDate.ToDateTime(),
                DateRegistered = savedFood.DateRegistered.ToDateTime()
            };

            return responseDto;
        }

        //public async Task<AddInventoryItemRequestDto> isCustomItemHelper(AddInventoryItemRequestDto requestDto)
        //{
        //    if (!requestDto.IsCustomItem)
        //    {
        //        if (string.IsNullOrEmpty(requestDto.BarcodeRef))
        //        {
        //            throw new ArgumentException("Barcoded items must have a barcode reference.");

        //        }

        //        requestDto = await getBarcodedItemToAdd(requestDto);
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(requestDto.CustomName) || string.IsNullOrEmpty(requestDto.CustomCategory) || requestDto.CustomWeightGrams == null)
        //        {
        //            throw new ArgumentException("Custom items must have a name, category, and weight.");
        //        }

        //        requestDto.BarcodeRef = null; // Ensure barcode is null for custom items
        //        requestDto.CustomName = requestDto.CustomName.Trim();
        //        requestDto.CustomWeightGrams = Math.Round(requestDto.CustomWeightGrams.Value, 2); // Round weight to 2 decimal places   
        //        requestDto.CustomCategory = requestDto.CustomCategory.Trim();
        //        requestDto.CustomPrice = requestDto.CustomPrice != null ? Math.Round(requestDto.CustomPrice.Value, 2) : null; // Round price to 2 decimal places if provided

        //    }

        //    return requestDto;
        //}

        //public async Task<AddInventoryItemRequestDto> getBarcodedItemToAdd(AddInventoryItemRequestDto requestDto)
        //{
        //    Product prod = await _productDataService.GetProductAsync(requestDto.BarcodeRef);
        //    requestDto.CustomName = prod.Name;
        //    requestDto.CustomCategory = prod.Category;
        //    requestDto.CustomWeightGrams = prod.WeightGrams;
        //    requestDto.CustomPrice = prod.Price;
        //    return requestDto;
        //}

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

            //var processedDTO = await isCustomItemHelper(requestDto);

            var updatedEntity = new Food
            {
                InventoryId = inventoryId,

                IsCustomItem = requestDto.IsCustomItem,
                BarcodeRef = requestDto.BarcodeRef,

                CustomName = requestDto.CustomName,
                CustomCategory = requestDto.CustomCategory,
                CustomWeightGrams = requestDto.CustomWeightGrams,
                CustomPrice = requestDto.CustomPrice,

                Quantity = requestDto.Quantity,
                Notes = requestDto.Notes,
                Quality = requestDto.Quality,

                ExpirationDate = Timestamp.FromDateTime(
                    requestDto.ExpirationDate.ToUniversalTime()
                ),

                // preserve original
                DateRegistered = existingItem.DateRegistered,
                isDiscarded = existingItem.isDiscarded
            };

            return await _dataService.UpdateInventoryItemAsync(updatedEntity, userId);
        }
    }
}

