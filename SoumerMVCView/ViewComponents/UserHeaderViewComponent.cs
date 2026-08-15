using DataAccess.IRepositories;
using Microsoft.AspNetCore.Mvc;
using SoumerMVCView.Models;
using System.Security.Claims;

namespace SoumerMVCView.ViewComponents
{
    public class UserHeaderViewComponent : ViewComponent
    {
        private readonly IUserRepository _userRepository;

        public UserHeaderViewComponent(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return View(new UserHeaderViewModel());

            var user = await _userRepository.GetById(userId);

            var model = new UserHeaderViewModel
            {
                UserId = user?.Id ?? "",
                UserName = user?.UserName ?? UserClaimsPrincipal.Identity?.Name ?? "",
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                AvatarUrl = user?.AvatarUrl ?? ""
            };

            return View(model);
        }
    }
}