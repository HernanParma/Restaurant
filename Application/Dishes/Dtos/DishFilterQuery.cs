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
        public string? SortByPrice { get; set; } = "asc";
        public bool OnlyActive { get; set; } = true;
    }
}

