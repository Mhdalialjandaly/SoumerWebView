using Core.Enums;
using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.IRepositories
{
    public interface IPointsCodeRepository : IBaseRepository<PointsCodeDto, PointsCode>, IInjectable
    {
        Task<PointsCodeDto> GetByCode(string code);
        Task<List<PointsCodeDto>> GetValidCodes();
        Task<List<PointsCodeDto>> GetUsedCodes();
        Task<bool> MarkAsUsed(int id, string userId);
    }
}