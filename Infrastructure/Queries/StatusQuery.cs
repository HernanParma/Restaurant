using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public sealed class StatusQuery : IStatusQuery
    {
        private readonly AppDbContext _db;
        public StatusQuery(AppDbContext db) => _db = db;
        public async Task<IReadOnlyList<StatusDto>> GetAllAsync(CancellationToken ct) =>
        await _db.Statuses.AsNoTracking()
         .OrderBy(x => x.Id)
         .Select(x => new StatusDto { Id = x.Id, Name = x.Name })
         .ToListAsync(ct);
        public Task<bool> ExistsAsync(int statusId, CancellationToken ct = default)
           => _db.Statuses.AsNoTracking().AnyAsync(s => s.Id == statusId, ct);
    }
}