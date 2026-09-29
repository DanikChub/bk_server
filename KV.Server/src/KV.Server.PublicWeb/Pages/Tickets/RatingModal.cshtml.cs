namespace KV.Server.PublicWeb.Pages.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class RatingModalModel : ServerPageModel
{
    private readonly ITicketsPublicAppService _ticketsAppService;

    public RatingModalModel(ITicketsPublicAppService ticketsAppService)
    {
        this._ticketsAppService = ticketsAppService;
    }

    [BindProperty] public RatingViewModal RatingModel { get; set; } = new();

    public async Task OnGetAsync(long ticketId)
    {
        var ticketRating = await this._ticketsAppService.GetTicketRatingAsync(ticketId);
        this.RatingModel = new RatingViewModal
        {
            TicketId = ticketId,
            Rating = ticketRating?.Rating ?? 1.0,
            Comment = ticketRating?.Comment
        };
    }

    public async Task OnPostAsync() => await this._ticketsAppService.SetRatingTicketAsync(this.RatingModel.TicketId, new CreateUpdateTicketRatingDto
    {
        Comment = this.RatingModel.Comment,
        Rating = this.RatingModel.Rating
    });

    public class RatingViewModal
    {
        public double Rating { get; set; } = 1.0;
        public string? Comment { get; set; }
        public long TicketId { get; set; }
    }
}
