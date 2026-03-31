using System.ComponentModel;
namespace Core.Enums
{
    public enum TransactionType
    {
        [Description("دخل")]
        In,
        [Description("خرج")]
        Out
    }
}
