using Application.Dishes.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public interface IDishQuery
    {
        Task<DishResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<DishResponseDto>> SearchAsync(DishFilterQuery q, CancellationToken ct = default);
    }
}