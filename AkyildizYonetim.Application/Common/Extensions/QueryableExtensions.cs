using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AkyildizYonetim.Application.Common.Models;

namespace AkyildizYonetim.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Enforce standards
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var count = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, count, pageNumber, pageSize);
    }
}
