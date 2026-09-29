namespace KV.Server.Web.Pages.Users;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Dtos.Users;
using Microsoft.AspNetCore.Mvc;

public class CreateUserModel : ServerPageModel
{
    private readonly IUsersAppService _usersAppService;
    private readonly ICrudCustomerAppService _customerAppService;
    public CreateUserModel(IUsersAppService usersAppService, ICrudCustomerAppService customerAppService)
    {
        this._usersAppService = usersAppService;
        this._customerAppService = customerAppService;
    }
    [BindProperty]
    public CreateUserVM CreateUserViewModel { get; set; }
    public CustomerDto Client { get; set; }
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    public async Task OnGetAsync(Guid id)
    {
        this.CreateUserViewModel = new CreateUserVM
        {
            TenantId = id
        };
        this.Client = await this._customerAppService.GetAsync(id);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var dto = this.ObjectMapper.Map<CreateUserVM, CreateUpdateUserDto>(this.CreateUserViewModel);

        await this._usersAppService.CreateUserByTenantIdAsync(dto);

        return this.RedirectToPage("/Customers/Details", new { id = this.Id });
    }
}
public class CreateUserVM
{
    /// <summary>
    ///     Имя
    /// </summary>
    [MaxLength(256)]
    public string FirstName { get; set; }

    /// <summary>
    ///     Фамилия
    /// </summary>
    [MaxLength(256)]
    public string LastName { get; set; }

    /// <summary>
    ///     Отчество
    /// </summary>
    [MaxLength(256)]
    public string MiddleName { get; set; }

    public string JobPost { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    [MaxLength(12)]
    public string PhoneNumber { get; set; }

    [HiddenInput]
    public Guid? TenantId { get; set; }
}
