namespace KV.Server.Web.Pages.Stories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Content;

[Authorize]
public class EditSliderModalModel : ServerPageModel
{
    private readonly IStoriesManagementAppService _storyAppService;

    public EditSliderModalModel(IStoriesManagementAppService storyAppService)
    {
        this._storyAppService = storyAppService;
    }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid Id { get; set; }

    [BindProperty][HiddenInput] public IFormFile CoverImage { get; set; }

    [BindProperty] public UpdateSliderViewModel Slider { get; set; }

    public async Task OnGetAsync()
    {
        var dto = await this._storyAppService.GetSliderBySliderIdAsync(this.Id);
        this.Slider = this.ObjectMapper.Map<SlideDto, UpdateSliderViewModel>(dto);
        this.Slider.Id = this.Id;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var slider = this.ObjectMapper.Map<UpdateSliderViewModel, CreateUpdateSlideDto>(this.Slider);
        slider.ImageUrl = this.Slider.ImageUrl;
        if (this.HttpContext.Request.Form.Files.Count != 0)
        {
            var formFiles = this.HttpContext.Request.Form.Files;
            var formFile = formFiles.FirstOrDefault();
            var uploadedFile = await this._storyAppService.UploadAsync(
                new RemoteStreamContent(formFile.OpenReadStream(),
                    formFile.FileName, formFile.ContentType));
            slider.ImageUrl = $"/api/content/stories/{uploadedFile.Id}";
        }
        try
        {
            await this._storyAppService.UpdateSlideAsync(this.Slider.Id, slider);
        }
        catch (Exception ex)
        {
            return this.BadRequest(ex.Message);
        }

        return this.NoContent();
    }
}

public class UpdateSliderViewModel
{
    [HiddenInput] public Guid Id { get; set; }

    [DataType(DataType.Text)] public string ImageUrl { get; set; }

    public int OrderIndex { get; set; }

    [HiddenInput] public Guid StoryId { get; set; }
}
