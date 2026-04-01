using DataAccess.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace SoumerMVCView.Controllers.UsersManagments
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAll();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            if (await _userRepository.IsEmailExist(model.Email))
                return BadRequest("Email already exists");

            var user = await _userRepository.Create(model);
            return Ok(user);
        }
    }
}