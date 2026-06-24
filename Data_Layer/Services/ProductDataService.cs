using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data_Layer.Services
{
    public class ProductDataService : IProductDataService
    {
        private readonly FirestoreDb _firestoreDb;
        public ProductDataService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }


        public async Task<Product> GetProductAsync(string barcode)
        {
            var collectionRef = _firestoreDb.Collection("Product Catalog").Document(barcode);
            var snapshot = await collectionRef.GetSnapshotAsync();
           

          Product product = snapshot.ConvertTo<Product>();

            return product;
      
        }

        public async Task<List<Product>> getProductsAsync()
        {
            var collectionRef = _firestoreDb.Collection("Product Catalog");
            var snapshot = await collectionRef.GetSnapshotAsync();
            List <Product> productList = new List<Product>();

            foreach(var document in snapshot.Documents)
            {
                if(document.Exists)
                {
                    var product = document.ConvertTo<Product>();
                    productList.Add(product);
                }
            }

            return productList;
        }

        public async Task<List<Product>> getProductsByCategoryAsync(string category)
        {
            var collectionRef = _firestoreDb.Collection("Product Catalog");
            var snapshot = collectionRef.WhereEqualTo("category", category).GetSnapshotAsync();


            List<Product> productList = new List<Product>();

            foreach (var document in snapshot.Result)
            {
                if (document.Exists)
                {
                    var product = document.ConvertTo<Product>();
                    productList.Add(product);
                }
            }

            return productList;
        }

        public async Task<bool> RegisterProductAsync(Product product)
        {
            try
            {
                CollectionReference collectionRef = _firestoreDb.Collection("Product Catalog");

                DocumentReference documentRef = collectionRef.Document(product.Barcode);

                await documentRef.SetAsync(product);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering product: {ex.Message}");
                return false;
            }
        }
       


        public Task RemoveProductAsync(string barcode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var documentRef = _firestoreDb
                    .Collection("Product Catalog")
                    .Document(product.Barcode);

                var snapshot = await documentRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return false;
                }

                await documentRef.SetAsync(product, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return false;
            }
        }
    }
}
