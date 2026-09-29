namespace KV.Server.Web.Pages.Stories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Content;

[Authorize]
public class CreateStoryModalModel : ServerPageModel
{
    private readonly IStoriesManagementAppService _storyAppService;

    public CreateStoryModalModel(IStoriesManagementAppService storyAppService)
    {
        this._storyAppService = storyAppService;
    }

    [BindProperty] public CreateStoryViewModel Story { get; set; }

    public List<SelectListItem> StoryStatuses { get; set; }

    public void OnGet()
    {
        var allStoryStatus = (StoryStatus[])Enum.GetValues(typeof(StoryStatus));
        this.StoryStatuses = new List<SelectListItem>();
        foreach (var storyStatus in allStoryStatus)
        {
            this.StoryStatuses.Add(new SelectListItem
            { Text = storyStatus.ToString(), Value = ((int)storyStatus).ToString() });
        }

        this.Story = new CreateStoryViewModel();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var story = this.ObjectMapper.Map<CreateStoryViewModel, CreateUpdateStoryDto>(this.Story);
        story.CoverImageUrl = this.Story.CoverImageUrl;
        if (this.Story.CoverImageUrl.IsNullOrEmpty())
        {
            if (this.HttpContext.Request.Form.Files.Count != 0)
            {
                var formFiles = this.HttpContext.Request.Form.Files;
                var formFile = formFiles.FirstOrDefault();
                var uploadedFile = await this._storyAppService.UploadAsync(
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType));
                story.CoverImageUrl = $"/api/content/stories/{uploadedFile.Id}";
            }
        }
        await this._storyAppService.CreateStoryAsync(story);
        return this.NoContent();
    }

    public class CreateStoryViewModel
    {
        [DataType(DataType.Text)] public string GroupName { get; set; }

        [DataType(DataType.Text)] public string Name { get; set; }

        [DataType(DataType.Text)] public string CoverImageUrl { get; set; }

        [HiddenInput] public int OrderIndex { get; set; } = 1;

        public StoryStatus Status { get; set; }
    }
}
