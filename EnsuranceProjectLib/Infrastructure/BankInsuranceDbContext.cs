
using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Repository.AdminRepo;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectLib.Infrastructure
{
    //public class BankInsuranceDbContext:DbContext
    //{
    //    public BankInsuranceDbContext()
    //    { }

    //    public DbSet<Admin> Admins { get; set; }
    //    public DbSet<Customer> Customers { get; set; }
    //    public DbSet<CustomerDocument> CustomersDocuments { get; set; }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {
    //        optionsBuilder.UseSqlServer(@"server=.\sqlexpress;database=BankInsurance;trusted_connection=true");
    //    }
    //}
    public class BankInsuranceDbContext : IdentityDbContext<EnsuranceProjectEntityLib.Model.Common.ApplicationUser>,IBankInsuranceDbContext
    {

        public BankInsuranceDbContext(DbContextOptions<BankInsuranceDbContext> options):base(options)
        { }

        public DbSet<ApplicationUser> Admins { get; set; }
        //public DbSet<ApplicationUser> Employees { get; set; }
        //public DbSet<ApplicationUser> Customers { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Query> Queries { get; set; } 
        public DbSet<CustomerDocument> CustomersDocuments { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<InsuranceScheme> InsuranceSchemes { get; set; }
        public DbSet<InsurancePlan> InsurancePlans { get; set;}
        public DbSet<InsuranceAccount> InsuranceAccounts { get; set;}
        public DbSet<CommissionRecord> CommissionRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(@"server=.\sqlexpress;database=BankInsurance;trusted_connection=true");
        //}
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        
    }
}
