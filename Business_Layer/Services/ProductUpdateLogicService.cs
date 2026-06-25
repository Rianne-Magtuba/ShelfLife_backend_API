using Business_Layer.DTOs.ProductDTO;
using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.Services
{
    public class ProductUpdateLogicService
    {
        private readonly IProductUpdateDataService _updateDataService;
        private readonly IProductDataService _productDataService;

        public ProductUpdateLogicService(IProductUpdateDataService updateDataService, IProductDataService productDataService)
        {
            _updateDataService = updateDataService;
            _productDataService = productDataService;
        }

        public async Task<bool> RequestUpdateAsync(CreateProductUpdateRequestDTO dto, string userId)
        {
            // 1. Validate that the product exists before allowing a request
            var existingProduct = await _productDataService.GetProductAsync(dto.Barcode);
            if (existingProduct == null)
            {
                return false;
            }

            // 2. Map to entity
            var request = new ProductUpdateRequest
            {
                Barcode = dto.Barcode,
                UserId = userId,
                ProposedName = dto.ProposedName,
                ProposedCategory = dto.ProposedCategory,
                ProposedWeightGrams = dto.ProposedWeightGrams,
                ProposedPrice = dto.ProposedPrice,
                Status = "Pending",
                RequestDate = Timestamp.GetCurrentTimestamp()
            };

            return await _updateDataService.CreateRequestAsync(request);
        }

        public async Task<List<ProductUpdateRequest>> GetPendingUpdatesAsync()
        {
            return await _updateDataService.GetPendingRequestsAsync();
        }

        public async Task<bool> ProcessUpdateRequestAsync(string requestId, string newStatus)
        {
            // 1. Fetch the pending request
            var updateRequest = await _updateDataService.GetRequestByIdAsync(requestId);

            if (updateRequest == null || updateRequest.Status != "Pending")
            {
                return false; // Request doesn't exist or was already processed
            }

            // 2. If Approved, apply the changes to the actual Product Catalog
            if (newStatus == "Approved")
            {
                var actualProduct = await _productDataService.GetProductAsync(updateRequest.Barcode);

                if (actualProduct != null)
                {
                    actualProduct.Name = updateRequest.ProposedName;
                    actualProduct.Category = updateRequest.ProposedCategory;
                    actualProduct.WeightGrams = updateRequest.ProposedWeightGrams;
                    actualProduct.Price = updateRequest.ProposedPrice;

                    // Save changes to the main Product Catalog
                    var productUpdated = await _productDataService.UpdateProductAsync(actualProduct);

                    if (!productUpdated) return false; // Failsafe
                }
            }

            // 3. Update the status on the request ticket itself
            updateRequest.Status = newStatus;

            // Call the method you just wrote!
            return await _updateDataService.UpdateProductStatus(updateRequest);
        }
    }
}
