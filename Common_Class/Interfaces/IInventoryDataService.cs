using Common_Class.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Class.Interfaces
{
    public interface IInventoryDataService
    {
        Task<List<Food>> GetUserInventoryAsync(string userId);

        Task<bool> AddInventoryItemAsync(Food item, string userId);


    }
}
