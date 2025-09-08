using Application.Dishes.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Dishes.Command.UpdateDish.DishUpdateCommand;

namespace Application.Dishes.Command.UpdateDish
{
    public interface IDishUpdateHandler
    {
        Task<DishResponseDto> HandleAsync(UpdateDishCommand command, CancellationToken ct = default);
    }
}