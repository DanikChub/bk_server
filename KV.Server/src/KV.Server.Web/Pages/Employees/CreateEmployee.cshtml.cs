using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Identity;

namespace KV.Server.Web.Pages.Employees;

public class CreateEmployeeModel : ServerPageModel
{
    private readonly IEmployeeProfileAppService _usersAppService;
    private readonly IIdentityRoleAppService _roleAppService;
    [BindProperty] public CreateEmployeeVM EmployeeVM { get; set; }
    [BindProperty(SupportsGet = true)]
    public IFormFile ImgFile { get; set; }
    public ListResultDto<IdentityRoleDto> Roles { get; set; }
    public CreateEmployeeModel(IEmployeeProfileAppService usersAppService, IIdentityRoleAppService roleAppService)
    {
        _usersAppService = usersAppService;
        _roleAppService = roleAppService;
    }
    public async Task OnGetAsync()
    {
        Roles = await _roleAppService.GetAllListAsync();
        EmployeeVM = new CreateEmployeeVM();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var dto = ObjectMapper.Map<CreateEmployeeVM, CreateUpdateEmployeeProfileDto>(EmployeeVM);

        var fioParts = EmployeeVM.Fio.Split(' '); // Split the FIO into separate parts using space as the delimiter

        // Assign the parts to the corresponding properties in the dto
        switch (fioParts.Count())
        {
            case 1:
                dto.LastName = fioParts[0];
                break;
            case 2:
                dto.LastName = fioParts[0];
                dto.FirstName = fioParts[1];
                break;
            case 3:
                dto.LastName = fioParts[0];
                dto.FirstName = fioParts[1];
                dto.MiddleName = fioParts[2];
                break;
        }

        try { 
        var employee = await _usersAppService.CreateEmployeeAsync(dto);
        if (ImgFile != null)
        {
            await _usersAppService.UploadEmployeeAvatarAsync(employee.Id, new RemoteStreamContent(ImgFile.OpenReadStream(), ImgFile.FileName, ImgFile.ContentType));
        }
        }
        catch (UserFriendlyException ex)
        {
            Alerts.Danger(ex.Message);
            return Page();
        }

        return RedirectToPage("Index");

    }
}
public class CreateEmployeeVM
{
    [HiddenInput] public Guid Id { get; set; }
    [Required]
    public string Fio { get; set; }
    public string JobPost { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Birthday { get; set; }
    
    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }
    [Required][MaxLength(256)] public string Login { get; set; }
    [MinLength(8)]
    [MaxLength(128)]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$",
  ErrorMessage =
      "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
    public string Password { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    [MaxLength(12)]
    [Phone]
    [Required]
    public string Phone { get; set; }
    [Required]
    public List<string> RoleNames { get; set; }
}
