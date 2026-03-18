namespace AkyildizYonetim.Application.DTOs;

using System;
using AkyildizYonetim.Domain.Entities;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? BankName { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNumber { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? OwnerName { get; set; }
    public string? FlatInfo { get; set; }
    public int? PeriodYear { get; set; }
    public int? PeriodMonth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DebtType> DebtTypes { get; set; } = new();
}