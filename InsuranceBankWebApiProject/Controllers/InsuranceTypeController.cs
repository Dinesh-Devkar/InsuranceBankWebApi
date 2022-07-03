using EnsuranceProjectEntityLib.Model.Common;
using EnsuranceProjectEntityLib.Model.Insurance;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Common;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceTypeController : ControllerBase
    {
        private readonly IAllRepository<InsuranceType> _insuranceTypeManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public InsuranceTypeController(BankInsuranceDbContext bankInsuranceDb, IConfiguration configuration,IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this._insuranceTypeManager = new AllRepository<InsuranceType>(bankInsuranceDb);
            this._webHostEnvironment = webHostEnvironment;
            this._userManager = userManager;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
        }
        [HttpGet]
        [Route("GetAllInsuranceTypes")]
        public async Task<List<InsuranceTypeGetDto>> GetAllInsuranceTypes()
        {
            //Return a list of all insurance types which are active
            List<InsuranceTypeGetDto> insuranceTypeList = new List<InsuranceTypeGetDto>();
            var insuranceTypes = this._insuranceTypeManager.GetAll().Where(x=>x.Status=="Active");
            
            foreach (var insuranceType in insuranceTypes)
            {
                //using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                //{
                //    stream.Position = 0;
                //    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                //    insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
                //}
                insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = insuranceType.Image, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status,Id=insuranceType.Id });
            }
            return insuranceTypeList;
        }
        [HttpGet]
        [Route("{userId}/GetAllInsuranceTypesForAdmin")]
        [Authorize(Roles =UserRoles.Admin+","+UserRoles.Employee)]
        public async Task<IActionResult> GetAllInsuranceTypesForAdmin(string userId)
        {
            //return a list of all insurance type including active and inactive
            List<InsuranceTypeGetDto> insuranceTypeList = new List<InsuranceTypeGetDto>();
            dynamic insuranceTypes = null;
            var user = await this._userManager.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
            }
            if (user.UserRoll == UserRoles.Admin || user.UserRoll == UserRoles.Employee)
            {
                insuranceTypes = this._insuranceTypeManager.GetAll();
            }
            else
            {
                insuranceTypes = (List<InsuranceTypeGetDto>?)this._insuranceTypeManager.GetAll().Where(x => x.Status == "Active");
            }
            foreach (var insuranceType in insuranceTypes)
            {
                //using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                //{
                //    stream.Position = 0;
                //    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                //    insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
                //}
                insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = insuranceType.Image, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status, Id = insuranceType.Id });
            }
            return this.Ok(insuranceTypeList);
        }
        [HttpPost]
        [Route("AddInsuranceType")]
        //[Consumes("multipart/form-data")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddInsuranceType(InsuranceTypeAddDto model)
        {
            //To add new insurance type
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var insuranceType = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceName);
            if (insuranceType != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Already Exists" });
            }
            var splited = model.Image.Split(new char[] { ',' }, StringSplitOptions.None);
            byte[] bytes;
            if (splited.Length > 0)
            {
                bytes = Convert.FromBase64String(splited[1]);
                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }
                string folder = "images/";
                folder += Guid.NewGuid().ToString() + "_" + model.InsuranceName;
                folder += ".png";
                string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                Debug.WriteLine(folder);
                Debug.WriteLine(serverFolder);
                Debug.WriteLine(image);
                var userImagesPath = Path.Combine(this._webHostEnvironment.WebRootPath, "images");
                //DirectoryInfo dir = new DirectoryInfo(userImagesPath);
                //FileInfo[] files = dir.GetFiles();
                //Debug.WriteLine(files.Length);
                //foreach (var f in files)
                //{
                //    Debug.WriteLine(f.FullName);
                //    Debug.WriteLine(f.Directory);
                //    Debug.WriteLine(f.Name);
                //}
                image.Save(serverFolder, System.Drawing.Imaging.ImageFormat.Jpeg);
                await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = folder });
                //// model.Image = stream.ToArray();
                return this.Ok(new Response { Message = "InsuranceType Added Successfully", Status = "Success" });
            }
            //if (model.Image.Length > 0)
            //{
            //    using (var stream = new MemoryStream())
            //    {
            //        //await model.Image.CopyTo(stream);
            //        //string folder = "images/";
            //        //folder += Guid.NewGuid().ToString() + "_" + model.Image.FileName;
            //        //string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            //        //await model.Image.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            //        //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status,Image=folder });
            //        ////// model.Image = stream.ToArray();
            //        //return this.Ok(new Response { Message = "InsuranceType Added Successfully", Status = "Success" });
            //    }
            //}
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceType Not Added" });
        }

        [HttpPut]
        [Route("{insuranceTypeId}/UpdateInsuranceType")]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Employee)]
        public async Task<IActionResult> UpdateInsuranceType(int insuranceTypeId,InsuranceTypeAddDto model)
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

                //insuranceType.Status = model.Status;
                //insuranceType.InsuranceName = model.InsuranceName;
                //var newImage = insuranceType.Image;
                //if (model.Image.Length > 0)
                //{
                //    using (var stream = new MemoryStream())
                //    {
                //        //await model.Image.CopyToAsync(stream);
                //        //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() });
                //        // model.Image = stream.ToArray();
                //        //newImage = stream.ToArray();

                //    }
                //}
                var splited = model.Image.Split(new char[] { ',' }, StringSplitOptions.None);
                byte[] bytes;
                if (splited.Length > 0)
                {
                    bytes = Convert.FromBase64String(splited[1]);
                    Image image;
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        image = Image.FromStream(ms);
                    }
                    string folder = "images/";
                    folder += Guid.NewGuid().ToString() + "_" + model.InsuranceName;
                    folder += ".png";
                    string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    Debug.WriteLine(folder);
                    Debug.WriteLine(serverFolder);
                    Debug.WriteLine(image);
                    var userImagesPath = Path.Combine(this._webHostEnvironment.WebRootPath, "images");
                    //DirectoryInfo dir = new DirectoryInfo(userImagesPath);
                    //FileInfo[] files = dir.GetFiles();
                    //Debug.WriteLine(files.Length);
                    //foreach (var f in files)
                    //{
                    //    Debug.WriteLine(f.FullName);
                    //    Debug.WriteLine(f.Directory);
                    //    Debug.WriteLine(f.Name);
                    //}
                    image.Save(serverFolder, System.Drawing.Imaging.ImageFormat.Jpeg);
                    insuranceType.Status = model.Status;
                    insuranceType.InsuranceName = model.InsuranceName;
                    insuranceType.Image = folder;
                }
                await this._insuranceTypeManager.Update(insuranceType);

                return this.Ok(new Response { Message = "InsuranceType Updated Successfully", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceType Not Update" });

        }

        [HttpGet]
        [Route("{insuranceTypeId}/GetInsuranceType")]
       
        public async Task<IActionResult> GetInsuranceType(string insuranceTypeId)
        {
            //Get insurance type details by insuranceTypeId
            var insuranceType=this._insuranceTypeManager.GetAll().Where(x=>x.Id== int.Parse(insuranceTypeId)).FirstOrDefault();
            if (insuranceType == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }

            return this.Ok(new InsuranceTypeGetDto { Id=insuranceType.Id,Image=insuranceType.Image,InsuranceName=insuranceType.InsuranceName,Status=insuranceType.Status });
        }
       
        [HttpPost]
        [Route("UploadImage")]
        public async Task<IActionResult> UploadImage(SampleDto model)
        {

            var insuranceName=model.InsuranceName;
            Debug.WriteLine(insuranceName);
            Debug.WriteLine(model.Image);
            var splited = model.Image.Split(new char[] { ',' }, StringSplitOptions.None);
            byte[] bytes;
            if (splited.Length > 0)
            {
                bytes = Convert.FromBase64String(splited[1]);
                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }
                string folder = "images/";
                folder += Guid.NewGuid().ToString() + "_" + insuranceName;
                folder += ".png";
                string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                var baseUrl = Request.GetTypedHeaders().Referer.ToString();
                var appUrl = _configuration["profiles:applicationUrl"];
                Debug.WriteLine("http://localhost:5137"+folder);
                Debug.WriteLine(appUrl);
                Debug.WriteLine(baseUrl);
                Debug.WriteLine(folder);
                Debug.WriteLine(serverFolder);
                Debug.WriteLine(image);
                var userImagesPath = Path.Combine(this._webHostEnvironment.WebRootPath, "images");
                DirectoryInfo dir = new DirectoryInfo(userImagesPath);
                FileInfo[] files = dir.GetFiles();
                Debug.WriteLine(files.Length);
                foreach(var f in files)
                {
                    Debug.WriteLine(f.FullName);
                    Debug.WriteLine(f.Directory);
                    Debug.WriteLine(f.Name);
                }
                image.Save(serverFolder,System.Drawing.Imaging.ImageFormat.Jpeg);
            }


            //await model.CopyToAsync(new FileStream(serverFolder, FileMode.Create));



            //string imageName = null;
            //var httpRequest = this._httpContextAccessor.HttpContext.Request;
            ////IFormFile file = Request.Form.Files.FirstOrDefault();
            //Debug.WriteLine("The Count IS : " + httpRequest);
            ////Debug.WriteLine(model.FileName);
            //Debug.WriteLine("The Caption Is : " + Request.Form.Files.GetFile("Image"));
            //var a = Request.Form.Files.Count;
            //Debug.WriteLine(HttpContext.Request.Form["Image"]);
            //Debug.WriteLine("The Count IS : " + a);
            ////Debug.WriteLine(file);
            ////Upload Image
            //var t=Request.Form.Keys.FirstOrDefault();
            //Debug.WriteLine(t.GetType());
            //var l = HttpContext.Request.Form.Files.GetFile("Image");
            //Debug.WriteLine(l);
            //try

            //{
            //    var postedFile = httpRequest.Form.Files.Count; // .Files["Image"].FileName;
            //    Debug.WriteLine(postedFile);
            //    Debug.WriteLine(postedFile);
            //    //Create custom filename
            //    //imageName = new String(Path.GetFileName(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            //    //imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            //    //var filePath = HttpContext.Server.MapPath("~/Image/" + imageName);
            //    //postedFile.SaveAs(filePath);


            //    //string folder = "images/";
            //    //folder += Guid.NewGuid().ToString() + "_" + postedFile.FileName;
            //    //string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            //    //await model.Image.CopyToAsync(new FileStream(serverFolder, FileMode.Create));

            //    foreach (var formFile in  HttpContext.Request.Form.Files.ToList())
            //    {
            //        if (formFile.Length > 0)
            //        {
            //            Debug.WriteLine(formFile);
            //        }
            //    }

            //    return this.Ok("Image Added Successfully");

            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
            ////var postedFile = httpRequest.Form.Files["Image"];
            ////var postedFile = httpRequest.Form.Files[0];
            ////Create custom filename
            ////imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            ////imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            ////var filePath = HttpContext.Server.MapPath("~/Image/" + imageName);
            ////postedFile.SaveAs(filePath);

            //////Save to DB
            ////using (DBModel db = new DBModel())
            ////{
            ////    Image image = new Image()
            ////    {
            ////        ImageCaption = httpRequest["ImageCaption"],
            ////        ImageName = imageName
            ////    };
            ////    db.Images.Add(image);
            ////    db.SaveChanges();
            ////}
            ////return Request.CreateResponse(HttpStatusCode.Created);

            //FileManagerModel model = new FileManagerModel();
            
            return this.Ok("Image Uploaded");
        }
        [HttpGet]
        [Route("GetAllImages")]
        public async Task<IActionResult> GetImages()
        {
            var userImagesPath = Path.Combine(this._webHostEnvironment.WebRootPath, "images");
            DirectoryInfo dir = new DirectoryInfo(userImagesPath);
            FileInfo[] files = dir.GetFiles();
            Debug.WriteLine(files.Length);
            return this.Ok("Hello");
        }
    }
}
