namespace KV.Server.Web.Pages.Customers;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Content;

[Authorize]
public class EditEmployeeModalModel : ServerPageModel
{
    private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

    public EditEmployeeModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService)
    {
        this._customerUserProfilesAppService = customerUserProfilesAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateEmployeeViewModel CreateUpdateEmployeeViewModel { get; set; }

    [BindProperty(SupportsGet = true)] 
    public IFormFile ImgFile { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        this.Id = id;
        var employee = await this._customerUserProfilesAppService.GetAsync(id);
        this.CreateUpdateEmployeeViewModel = this.ObjectMapper.Map<CustomerUserProfileDto, CreateUpdateEmployeeViewModel>(employee);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var customerDto = this.ObjectMapper.Map<CreateUpdateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>(this.CreateUpdateEmployeeViewModel);
        customerDto.Id = this.Id;

        if (ImgFile is not null)
        {
            await _customerUserProfilesAppService.UploadAvatarAsync(customerDto.Id, new RemoteStreamContent(ImgFile.OpenReadStream(), ImgFile.FileName, ImgFile.ContentType));
        }

        return this.NoContent();
    }
}
public class CreateUpdateEmployeeViewModel
{
    [Required] public string LastName { get; set; }

    [Required] public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [DataType(DataType.Text)] public DateTime? DateOfBirthDay { get; set; }

    [Required] public string JobPost { get; set; }

    [DataType(DataType.PhoneNumber)] public string PhoneNumber { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public string Note { get; set; }

    public string Email { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    [HiddenInput] public Guid Id { get; set; }

    [HiddenInput] public Guid TenantId { get; set; }

    [HiddenInput] public Guid IdentityUserId { get; set; }

    [HiddenInput] public Guid? UserAvatarFileId { get; set; }
}
