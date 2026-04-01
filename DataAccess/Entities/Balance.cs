using Core;
using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Balance : IEntity
    {
        public Balance() 
        {
            BalanceTransactions = new HashSet<BalanceTransaction>();
        }
        
        public int Id { get; set; }
        public decimal Amount { get; set; }

        // إضافة العلاقة العكسية
        public string UserId { get; set; }
        public virtual User User { get; set; }
        
        public virtual ICollection<BalanceTransaction> BalanceTransactions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}