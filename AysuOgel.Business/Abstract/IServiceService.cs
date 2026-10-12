using AysuOgel.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AysuOgel.Business.Abstract
{
    public interface IServiceService
    {
        Task<List<Service>> GetActiveAsync();
        Task<List<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(int id);
        Task CreateAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(int id);
    }
}
