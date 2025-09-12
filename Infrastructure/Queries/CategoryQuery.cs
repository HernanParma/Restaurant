using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public sealed class CategoryQuery : ICategoryQuery
    {
        private readonly AppDbContext _db;
        public CategoryQuery(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct) =>
            await _db.Categories.AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description, Order = c.Order })
                .ToListAsync(ct);
    }
}
