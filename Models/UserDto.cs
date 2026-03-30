using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }      
        public string Email { get; set; }         
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }

    }

    // لعملية الإنشاء (Create)
    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // لعملية التحديث (Update)
    public class UpdateUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    // لعرض قائمة المستخدمين
    public class UserListDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
