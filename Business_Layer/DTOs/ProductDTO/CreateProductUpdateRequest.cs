using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.DTOs.ProductDTO
{
 public class CreateProductUpdateRequestDTO
    {
        public string Barcode { get; set; } = string.Empty;
        public string ProposedName { get; set; } = string.Empty;
        public string ProposedCategory { get; set; } = string.Empty;
        public double ProposedWeightGrams { get; set; }
        public double ProposedPrice { get; set; }
    }
}
