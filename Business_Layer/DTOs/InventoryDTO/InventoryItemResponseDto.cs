using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.InventoryDTO
{
    public class InventoryItemResponseDto
    {
        public string InventoryId { get; set; } = string.Empty;

        // Metadata so Flutter knows what kind of item it is (
        public bool IsCustomItem { get; set; }
        public string? BarcodeRef { get; set; }

        // Unified Display Fields 
        public string DisplayName { get; set; } = string.Empty;
        public string DisplayCategory { get; set; } = string.Empty;
     
        public double? WeightGrams { get; set; }

        // Instance-specific inventory fields
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
