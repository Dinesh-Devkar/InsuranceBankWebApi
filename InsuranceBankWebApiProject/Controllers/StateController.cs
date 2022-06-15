using EnsuranceProjectEntityLib.Model.AdminModel;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
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
        public async Task<IActionResult> AddState([FromBody] StateAddDto model)
        {
            Debug.WriteLine("Inside Add State");
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("Inside Model State");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            if (model.StateName.Length == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            Debug.WriteLine("Outside IF Condition");
            //var isStateExist = _bankInsuranceDbContext.States.ToList().Find(x=>x.StateName==model.StateName);
            var isStateExist = this._stateManager.GetAll().ToList().Find(x => x.StateName == model.StateName);
            if (isStateExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "State Already Exists" });
            }
            //_bankInsuranceDbContext.States.Add(new State() { StateName = model.StateName,Status=model.Status});
            //_bankInsuranceDbContext.SaveChanges();
            await this._stateManager.Add(new State() { StateName = model.StateName, Status = model.Status });
            return this.Ok(new Response { Message = "State Added Successfully", Status = "Success" });
        }


        [HttpPut]
        [Route("UpdateState")]
        public async Task<IActionResult> UpdateState([FromBody] StateAddDto model)
        {
            Debug.WriteLine("Inside Add State");
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("Inside Model State");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            Debug.WriteLine("Outside IF Condition");
            //var isStateExist = _bankInsuranceDbContext.States.ToList().Find(x=>x.StateName==model.StateName);
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
            return this._stateManager.GetAll();
        }
    }
}
