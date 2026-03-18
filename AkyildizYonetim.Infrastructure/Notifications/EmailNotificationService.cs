using AkyildizYonetim.Application.Common.Interfaces;
using AkyildizYonetim.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AkyildizYonetim.Infrastructure.Notifications;

public class EmailNotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailSender emailSender,
        ILogger<EmailNotificationService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendPaymentConfirmationAsync(
        Guid paymentId,
        decimal amount,
        PaymentType type,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Ödeme Onayı - {type}";
            var body = GeneratePaymentConfirmationEmail(recipientName, amount, type, paymentId);
            
            await _emailSender.SendEmailAsync(recipientEmail, subject, body);
            
            _logger.LogInformation("Ödeme onay emaili gönderildi: {PaymentId} -> {Email}", paymentId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ödeme onay emaili gönderilemedi: {PaymentId} -> {Email}", paymentId, recipientEmail);
        }
    }

    public async Task SendDebtAllocationNotificationAsync(
        Guid tenantId,
        List<DebtAllocationInfo> allocations,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Borç Eşleştirme Bildirimi";
            var body = GenerateDebtAllocationEmail(recipientName, allocations);
            
            await _emailSender.SendEmailAsync(recipientEmail, subject, body);
            
            _logger.LogInformation("Borç eşleştirme emaili gönderildi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Borç eşleştirme emaili gönderilemedi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
    }

    public async Task SendAdvanceAccountUsageNotificationAsync(
        Guid tenantId,
        decimal amount,
        decimal newBalance,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Avans Hesabı Kullanım Bildirimi";
            var body = GenerateAdvanceAccountUsageEmail(recipientName, amount, newBalance);
            
            await _emailSender.SendEmailAsync(recipientEmail, subject, body);
            
            _logger.LogInformation("Avans kullanım emaili gönderildi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avans kullanım emaili gönderilemedi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
    }

    public async Task SendOverdueDebtReminderAsync(
        Guid tenantId,
        List<UtilityDebt> overdueDebts,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = "Gecikmiş Borç Hatırlatması – Akyıldız Yönetim";
            var body = GenerateOverdueDebtEmail(recipientName, overdueDebts);

            await _emailSender.SendEmailAsync(recipientEmail, subject, body);

            _logger.LogInformation("Gecikmiş borç hatırlatması gönderildi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gecikmiş borç hatırlatması gönderilemedi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
    }

    public async Task SendAnnouncementEmailAsync(
        string title,
        string message,
        string recipientEmail,
        string recipientName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subject = $"Duyuru: {title} – Akyıldız Yönetim";
            var body = GenerateAnnouncementEmail(recipientName, title, message);

            await _emailSender.SendEmailAsync(recipientEmail, subject, body);

            _logger.LogInformation("Duyuru e-postası gönderildi: {Email}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Duyuru e-postası gönderilemedi: {Email}", recipientEmail);
        }
    }

    private static string GeneratePaymentConfirmationEmail(string recipientName, decimal amount, PaymentType type, Guid paymentId)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>Sayın {recipientName},</h2>");
        sb.AppendLine("<p>Ödemeniz başarıyla alınmıştır.</p>");
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
        sb.AppendLine($"<strong>Ödeme Detayları:</strong><br>");
        sb.AppendLine($"• Tutar: {amount:C}<br>");
        sb.AppendLine($"• Tür: {type}<br>");
        sb.AppendLine($"• Ödeme No: {paymentId}<br>");
        sb.AppendLine($"• Tarih: {DateTime.UtcNow:dd.MM.yyyy HH:mm}<br>");
        sb.AppendLine("</div>");
        sb.AppendLine("<p>Teşekkür ederiz.</p>");
        sb.AppendLine("<p><em>Akyıldız Yönetim</em></p>");
        
        return sb.ToString();
    }

    private static string GenerateDebtAllocationEmail(string recipientName, List<DebtAllocationInfo> allocations)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>Sayın {recipientName},</h2>");
        sb.AppendLine("<p>Ödemeniz aşağıdaki borçlara eşleştirilmiştir:</p>");
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
        
        foreach (var allocation in allocations)
        {
            sb.AppendLine($"<div style='border-bottom: 1px solid #dee2e6; padding: 10px 0;'>");
            sb.AppendLine($"<strong>{allocation.Description}</strong><br>");
            sb.AppendLine($"Ödenen: {allocation.AllocatedAmount:C}<br>");
            sb.AppendLine($"Kalan: {allocation.RemainingAmount:C}<br>");
            sb.AppendLine("</div>");
        }
        
        sb.AppendLine("</div>");
        sb.AppendLine("<p>Teşekkür ederiz.</p>");
        sb.AppendLine("<p><em>Akyıldız Yönetim</em></p>");
        
        return sb.ToString();
    }

    private static string GenerateAdvanceAccountUsageEmail(string recipientName, decimal amount, decimal newBalance)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>Sayın {recipientName},</h2>");
        sb.AppendLine("<p>Avans hesabınızdan borç ödemesi yapılmıştır.</p>");
        sb.AppendLine("<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>");
        sb.AppendLine($"<strong>İşlem Detayları:</strong><br>");
        sb.AppendLine($"• Kullanılan Tutar: {amount:C}<br>");
        sb.AppendLine($"• Yeni Bakiye: {newBalance:C}<br>");
        sb.AppendLine($"• Tarih: {DateTime.UtcNow:dd.MM.yyyy HH:mm}<br>");
        sb.AppendLine("</div>");
        sb.AppendLine("<p>Teşekkür ederiz.</p>");
        sb.AppendLine("<p><em>Akyıldız Yönetim</em></p>");
        
        return sb.ToString();
    }

    private static string DebtTypeLabel(DebtType type) => type switch
    {
        DebtType.Aidat       => "Aidat",
        DebtType.Electricity => "Elektrik",
        DebtType.Water       => "Su",
        _                    => type.ToString()
    };

    private static string GenerateOverdueDebtEmail(string recipientName, List<UtilityDebt> overdueDebts)
    {
        var totalAmount = overdueDebts.Sum(d => d.RemainingAmount);
        var sb = new StringBuilder();

        sb.AppendLine(@"<!DOCTYPE html><html lang='tr'><head><meta charset='UTF-8'></head><body style='margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;'>");
        sb.AppendLine(@"<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6f9;padding:32px 0;'><tr><td align='center'>");
        sb.AppendLine(@"<table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>");

        // Header
        sb.AppendLine(@"<tr><td style='background:#0d3460;padding:28px 36px;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:22px;font-weight:bold;color:#ffffff;'>Akyıldız Yönetim</p>");
        sb.AppendLine(@"<p style='margin:6px 0 0;font-size:13px;color:#90afd4;'>Bina ve Kiracı Yönetim Sistemi</p>");
        sb.AppendLine("</td></tr>");

        // Orange warning bar
        sb.AppendLine(@"<tr><td style='background:#e65100;padding:14px 36px;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:15px;font-weight:bold;color:#ffffff;'>⚠️ Gecikmiş Borç Hatırlatması</p>");
        sb.AppendLine("</td></tr>");

        // Body
        sb.AppendLine(@"<tr><td style='padding:32px 36px;'>");
        sb.AppendLine($"<p style='margin:0 0 16px;font-size:15px;color:#333;'>Sayın <strong>{recipientName}</strong>,</p>");
        sb.AppendLine(@"<p style='margin:0 0 24px;font-size:14px;color:#555;line-height:1.6;'>Aşağıda listelenen borçlarınızın ödeme tarihi geçmiş olup henüz ödeme yapılmamıştır. Lütfen en kısa sürede gerekli ödemeyi gerçekleştiriniz.</p>");

        // Debt table
        sb.AppendLine(@"<table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;font-size:13px;'>");
        sb.AppendLine(@"<thead><tr style='background:#0d3460;color:#fff;'>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:left;border-radius:6px 0 0 0;'>Borç Türü</th>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:left;'>Dönem</th>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:left;'>Açıklama</th>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:right;'>Tutar</th>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:right;'>Kalan</th>");
        sb.AppendLine(@"<th style='padding:10px 12px;text-align:center;border-radius:0 6px 0 0;'>Vade</th>");
        sb.AppendLine("</tr></thead><tbody>");

        var rowAlt = false;
        foreach (var debt in overdueDebts)
        {
            var rowBg = rowAlt ? "#f9f9f9" : "#ffffff";
            var typeLabel = DebtTypeLabel(debt.Type);
            var period = $"{debt.PeriodMonth:D2}/{debt.PeriodYear}";
            var description = !string.IsNullOrWhiteSpace(debt.Description)
                ? debt.Description
                : (debt.InvoiceNumber != null ? $"Fatura No: {debt.InvoiceNumber}" : "-");
            sb.AppendLine($"<tr style='background:{rowBg};'>");
            sb.AppendLine($"<td style='padding:10px 12px;color:#333;font-weight:bold;'>{typeLabel}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;color:#555;'>{period}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;color:#555;'>{description}</td>");
            sb.AppendLine($"<td style='padding:10px 12px;text-align:right;color:#333;'>{debt.Amount:N2} ₺</td>");
            sb.AppendLine($"<td style='padding:10px 12px;text-align:right;color:#c0392b;font-weight:bold;'>{debt.RemainingAmount:N2} ₺</td>");
            sb.AppendLine($"<td style='padding:10px 12px;text-align:center;color:#e65100;'>{debt.DueDate:dd.MM.yyyy}</td>");
            sb.AppendLine("</tr>");
            rowAlt = !rowAlt;
        }

        // Total row
        sb.AppendLine($"<tr style='background:#fff3cd;'>");
        sb.AppendLine($"<td colspan='4' style='padding:12px;font-weight:bold;color:#333;text-align:right;'>Toplam Gecikmiş Tutar:</td>");
        sb.AppendLine($"<td colspan='2' style='padding:12px;font-weight:bold;color:#c0392b;font-size:15px;text-align:right;'>{totalAmount:N2} ₺</td>");
        sb.AppendLine("</tr>");

        sb.AppendLine("</tbody></table>");

        // Contact box
        sb.AppendLine(@"<div style='margin-top:28px;padding:18px 20px;background:#eaf2ff;border-left:4px solid #0d3460;border-radius:6px;'>");
        sb.AppendLine(@"<p style='margin:0 0 6px;font-size:13px;font-weight:bold;color:#0d3460;'>Herhangi bir sorunuz veya itirazınız mı var?</p>");
        sb.AppendLine(@"<p style='margin:0;font-size:13px;color:#555;'>Bize <a href='mailto:ensar@akyildizlojistik.com' style='color:#0d3460;font-weight:bold;'>ensar@akyildizlojistik.com</a> adresi üzerinden ulaşabilirsiniz.</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</td></tr>");

        // Footer
        sb.AppendLine(@"<tr><td style='background:#f4f6f9;padding:20px 36px;border-top:1px solid #e0e0e0;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:12px;color:#888;text-align:center;'>Bu e-posta Akyıldız Yönetim sistemi tarafından otomatik olarak gönderilmiştir.</p>");
        sb.AppendLine(@"<p style='margin:6px 0 0;font-size:12px;color:#888;text-align:center;'>© " + DateTime.UtcNow.Year + " Akyıldız Yönetim. Tüm hakları saklıdır.</p>");
        sb.AppendLine("</td></tr>");

        sb.AppendLine("</table></td></tr></table></body></html>");
        return sb.ToString();
    }

    private static string GenerateAnnouncementEmail(string recipientName, string title, string message)
    {
        var sb = new StringBuilder();

        sb.AppendLine(@"<!DOCTYPE html><html lang='tr'><head><meta charset='UTF-8'></head><body style='margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;'>");
        sb.AppendLine(@"<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6f9;padding:32px 0;'><tr><td align='center'>");
        sb.AppendLine(@"<table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);'>");

        // Header
        sb.AppendLine(@"<tr><td style='background:#0d3460;padding:28px 36px;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:22px;font-weight:bold;color:#ffffff;'>Akyıldız Yönetim</p>");
        sb.AppendLine(@"<p style='margin:6px 0 0;font-size:13px;color:#90afd4;'>Bina ve Kiracı Yönetim Sistemi</p>");
        sb.AppendLine("</td></tr>");

        // Blue announcement bar
        sb.AppendLine(@"<tr><td style='background:#1565c0;padding:14px 36px;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:15px;font-weight:bold;color:#ffffff;'>📢 Duyuru</p>");
        sb.AppendLine("</td></tr>");

        // Body
        sb.AppendLine(@"<tr><td style='padding:32px 36px;'>");
        sb.AppendLine($"<p style='margin:0 0 16px;font-size:15px;color:#333;'>Sayın <strong>{recipientName}</strong>,</p>");

        sb.AppendLine($"<div style='background:#f0f4ff;border-left:4px solid #1565c0;border-radius:6px;padding:18px 20px;margin-bottom:24px;'>");
        sb.AppendLine($"<p style='margin:0 0 10px;font-size:16px;font-weight:bold;color:#0d3460;'>{title}</p>");
        sb.AppendLine($"<p style='margin:0;font-size:14px;color:#444;line-height:1.7;'>{message.Replace("\n", "<br>")}</p>");
        sb.AppendLine("</div>");

        // Contact box
        sb.AppendLine(@"<div style='margin-top:16px;padding:18px 20px;background:#eaf2ff;border-left:4px solid #0d3460;border-radius:6px;'>");
        sb.AppendLine(@"<p style='margin:0 0 6px;font-size:13px;font-weight:bold;color:#0d3460;'>Herhangi bir sorunuz mu var?</p>");
        sb.AppendLine(@"<p style='margin:0;font-size:13px;color:#555;'>Bize <a href='mailto:ensar@akyildizlojistik.com' style='color:#0d3460;font-weight:bold;'>ensar@akyildizlojistik.com</a> adresi üzerinden ulaşabilirsiniz.</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</td></tr>");

        // Footer
        sb.AppendLine(@"<tr><td style='background:#f4f6f9;padding:20px 36px;border-top:1px solid #e0e0e0;'>");
        sb.AppendLine(@"<p style='margin:0;font-size:12px;color:#888;text-align:center;'>Bu e-posta Akyıldız Yönetim sistemi tarafından otomatik olarak gönderilmiştir.</p>");
        sb.AppendLine(@"<p style='margin:6px 0 0;font-size:12px;color:#888;text-align:center;'>© " + DateTime.UtcNow.Year + " Akyıldız Yönetim. Tüm hakları saklıdır.</p>");
        sb.AppendLine("</td></tr>");

        sb.AppendLine("</table></td></tr></table></body></html>");
        return sb.ToString();
    }
} 