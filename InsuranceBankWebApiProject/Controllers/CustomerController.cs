using EnsuranceProjectEntityLib.Model.CustomerModel;
using EnsuranceProjectLib.Repository.AdminRepo;
using InsuranceBankWebApiProject.DtoClasses.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceBankWebApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        //private readonly AllRepository<Customer> _customerRepo;
        //public CustomerController()
        //{
        //    this._customerRepo = new AllRepository<Customer>();
        //}

        //[HttpPost]
        //[Route("AddCustomer")]
        //public async Task<IActionResult> AddCustomer(CustomerAddDto customerDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Pleas Fill Proper Data");
        //    }
        //    var customer = new Customer()
        //    {
        //        Name = customerDto.Name,
        //        Address = customerDto.Address,
        //        City = customerDto.City,
        //        DateOfBirth = customerDto.DateOfBirth,
        //        Email = customerDto.Email,
        //        LoginId = customerDto.LoginId,
        //        MobileNumber = customerDto.MobileNumber,
        //        NomineeName = customerDto.NomineeName,
        //        NomineeRelation = customerDto.NomineeRelation,
        //        PinCode = customerDto.PinCode,
        //        State = customerDto.State,
        //        Password = customerDto.Password
        //    };
        //    _customerRepo.Add(customer);
        //    return this.Ok("Customer Added Successfully");
        //}
        //[HttpGet]
        //[Route("GetAllCustomer")]
        //public async Task<List<CustomerGetDto>> GetAllCustomers()
        //{
        //    var customers = await _customerRepo.GetAll();
        //    List<CustomerGetDto> customersList = new List<CustomerGetDto>();
        //    foreach (var customer in customers)
        //    {
        //        customersList.Add(new CustomerGetDto()
        //        {
        //            Name = customer.Name,
        //            Address = customer.Address,
        //            City = customer.City,
        //            DateOfBirth = customer.DateOfBirth,
        //            Email = customer.Email,
        //            LoginId = customer.LoginId,
        //            MobileNumber = customer.MobileNumber,
        //            NomineeName = customer.NomineeName,
        //            NomineeRelation = customer.NomineeRelation,
        //            PinCode = customer.PinCode,
        //            State = customer.State,
        //        });
        //    }
        //    return customersList;
        //}
    }
}
