using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces
{
    public interface IDeleteDishService
    {
        Task<DishResponseDto> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}