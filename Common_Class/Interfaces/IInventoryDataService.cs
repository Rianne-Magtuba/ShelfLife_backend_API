using Common_Class.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Class.Interfaces
{
    public interface IInventoryDataService
    {
        Task<List<InventoryEntity>> GetUserInventoryAsync(string userId);
    }
}
