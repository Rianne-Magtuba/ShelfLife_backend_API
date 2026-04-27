using System;
using System.Collections.Generic;
using System.Text;
using Common_Class.Entities;


namespace Common_Class.Interfaces
{
    public interface IProductDataService
    {
        public Task<bool> RegisterProductAsync(Product product);

        public Task RemoveProductAsync(string barcode);

        public Task<Product> GetProductAsync(string barcode);

        public Task<List<Product>> getProductsAsync();
    }
}
