using Models;

namespace SoumerMVCView.Models
{
    public class PointsCodesViewModel
    {
        public List<PointsCodeDto> ValidCodes { get; set; } = new();
        public List<PointsCodeDto> UsedCodes { get; set; } = new();
        public int TotalValid => ValidCodes.Count;
        public int TotalUsed => UsedCodes.Count;
        public decimal TotalValidPoints => ValidCodes.Sum(c => c.PointsValue);
        public decimal TotalUsedPoints => UsedCodes.Sum(c => c.PointsValue);
    }

}
