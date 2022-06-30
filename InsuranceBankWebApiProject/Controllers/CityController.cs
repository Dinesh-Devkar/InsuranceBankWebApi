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
    public class CityController : ControllerBase
    {
        private readonly IAllRepository<City> _cityManager;
        private readonly IAllRepository<State> _stateManager;
        public CityController(BankInsuranceDbContext dbContext)
        {
            this._cityManager=new AllRepository<City>(dbContext);
            this._stateManager = new AllRepository<State>(dbContext);
        }
        [HttpPost]
        [Route("AddCity")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddCity([FromBody] CityAddDto model)
        {
             //To Add New City In Database
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }

            var city = this._cityManager.GetAll().ToList().Find(x => x.CityName == model.CityName);
            if (city != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "City Already Exists" });
            }
            await this._cityManager.Add(new City() { CityName = model.CityName, Status = model.Status, State = model.State });
            return this.Ok(new Response { Message = "City Added Successfully", Status = "Success" });
        }


        [HttpPut]
        [Route("UpdateCity")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> UpdateCity([FromBody] CityAddDto model)
        {
            //To Update City
            if (!ModelState.IsValid)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });
            }

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
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<List<City>> GetAllCities()
        {
            //this method will return all the cities including InActive cities it will used by Admin and Agent for update city
            return this._cityManager.GetAll();
        }

        [HttpGet]
        [Route("GetAllActiveCities")]
        public async Task<List<City>> GetAllActiveCities()
        {
            //this method will return all the active cities it will use by customer and agent
            return this._cityManager.GetAll().Where(x=>x.Status=="Active").ToList();
        }
        [HttpGet]
        [Route("{stateName}/GetCitiesByState")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee + "," + UserRoles.Agent + "," + UserRoles.Customer)]
        public async Task<IActionResult> GetCitiesByState(string stateName)
        {
            //return a list of cities by state name   
            var state=this._stateManager.GetAll().Find(x=>x.StateName == stateName);
            if (state == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "State Not Found" });
            }
            List<string> cityList = new List<string>();
            var cities=this._cityManager.GetAll().Where(x=>x.State==state.StateName && x.Status=="Active").ToList();
            foreach (var city in cities)
            {
                cityList.Add(city.CityName);
               
            }
            return this.Ok(cityList);

        }
    }
}
