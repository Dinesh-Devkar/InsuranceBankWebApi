using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceTypeController : ControllerBase
    {
        private readonly IAllRepository<InsuranceType> _insuranceTypeManager;
        public InsuranceTypeController(BankInsuranceDbContext bankInsuranceDb)
        {
            this._insuranceTypeManager = new AllRepository<InsuranceType>(bankInsuranceDb);
        }
        [HttpGet]
        [Route("GetAllInsuranceTypes")]
        public async Task<List<InsuranceTypeGetDto>> GetAllInsuranceTypes()
        {
            List<InsuranceTypeGetDto> insuranceTypeList = new List<InsuranceTypeGetDto>();
            var insuranceTypes = this._insuranceTypeManager.GetAll();
            foreach (var insuranceType in insuranceTypes)
            {
                //using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                //{
                //    stream.Position = 0;
                //    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                //    insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
                //}
                insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = insuranceType.Image, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
            }
            return insuranceTypeList;
        }

        [HttpPost]
        [Route("AddInsuranceType")]
        public async Task<IActionResult> AddInsuranceType([FromBody] InsuranceTypeAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var insuranceType = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceName);
            if (insuranceType != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Already Exists" });
            }
            if (model.Image.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    //await model.Image.CopyTo(stream);
                    await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status,Image=model.Image });
                    // model.Image = stream.ToArray();
                    return this.Ok(new Response { Message = "InsuranceType Added Successfully", Status = "Success" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceType Not Added" });
        }
        [HttpGet]
        [Route("GetSingleInsurance")]
        public async Task<IActionResult> GetSingleInsurance()
        {

            var insuranceTypes = this._insuranceTypeManager.GetAll();
            foreach (var insuranceType in insuranceTypes)
            {
                //using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                //{
                //    stream.Position = 0;
                //    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                //    return File(insuranceType.Image, "image/jpg", "Dinesh.jpg");
                //    //return this.Ok(insuranceType.Image);
                //    //insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
                //}
            }
            return this.Ok("Image Not Found");
        }

        [HttpPut]
        [Route("{insuranceTypeId}/UpdateInsuranceType")]
        public async Task<IActionResult> UpdateInsuranceType([FromForm] InsuranceTypeAddDto model, int insuranceTypeId)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var insuranceType = this._insuranceTypeManager.GetAll().Find(x => x.Id == insuranceTypeId);
            if (insuranceType == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if (model.Status == "Active" || model.Status == "InActive")
            {

                insuranceType.Status = model.Status;
                insuranceType.InsuranceName = model.InsuranceName;
                var newImage = insuranceType.Image;
                if (model.Image.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        //await model.Image.CopyToAsync(stream);
                        //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() });
                        // model.Image = stream.ToArray();
                        //newImage = stream.ToArray();

                    }
                }
                await this._insuranceTypeManager.Update(insuranceType);

                return this.Ok(new Response { Message = "InsuranceType Updated Successfully", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceType Not Update" });

        }
    }
}
