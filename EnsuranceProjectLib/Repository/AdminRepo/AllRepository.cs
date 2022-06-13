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

        public AllRepository()
        {
            //_bankDb = new BankInsuranceDbContext();
            _dbEntity = _bankDb.Set<T>();
        }
        //public async Task<string> AddAdmin(AdminAddDto admin)
        //{
        //    if (admin != null)
        //    {
        //        await this._bankDb.Admins.AddAsync(new Admin()
        //        {
        //            Name = admin.Name,
        //            DateOfBirth = admin.DateOfBirth,
        //            MobileNumber = admin.MobileNumber,
        //            LoginId = admin.LoginId,
        //            Password = admin.Password,
        //        });
        //        if (await this._bankDb.SaveChangesAsync() > 0)
        //        {
        //            return "Admin Added Successfully";
        //        }
        //    }
        //    return "Something Went Wrong User Not Added";
        //}
        public void Add(T entity)
        {
            _dbEntity.Add(entity);
            //_bankDb.Admins.Add(entity);
            _bankDb.SaveChanges();

        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return _dbEntity.ToList();
        }
    }
}
