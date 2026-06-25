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

        public async Task<bool> UpdateProductStatus(ProductUpdateRequest request)
        {
            var documentRef = _firestoreDb.Collection("ProductUpdateRequests").Document(request.RequestId);
            var snapshot = await documentRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return false;
            }

            await documentRef.SetAsync(request, SetOptions.Overwrite);

            return true;
        }

        public async Task<ProductUpdateRequest?> GetRequestByIdAsync(string requestId)
        {
            var doc = await _firestoreDb.Collection("ProductUpdateRequests").Document(requestId).GetSnapshotAsync();
            if (!doc.Exists) return null;

            return doc.ConvertTo<ProductUpdateRequest>();
        }
    }
}
