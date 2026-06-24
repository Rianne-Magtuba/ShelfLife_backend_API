using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Class.Entities
{
    [FirestoreData]
    public class ProductUpdateRequest
    {
        // Firestore will auto-generate this ID when added
        [FirestoreDocumentId]
        public string RequestId { get; set; } = string.Empty;

        [FirestoreProperty("barcode")]
        public  string Barcode { get; set; } = string.Empty;

        [FirestoreProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [FirestoreProperty("proposedName")]
        public string ProposedName { get; set; } = string.Empty;

        [FirestoreProperty("proposedCategory")]
        public string ProposedCategory { get; set; } = string.Empty;

        [FirestoreProperty("proposedWeightGrams")]
        public double ProposedWeightGrams { get; set; }

        [FirestoreProperty("proposedPrice")]
        public double ProposedPrice { get; set; }

        // Tracks the lifecycle: "Pending", "Approved", or "Rejected"
        [FirestoreProperty("status")]
        public string Status { get; set; } = "Pending";

        [FirestoreProperty("requestDate")]
        public Timestamp RequestDate { get; set; }
    }
}
