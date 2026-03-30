using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface IUserRepository:IBaseRepository<UserDto,User>,IInjectable
    {
        Task<UserDto> GetById(string id);
        Task<UserDto> GetByEmail(string email);
        Task<UserDto> GetByUserName(string userName);
        Task<UserDto> Create(CreateUserDto model);
        Task Update(UpdateUserDto model);
        Task Delete(string id);
        Task SoftDelete(string id);
        Task Restore(string id);
        Task<bool> IsEmailExist(string email, string excludeUserId = null);
        Task<bool> IsUserNameExist(string userName, string excludeUserId = null);
        Task UpdatePassword(string userId, string currentPassword, string newPassword);
        Task UpdateLastLogin(string userId);
        Task AssignToRole(string userId, string role);
        Task<List<string>> GetUserRoles(string userId);
    }
}
