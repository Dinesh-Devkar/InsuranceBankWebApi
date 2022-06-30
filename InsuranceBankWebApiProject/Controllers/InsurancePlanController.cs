using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsurancePlanController : ControllerBase
    {
        private readonly IAllRepository<InsuranceScheme> _insuranceSchemeManager;
        private readonly IAllRepository<InsuranceType> _insuranceTypeManager;
        private readonly IAllRepository<InsurancePlan> _insurancePlanManager;
        public InsurancePlanController(BankInsuranceDbContext bankInsuranceDb)
        {
            this._insuranceSchemeManager = new AllRepository<InsuranceScheme>(bankInsuranceDb);
            this._insuranceTypeManager = new AllRepository<InsuranceType>(bankInsuranceDb);
            this._insurancePlanManager = new AllRepository<InsurancePlan>(bankInsuranceDb);
        }
        [HttpGet]
        [Route("GetAllInsurancePlans")]
        public async Task<List<InsurancePlanGetDto>> GetAllInsurancePlans()
        {
            //return a list of all insurance plans

            List<InsurancePlanGetDto> insurancePlansList = new List<InsurancePlanGetDto>();
            var insurancePlans = this._insurancePlanManager.GetAll();
            foreach (var insurancePlan in insurancePlans)
            {
                insurancePlansList.Add(new InsurancePlanGetDto()
                {
                    InsuranceType = insurancePlan.InsuranceType,
                    InsuranceScheme = insurancePlan.InsuranceScheme,
                    InsurancePlanName = insurancePlan.InsurancePlanName,
                    MinimumYears = insurancePlan.MinimumYears,
                    MaximumYears = insurancePlan.MaximumYears,
                    MinimumAge = insurancePlan.MinimumAge,
                    MaximumAge = insurancePlan.MaximumAge,
                    MinimumInvestAmt = insurancePlan.MinimumInvestAmt,
                    MaximumInvestAmt = insurancePlan.MaximumInvestAmt,
                    ProfitRatio = insurancePlan.ProfitRatio,
                    Status = insurancePlan.Status

                });
            }
            return insurancePlansList;
        }
        [HttpPost]
        [Route("AddInsurancePlan")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddInsurancePlan([FromBody] InsurancePlanAddDto model)
        {
            //To add a new insurance plan
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var isInsuranceTypeExist = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceType);
            var isInsuranceSchemeExit = this._insuranceSchemeManager.GetAll().Find(x => x.InsuranceSchemeName == model.InsuranceScheme);
            if (isInsuranceTypeExist == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if (isInsuranceSchemeExit == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceScheme Not Found" });
            }
            var isInsurancePlanExit = this._insurancePlanManager.GetAll().Find(x => x.InsurancePlanName == model.InsurancePlanName);
            if (isInsurancePlanExit != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsurancePlan Already Exists" });
            }
            await this._insurancePlanManager.Add(new InsurancePlan()
            {
                InsurancePlanName = model.InsurancePlanName,
                InsuranceScheme = model.InsuranceScheme,
                InsuranceType = model.InsuranceType,
                MaximumAge = model.MaximumAge,
                MinimumAge = model.MinimumAge,
                MaximumInvestAmt = model.MaximumInvestAmt,
                MinimumInvestAmt = model.MinimumInvestAmt,
                MaximumYears = model.MaximumYears,
                MinimumYears = model.MinimumYears,
                ProfitRatio = model.ProfitRatio,
                Status = model.Status
            });
            return this.Ok(new Response() { Status = "Success", Message = "InsurancePlan Added Successfully" });
        }
        [HttpGet]
        [Route("{insurancePlanName}/GetInsurancePlan")]
        public async Task<IActionResult> GetInsurancePlan(string insurancePlanName)
        {
            //get particular insurance plan details by insurance name
            var insurancePlan=this._insurancePlanManager.GetAll().Find(x=>x.InsurancePlanName == insurancePlanName);
            if (insurancePlan == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Insurance Plan Not Found", Status = "Error" });
            }
            return this.Ok(new InsurancePlanGetDto
            {
                Status = insurancePlan.Status,
                InsurancePlanName = insurancePlan.InsurancePlanName,
                InsuranceScheme = insurancePlan.InsuranceScheme,
                InsuranceType = insurancePlan.InsuranceType,
                MaximumAge = insurancePlan.MaximumAge,
                MaximumInvestAmt = insurancePlan.MaximumInvestAmt,
                MaximumYears = insurancePlan.MaximumYears,
                MinimumAge = insurancePlan.MinimumAge,
                MinimumInvestAmt = insurancePlan.MinimumInvestAmt,
                MinimumYears = insurancePlan.MinimumYears,
                ProfitRatio = insurancePlan.ProfitRatio,
                Id=insurancePlan.Id
            });
        }
        [HttpPut]
        [Route("{insurancePlanId}/UpdateInsurancePlan")]
        [Authorize(Roles = UserRoles.Employee + "," + UserRoles.Admin)]
        public async Task<IActionResult> UpdateInsurancePlan(InsurancePlanAddDto model, int insurancePlanId)
        {
            //To Update Insurance Plan
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var isInsuranceTypeExist = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceType);
            var isInsuranceSchemeExit = this._insuranceSchemeManager.GetAll().Find(x => x.InsuranceSchemeName == model.InsuranceScheme);
            if (isInsuranceTypeExist == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if (isInsuranceSchemeExit == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceScheme Not Found" });
            }
            var insurancePlan = this._insurancePlanManager.GetAll().Find(x => x.Id == insurancePlanId);
            if (insurancePlan == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsurancePlan Not Found" });
            }
            insurancePlan.Status = model.Status;
            insurancePlan.MinimumYears = model.MinimumYears;
            insurancePlan.InsurancePlanName = model.InsurancePlanName;
            insurancePlan.InsuranceScheme = model.InsuranceScheme;
            insurancePlan.ProfitRatio = model.ProfitRatio;
            insurancePlan.InsuranceType = model.InsuranceType;
            insurancePlan.MaximumYears = model.MaximumYears;
            insurancePlan.MaximumAge = model.MaximumAge;
            insurancePlan.MinimumAge = model.MinimumAge;
            insurancePlan.MinimumInvestAmt = model.MinimumInvestAmt;
            insurancePlan.MaximumInvestAmt = model.MaximumInvestAmt;
            await this._insurancePlanManager.Update(insurancePlan);
            return this.Ok(new Response() { Status = "Success", Message = "InsurancePlan Updated Successfully" });
        }

        [HttpGet]
        [Route("{insuranceType}/GetInsurancePlansByInsuranceType")]   
        public async Task<IActionResult> GetInsurancePlansByInsuranceType(string insuranceType)
        {
            var insType=this._insuranceTypeManager.GetAll().Find(x=>x.InsuranceName==insuranceType);
            if (insType == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            List<InsurancePlansPurchaseDto> insurancePlansList = new List<InsurancePlansPurchaseDto>();
            var insurancePlans = this._insurancePlanManager.GetAll().Where(x=>x.InsuranceType==insType.InsuranceName).ToList();
            foreach (var insurancePlan in insurancePlans)
            {               
                insurancePlansList.Add(new InsurancePlansPurchaseDto()
                {                 
                    InsuranceType = insurancePlan.InsuranceType,
                    InsuranceScheme = insurancePlan.InsuranceScheme,
                    InsurancePlanName = insurancePlan.InsurancePlanName,
                    MinimumYears = insurancePlan.MinimumYears,
                    MaximumYears = insurancePlan.MaximumYears,
                    MinimumAge = insurancePlan.MinimumAge,
                    MaximumAge = insurancePlan.MaximumAge,
                    MinimumInvestAmt = insurancePlan.MinimumInvestAmt,
                    MaximumInvestAmt = insurancePlan.MaximumInvestAmt,
                    ProfitRatio = insurancePlan.ProfitRatio,
                    Note=this._insuranceSchemeManager.GetAll().Where(x=>x.InsuranceSchemeName==insurancePlan.InsuranceScheme).Select(x=>x.Note).FirstOrDefault()
                });
            }
            return this.Ok(insurancePlansList);
        }
    }
}
