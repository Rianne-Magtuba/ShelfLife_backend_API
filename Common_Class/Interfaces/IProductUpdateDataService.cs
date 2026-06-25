using Common_Class.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Class.Interfaces
{
    public interface IProductUpdateDataService
    {
        Task<bool> CreateRequestAsync(ProductUpdateRequest request);
        Task<List<ProductUpdateRequest>> GetPendingRequestsAsync();

        Task<bool> UpdateProductStatus(ProductUpdateRequest request);
        Task<ProductUpdateRequest?> GetRequestByIdAsync(string requestId);
    }
}
