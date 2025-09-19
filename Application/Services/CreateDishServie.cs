using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions;   
using Application.Interfaces;   
using Application.Queries;      

namespace Application.Services
{
    public sealed class CreateDishService : ICreateDishService
    {
        private readonly IDishCommand _command;
        private readonly IDishQuery _query;
        private readonly ICategoryQuery _categoryQuery;

        public CreateDishService(IDishCommand command, IDishQuery query, ICategoryQuery categoryQuery)
        {
            _command = command;
            _query = query;
            _categoryQuery = categoryQuery;
        }

        public async Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default)
        {
            // Reglas de negocio (no de forma):
            if (dto.Price <= 0)
                throw new BusinessRuleException("El precio debe ser mayor a cero.");

            if (!await _categoryQuery.ExistsAsync(dto.Category, ct))
                throw new NotFoundException("La categoría no existe.");

            var nameTaken = await _query.ExistsByNameAsync(dto.Name, ct);
            if (nameTaken)
                throw new ConflictException("Ya existe un plato con ese nombre.");

            // Persistencia delegada al command (sin reglas)
            return await _command.CreateAsync(dto, ct);
        }
    }
}
