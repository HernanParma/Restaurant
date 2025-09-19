using Application.Dtos;
using Application.Interfaces;
using Application.Mappers;
using Application.Queries;
using Domain.Enums; 
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Application.Interfaces.IDishQuery;

namespace Infrastructure.Queries
{
    public class DishQuery : IDishQuery
    {
        private readonly AppDbContext _db;
        public DishQuery(AppDbContext db) => _db = db;

        public async Task<IEnumerable<DishResponseDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Dishes
                .AsNoTracking()
                .OrderBy(d => d.Price)
                .Select(DishMapper.ToDtoProjection())   //mapper que me dijo
                .ToListAsync(ct);
        }
        public async Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery q, CancellationToken ct = default)
        {
            var query = _db.Dishes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Name))
            {
                var name = q.Name.Trim();
                query = query.Where(d => EF.Functions.Like(d.Name, $"%{name}%"));
            }

            if (q.Category.HasValue)
                query = query.Where(d => d.CategoryId == q.Category.Value);

            if (q.OnlyActive.GetValueOrDefault())
                query = query.Where(d => d.Available);

            if (q.SortByPrice.HasValue)
            {
                query = q.SortByPrice.Value == PriceSort.desc
                    ? query.OrderByDescending(d => d.Price)
                    : query.OrderBy(d => d.Price);
            }
            else
            {
                query = query.OrderBy(d => d.Price);
            }

            return await query
                .Select(DishMapper.ToDtoProjection())  //mapper que me dijo
                .ToListAsync(ct);
        }
        public async Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Dishes
                 .AsNoTracking()
                 .Where(x => x.DishId == id)
                 .Select(DishMapper.ToDtoProjection())   // el mapper que me pidio
                 .FirstOrDefaultAsync(ct);
        }
        public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
            => _db.Dishes.AsNoTracking().AnyAsync(d => d.Name == name, ct);
        public async Task<bool> IsInActiveOrdersAsync(Guid dishId, CancellationToken ct = default)
        {
            var activeStatusIds = new[] { 1, 2 };

            return await _db.Orders
                .AsNoTracking()
                .Where(o => activeStatusIds.Contains(o.OverallStatusId))
                .AnyAsync(o => o.Items.Any(i => i.DishId == dishId), ct);
        }
        public async Task<IReadOnlyList<DishBasicInfo>> GetBasicByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var set = ids.ToHashSet();
            return await _db.Dishes
                .AsNoTracking()
                .Where(d => set.Contains(d.DishId))
                .Select(d => new DishBasicInfo
                {
                    Id = d.DishId,
                    Price = d.Price,
                    IsActive = d.Available
                })
                .ToListAsync(ct);
        }
    }
}