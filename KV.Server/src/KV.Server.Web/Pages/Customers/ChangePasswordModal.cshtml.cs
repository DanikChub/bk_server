using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KV.Server.Web.Pages.Customers
{
    public class ChangePasswordModalModel : ServerPageModel
    {
        private readonly IUsersAppService _usersAppService;

        private readonly ICustomerUserProfilesAppService _customerUserProfilesAppService;

        public ChangePasswordModalModel(ICustomerUserProfilesAppService customerUserProfilesAppService, IUsersAppService usersAppService)
        {
            _customerUserProfilesAppService = customerUserProfilesAppService;
            _usersAppService = usersAppService;
        }

        [BindProperty]
        public ChangePasswordDto ChangePassword { get; set; }

        public async Task OnGetAsync(Guid id)
        {
            ChangePassword = new ChangePasswordDto();
            ChangePassword.UserId = id.ToString();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = ChangePassword.UserId;
            await _customerUserProfilesAppService.ResetUserPasswordByIdAsync(userId, ChangePassword.NewPassword);
            return NoContent();
        }
        public class ChangePasswordDto
        {
            [HiddenInput] public string UserId { get; set; }
            [Required][PasswordPropertyText] public string NewPassword { get; set; }
        }
    }
}
