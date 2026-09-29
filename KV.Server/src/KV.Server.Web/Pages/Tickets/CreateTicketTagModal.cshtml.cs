namespace KV.Server.Web.Pages.Tickets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class CreateTicketTagModal : ServerPageModel
{
    private readonly ITagsAppService _tagsAppService;

    public CreateTicketTagModal(ITagsAppService tagsAppService)
    {
        this._tagsAppService = tagsAppService;
    }

    [BindProperty] public CreateTicketTagViewModel Tag { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public long TicketId { get; set; }

    public void OnGet() => this.Tag = new CreateTicketTagViewModel
    {
        TicketId = this.TicketId
    };

    public async Task<IActionResult> OnPostAsync()
    {
        await this._tagsAppService.InsertTagInTicketAsync(new CreateUpdateTicketTagDto
        {
            TicketId = this.Tag.TicketId,
            TagName = this.Tag.TagName
        });
        return this.NoContent();
    }

    public class CreateTicketTagViewModel
    {
        public string TagName { get; set; }

        [HiddenInput] public long TicketId { get; set; }
    }
}
