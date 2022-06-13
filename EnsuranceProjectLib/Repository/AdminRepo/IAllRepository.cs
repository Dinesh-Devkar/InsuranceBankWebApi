using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectLib.Repository.AdminRepo
{
    public interface IAllRepository<T> where T : class
    {
        void Add(T entity);
        Task<IEnumerable<T>> GetAll();
    }
}
