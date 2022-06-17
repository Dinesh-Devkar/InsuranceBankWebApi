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
    public class CityController : ControllerBase
    {
        private readonly IAllRepository<City> _cityManager;
        public CityController(BankInsuranceDbContext dbContext)
        {
            this._cityManager=new AllRepository<City>(dbContext);
        }
        [HttpPost]
        [Route("AddCity")]
        public async Task<IActionResult> AddCity([FromBody] CityAddDto model)
        {
            Debug.WriteLine("Inside Add City");
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("Inside Model State");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            Debug.WriteLine("Outside IF Condition");
            //var isStateExist = _bankInsuranceDbContext.States.ToList().Find(x=>x.StateName==model.StateName);
            var city = this._cityManager.GetAll().ToList().Find(x => x.CityName == model.CityName);
            if (city != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "City Already Exists" });
            }
            //_bankInsuranceDbContext.States.Add(new State() { StateName = model.StateName,Status=model.Status});
            //_bankInsuranceDbContext.SaveChanges();
            await this._cityManager.Add(new City() { CityName = model.CityName, Status = model.Status, State = model.State });
            return this.Ok(new Response { Message = "City Added Successfully", Status = "Success" });
        }


        [HttpPut]
        [Route("UpdateCity")]
        public async Task<IActionResult> UpdateCity([FromBody] CityAddDto model)
        {
            Debug.WriteLine("Inside Add State");
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("Inside Model State");
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }
            Debug.WriteLine("Outside IF Condition");
            //var isStateExist = _bankInsuranceDbContext.States.ToList().Find(x=>x.StateName==model.StateName);
            var city = this._cityManager.GetAll().ToList().Find(x => x.CityName == model.CityName);
            if (city != null)
            {
                if (model.Status == "Active" || model.Status == "InActive")
                {
                    city.Status = model.Status;
                    city.State = model.State;
                    await this._cityManager.Update(city);
                    return this.Ok(new Response { Message = "City Updated Successfully", Status = "Success" });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid City Status" });

            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "City Not Found" });


        }

        [HttpGet]
        [Route("GetAllCities")]
        public async Task<List<City>> GetAllCities()
        {
            return this._cityManager.GetAll();
        }
    }
}
