using Application.Dtos;
using Application.Interfaces;

namespace Application.Services
{
    public class UpdateDishService : IUpdateDishService
    {
        private readonly IDishCommand _command;
        public UpdateDishService(IDishCommand command) => _command = command;
        public Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct = default)
            => _command.UpdateAsync(id, dto, ct);
    }
}