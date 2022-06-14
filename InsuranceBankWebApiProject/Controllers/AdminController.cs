using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EnsuranceProjectLib.Infrastructure;
using EnsuranceProjectLib.Repository.AdminRepo;
using EnsuranceProjectEntityLib.Model.AdminModel;
using Microsoft.AspNetCore.Identity;
using InsuranceBankWebApiProject.DtoClasses.Admin;
using InsuranceBankWebApiProject.DtoClasses.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data.Entity;
using Microsoft.AspNetCore.Authorization;
using EnsuranceProjectEntityLib.Model.Common;
using System.Diagnostics;
using EnsuranceProjectEntityLib.Model.Insurance;
using InsuranceBankWebApiProject.DtoClasses.Insurance;
using System.Drawing;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAllRepository<City> _cityManager;
        private readonly IAllRepository<InsurancePlan> _insurancePlanManager;
        private readonly IAllRepository<InsuranceScheme> _insuranceSchemeManager;
        private readonly IAllRepository<State> _stateManager;
        private readonly IAllRepository<InsuranceType> _insuranceTypeManager;
        private readonly BankInsuranceDbContext _bankInsuranceDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        
        

        public AdminController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager, IConfiguration configuration, BankInsuranceDbContext bankInsuranceDb) 
        {
            
            this._stateManager = new AllRepository<State>(bankInsuranceDb);
            this._cityManager = new AllRepository<City>(bankInsuranceDb);
            this._insuranceTypeManager= new AllRepository<InsuranceType>(bankInsuranceDb);
            this._insuranceSchemeManager= new AllRepository<InsuranceScheme>(bankInsuranceDb);
            this._insurancePlanManager= new AllRepository<InsurancePlan>(bankInsuranceDb);
            this._bankInsuranceDbContext = bankInsuranceDb;
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]AdminAddDto model)
        {
            var userExist = await this._userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Already Exists" });
            }
            //var loginIdExists=await 
            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                LoginId=model.LoginId,
                UserStatus="Active"
            };
            var result = await this._userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Created" });
            }
            if(! await this._roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await this._roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }
            if (await this._roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await this._userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return this.Ok(new Response { Message = "User Created Successfully", Status = "Success" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Debug.WriteLine("The Email is : "+model.Email);
            var user = await this._userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
            }
            if(user!=null && await this._userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles=await this._userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var token = new JwtSecurityToken(
                    issuer:_configuration["Jwt:ValidIssuer"],
                    audience:_configuration["Jwt:ValidAudience"],
                    claims:authClaims,    //null original value
                    expires: DateTime.Now.AddMinutes(120),

             //notBefore:
             signingCredentials: new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256));

                return this.Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expire = token.ValidTo,
                    UserName = user.UserName,
                    userId=user.Id

                });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Invalid Password" }); ;
            
        }
        [HttpPut]
        [Route("{adminId}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string adminId, ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userExist = await this._userManager.FindByIdAsync(adminId);
                if (userExist == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Not Found" });
                }
                if (model.NewPassword.Equals(model.ConfirmNewPassword))
                {
                    var result = await this._userManager.ChangePasswordAsync(userExist, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.Ok(new Response { Message = "Password Updated Successfully", Status = "Success" });
                    }
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password and Confirm Password does not match" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields are Required" });

        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("GetAllAdmins")]
        
        public async Task<List<ApplicationUser>> GetAllAdmins()
        {
            
            return this._userManager.Users.ToList();
           
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
            if (isStateExist!=null)
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
                if(model.Status=="Active" || model.Status == "InActive")
                {
                    isStateExist.Status=model.Status;
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

        [HttpPost]
        [Route("AddCity")]
        public async Task<IActionResult> AddCity([FromBody] CityAddDto model)
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
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "City Already Exists" });
            }
            //_bankInsuranceDbContext.States.Add(new State() { StateName = model.StateName,Status=model.Status});
            //_bankInsuranceDbContext.SaveChanges();
            await this._cityManager.Add(new City() { CityName = model.CityName, Status = model.Status,State=model.State });
            return this.Ok(new Response { Message = "State Added Successfully", Status = "Success" });
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
                    city.State= model.State;
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
        [HttpGet]
        [Route("GetAllInsuranceTypes")]
        public async Task<List<InsuranceTypeGetDto>> GetAllInsuranceTypes()
        {
            List<InsuranceTypeGetDto> insuranceTypeList=new List<InsuranceTypeGetDto>();
            var insuranceTypes= this._insuranceTypeManager.GetAll();
            foreach(var insuranceType in insuranceTypes)
            {
                using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                {
                    stream.Position=0;
                    Bitmap returnImage = (Bitmap)Image.FromStream(stream,true);
                    insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage,InsuranceName=insuranceType.InsuranceName,Status=insuranceType.Status });
                }
            }
            return insuranceTypeList;
        }

        [HttpPost]
        [Route("AddInsuranceType")]
        public async Task<IActionResult> AddInsuranceType([FromForm] InsuranceTypeAddDto model)
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
                    await model.Image.CopyToAsync(stream);
                    await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() }) ;
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
                using (MemoryStream stream = new MemoryStream(insuranceType.Image))
                {
                    stream.Position = 0;
                    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                    return File(insuranceType.Image, "image/jpg", "Dinesh");
                    //return this.Ok(insuranceType.Image);
                    //insuranceTypeList.Add(new InsuranceTypeGetDto() { Image = returnImage, InsuranceName = insuranceType.InsuranceName, Status = insuranceType.Status });
                }
            }
            return this.Ok("Image Not Found");
        }

        [HttpPut]
        [Route("{insuranceTypeId}/UpdateInsuranceType")]
        public async Task<IActionResult> UpdateInsuranceType([FromForm]InsuranceTypeAddDto model,int insuranceTypeId)
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
               
                insuranceType.Status =model.Status;
                insuranceType.InsuranceName = model.InsuranceName;
                var newImage = insuranceType.Image;
                if (model.Image.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(stream);
                        //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() });
                        // model.Image = stream.ToArray();
                        newImage = stream.ToArray();
                        
                    }
                }
                await this._insuranceTypeManager.Update(insuranceType);

                return this.Ok(new Response { Message = "InsuranceType Updated Successfully", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceType Not Update" });

        }

        [HttpGet]
        [Route("GetAllInsuranceSchemes")]
        public async Task<List<InsuranceSchemeGetDto>> GetAllInsuranceSchemes()
        {
            List<InsuranceSchemeGetDto> insuranceSchemeList = new List<InsuranceSchemeGetDto>();
            var insuranceSchemes = this._insuranceSchemeManager.GetAll();
            foreach (var insuranceScheme in insuranceSchemes)
            {
                using (MemoryStream stream = new MemoryStream(insuranceScheme.Image))
                {
                    stream.Position = 0;
                    Bitmap returnImage = (Bitmap)Image.FromStream(stream, true);
                    insuranceSchemeList.Add(new InsuranceSchemeGetDto() 
                    { 
                        Image = returnImage,
                        InsuranceSchemeName=insuranceScheme.InsuranceSchemeName,
                        Status = insuranceScheme.Status,
                        InstallmentComission=insuranceScheme.InstallmentComission,
                        InsuranceTypeName=insuranceScheme.InsuranceTypeName,
                        NewRegComission=insuranceScheme.NewRegComission });
                }
            }
            return insuranceSchemeList;
        }
        [HttpPost]
        [Route("AddInsuranceScheme")]
        public async Task<IActionResult> AddInsuranceScheme([FromForm] InsuranceSchemeAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "All Fields Are Required" });
            }
            var isInsuranceTypeExist = this._insuranceTypeManager.GetAll().Find(x => x.InsuranceName == model.InsuranceTypeName);
            var isInsuranceSchemeExit= this._insuranceSchemeManager.GetAll().Find(x=>x.InsuranceSchemeName == model.InsuranceSchemeName);
            if(isInsuranceTypeExist == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceType Not Found" });
            }
            if(isInsuranceSchemeExit != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "InsuranceScheme Already Exist" });
            }
            if (model.Image.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await model.Image.CopyToAsync(stream);
                    //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() });
                    // model.Image = stream.ToArray();
                    await this._insuranceSchemeManager.Add(new InsuranceScheme()
                    {
                        InsuranceTypeName = model.InsuranceTypeName,
                        InsuranceSchemeName = model.InsuranceSchemeName,
                        Image = stream.ToArray(),
                        InstallmentComission = model.InstallmentComission,
                        NewRegComission = model.NewRegComission,
                        Status = model.Status,
                    });
                    return this.Ok(new Response { Message = "InsuranceScheme Added Successfully", Status = "Success" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceScheme Not Added" });
        }
        [HttpPut]
        [Route("{insuranceSchemeId}/UpdateInsuranceScheme")]
        public async Task<IActionResult> UpdateInsuranceScheme([FromForm] InsuranceSchemeAddDto model, int insuranceSchemeId)
        {
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
                insuranceScheme.NewRegComission= model.NewRegComission;
                
                var newImage = insuranceScheme.Image;
                if (model.Image.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(stream);
                        //await this._insuranceTypeManager.Add(new InsuranceType() { InsuranceName = model.InsuranceName, Status = model.Status, Image = stream.ToArray() });
                        // model.Image = stream.ToArray();
                        newImage = stream.ToArray();

                    }
                }
                await this._insuranceTypeManager.Update(insuranceType);

                return this.Ok(new Response { Message = "InsuranceScheme Updated Successfully", Status = "Success" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Something Went Wrong InsuranceScheme Not Update" });

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
        public async Task<IActionResult> AddInsurancePlan([FromForm] InsurancePlanAddDto model)
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
                MaximumYears = model.MaximumYear,
                MinimumYears = model.MinimumYears,
                ProfitRatio = model.ProfitRatio,
                Status = model.Status
            });
            return this.Ok(new Response() { Status = "Success", Message = "InsurancePlan Added Successfully" });
        }

    }
}
