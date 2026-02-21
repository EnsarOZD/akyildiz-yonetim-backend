# Flats API Dokümantasyonu

## Genel Bilgi
Flats modülü, apartman yönetim sisteminde daireleri, giriş katlarını ve otoparkları yönetmek için kullanılır. Bu modül sayesinde:
- Daireler eklenebilir, güncellenebilir ve silinebilir
- Daire tipleri (Floor, Entry, Parking) yönetilebilir
- Grup stratejileri ile bölünmüş katlar yönetilebilir
- Hisse hesaplamaları otomatik yapılır
- Mal sahibi ve kiracı ilişkileri yönetilebilir

## Base URL
```
https://your-api-domain.com/Flats
```

## Veri Modelleri

### FlatDto
```typescript
interface FlatDto {
  id: string;
  code: string;                    // "7", "3A", "GA", "OTOPARK"
  floorNumber?: number;             // OTOPARK için null
  section?: string;                 // "A" | "B" | null
  type: UnitType;
  groupKey?: string;               // "3", "G" veya null
  groupStrategy: GroupStrategy;
  isActive: boolean;
  isOccupied: boolean;
  ownerId?: string;
  tenantId?: string;
  unitArea: number;
  monthlyRent: number;
  description: string;
  effectiveShare?: number;          // Runtime'da hesaplanır
  createdAt: string;
  updatedAt?: string;
  // Legacy alanlar
  number: string;
  unitNumber: string;
  apartmentNumber: string;
}
```

### FlatSummaryDto
```typescript
interface FlatSummaryDto {
  id: string;
  code: string;
  floorNumber?: number;
  type: UnitType;
  isOccupied: boolean;
  isActive: boolean;
  unitArea: number;
  effectiveShare?: number;
  ownerName: string;
  tenantCompanyName?: string;
}
```

### CreateFlatDto
```typescript
interface CreateFlatDto {
  code: string;                     // Zorunlu
  type: UnitType;                   // Zorunlu
  floorNumber?: number;             // Floor için zorunlu, Parking için null
  section?: string;                 // A/B sadece split gruplarında
  groupKey?: string;               // "3"/"G" sadece split gruplarında
  groupStrategy: GroupStrategy;
  isActive: boolean;
  isOccupied: boolean;
  ownerId?: string;
  tenantId?: string;
  unitArea: number;
  monthlyRent: number;
  description: string;
  // Legacy alanlar
  number: string;
  unitNumber: string;
  apartmentNumber: string;
}
```

### UpdateFlatDto
```typescript
interface UpdateFlatDto extends CreateFlatDto {
  id: string;                       // Zorunlu
}
```

### UnitType Enum
```typescript
enum UnitType {
  Floor = 0,    // Normal kat
  Entry = 1,    // Giriş katı
  Parking = 2   // OTOPARK
}
```

### GroupStrategy Enum
```typescript
enum GroupStrategy {
  None = 0,
  SplitIfMultiple = 1  // Grup içinde dolu sayısına böl
}
```

## API Endpoints

### 1. Daireleri Listele
```http
GET /Flats
```

**Query Parameters:**
- `ownerId` (optional): Mal sahibi ID'si
- `tenantId` (optional): Kiracı ID'si
- `code` (optional): Daire kodu
- `floorNumber` (optional): Kat numarası
- `type` (optional): Daire tipi (0=Floor, 1=Entry, 2=Parking)
- `isOccupied` (optional): Dolu/boş durumu
- `isActive` (optional): Aktif durumu

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "code": "7",
      "floorNumber": 7,
      "section": null,
      "type": 0,
      "groupKey": null,
      "groupStrategy": 0,
      "isActive": true,
      "isOccupied": true,
      "ownerId": "guid",
      "tenantId": "guid",
      "unitArea": 120.50,
      "monthlyRent": 0,
      "description": "Normal daire",
      "effectiveShare": 1.0,
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": null,
      "number": "7",
      "unitNumber": "7",
      "apartmentNumber": "7"
    }
  ]
}
```

### 2. Tek Daire Getir
```http
GET /Flats/{id}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "code": "3A",
    "floorNumber": 3,
    "section": "A",
    "type": 0,
    "groupKey": "3",
    "groupStrategy": 1,
    "isActive": true,
    "isOccupied": true,
    "ownerId": "guid",
    "tenantId": "guid",
    "unitArea": 100.25,
    "monthlyRent": 0,
    "description": "Bölünmüş kat",
    "effectiveShare": 0.5,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null,
    "number": "3A",
    "unitNumber": "3A",
    "apartmentNumber": "3A"
  }
}
```

### 3. Yeni Daire Ekle
```http
POST /Flats
```

**Request Body:**
```json
{
  "code": "8",
  "type": 0,
  "floorNumber": 8,
  "section": null,
  "groupKey": null,
  "groupStrategy": 0,
  "isActive": true,
  "isOccupied": false,
  "ownerId": "guid",
  "tenantId": null,
  "unitArea": 110.75,
  "monthlyRent": 0,
  "description": "Yeni daire",
  "number": "8",
  "unitNumber": "8",
  "apartmentNumber": "8"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": "new-guid"
}
```

### 4. Daire Güncelle
```http
PUT /Flats/{id}
```

**Request Body:**
```json
{
  "id": "guid",
  "code": "8",
  "type": 0,
  "floorNumber": 8,
  "section": null,
  "groupKey": null,
  "groupStrategy": 0,
  "isActive": true,
  "isOccupied": true,
  "ownerId": "guid",
  "tenantId": "guid",
  "unitArea": 110.75,
  "monthlyRent": 0,
  "description": "Güncellenmiş daire",
  "number": "8",
  "unitNumber": "8",
  "apartmentNumber": "8"
}
```

**Response:**
```json
{
  "isSuccess": true
}
```

### 5. Daire Sil
```http
DELETE /Flats/{id}
```

**Response:**
```json
{
  "isSuccess": true
}
```

## Özel Kullanım Senaryoları

### Bölünmüş Katlar (Split Flats)
Bölünmüş katlar için özel yapılandırma:

```json
{
  "code": "3A",
  "type": 0,
  "floorNumber": 3,
  "section": "A",
  "groupKey": "3",
  "groupStrategy": 1,
  "unitArea": 100.25
}
```

**Açıklama:**
- `groupKey`: "3" - Aynı gruptaki daireler için ortak anahtar
- `section`: "A" - Grup içindeki bölüm
- `groupStrategy`: 1 - Grup içinde dolu sayısına göre hisse hesapla

### Otopark Daireleri
Otopark daireleri için özel yapılandırma:

```json
{
  "code": "OTOPARK",
  "type": 2,
  "floorNumber": null,
  "section": null,
  "groupKey": null,
  "groupStrategy": 0,
  "unitArea": 0,
  "description": "Otopark"
}
```

**Açıklama:**
- `type`: 2 (Parking)
- `floorNumber`: null (otopark için kat numarası yok)
- `section`: null
- `groupKey`: null

### Giriş Katı
Giriş katı için yapılandırma:

```json
{
  "code": "G",
  "type": 1,
  "floorNumber": 0,
  "section": null,
  "groupKey": "G",
  "groupStrategy": 1,
  "unitArea": 200.50,
  "description": "Giriş katı"
}
```

## Hata Yönetimi

### Hata Response Formatı
```json
{
  "isSuccess": false,
  "errorMessage": "Bu Code ile kayıt zaten mevcut.",
  "errors": ["Detaylı hata mesajları"]
}
```

### Yaygın Hata Kodları
- **400 Bad Request**: Geçersiz veri veya validasyon hatası
- **404 Not Found**: Daire bulunamadı
- **500 Internal Server Error**: Sunucu hatası

### Validasyon Kuralları
- `code`: Zorunlu, benzersiz olmalı
- `type`: Zorunlu, 0-2 arası
- `floorNumber`: Floor/Entry için zorunlu, Parking için null
- `section`: Split gruplarında zorunlu
- `groupKey`: Split gruplarında zorunlu
- `unitArea`: Zorunlu, 0'dan büyük
- `monthlyRent`: Opsiyonel, 0'dan büyük veya eşit

## Frontend Kullanım Örnekleri

### React/TypeScript Örneği
```typescript
// Daireleri getir
const getFlats = async (filters?: {
  ownerId?: string;
  tenantId?: string;
  code?: string;
  floorNumber?: number;
  type?: UnitType;
  isOccupied?: boolean;
  isActive?: boolean;
}) => {
  const params = new URLSearchParams();
  if (filters?.ownerId) params.append('ownerId', filters.ownerId);
  if (filters?.tenantId) params.append('tenantId', filters.tenantId);
  if (filters?.code) params.append('code', filters.code);
  if (filters?.floorNumber) params.append('floorNumber', filters.floorNumber.toString());
  if (filters?.type !== undefined) params.append('type', filters.type.toString());
  if (filters?.isOccupied !== undefined) params.append('isOccupied', filters.isOccupied.toString());
  if (filters?.isActive !== undefined) params.append('isActive', filters.isActive.toString());

  const response = await fetch(`/Flats?${params}`);
  return await response.json();
};

// Yeni daire ekle
const createFlat = async (data: CreateFlatDto) => {
  const response = await fetch('/Flats', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(data),
  });
  return await response.json();
};

// Daire güncelle
const updateFlat = async (id: string, data: UpdateFlatDto) => {
  const response = await fetch(`/Flats/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ ...data, id }),
  });
  return await response.json();
};

// Daire sil
const deleteFlat = async (id: string) => {
  const response = await fetch(`/Flats/${id}`, {
    method: 'DELETE',
  });
  return await response.json();
};
```

### Vue.js Örneği
```javascript
// Composables/flats.js
export const useFlats = () => {
  const getFlats = async (filters = {}) => {
    const { data } = await $fetch('/Flats', {
      query: filters
    });
    return data;
  };

  const getFlatById = async (id) => {
    const { data } = await $fetch(`/Flats/${id}`);
    return data;
  };

  const createFlat = async (flat) => {
    const { data } = await $fetch('/Flats', {
      method: 'POST',
      body: flat
    });
    return data;
  };

  const updateFlat = async (id, flat) => {
    const { data } = await $fetch(`/Flats/${id}`, {
      method: 'PUT',
      body: { ...flat, id }
    });
    return data;
  };

  const deleteFlat = async (id) => {
    const { data } = await $fetch(`/Flats/${id}`, {
      method: 'DELETE'
    });
    return data;
  };

  return {
    getFlats,
    getFlatById,
    createFlat,
    updateFlat,
    deleteFlat
  };
};
```

## Önemli Notlar

1. **Code Benzersizliği**: Her daire için `code` alanı benzersiz olmalıdır.

2. **Hisse Hesaplama**: `effectiveShare` değeri runtime'da otomatik hesaplanır ve DB'ye yazılmaz.

3. **Grup Stratejileri**: 
   - `None`: Normal hisse hesaplama
   - `SplitIfMultiple`: Grup içinde dolu sayısına göre böl

4. **Daire Tipleri**:
   - `Floor`: Normal kat daireleri
   - `Entry`: Giriş katı
   - `Parking`: Otopark

5. **Soft Delete**: Daireler silindiğinde veritabanından tamamen silinmez, `IsDeleted` flag'i true yapılır.

6. **Legacy Desteği**: Eski sistem uyumluluğu için `number`, `unitNumber`, `apartmentNumber` alanları korunmuştur.

7. **Bölünmüş Katlar**: Aynı `groupKey`'e sahip daireler grup olarak işlenir ve hisse hesaplamaları birlikte yapılır.

8. **Otopark Özel Durumu**: Otopark daireleri için `floorNumber` null olabilir ve özel işlemler gerekebilir.

Bu dokümantasyon frontend geliştiricilerin Flats API'sini etkili bir şekilde kullanmasını sağlayacaktır.




