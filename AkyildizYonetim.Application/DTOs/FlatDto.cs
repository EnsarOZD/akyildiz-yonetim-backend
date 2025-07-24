namespace AkyildizYonetim.Application.DTOs;

using System;

public class FlatDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
} 