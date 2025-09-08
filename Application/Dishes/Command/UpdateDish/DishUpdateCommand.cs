using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dishes.Dtos;

namespace Application.Dishes.Command.UpdateDish
{
    public class DishUpdateCommand
    {
        public sealed record UpdateDishCommand(Guid Id, DishUpdateDto Dto);
    }
}