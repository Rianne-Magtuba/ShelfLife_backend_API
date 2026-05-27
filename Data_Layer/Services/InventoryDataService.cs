using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data_Layer.Services
{
    public class InventoryDataService : IInventoryDataService
    {
        private readonly FirestoreDb _firestoreDb;

        public InventoryDataService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }


        public async Task<List<Food>> GetUserInventoryAsync(string userId)
        {
            var collectionRef = _firestoreDb.Collection("Users").Document(userId).Collection("inventory");
            var query = collectionRef.
                WhereEqualTo("isDiscarded", false).
                WhereEqualTo("isConsumed", false);
            var snapshot = await query.GetSnapshotAsync();
            var inventoryList = new List<Food>();

            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var inventoryItem = document.ConvertTo<Food>();
                    inventoryList.Add(inventoryItem);
                }
            }

            return inventoryList;

        }

        public async Task<Food> AddInventoryItemAsync(Food item, string userId)
        {
            try
            {
                CollectionReference collectionRef = _firestoreDb.Collection("Users").Document(userId).Collection("inventory");
                DocumentReference docRef = await collectionRef.AddAsync(item);

                var food = new Food
                {
                    InventoryId = docRef.Id,
                    IsCustomItem = item.IsCustomItem,
                    BarcodeRef = item.BarcodeRef,
                    CustomName = item.CustomName,
                    CustomCategory = item.CustomCategory,
                    CustomWeightGrams = item.CustomWeightGrams,
                    CustomPrice = item.CustomPrice,
                    Quantity = item.Quantity,
                    ExpirationDate = item.ExpirationDate,
                    DateRegistered = item.DateRegistered,
                    Notes = item.Notes,
                    isDiscarded = item.isDiscarded,
                    Quality = item.Quality
                };
                return food;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DiscardFoodItemAsync(string inventoryId, string userId)
        {
            try
            {
                DocumentReference documentRef = _firestoreDb.Collection("Users")
                                                            .Document(userId)
                                                            .Collection("inventory")
                                                            .Document(inventoryId);

                await documentRef.UpdateAsync("isDiscarded", true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discarding item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateInventoryItemAsync(Food item, string userId)
        {
            try
            {
                DocumentReference documentRef = _firestoreDb
                    .Collection("Users")
                    .Document(userId)
                    .Collection("inventory")
                    .Document(item.InventoryId);

                var snapshot = await documentRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return false;
                }

                await documentRef.SetAsync(item, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating inventory item: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConsumeFoddItemAsync(string inventoryId, string userId)
        {
            try
            {
                DocumentReference documentRef = _firestoreDb.Collection("Users")
                                                            .Document(userId)
                                                            .Collection("inventory")
                                                            .Document(inventoryId);

                await documentRef.UpdateAsync("isConsumed", true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error discarding item: {ex.Message}");
                return false;
            }
        }
    }
}
