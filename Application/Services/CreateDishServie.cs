using Application.Dtos;
using Application.Interfaces;

namespace Application.Services
{
    public class CreateDishService : ICreateDishService
    {
        private readonly IDishCommand _command;
        public CreateDishService(IDishCommand command) => _command = command;
        public Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default)
            => _command.CreateAsync(dto, ct);
    }
}