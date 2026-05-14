using System;
using System.Collections.Generic;
using System.Text;
using Business_Layer.DTOs.ProductDTO;
using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;

namespace Business_Layer.Services
{
    public class ProductLogicService
    {
        private readonly IProductDataService _dataService;

        public ProductLogicService(IProductDataService dataService)
        {
            _dataService = dataService;
        }


        public async Task<bool> RegisterProductAsync(ProductRequestDTO requestDto)
        {
            if (requestDto == null) throw new ArgumentNullException(nameof(requestDto));

            // Map the DTO to your actual database Entity
            var productEntity = new Product
            {
                Barcode = requestDto.Barcode,
                Name = requestDto.Name,
                Category = requestDto.Category,
                WeightGrams = requestDto.WeightGrams,
                Price = requestDto.Price,

                // THE SERVER GENERATES THE TIMESTAMP HERE
                DateAdded = Timestamp.GetCurrentTimestamp()
            };
            // Pass the fully constructed Entity down to the Data Layer
            return await _dataService.RegisterProductAsync(productEntity);
        }

        public async Task<ProductRequestDTO> GetProductAsync(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) throw new ArgumentException("Barcode cannot be null or empty.", nameof(barcode));
            var productEntity = await _dataService.GetProductAsync(barcode);

            var prodRequest = new ProductRequestDTO
            {
                Barcode = productEntity.Barcode,
                Name = productEntity.Name,
                Category = productEntity.Category,    
                WeightGrams = productEntity.WeightGrams,
                Price = productEntity.Price,
            };

            return prodRequest;
        }


        public async Task<List<ProductRequestDTO>> GetProductsAsync()
        {
            var productEntities = await _dataService.getProductsAsync();
            var productDTOs = new List<ProductRequestDTO>();
            foreach (var product in productEntities)
            {
                var dto = new ProductRequestDTO
                {
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Category = product.Category,
                   
                    WeightGrams = product.WeightGrams,
                    Price = product.Price,

                };
                productDTOs.Add(dto);
            }
            return productDTOs;
        }

        public async Task<bool> UpdateProductAsync(ProductRequestDTO requestDto)
        {
            if (requestDto == null)
                throw new ArgumentNullException(nameof(requestDto));

            var existingProduct = await _dataService.GetProductAsync(requestDto.Barcode);

            if (existingProduct == null)
                return false;

            var updatedProduct = new Product
            {
                Barcode = requestDto.Barcode,
                Name = requestDto.Name,
                Category = requestDto.Category,
                WeightGrams = requestDto.WeightGrams,
                Price = requestDto.Price,

                // preserve original date
                DateAdded = existingProduct.DateAdded
            };

            return await _dataService.UpdateProductAsync(updatedProduct);
        }

    }
}
