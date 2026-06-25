using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.DTOs.ProductDTO
{
    public class ReviewUpdateRequestDTO
    {
        // Should be exactly "Approved" or "Rejected"
        public string NewStatus { get; set; } = string.Empty;
    }
}
