using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectLib.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectLib.Repository.Common
{
    public class StateRepository : IStateRepository
    {
        private BankInsuranceDbContext _bankDb;
        public StateRepository(BankInsuranceDbContext dbContext)
        {
            _bankDb = dbContext;
        }

        public async Task<bool> AddState(State state)
        {
            //if (state != null)
            //{
            //    this._bankDb.States.Add(state);
            //    if (this._bankDb.SaveChangesAsync() > 0)
            //    {

            //    }
            //}
            return true;
        }

        public async Task<IEnumerable<State>> GetAllStates()
        {
            return await this._bankDb.States.ToListAsync();
        }

        public async Task<bool> UpdateState(State stateUpdate)
        {
            var state = this._bankDb.States.ToList().Find(x => x.StateName == stateUpdate.StateName);
            if (state != null)
            {
                this._bankDb.Entry(state).State=EntityState.Modified;
                if(await this._bankDb.SaveChangesAsync()>0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
