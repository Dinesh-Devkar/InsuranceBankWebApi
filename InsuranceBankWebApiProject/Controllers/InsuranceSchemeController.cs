using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceSchemeController : ControllerBase
    {
        private readonly IAllRepository<InsuranceScheme> _insuranceSchemeManager;
        private readonly IAllRepository<InsuranceType> _insuranceTypeManager;
        public InsuranceSchemeController(BankInsuranceDbContext bankInsuranceDb)
        {
            this._insuranceSchemeManager=new AllRepository<InsuranceScheme>(bankInsuranceDb);
            this._insuranceTypeManager = new AllRepository<InsuranceType>(bankInsuranceDb);
        }
        [HttpGet]
        [Route("GetAllInsuranceSchemes")]
        public async Task<List<InsuranceSchemeGetDto>> GetAllInsuranceSchemes()
        {
            //Return a list of all insurance schemes
            List<InsuranceSchemeGetDto> insuranceSchemeList = new List<InsuranceSchemeGetDto>();
            var insuranceSchemes = this._insuranceSchemeManager.GetAll();
            foreach (var insuranceScheme in insuranceSchemes)
            {
                insuranceSchemeList.Add(new InsuranceSchemeGetDto()
                {
                    Image = insuranceScheme.Image,
                    InsuranceSchemeName = insuranceScheme.InsuranceSchemeName,
                    Status = insuranceScheme.Status,
                    InstallmentComission = insuranceScheme.InstallmentComission,
                    InsuranceTypeName = insuranceScheme.InsuranceTypeName,
                    NewRegComission = insuranceScheme.NewRegComission,
                    Note=insuranceScheme.Note
                });
            }
        
            return insuranceSchemeList;
        }
        [HttpGet]
        [Route("{insuranceType}/GetInsuranceSchemesByInsuranceType")]
        public async Task<List<InsuranceSchemeGetDto>> GetInsuranceSchemesByInsuranceType(string insuranceType)
        {
            //Return a list of insurance schemes by insurance type
            List<InsuranceSchemeGetDto> insuranceSchemeList = new List<InsuranceSchemeGetDto>();
            var insuranceSchemes = this._insuranceSchemeManager.GetAll().Where(x => x.InsuranceTypeName == insuranceType).ToList();
            foreach (var insuranceScheme in insuranceSchemes)
            {
                insuranceSchemeList.Add(new InsuranceSchemeGetDto()
                {
                    Image = insuranceScheme.Image,
                    InsuranceSchemeName = insuranceScheme.InsuranceSchemeName,
                    Status = insuranceScheme.Status,
                    InstallmentComission = insuranceScheme.InstallmentComission,
                    InsuranceTypeName = insuranceScheme.InsuranceTypeName,
                    NewRegComission = insuranceScheme.NewRegComission,
                    Note = insuranceScheme.Note
                });
                
            }
            return insuranceSchemeList;
        }
        [HttpPost]
        [Route("AddInsuranceScheme")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddInsuranceScheme([FromBody] InsuranceSchemeAddDto model)
        {
            //To add a new insurance scheme
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var isInsuranceTypeExist = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceTypeName);
            var isInsuranceSchemeExit = this._insuranceSchemeManager.GetAll().Find(x => x.InsuranceSchemeName == model.InsuranceSchemeName);
            if (isInsuranceTypeExist == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if (isInsuranceSchemeExit != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceScheme Already Exist" });
            }
            if (model.Image.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await this._insuranceSchemeManager.Add(new InsuranceScheme()
                    {
                        InsuranceTypeName = model.InsuranceTypeName,
                        InsuranceSchemeName = model.InsuranceSchemeName,
                        Image = model.Image,
                        InstallmentComission = model.InstallmentComission,
                        NewRegComission = model.NewRegComission,
                        Status = model.Status,
                        Note = model.Note
                    });
                    return this.Ok(new Response { Message = "InsuranceScheme Added Successfully", Status = "Success" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceScheme Not Added" });
        }
        [HttpPut]
        [Route("{insuranceSchemeId}/UpdateInsuranceScheme")]
        [Authorize(Roles = UserRoles.Employee + "," + UserRoles.Admin)]
        public async Task<IActionResult> UpdateInsuranceScheme(int insuranceSchemeId,InsuranceSchemeAddDto model)
        {
            //To update a insurance scheme
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var insuranceScheme = this._insuranceSchemeManager.GetAll().Find(x => x.Id == insuranceSchemeId);
            if (insuranceScheme == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceScheme Not Found" });
            }
            var insuranceType = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceTypeName);
            if (insuranceType == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if (model.Status == "Active" || model.Status == "InActive")
            {

                insuranceScheme.Status = model.Status;
                insuranceScheme.InsuranceTypeName = model.InsuranceTypeName;
                insuranceScheme.InsuranceSchemeName = model.InsuranceSchemeName;
                insuranceScheme.InstallmentComission = model.InstallmentComission;
                insuranceScheme.NewRegComission = model.NewRegComission;
                insuranceScheme.Image=model.Image;
                insuranceScheme.Note=model.Note;
                await this._insuranceTypeManager.Update(insuranceType);

                return this.Ok(new Response { Message = "InsuranceScheme Updated Successfully", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceScheme Not Update" });

        }
        [HttpGet]
        [Route("{insuranceSchemeName}/GetInsuranceSchemeDetails")]
        public async Task<IActionResult> GetInsuranceSchemeDetails(string insuranceSchemeName)
        {
            //Return a insurance scheme details by insurance scheme name
            var insuranceScheme=this._insuranceSchemeManager.GetAll().Find(x=>x.InsuranceSchemeName==insuranceSchemeName);
            if (insuranceScheme == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Insurance Scheme Not Found" });
            }
            return this.Ok(new InsuranceSchemeGetDto
            {
                Note = insuranceScheme.Note,
                Image = insuranceScheme.Image,
                InsuranceSchemeName = insuranceSchemeName,
                InstallmentComission = insuranceScheme.InstallmentComission,
                InsuranceTypeName = insuranceScheme.InsuranceTypeName,
                NewRegComission = insuranceScheme.NewRegComission,
                Status = insuranceScheme.Status,
                Id=insuranceScheme.Id
            });
        }

    }
}
