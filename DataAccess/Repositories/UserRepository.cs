using AutoMapper;
using Core;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class UserRepository : BaseRepository<UserDto, User>, IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly User _currentUser;
        private readonly ApplicationDbContext _context;
        public UserRepository(IMapper mapper,
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,ApplicationDbContext applicationDbContext) : base(mapper, context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _currentUser = Global.GetValue(GlobalKeys.LoggedUser) as User;
            _context = applicationDbContext;
        }
        public async Task<UserDto> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public override async Task<List<UserDto>> GetAll()
        {
            var users = await _userManager.Users
                .Where(u => u.DeletedAt == null)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> Create(CreateUserDto model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Description = model.Description,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors));

            return _mapper.Map<UserDto>(user);
        }

        public async Task Update(UpdateUserDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                throw new Exception("User not found");

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.Description = model.Description;
            user.IsActive = model.IsActive;
            user.ModifiedAt = DateTime.Now;
            user.ModifiedBy = _currentUser?.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors));
        }

        public async Task Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors));
            }
        }

        public async Task SoftDelete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                //user.DeletedAt = DateTime.Now;
                //user.DeletedBy = _currentUser?.UserName;
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                //user.DeletedAt = null;
                //user.DeletedBy = null;
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<bool> IsEmailExist(string email, string excludeUserId = null)
        {
            return await _userManager.Users
                .AnyAsync(u => u.Email == email && u.Id != excludeUserId && u.DeletedAt == null);
        }

        public async Task<bool> IsUserNameExist(string userName, string excludeUserId = null)
        {
            return await _userManager.Users
                .AnyAsync(u => u.UserName == userName && u.Id != excludeUserId && u.DeletedAt == null);
        }

        public async Task UpdatePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors));
        }

        public async Task UpdateLastLogin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task AssignToRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors));
        }

        public async Task<List<string>> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }
    }
}