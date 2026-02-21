using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.CreateUtilityDebt.CreateAidatForPeriod;

public record CreateAidatForPeriodCommand(string Period, DateTime DueDate)
    : IRequest<(int tenantDuesCreated, int ownerDuesCreated)>;

public class CreateAidatForPeriodCommandHandler
    : IRequestHandler<CreateAidatForPeriodCommand, (int tenantDuesCreated, int ownerDuesCreated)>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public CreateAidatForPeriodCommandHandler(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<(int tenantDuesCreated, int ownerDuesCreated)> Handle(
        CreateAidatForPeriodCommand request, CancellationToken ct)
    {
        // 1) Period çöz (YYYY-MM)
        if (!TryParsePeriod(request.Period, out var year, out var month))
            return (0, 0);

        // 2) Aktif kiracılar ve daireleri
        var tenants = await _context.Tenants
            .AsNoTracking()
            .Include(t => t.Flats)
            .Where(t => t.IsActive && !t.IsDeleted)
            .ToListAsync(ct);

        // 3) İlgili yılın aidat tanımları
        var defs = await _context.AidatDefinitions
            .AsNoTracking()
            .Where(d => d.Year == year && !d.IsDeleted)
            .ToListAsync(ct);

        // TenantId bazlı gruplama (Unit eşleşmesi bazen tutmayabilir, sadece Tenant+Year yeterli olabilir)
        // Ancak iş kuralı gereği: Bir kiracının o yıl için tanımlanmış aidat tutarı bellidir.
        // Eğer birden fazla dairesi varsa ve ayrı ayrı tanımlanmışsa Unit de önemli, 
        // ama basitleştirmek adına TenantId ile eşleşen ilk tanımı alalım.
        var defByTenant = defs
            .GroupBy(d => d.TenantId)
            .ToDictionary(g => g.Key, g => g.First());

        int tenantCreated = 0;
        int ownerCreated  = 0;

        foreach (var tenant in tenants)
        {
            // Kiracının aidat tanımı var mı?
            if (!defByTenant.TryGetValue(tenant.Id, out var def))
                continue;

            foreach (var flat in tenant.Flats)
            {
                // Unit kontrolü (opsiyonel): Eğer def.Unit doluysa ve flat.Code ile uyuşmuyorsa atla denebilir.
                // Şimdilik esnek davranıp kiracıya tanımlı tutarı tüm dairelerine uyguluyoruz.

                // Aynı dönem/daire için zaten Aidat var mı?
                bool exists = await _context.UtilityDebts.AnyAsync(u =>
                        !u.IsDeleted &&
                        u.FlatId == flat.Id &&
                        u.Type == DebtType.Aidat &&
                        u.PeriodYear == year &&
                        u.PeriodMonth == month, ct);

                if (exists) continue;

                // KDV dahil tutar (VatIncludedAmount non-nullable olduğundan koşullu seç)
                var amount = def.VatIncludedAmount != 0m
                    ? def.VatIncludedAmount
                    : decimal.Round(def.Amount * 1.20m, 2, MidpointRounding.AwayFromZero);

                var cmd = new CreateUtilityDebtCommand
                {
                    FlatId          = flat.Id,
                    Type            = DebtType.Aidat,
                    PeriodYear      = year,
                    PeriodMonth     = month,
                    Amount          = amount,
                    Status          = DebtStatus.Unpaid,
                    PaidAmount      = 0,
                    RemainingAmount = amount,
                    DueDate         = request.DueDate,
                    Description     = $"Aidat {year}-{month:00}",
                    TenantId        = tenant.Id,
                    OwnerId         = null
                };

                // Result<Guid> içindeki bir başarı bayrağına bağlı kalmadan:
                await _mediator.Send(cmd, ct);
                tenantCreated++;
            }
        }

        // (İhtiyaç yoksa owner bloğunu bu şekilde boş bırak.)
        return (tenantCreated, ownerCreated);
    }

    private static bool TryParsePeriod(string period, out int year, out int month)
    {
        year = month = 0;
        if (string.IsNullOrWhiteSpace(period)) return false;
        var parts = period.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        return int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month) && month is >= 1 and <= 12;
    }
}
