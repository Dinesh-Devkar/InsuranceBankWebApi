using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly IAllRepository<State> _stateManager;
        public StateController(BankInsuranceDbContext bankInsuranceDb)
        {
            this._stateManager=new AllRepository<State>(bankInsuranceDb);
        }
        [HttpPost]
        [Route("AddState")]
        [Authorize(UserRoles.Admin)]
        public async Task<IActionResult> AddState([FromBody] StateAddDto model)
        {
            //To add a new State
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            if (model.StateName.Length == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            
            var isStateExist = this._stateManager.GetAll().ToList().Find(x => x.StateName == model.StateName);
            if (isStateExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "State Already Exists" });
            }
            await this._stateManager.Add(new State() { StateName = model.StateName, Status = model.Status });
            return this.Ok(new Response { Message = "State Added Successfully", Status = "Success" });
        }


        [HttpPut]
        [Route("UpdateState")]
        [Authorize(Roles = UserRoles.Employee + "," + UserRoles.Admin)]
        public async Task<IActionResult> UpdateState([FromBody] StateAddDto model)
        {
           //To update a state
            if (!ModelState.IsValid)
            {
               
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            var isStateExist = this._stateManager.GetAll().ToList().Find(x => x.StateName == model.StateName);
            if (isStateExist != null)
            {
                if (model.Status == "Active" || model.Status == "InActive")
                {
                    isStateExist.Status = model.Status;
                    await this._stateManager.Update(isStateExist);
                    return this.Ok(new Response { Message = "State Updated Successfully", Status = "Success" });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid State Status" });

            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "State Not Found" });


        }

        [HttpGet]
        [Route("GetAllStates")]
        public async Task<List<State>> GetAllStates()
        {
            //Return a list of all states
            return this._stateManager.GetAll();
        }
    }
}
