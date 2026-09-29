namespace KV.Server.Web.Pages.Stories;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class SlidersModel : ServerPageModel
{
    [BindProperty(SupportsGet = true)] public Guid StoryId { get; set; }

    public void OnGet(Guid id) => this.StoryId = id;
}
