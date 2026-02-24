using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Application.Common.Models;
using AkyildizYonetim.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace AkyildizYonetim.Application.UtilityDebts.Commands.ImportUtilityDebts;

public record ImportUtilityDebtsFromExcelCommand : IRequest<Result<int>>
{
    public Stream ExcelStream { get; init; } = null!;
}

public class ImportUtilityDebtsFromExcelCommandHandler : IRequestHandler<ImportUtilityDebtsFromExcelCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public ImportUtilityDebtsFromExcelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(ImportUtilityDebtsFromExcelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = request.ExcelStream.Query(useHeaderRow: true).ToList();
            if (!rows.Any())
                return Result<int>.Failure("Excel dosyası boş veya başlıklar bulunamadı.");

            var tenants = await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync(cancellationToken);
            var flats = await _context.Flats.Where(f => !f.IsDeleted).ToListAsync(cancellationToken);
            
            int importedCount = 0;
            var newDebts = new List<UtilityDebt>();

            foreach (var row in rows)
            {
                var rowDict = (IDictionary<string, object>)row;

                // Sütun eşleştirme (Case-insensitive ve boşluk temizleme)
                string? firmaIsmi = GetValue(rowDict, "Firma");
                string? tarihRaw = GetValue(rowDict, "Tarih");
                string? turRaw = GetValue(rowDict, "Ödeme Türü");
                string? tutarRaw = GetValue(rowDict, "Tutar");
                string? aciklama = GetValue(rowDict, "Açıklama");

                if (string.IsNullOrEmpty(firmaIsmi) || string.IsNullOrEmpty(tutarRaw)) continue;

                // Kiracı Bulma
                var tenant = tenants.FirstOrDefault(t => 
                    string.Equals(t.CompanyName, firmaIsmi, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.ContactPersonName, firmaIsmi, StringComparison.OrdinalIgnoreCase));

                if (tenant == null) continue;

                // Daire Bulma (Kiracıya bağlı aktif daire)
                var flat = flats.FirstOrDefault(f => f.TenantId == tenant.Id) 
                           ?? flats.FirstOrDefault(f => f.OwnerId == tenant.Id);
                
                if (flat == null) continue;

                // Tarih Parçalama
                if (!DateTime.TryParse(tarihRaw, out DateTime tarih)) tarih = DateTime.UtcNow;

                // Tutar
                if (!decimal.TryParse(tutarRaw, out decimal tutar)) continue;

                // Borç Tipi Eşleştirme
                DebtType type = DebtType.Aidat;
                string finalDescription = aciklama ?? "";

                if (turRaw?.Contains("Elektrik", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Electricity;
                else if (turRaw?.Contains("Su", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Water;
                else if (turRaw?.Contains("Aidat", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Aidat;
                else
                {
                    // Özel tipler (Asbaş, Senet vb.) açıklamaya eklenir
                    finalDescription = $"[{turRaw}] {finalDescription}".Trim();
                }

                var debt = new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flat.Id,
                    TenantId = tenant.Id,
                    Type = type,
                    PeriodYear = tarih.Year,
                    PeriodMonth = tarih.Month,
                    Amount = tutar,
                    RemainingAmount = tutar,
                    Status = DebtStatus.Unpaid,
                    DueDate = tarih,
                    Description = finalDescription,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                newDebts.Add(debt);
                importedCount++;
            }

            if (newDebts.Any())
            {
                _context.UtilityDebts.AddRange(newDebts);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result<int>.Success(importedCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"İçe aktarma hatası: {ex.Message}");
        }
    }

    private string? GetValue(IDictionary<string, object> row, string key)
    {
        var match = row.Keys.FirstOrDefault(k => k.Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
        return match != null ? row[match]?.ToString()?.Trim() : null;
    }
}
