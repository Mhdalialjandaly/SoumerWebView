using System.ComponentModel;
namespace Core.Enums
{
    public enum TransactionType
    {
        [Description("إيداع")]
        Credit = 1,
        [Description("سحب")]
        Debit = 2,
        [Description("دخل")]
        In = 3,
        [Description("خرج")]
        Out = 4
    }
}
