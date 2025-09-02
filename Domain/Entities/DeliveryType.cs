using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DeliveryType
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        //Relacion de tablas
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
