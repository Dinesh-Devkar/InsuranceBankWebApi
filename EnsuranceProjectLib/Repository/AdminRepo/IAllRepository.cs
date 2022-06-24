using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectLib.Repository.AdminRepo
{
    public interface IAllRepository<T> where T : class
    {
        Task Add(T entity);
        List<T> GetAll();
        Task Update(T entity);
        Task<T> GetById(int entityId);
        Task Delete(T entity);

    }
}
