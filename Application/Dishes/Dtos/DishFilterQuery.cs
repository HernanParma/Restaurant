using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dishes.Dtos
{
    public class DishFilterQuery
    {
        public string? Name { get; set; }
        public int? Category { get; set; }
        public PriceSort? SortByPrice { get; set; }
        public bool? OnlyActive { get; set; }
    }
}

