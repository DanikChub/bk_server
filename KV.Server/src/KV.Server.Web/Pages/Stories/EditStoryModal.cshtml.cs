namespace KV.Server.Web.Pages.Stories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Content;

[Authorize]
public class EditStoryModalModel : ServerPageModel
{
    private readonly IStoriesManagementAppService _storyAppService;

    public EditStoryModalModel(IStoriesManagementAppService storyAppService)
    {
        this._storyAppService = storyAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty][HiddenInput] public IFormFile CoverImage { get; set; }

    [BindProperty] public UpdateStoryViewModel Story { get; set; }

    public List<SelectListItem> StoryStatuses { get; set; }

    public async Task OnGetAsync()
    {
        var allStoryStatus = (StoryStatus[])Enum.GetValues(typeof(StoryStatus));
        this.StoryStatuses = new List<SelectListItem>();
        foreach (var storyStatus in allStoryStatus)
        {
            this.StoryStatuses.Add(new SelectListItem
            { Text = storyStatus.ToString(), Value = ((int)storyStatus).ToString(CultureInfo.InvariantCulture) });
        }

        var dto = await this._storyAppService.GetStoryAsync(this.Id);
        this.Story = this.ObjectMapper.Map<StoryDto, UpdateStoryViewModel>(dto);
        this.Story.Id = this.Id;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var story = this.ObjectMapper.Map<UpdateStoryViewModel, CreateUpdateStoryDto>(this.Story);
        story.CoverImageUrl = this.Story.CoverImageUrl;
        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            var formFile = formFiles.FirstOrDefault();
            var uploadedFile = await this._storyAppService.UploadAsync(
                new RemoteStreamContent(formFile.OpenReadStream(),
                    formFile.FileName, formFile.ContentType));
            story.CoverImageUrl = $"/api/content/stories/{uploadedFile.Id}";
        }

        try
        {
            await this._storyAppService.UpdateStoryAsync(story);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }

        return this.NoContent();
    }
}

public class UpdateStoryViewModel
{
    [HiddenInput] public Guid Id { get; set; }

    [DataType(DataType.Text)] public string GroupName { get; set; }

    [DataType(DataType.Text)] public string Name { get; set; }

    [DataType(DataType.Text)] public string CoverImageUrl { get; set; }

    [HiddenInput] public int OrderIndex { get; set; }

    public StoryStatus Status { get; set; }
}
