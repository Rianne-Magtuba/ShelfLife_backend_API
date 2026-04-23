using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Class.Entities
{
    [FirestoreData]
    public class InventoryEntity
    {
        [FirestoreDocumentId]
        public string InventoryId { get; set; } = string.Empty;

        [FirestoreProperty("isCustomItem")]
        public bool IsCustomItem { get; set; }

        // The raw reference to the global catalog
        [FirestoreProperty("barcodeRef")]
        public string? BarcodeRef { get; set; }

        [FirestoreProperty("customName")]
        public string? CustomName { get; set; }

        [FirestoreProperty("customCategory")]
        public string? CustomCategory { get; set; }

        [FirestoreProperty("customWeightGrams")]
        public double? CustomWeightGrams { get; set; }

        [FirestoreProperty("quantity")]
        public int Quantity { get; set; }

        // Storing timestamps exactly as Firestore requires them
        [FirestoreProperty("expirationDate")]
        public Timestamp ExpirationDate { get; set; }

        [FirestoreProperty("dateRegistered")]
        public Timestamp DateRegistered { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; } = "active";

    }
}
