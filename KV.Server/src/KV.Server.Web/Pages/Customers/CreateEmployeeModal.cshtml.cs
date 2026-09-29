namespace KV.Server.Web.Pages.Customers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateEmployeeModalModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public CreateEmployeeModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    [BindProperty] public CreateEmployeeViewModel CreateEmployee { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public void OnGet(Guid tenantId)
    {
        CreateEmployee = new CreateEmployeeViewModel
        {
            TenantId = tenantId
        };

    }

    public async Task<IActionResult> OnPostAsync()
    {
        var createDto = this.ObjectMapper.Map<CreateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>(this.CreateEmployee);
        if (!string.IsNullOrEmpty(CreateEmployee.FIO))
        {
            var splitFio = CreateEmployee.FIO.Split(" ");
            if (splitFio.Length == 2)
            {
                createDto.LastName = splitFio[0];
                createDto.FirstName = splitFio[1];
            }
            else if (splitFio.Length == 1)
            {
                createDto.LastName = splitFio[0];
            }
            else
            {
                createDto.LastName = splitFio[0];
                createDto.FirstName = splitFio[1];
                createDto.MiddleName = string.Join(" ", splitFio.Skip(2));
            }
        }

        await this._customerUserProfilesAppService.CreateWithIdentityUserAsync(createDto);
        return this.NoContent();
    }
}

public class CreateEmployeeViewModel
{
    [Required] public string FIO { get; set; }
    [Required] public string LastName { get; set; }

    [Required] public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [DataType(DataType.Date)] public DateTime? DateOfBirthDay { get; set; }

    [Required] public string JobPost { get; set; }

    [DataType(DataType.PhoneNumber)] public string PhoneNumber { get; set; }

    [Required] public string Login { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(10)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [HiddenInput] public Guid TenantId { get; set; }
}
