using EnsuranceProjectLib.Infrastructure;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using System.Data.Entity;

namespace EnsuranceProjectLib.Repository.AdminRepo
{
    public class AllRepository<T> : IAllRepository<T> where T : class
    {
        private readonly BankInsuranceDbContext _bankDb;
        private DbSet<T> _dbEntity;

        public AllRepository(BankInsuranceDbContext bankInsuranceDb)
        {
            _bankDb = bankInsuranceDb;
            _dbEntity = _bankDb.Set<T>();
        }
       
        public async Task Add(T entity)
        {
            await _dbEntity.AddAsync(entity);
            await _bankDb.SaveChangesAsync().ConfigureAwait(true);

        }

        public async Task Delete(T entity)
        {
             _dbEntity.Remove(entity);
            await _bankDb.SaveChangesAsync().ConfigureAwait(true);
        }

        public  List<T> GetAll()
        {
            
            return _dbEntity.ToList();
        }

        public async Task<T> GetById(int entityId)
        {
            return await _dbEntity.FindAsync(_dbEntity);
        }

        public async Task Update(T entity)
        {
           _dbEntity.Update(entity);           
           await _bankDb.SaveChangesAsync();
        }

     
    }
}
