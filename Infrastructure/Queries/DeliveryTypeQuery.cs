using Application.Dtos;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries
{
    public sealed class DeliveryTypeQuery : IDeliveryTypeQuery
    {
        private readonly AppDbContext _db;
        public DeliveryTypeQuery(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<DeliveryTypeDto>> GetAllAsync(CancellationToken ct) =>
            await _db.DeliveryTypes.AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new DeliveryTypeDto { Id = x.Id, Name = x.Name })
                .ToListAsync(ct);
        public Task<bool> ExistsAsync(int deliveryTypeId, CancellationToken ct = default)
            => _db.DeliveryTypes.AsNoTracking().AnyAsync(d => d.Id == deliveryTypeId, ct);
    }
}
