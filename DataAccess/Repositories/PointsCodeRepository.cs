using AutoMapper;
using Core;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class PointsCodeRepository : BaseRepository<PointsCodeDto, PointsCode>, IPointsCodeRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public PointsCodeRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<PointsCodeDto> GetByCode(string code)
        {
            try
            {
                var pointsCode = await _context.Set<PointsCode>()
                    .Include(pc => pc.UsedByUser)
                    .FirstOrDefaultAsync(pc => pc.Code == code && pc.DeletedAt == null);

                return _mapper.Map<PointsCodeDto>(pointsCode);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<PointsCodeDto>> GetValidCodes()
        {
            try
            {
                var codes = await _context.Set<PointsCode>()
                    .Where(pc => pc.DeletedAt == null && !pc.IsUsed &&
                                (!pc.ExpiryDate.HasValue || pc.ExpiryDate > DateTime.Now))
                    .OrderByDescending(pc => pc.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<PointsCodeDto>>(codes);
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
                var codes = await _context.Set<PointsCode>()
                    .Include(pc => pc.UsedByUser)
                    .Where(pc => pc.DeletedAt == null && pc.IsUsed)
                    .OrderByDescending(pc => pc.UsedAt)
                    .ToListAsync();

                return _mapper.Map<List<PointsCodeDto>>(codes);
            }
            catch (Exception)
            {
                return new List<PointsCodeDto>();
            }
        }

        public async Task<bool> MarkAsUsed(int id, string userId)
        {
            try
            {
                var pointsCode = await _context.Set<PointsCode>().FindAsync(id);
                if (pointsCode == null || pointsCode.IsUsed)
                    return false;

                pointsCode.IsUsed = true;
                pointsCode.UsedByUserId = userId;
                pointsCode.UsedAt = DateTime.Now;
                pointsCode.ModifiedAt = DateTime.Now;

                await SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}