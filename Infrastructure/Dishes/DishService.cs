using Application.Dishes;
using Application.Dishes.Command.CreateDish;
using Application.Dishes.Command.UpdateDish;
using Application.Dishes.Dtos;
using Application.Queries;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Application.Dishes.Command.UpdateDish.DishUpdateCommand;

namespace Infrastructure.Dishes
{
    public class DishService : IDishService
    {
        private readonly ICreateDishHandler _createHandler;
        private readonly IDishQuery _dishQuery;
        private readonly IDishUpdateHandler _updateHandler;

        public DishService(IDishQuery dishQuery, IDishUpdateHandler updateHandler, ICreateDishHandler createHandler)
        {
            _updateHandler = updateHandler;
            _dishQuery = dishQuery;
            _createHandler = createHandler;
        }
        public Task<DishResponseDto> CreateAsync(DishCreateDto dto, CancellationToken ct = default)
        => _createHandler.HandleAsync(new CreateDishCommand(dto), ct);
        
        public Task<DishResponseDto> UpdateAsync(Guid id, DishUpdateDto dto, CancellationToken ct)
        => _updateHandler.HandleAsync(new UpdateDishCommand(id, dto), ct);

        public Task<IEnumerable<DishResponseDto>> SearchAsync(DishFilterQuery q, CancellationToken ct)
        => _dishQuery.SearchAsync(q, ct)
            .ContinueWith(t => (IEnumerable<DishResponseDto>)t.Result, ct);
    }
}