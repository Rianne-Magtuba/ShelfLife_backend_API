using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.DTOs.ProductDTO
{
    public class ProductRequestDTO
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public double WeightGrams { get; set; }

        public double Price { get; set; }   
    }
}
