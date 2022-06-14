using EnsuranceProjectEntityLib.Model.AdminModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsuranceProjectLib.Repository.Common
{
    public interface IStateRepository
    {
        Task<IEnumerable<State>> GetAllStates();
        Task<Boolean> UpdateState(State state);

        Task<Boolean> AddState(State state);
         
    }
}
