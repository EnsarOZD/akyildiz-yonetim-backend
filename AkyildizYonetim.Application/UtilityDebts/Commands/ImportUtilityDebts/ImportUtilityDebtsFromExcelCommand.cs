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
                return Result<int>.Failure("Excel dosyası boş veya başlık satırı bulunamadı.");

            var tenants = await _context.Tenants.Where(t => !t.IsDeleted).ToListAsync(cancellationToken);
            var flats = await _context.Flats.Where(f => !f.IsDeleted).ToListAsync(cancellationToken);
            
            int importedCount = 0;
            var newDebts = new List<UtilityDebt>();
            var errors = new List<string>();
            int rowIndex = 1; // Başlık satırından sonraki ilk veri satırı

            foreach (var row in rows)
            {
                rowIndex++;
                var rowDict = (IDictionary<string, object>)row;

                string? firmaIsmi = GetValue(rowDict, "Firma");
                string? tarihRaw = GetValue(rowDict, "Tarih");
                string? turRaw = GetValue(rowDict, "Ödeme Türü");
                string? tutarRaw = GetValue(rowDict, "Tutar");
                string? aciklama = GetValue(rowDict, "Açıklama");

                if (string.IsNullOrEmpty(firmaIsmi) && string.IsNullOrEmpty(tutarRaw)) continue;

                if (string.IsNullOrEmpty(firmaIsmi))
                {
                    errors.Add($"Satır {rowIndex}: Firma ismi boş olamaz.");
                    continue;
                }

                if (string.IsNullOrEmpty(tutarRaw) || !decimal.TryParse(tutarRaw, out decimal tutar))
                {
                    errors.Add($"Satır {rowIndex}: Geçersiz tutar ({tutarRaw}).");
                    continue;
                }

                // Kiracı Bulma
                var tenant = tenants.FirstOrDefault(t => 
                    string.Equals(t.CompanyName, firmaIsmi, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.ContactPersonName, firmaIsmi, StringComparison.OrdinalIgnoreCase));

                if (tenant == null)
                {
                    errors.Add($"Satır {rowIndex}: '{firmaIsmi}' isimli kiracı bulunamadı.");
                    continue;
                }

                // Daire Bulma
                var flat = flats.FirstOrDefault(f => f.TenantId == tenant.Id) 
                           ?? flats.FirstOrDefault(f => f.OwnerId == tenant.Id);
                
                if (flat == null)
                {
                    errors.Add($"Satır {rowIndex}: Kiracıya or mülk sahibine bağlı aktif bir daire bulunamadı.");
                    continue;
                }

                // Tarih Parçalama
                DateTime tarih = ParseTurkishDate(tarihRaw);

                // Dönem Parçalama (Örn: 2025/03 veya 2025-03)
                int periodYear = tarih.Year;
                int periodMonth = tarih.Month;
                string? donemRaw = GetValue(rowDict, "Dönem");

                if (!string.IsNullOrEmpty(donemRaw))
                {
                    var parts = donemRaw.Split(new[] { '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && int.TryParse(parts[0], out int py) && int.TryParse(parts[1], out int pm))
                    {
                        periodYear = py;
                        periodMonth = pm;
                    }
                }

                // Borç Tipi Eşleştirme
                DebtType type = DebtType.Aidat;
                string finalDescription = aciklama ?? "";

                if (turRaw?.Contains("Elektrik", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Electricity;
                else if (turRaw?.Contains("Su", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Water;
                else if (turRaw?.Contains("Aidat", StringComparison.OrdinalIgnoreCase) == true) type = DebtType.Aidat;
                else if (!string.IsNullOrEmpty(turRaw))
                {
                    finalDescription = $"[{turRaw}] {finalDescription}".Trim();
                }

                // Son Ödeme Tarihi Parçalama
                DateTime dueDate = tarih;
                string? sonOdemeRaw = GetValue(rowDict, "Son Ödeme Tarihi");

                if (!string.IsNullOrEmpty(sonOdemeRaw))
                {
                    dueDate = ParseTurkishDate(sonOdemeRaw);
                }

                newDebts.Add(new UtilityDebt
                {
                    Id = Guid.NewGuid(),
                    FlatId = flat.Id,
                    TenantId = tenant.Id,
                    Type = type,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    Amount = tutar,
                    RemainingAmount = tutar,
                    Status = DebtStatus.Unpaid,
                    DueDate = dueDate,
                    Description = finalDescription,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                importedCount++;
            }

            if (errors.Any() && !newDebts.Any())
            {
                return Result<int>.Failure(string.Join(" | ", errors.Take(5)) + (errors.Count > 5 ? $" ve {errors.Count - 5} hata daha..." : ""));
            }

            if (newDebts.Any())
            {
                _context.UtilityDebts.AddRange(newDebts);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (errors.Any())
            {
                return Result<int>.Success(importedCount, $"{importedCount} borç başarıyla eklendi, ancak {errors.Count} satırda hata oluştu: " + string.Join(" | ", errors.Take(3)));
            }

            return Result<int>.Success(importedCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"İçe aktarma sırasında beklenmedik hata: {ex.Message}");
        }
    }

    private string? GetValue(IDictionary<string, object> row, string key)
    {
        var match = row.Keys.FirstOrDefault(k => k.Trim().Equals(key, StringComparison.OrdinalIgnoreCase));
        return match != null ? row[match]?.ToString()?.Trim() : null;
    }

    private DateTime ParseTurkishDate(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return DateTime.UtcNow;

        if (DateTime.TryParse(raw, out DateTime parsed)) return parsed;

        var parts = raw.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            string monthAbbrev = parts[0].ToLower().Trim();
            string yearRaw = parts[1].Trim();

            int month = monthAbbrev switch
            {
                "oca" => 1,
                "şub" => 2,
                "mar" => 3,
                "nis" => 4,
                "may" => 5,
                "haz" => 6,
                "tem" => 7,
                "ağu" => 8,
                "eyl" => 9,
                "ekı" => 10,
                "eki" => 10,
                "kas" => 11,
                "ara" => 12,
                _ => 0
            };

            if (month > 0 && int.TryParse(yearRaw, out int yearShort))
            {
                int year = yearShort < 100 ? 2000 + yearShort : yearShort;
                return new DateTime(year, month, 1);
            }
        }

        return DateTime.UtcNow;
    }
}
