using Application.Dishes.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dishes.Command.CreateDish
{
    public interface ICreateDishHandler
    {
        Task<DishResponseDto> HandleAsync(CreateDishCommand command, CancellationToken ct = default);
    }
}