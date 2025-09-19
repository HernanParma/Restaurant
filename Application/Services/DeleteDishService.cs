using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Exceptions;     
using Application.Interfaces;
using Application.Queries;

namespace Application.Services
{
    public sealed class DeleteDishService : IDeleteDishService
    {
        private readonly IDishQuery _dishQuery;
        private readonly IDishCommand _dishCommand;

        public DeleteDishService(IDishQuery dishQuery, IDishCommand dishCommand)
        {
            _dishQuery = dishQuery;
            _dishCommand = dishCommand;
        }

        public async Task<DishResponseDto> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            // 1) Debe existir
            var dish = await _dishQuery.GetByIdAsync(id, ct);
            if (dish is null)
                throw new NotFoundException("Plato no encontrado");

            // 2) No debe estar en órdenes activas
            var inActiveOrders = await _dishQuery.IsInActiveOrdersAsync(id, ct);
            if (inActiveOrders)
                throw new ConflictException("No se puede eliminar el plato porque está incluido en órdenes activas");

            // 3) Eliminar (command)
            return await _dishCommand.DeleteAsync(id, ct);
        }
    }
}
