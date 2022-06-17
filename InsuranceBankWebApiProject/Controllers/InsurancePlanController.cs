using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> AddInsurancePlan([FromBody] InsurancePlanAddDto model)
        {
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
        [HttpPut]
        [Route("{insurancePlanId}/UpdateInsurancePlan")]
        public async Task<IActionResult> UpdateInsurancePlan([FromForm] InsurancePlanAddDto model, int insurancePlanId)
        {
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
            insurancePlan.MinimumYears = model.MinimumYears;
            insurancePlan.MaximumAge = model.MaximumAge;
            insurancePlan.MinimumAge = model.MinimumAge;
            insurancePlan.MinimumInvestAmt = model.MinimumInvestAmt;
            insurancePlan.MaximumInvestAmt = model.MaximumInvestAmt;
            await this._insurancePlanManager.Update(insurancePlan);
            return this.Ok(new Response() { Status = "Success", Message = "InsurancePlan Updated Successfully" });
        }

    }
}
