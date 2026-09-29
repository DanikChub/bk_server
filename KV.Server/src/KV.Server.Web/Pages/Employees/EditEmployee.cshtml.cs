using System;
using System.Linq;
using System.Threading.Tasks;
using KV.Server.Dtos.Employees;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Identity;

namespace KV.Server.Web.Pages.Employees;

public class EditEmployeeModel : ServerPageModel
{
    private readonly IEmployeeProfileAppService _usersAppService;
    private readonly IIdentityRoleAppService _roleAppService;
    [BindProperty] public EmployeeProfileDto EmployeeProfDto { get; set; }
    [BindProperty(SupportsGet = true)]
    public IFormFile ImgFile { get; set; }
    [BindProperty] public CreateEmployeeVM EmployeeVM { get; set; }
    [HiddenInput][BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ListResultDto<IdentityRoleDto> Roles { get; set; }
    public EditEmployeeModel(IEmployeeProfileAppService usersAppService, IIdentityRoleAppService roleAppService)
    {
        _usersAppService = usersAppService;
        _roleAppService = roleAppService;
    }
    public async Task OnGetAsync(Guid id)
    {
       
        Roles = await _roleAppService.GetAllListAsync();
        EmployeeProfDto = await _usersAppService.GetEmployeeByIdAsync(id);
        EmployeeVM = new CreateEmployeeVM();
        EmployeeVM.JobPost = EmployeeProfDto.JobPost;
        EmployeeVM.Birthday = EmployeeProfDto.Birthday;
        EmployeeVM.Email = EmployeeProfDto.Email;
        EmployeeVM.Fio = $"{EmployeeProfDto.LastName} {EmployeeProfDto.FirstName} {EmployeeProfDto.MiddleName}";
        EmployeeVM.Phone = EmployeeProfDto.Phone;
        EmployeeVM.Login = EmployeeProfDto.Login;
        EmployeeVM.Password = EmployeeProfDto.NewPassword;
        EmployeeVM.CurrentPassword = string.Empty;
        EmployeeVM.Id = id;
        EmployeeVM.RoleNames = await _usersAppService.GetRoleNamesByEmployeeIdAsync(id);

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

        try
        {
            var employee = await _usersAppService.UpdateEmployeeAsync(dto.Id, dto);
            if (ImgFile != null)
            {
                await _usersAppService.UploadEmployeeAvatarAsync(employee.Id, new RemoteStreamContent(ImgFile.OpenReadStream(), ImgFile.FileName, ImgFile.ContentType));
            }
        }
        catch(UserFriendlyException ex)
        {
            Alerts.Danger(ex.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }
}
