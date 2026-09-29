namespace KV.Server.Controllers;

using System;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

[Authorize]
[Route("/api/content")]
public class ContentFileController : ServerController
{
    private readonly IBlobContainer _storiesContainer;
    private readonly IBlobContainer _avatarContainer;
    private readonly IDataFilter _dataFilter;
    private readonly IEmployeeProfileAppService _employeeProfileAppService;
    public ContentFileController(IBlobContainerFactory blobContainerFactory, IDataFilter dataFilter, IEmployeeProfileAppService employeeProfileAppService, IBlobContainerFactory avatarContainer)
    {
        this._storiesContainer = blobContainerFactory.Create(BlobContainers.STORIES);
        _dataFilter = dataFilter;
        _employeeProfileAppService = employeeProfileAppService;
        _avatarContainer = avatarContainer.Create(BlobContainers.DEFAULT_PUBLIC);
    }
    [Route("/avatar/image")]
    [HttpGet]
    public async Task<IActionResult> GetAvatarImage(string fileId)
    {
        var content = await this._avatarContainer.GetOrNullAsync(fileId);
        return File(content, "image/jpeg");
    }
    [Route("/publicavatar/image")]
    [HttpGet]
    public async Task<IActionResult> GetPublicAvatarImage(string responsibleId)
    {
        var res = await _employeeProfileAppService.GetEmployeeByIdAsync(Guid.Parse(responsibleId));
        var content = await this._avatarContainer.GetOrNullAsync(res.UserAvatarFileId.ToString());
        return File(content, "image/jpeg");
    }
    [HttpGet("stories/{id}")]
    public async Task<IActionResult> GetStoryFileContentAsync(string id)
    {
        using (CurrentTenant.Change(null))
        {
            var stream = await this._storiesContainer.GetAsync(id);
            return new FileStreamResult(stream, "image/jpeg");
        }
    }
}
