using Google.Cloud.Firestore;

namespace Common_Class.Entities
{
    public class ProductEntity
    {

        [FirestoreDocumentId]
        public string Barcode { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("description")]
        public string Description { get; set; } = string.Empty;

        [FirestoreProperty("category")]
        public string Category { get; set; } = string.Empty;

        [FirestoreProperty("weightGrams")]
        public double WeightGrams { get; set; }

        // Server-controlled audit field
        [FirestoreProperty("dateAdded")]
        public Timestamp DateAdded { get; set; }

    }
}
