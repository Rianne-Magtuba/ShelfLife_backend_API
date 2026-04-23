using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data_Layer.Services
{
    public class InventoryDataService: IInventoryDataService
    {
        private readonly FirestoreDb _firestoreDb;

        public InventoryDataService(FirestoreDb firestoreDb) {
            _firestoreDb = firestoreDb;
        }

        private void initializeFirebaseApp()
        {
        }

        public async Task<List<InventoryEntity>> GetUserInventoryAsync(string userId)
        {
            var collectionRef = _firestoreDb.Collection("Users").Document(userId).Collection("inventory");
            var snapshot = await collectionRef.GetSnapshotAsync();
            var inventoryList = new List<InventoryEntity>();

            foreach(var document in snapshot.Documents)
            {
                if(document.Exists)
                {
                    var inventoryItem = document.ConvertTo<InventoryEntity>();
                    inventoryList.Add(inventoryItem);
                }
            }

            return inventoryList;

        }
        private void addProduct(ProductEntity product)
        {

        }
       
    }
}
