using Core.Enums;
using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.BalanceService
{
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceRepository _balanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPointsCodeRepository _pointsCodeRepository;

        public BalanceService(
            IBalanceRepository balanceRepository,
            IUserRepository userRepository,
            IPointsCodeRepository pointsCodeRepository)
        {
            _balanceRepository = balanceRepository;
            _userRepository = userRepository;
            _pointsCodeRepository = pointsCodeRepository;
        }

        public async Task<BalanceViewModel> GetUserBalance(string userId)
        {
            try
            {
                var balance = await _balanceRepository.GetBalanceByUserId(userId);

                if (balance == null)
                {
                    // إنشاء رصيد جديد للمستخدم إذا لم يكن موجوداً
                    balance = await CreateInitialBalance(userId);
                }

                var recentTransactions = await _balanceRepository.GetTransactionsByBalanceId(balance.Id, 1, 5);

                return new BalanceViewModel
                {
                    UserName = balance.User?.UserName ?? "",
                    CurrentBalance = balance.Amount,
                    TotalPoints = balance.Amount,
                    RecentTransactions = recentTransactions,
                    LastUpdateDate = balance.ModifiedAt ?? balance.CreatedAt,
                    TotalTransactions = recentTransactions?.Count ?? 0
                };
            }
            catch (Exception)
            {
                return new BalanceViewModel
                {
                    CurrentBalance = 0,
                    TotalPoints = 0,
                    RecentTransactions = new List<BalanceTransactionDto>(),
                    TotalTransactions = 0,
                    LastUpdateDate = DateTime.Now
                };
            }
        }

        private async Task<BalanceDto> CreateInitialBalance(string userId)
        {
            var newBalance = new BalanceDto
            {
                User = new UserDto { Id = userId },
                Amount = 0,
                CreatedAt = DateTime.Now
            };

            return await _balanceRepository.Add(newBalance);
        }

        public async Task<BalanceTransactionDto> AddPoints(string userId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                    throw new ArgumentException("المبلغ يجب أن يكون أكبر من صفر");

                var balance = await _balanceRepository.GetBalanceByUserId(userId);
                if (balance == null)
                {
                    balance = await CreateInitialBalance(userId);
                }

                return await _balanceRepository.AddTransaction(balance.Id, amount, TransactionType.Credit, description);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BalanceTransactionDto> DeductPoints(string userId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                    throw new ArgumentException("المبلغ يجب أن يكون أكبر من صفر");

                var balance = await _balanceRepository.GetBalanceByUserId(userId);
                if (balance == null)
                {
                    throw new Exception("لا يوجد رصيد للمستخدم");
                }

                if (balance.Amount < amount)
                {
                    throw new Exception($"الرصيد غير كافي. الرصيد المتاح: {balance.Amount}");
                }

                return await _balanceRepository.AddTransaction(balance.Id, amount, TransactionType.Debit, description);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BalanceTransactionDto>> GetUserTransactions(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var balance = await _balanceRepository.GetBalanceByUserId(userId);
                if (balance == null)
                    return new List<BalanceTransactionDto>();

                return await _balanceRepository.GetTransactionsByBalanceId(balance.Id, page, pageSize);
            }
            catch (Exception)
            {
                return new List<BalanceTransactionDto>();
            }
        }

        public async Task<bool> TransferPoints(string fromUserId, string toUserId, decimal amount, string description)
        {
            try
            {
                // التحقق من صحة المدخلات
                if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(toUserId))
                    return false;

                if (fromUserId == toUserId)
                    return false;

                if (amount <= 0)
                    return false;

                // التحقق من وجود المستخدمين
                var fromUser = await _userRepository.GetById(fromUserId);
                var toUser = await _userRepository.GetById(toUserId);

                if (fromUser == null || toUser == null)
                    return false;

                // التحقق من كفاية الرصيد
                var fromBalance = await _balanceRepository.GetBalanceByUserId(fromUserId);
                if (fromBalance == null || fromBalance.Amount < amount)
                    return false;

                // خصم النقاط من المرسل
                var debitTransaction = await DeductPoints(
                    fromUserId, 
                    amount, 
                    $"تحويل نقاط إلى {toUser.UserName}" + (string.IsNullOrEmpty(description) ? "" : $" - {description}")
                );

                if (debitTransaction == null)
                    return false;

                // إضافة النقاط للمستلم
                var creditTransaction = await AddPoints(
                    toUserId, 
                    amount, 
                    $"استلام نقاط من {fromUser.UserName}" + (string.IsNullOrEmpty(description) ? "" : $" - {description}")
                );

                if (creditTransaction == null)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<UserSearchResult>> SearchUsers(string query, string currentUserId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(query) || query.Length < 2)
                    return new List<UserSearchResult>();

                // استخدام UserRepository للبحث
                var users = await _userRepository.SearchUsers(query, 10);

                if (users == null)
                    return new List<UserSearchResult>();

                var searchResults = users
                    .Where(u => currentUserId == null || u.Id != currentUserId)
                    .Select(u => new UserSearchResult
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FullName = u.FullName ?? "",
                        AvatarUrl = "" // يمكن إضافة حقل الصورة الرمزية في UserDto إذا كان موجوداً
                    })
                    .ToList();

                return searchResults;
            }
            catch (Exception)
            {
                return new List<UserSearchResult>();
            }
        }

        public async Task<decimal> GetUserBalanceAmount(string userId)
        {
            try
            {
                var balance = await _balanceRepository.GetBalanceByUserId(userId);
                return balance?.Amount ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> HasEnoughBalance(string userId, decimal amount)
        {
            try
            {
                var balance = await GetUserBalanceAmount(userId);
                return balance >= amount;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<PointsCodeDto>> GenerateCodes(decimal pointsValue, int numberOfCodes, DateTime? expiryDate)
        {
            try
            {
                if (pointsValue <= 0)
                    throw new ArgumentException("قيمة النقاط يجب أن تكون أكبر من صفر");

                if (numberOfCodes <= 0 || numberOfCodes > 100)
                    throw new ArgumentException("عدد الأكواد يجب أن يكون بين 1 و 100");

                var codes = new List<PointsCodeDto>();

                for (int i = 0; i < numberOfCodes; i++)
                {
                    var code = GenerateUniqueCode();
                    var pointsCode = new PointsCodeDto
                    {
                        Code = code,
                        PointsValue = pointsValue,
                        ExpiryDate = expiryDate,
                        IsUsed = false,
                        CreatedAt = DateTime.Now
                    };

                    var createdCode = await _pointsCodeRepository.Add(pointsCode);
                    if (createdCode != null)
                    {
                        codes.Add(createdCode);
                    }
                }

                return codes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PointsCodeDto> RedeemCode(string userId, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new ArgumentException("يرجى إدخال الكود");

                // البحث عن الكود
                var pointsCode = await _pointsCodeRepository.GetByCode(code.Trim().ToUpper());

                if (pointsCode == null)
                    throw new Exception("الكود غير موجود");

                if (pointsCode.IsUsed)
                    throw new Exception("هذا الكود مستخدم بالفعل");

                if (pointsCode.IsExpired)
                    throw new Exception("هذا الكود منتهي الصلاحية");

                // التحقق من أن المستخدم لم يستخدم الكود من قبل (اختياري)
                // يمكن إضافة جدول لتتبع استخدام الأكواد من قبل المستخدمين

                // تعليم الكود كمستخدم
                var markedAsUsed = await _pointsCodeRepository.MarkAsUsed(pointsCode.Id, userId);
                if (!markedAsUsed)
                    throw new Exception("حدث خطأ أثناء استخدام الكود");

                // إضافة النقاط للمستخدم
                var transaction = await AddPoints(
                    userId,
                    pointsCode.PointsValue,
                    $"استرداد كود نقاط: {pointsCode.Code}"
                );

                if (transaction == null)
                    throw new Exception("حدث خطأ أثناء إضافة النقاط");

                return pointsCode;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PointsCodeDto>> GetValidCodes()
        {
            try
            {
                return await _pointsCodeRepository.GetValidCodes();
            }
            catch (Exception)
            {
                return new List<PointsCodeDto>();
            }
        }

        public async Task<List<PointsCodeDto>> GetUsedCodes()
        {
            try
            {
                return await _pointsCodeRepository.GetUsedCodes();
            }
            catch (Exception)
            {
                return new List<PointsCodeDto>();
            }
        }

        private string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var code = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // تنسيق الكود: XXXX-XXXX-XXXX
            return $"{code.Substring(0, 4)}-{code.Substring(4, 4)}-{code.Substring(8, 4)}";
        }
    }
}