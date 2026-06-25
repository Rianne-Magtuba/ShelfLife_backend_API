using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Layer.Services
{
    public class ProductUpdateDataService : IProductUpdateDataService
    {
        private readonly FirestoreDb _firestoreDb;

        public ProductUpdateDataService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<bool> CreateRequestAsync(ProductUpdateRequest request)
        {
            try
            {
                var collectionRef = _firestoreDb.Collection("ProductUpdateRequests");
                await collectionRef.AddAsync(request);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating update request: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ProductUpdateRequest>> GetPendingRequestsAsync()
        {
            var collectionRef = _firestoreDb.Collection("ProductUpdateRequests");

            // Only fetch requests that haven't been resolved by an Admin
            var snapshot = await collectionRef.WhereEqualTo("status", "Pending").GetSnapshotAsync();

            List<ProductUpdateRequest> list = new List<ProductUpdateRequest>();
            foreach (var doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    list.Add(doc.ConvertTo<ProductUpdateRequest>());
                }
            }
            return list;
        }
    }
}
