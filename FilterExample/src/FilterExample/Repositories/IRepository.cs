using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilterExample.Repositories
{
    public interface IRepository<T>
    {
        Task<T> CreateAsync(T model);
        Task<T> FindAsync(params object[] keys);
        Task<IEnumerable<T>> GetAsync();
        Task<T> UpdateAsync(T model, params object[] key);
    }
}