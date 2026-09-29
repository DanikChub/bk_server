namespace KV.Server.Web.Pages.Stories;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Content;

[Authorize]
public class CreateSliderModalModel : ServerPageModel
{
    private readonly IStoriesManagementAppService _sliderAppService;

    public CreateSliderModalModel(IStoriesManagementAppService sliderAppService)
    {
        this._sliderAppService = sliderAppService;
    }

    [BindProperty] public CreateSliderViewModel Slider { get; set; }

    [BindProperty(SupportsGet = true)]
    [HiddenInput]
    public Guid StoryId { get; set; }

    public void OnGet() => this.Slider = new CreateSliderViewModel
    {
        StoryId = this.StoryId
    };

    public async Task<IActionResult> OnPostAsync()
    {
        var slider = this.ObjectMapper.Map<CreateSliderViewModel, CreateUpdateSlideDto>(this.Slider);
        slider.ImageUrl = this.Slider.ImageUrl;
        if (this.Slider.ImageUrl.IsNullOrEmpty())
        {
            if (this.HttpContext.Request.Form.Files.Count != 0)
            {
                var formFiles = this.HttpContext.Request.Form.Files;
                var formFile = formFiles.FirstOrDefault();
                var uploadedFile = await this._sliderAppService.UploadAsync(
                    new RemoteStreamContent(formFile.OpenReadStream(),
                        formFile.FileName, formFile.ContentType));
                slider.ImageUrl = $"/api/content/stories/{uploadedFile.Id}";
            }
        }
        await this._sliderAppService.CreateSlideAsync(slider);
        return this.NoContent();
    }

    public class CreateSliderViewModel
    {
        [DataType(DataType.Text)] public string ImageUrl { get; set; }

        public int OrderIndex { get; set; } = 1;

        [HiddenInput] public Guid StoryId { get; set; }
    }
}
