using System;
using System.Collections.Generic;
using System.Text;

namespace Business_Layer.DTOs.InventoryDTO
{
    public class AddInventoryItemRequestDto
    {
        public bool IsCustomItem { get; set; }
        // Populated ONLY if IsCustomItem == false
        public string? BarcodeRef { get; set; }

        // Populated ONLY if IsCustomItem == true
        public string? CustomName { get; set; }
        public string? CustomCategory { get; set; }
        public double? CustomWeightGrams { get; set; }
        public double? CustomPrice { get; set; }

        // Always required
        public int Quantity { get; set; }
        public string Quality { get; set; }

        public string Notes { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
