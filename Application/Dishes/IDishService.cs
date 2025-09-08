using Application.Dishes.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dishes
{
    public interface IDishService
    {
        Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default);
        Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery query, CancellationToken ct);
        Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct); // <-- nuevo
    }
}