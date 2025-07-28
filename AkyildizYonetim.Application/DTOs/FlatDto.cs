namespace AkyildizYonetim.Application.DTOs;

using System;

public class FlatDto
{
    public Guid Id { get; set; }
    
    // Temel Bilgiler
    public string Number { get; set; } = string.Empty; // Daire numarası
    public string UnitNumber { get; set; } = string.Empty; // A-101, B-205 gibi ünite numarası
    public int Floor { get; set; } // Kat
    public decimal UnitArea { get; set; } // m² alan
    public int RoomCount { get; set; } // Oda sayısı
    public string ApartmentNumber { get; set; } = string.Empty; // Apartment numarası
    
    // İlişkiler
    public Guid OwnerId { get; set; } // Mal sahibi ID
    public Guid? TenantId { get; set; } // Kiracı ID (opsiyonel)
    
    // Durum Bilgileri
    public bool IsActive { get; set; } = true; // Aktif/Pasif
    public bool IsOccupied { get; set; } = false; // Dolu/Boş durumu
    
    // Kategori ve Paylaşım
    public string Category { get; set; } = "Normal"; // Normal, OrtakAlan, Mescit, Otopark
    public int ShareCount { get; set; } = 1; // Ortak paylaşım hissesi
    
    // İş Hanı Özel Alanları
    public string BusinessType { get; set; } = string.Empty; // İş türü (Ticaret, Hizmet, Üretim)
    public decimal MonthlyRent { get; set; } = 0; // Aylık kira
    public string Description { get; set; } = string.Empty; // Açıklama
    
    // Sistem Alanları
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties (İlişkili veriler)
    public OwnerDto? Owner { get; set; } // Mal sahibi bilgileri
    public TenantDto? Tenant { get; set; } // Kiracı bilgileri (varsa)
}

// Kısa Flat bilgileri için (liste görünümünde kullanılacak)
public class FlatSummaryDto
{
    public Guid Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal UnitArea { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public string OwnerName { get; set; } = string.Empty; // Mal sahibi adı
    public string? TenantCompanyName { get; set; } // Kiracı şirket adı (varsa)
}

// Flat oluşturma için
public class CreateFlatDto
{
    public string Number { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal UnitArea { get; set; }
    public int RoomCount { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string Category { get; set; } = "Normal";
    public int ShareCount { get; set; } = 1;
    public string BusinessType { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
}

// Flat güncelleme için
public class UpdateFlatDto
{
    public string Number { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal UnitArea { get; set; }
    public int RoomCount { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string Category { get; set; } = "Normal";
    public int ShareCount { get; set; } = 1;
    public string BusinessType { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsOccupied { get; set; } = false;
} 