using System;
using System.Threading.Tasks;
using KV.Server.Dtos.File;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Volo.Abp.BlobStoring;

namespace KV.Server.Controllers;
public class EmployeeAvatarController : ServerController
{
    private readonly IBlobContainer _blobContainer;
    private readonly IEmployeeProfileAppService _usersAppService;
    private readonly ILogger<EmployeeAvatarController> _logger;

    private const string DefaultFallbackPath = "wwwroot/img/default.png";

    public EmployeeAvatarController(IBlobContainerFactory blobContainer, IEmployeeProfileAppService usersAppService, ILogger<EmployeeAvatarController> logger)
    {
        _blobContainer = blobContainer.Create(BlobContainers.AVATAR_EMPLOYEE);
        _usersAppService = usersAppService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    [ResponseCache(Duration = 180)]
    public async Task<IActionResult> Avatar(Guid? employeeId, Guid? indentityUserId)
    {
        using (CurrentTenant.Change(null))
        {
            UploadedFileDto img = null;
            if (indentityUserId != null)
            {
                var employee = await _usersAppService.GetEmployeeByIdentityIdAsync(indentityUserId);
                if (employee != null)
                {
                    img = await _usersAppService.GetAvatarByEmployeeIdAsync(employee.Id);
                }
                else
                {
                    img = await _usersAppService.GetAvatarByEmployeeIdAsync(employeeId ?? Guid.Empty);
                }
            }
            else
            {
                img = await _usersAppService.GetAvatarByEmployeeIdAsync(employeeId ?? Guid.Empty);
            }

            try
            {
                if (img != null)
                {
                    var stream = await _blobContainer.GetOrNullAsync(img.Id.ToString());
                    if (stream == null)
                    {
                        stream = System.IO.File.OpenRead(DefaultFallbackPath);
                    }
                    return new FileStreamResult(stream, "application/octet-stream");
                }
                else
                {
                    return new FileStreamResult(System.IO.File.OpenRead(DefaultFallbackPath), "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Default fallback avatar doesn't exists");
                return NotFound();
            }
        }
    }
}

