using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStatusQuery
    {
        Task<IReadOnlyList<StatusDto>> GetAllAsync(CancellationToken ct);
    }

}
