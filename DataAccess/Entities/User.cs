using Core;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class User : IdentityUser, IEntity
    {
        public User()
        {
            CourseRegistrations = new HashSet<CourseRegistration>();
        }

        public string Description { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }

        [ForeignKey("Balance")]
        public int? BalanceId { get; set; }

        public Balance Balance { get; set; }

        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}