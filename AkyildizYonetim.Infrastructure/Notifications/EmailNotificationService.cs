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
            var subject = "Gecikmiş Borç Hatırlatması";
            var body = GenerateOverdueDebtEmail(recipientName, overdueDebts);
            
            await _emailSender.SendEmailAsync(recipientEmail, subject, body);
            
            _logger.LogInformation("Gecikmiş borç hatırlatması gönderildi: {TenantId} -> {Email}", tenantId, recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gecikmiş borç hatırlatması gönderilemedi: {TenantId} -> {Email}", tenantId, recipientEmail);
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

    private static string GenerateOverdueDebtEmail(string recipientName, List<UtilityDebt> overdueDebts)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>Sayın {recipientName},</h2>");
        sb.AppendLine("<p>Aşağıdaki borçlarınızın ödeme tarihi geçmiştir:</p>");
        sb.AppendLine("<div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border: 1px solid #ffeaa7;'>");
        
        var totalAmount = 0m;
        foreach (var debt in overdueDebts)
        {
            sb.AppendLine($"<div style='border-bottom: 1px solid #ffeaa7; padding: 10px 0;'>");
            sb.AppendLine($"<strong>{debt.Description ?? $"{debt.Type} - {debt.PeriodYear}/{debt.PeriodMonth}"}</strong><br>");
            sb.AppendLine($"Tutar: {debt.Amount:C}<br>");
            sb.AppendLine($"Kalan: {debt.RemainingAmount:C}<br>");
            sb.AppendLine($"Vade: {debt.DueDate:dd.MM.yyyy}<br>");
            sb.AppendLine("</div>");
            totalAmount += debt.RemainingAmount;
        }
        
        sb.AppendLine($"<div style='margin-top: 15px; padding-top: 15px; border-top: 2px solid #ffeaa7;'>");
        sb.AppendLine($"<strong>Toplam Gecikmiş Tutar: {totalAmount:C}</strong>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("<p>Lütfen en kısa sürede ödemenizi yapınız.</p>");
        sb.AppendLine("<p>Teşekkür ederiz.</p>");
        sb.AppendLine("<p><em>Akyıldız Yönetim</em></p>");
        
        return sb.ToString();
    }
} 