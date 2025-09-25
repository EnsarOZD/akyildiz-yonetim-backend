# MeterReadings API Dökümantasyonu

## Genel Bilgiler
- **Base URL:** `/MeterReadings`
- **Authentication:** JWT Token gerekli
- **Content-Type:** `application/json`

---

## 1. Tüm Sayaç Okumalarını Getir

### Request
```http
GET /MeterReadings?flatId={guid}&type={int}&periodYear={int}&periodMonth={int}&startDate={datetime}&endDate={datetime}
```

### Query Parameters
| Parametre | Tip | Zorunlu | Açıklama |
|-----------|-----|---------|----------|
| `flatId` | Guid | Hayır | Daire ID'si |
| `type` | int | Hayır | Sayaç tipi (0=Elektrik, 1=Su) |
| `periodYear` | int | Hayır | Dönem yılı |
| `periodMonth` | int | Hayır | Dönem ayı |
| `startDate` | datetime | Hayır | Başlangıç tarihi |
| `endDate` | datetime | Hayır | Bitiş tarihi |

### Response
```json
[
  {
    "id": "b440c779-5a21-45d9-a580-0f4864f02ebd",
    "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
    "flatNumber": "",
    "type": 0,
    "periodYear": 2025,
    "periodMonth": 9,
    "readingValue": 2.00,
    "consumption": 1.00,
    "readingDate": "2025-09-12T00:00:00",
    "note": null,
    "createdAt": "2025-09-24T08:06:34.8122782",
    "updatedAt": "2025-09-24T08:06:34.7753432"
  }
]
```

---

## 2. Tek Sayaç Okuması Getir

### Request
```http
GET /MeterReadings/{id}
```

### Response
```json
{
  "id": "b440c779-5a21-45d9-a580-0f4864f02ebd",
  "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
  "flatNumber": "",
  "type": 0,
  "periodYear": 2025,
  "periodMonth": 9,
  "readingValue": 2.00,
  "consumption": 1.00,
  "readingDate": "2025-09-12T00:00:00",
  "note": null,
  "createdAt": "2025-09-24T08:06:34.8122782",
  "updatedAt": "2025-09-24T08:06:34.7753432"
}
```

---

## 3. Yeni Sayaç Okuması Oluştur

### Request
```http
POST /MeterReadings
```

### Request Body
```json
{
  "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
  "type": 0,
  "periodYear": 2025,
  "periodMonth": 9,
  "readingValue": 2.00,
  "consumption": 1.00,
  "readingDate": "2025-09-12T00:00:00.000Z",
  "note": "Opsiyonel not"
}
```

### Response
```json
{
  "id": "b440c779-5a21-45d9-a580-0f4864f02ebd"
}
```

---

## 4. Sayaç Okuması Güncelle

### Request
```http
PUT /MeterReadings/{id}
```

### Request Body
```json
{
  "id": "b440c779-5a21-45d9-a580-0f4864f02ebd",
  "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
  "type": 0,
  "periodYear": 2025,
  "periodMonth": 9,
  "readingValue": 2.00,
  "consumption": 1.00,
  "readingDate": "2025-09-12T00:00:00.000Z",
  "note": "Güncellenmiş not"
}
```

### Response
```http
200 OK
```

---

## 5. Sayaç Okuması Sil

### Request
```http
DELETE /MeterReadings/{id}
```

### Response
```http
200 OK
```

---

## 6. Son Okuma Getir

### Request
```http
GET /MeterReadings/last-readings/{flatId}/{type}
```

### Response
```json
{
  "id": "b440c779-5a21-45d9-a580-0f4864f02ebd",
  "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
  "flatNumber": "",
  "type": 0,
  "periodYear": 2025,
  "periodMonth": 9,
  "readingValue": 2.00,
  "consumption": 1.00,
  "readingDate": "2025-09-12T00:00:00",
  "note": null,
  "createdAt": "2025-09-24T08:06:34.8122782",
  "updatedAt": "2025-09-24T08:06:34.7753432"
}
```

**Not:** Eğer son okuma yoksa `204 No Content` döner.

---

## 7. Toplu Ekleme/Güncelleme (Bulk Upsert)

### Request
```http
POST /MeterReadings/bulk-upsert
```

### Request Body
```json
{
  "items": [
    {
      "id": null,
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "type": 0,
      "periodYear": 2025,
      "periodMonth": 9,
      "readingValue": 2.00,
      "consumption": 1.00,
      "readingDate": "2025-09-12T00:00:00.000Z",
      "note": null
    },
    {
      "id": "existing-id-here",
      "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
      "type": 0,
      "periodYear": 2025,
      "periodMonth": 9,
      "readingValue": 3.00,
      "consumption": 2.00,
      "readingDate": "2025-09-12T00:00:00.000Z",
      "note": "Güncellenen kayıt"
    }
  ]
}
```

### Response
```json
{
  "affected": 4
}
```

---

## 8. Ortak Tüketim Dağıtımı (Hesaplama)

### Request
```http
POST /MeterReadings/distribute-shared-consumption
```

### Request Body
```json
{
  "sharedAreaConsumption": 150.50,
  "mescitConsumption": 75.25,
  "periodYear": 2025,
  "periodMonth": 9,
  "sharedElectricityFlatIds": [
    "flat-id-1",
    "flat-id-2"
  ]
}
```

### Response
```json
[
  {
    "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
    "flatNumber": "5.KAT",
    "shareCount": 1,
    "distributedConsumption": 25.50
  },
  {
    "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
    "flatNumber": "2.KAT",
    "shareCount": 1,
    "distributedConsumption": 25.50
  }
]
```

---

## 9. Ortak Tüketim Uygulama

### Request
```http
POST /MeterReadings/apply-shared-consumption
```

### Request Body
```json
{
  "operationId": "shared-consumption-2025-09-001",
  "periodYear": 2025,
  "periodMonth": 9,
  "dueDate": "2025-10-15T00:00:00.000Z",
  "vatRate": 20,
  "btvRate": 5,
  "defaultUnitPrice": 2.50,
  "items": [
    {
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "shareCount": 1,
      "distributedConsumption": 25.50,
      "unitPrice": 2.50
    },
    {
      "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
      "shareCount": 1,
      "distributedConsumption": 25.50,
      "unitPrice": 2.50
    }
  ]
}
```

### Response
```json
{
  "operationId": "shared-consumption-2025-09-001",
  "createdMeterReadings": 2,
  "createdUtilityDebts": 2,
  "totalAmount": 160.65,
  "createdItems": [
    {
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "flatNumber": "5.KAT",
      "meterReadingId": "meter-reading-id-1",
      "utilityDebtId": "utility-debt-id-1",
      "consumption": 25.50,
      "unitPrice": 2.50,
      "amount": 80.325
    }
  ]
}
```

---

## 10. İstatistikler

### Request
```http
GET /MeterReadings/stats?year={int}&month={int}&type={int}
```

### Response
```json
{
  "totalReadings": 150,
  "totalConsumption": 2500.50,
  "averageConsumption": 16.67,
  "period": "2025-09"
}
```

---

## Veri Tipleri ve Validasyon

### MeterType Enum
- `0` = Elektrik
- `1` = Su

### Validasyon Kuralları
- `flatId`: Zorunlu, geçerli GUID olmalı
- `type`: Zorunlu, 0 veya 1 olmalı
- `periodYear`: Zorunlu, 2000-2100 arası
- `periodMonth`: Zorunlu, 1-12 arası
- `readingValue`: Zorunlu, 0'dan büyük veya eşit
- `consumption`: Opsiyonel, 0'dan büyük veya eşit
- `readingDate`: Zorunlu, geçerli tarih
- `note`: Opsiyonel, maksimum 500 karakter

### Hata Kodları
- `400 Bad Request`: Validasyon hatası
- `404 Not Found`: Kayıt bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

## Frontend Kullanım Örnekleri

### JavaScript Service Örneği
```javascript
// Toplu ekleme
const items = [
  {
    id: null, // Yeni kayıt için null
    flatId: "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
    type: 0,
    periodYear: 2025,
    periodMonth: 9,
    readingValue: 2.00,
    consumption: 1.00,
    readingDate: "2025-09-12T00:00:00.000Z",
    note: null
  }
];

const response = await fetch('/MeterReadings/bulk-upsert', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ items })
});

const result = await response.json();
console.log(`Etkilenen kayıt sayısı: ${result.affected}`);
```

### Ortak Tüketim Dağıtımı Örneği
```javascript
// 1. Ortak tüketim dağıtımını hesapla
const distributeRequest = {
  sharedAreaConsumption: 150.50,
  mescitConsumption: 75.25,
  periodYear: 2025,
  periodMonth: 9,
  sharedElectricityFlatIds: ["flat-id-1", "flat-id-2"]
};

const distributeResponse = await fetch('/MeterReadings/distribute-shared-consumption', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify(distributeRequest)
});

const distributionResults = await distributeResponse.json();
console.log('Dağıtım sonuçları:', distributionResults);

// 2. Dağıtımı uygula
const applyRequest = {
  operationId: `shared-consumption-${Date.now()}`,
  periodYear: 2025,
  periodMonth: 9,
  dueDate: "2025-10-15T00:00:00.000Z",
  vatRate: 20,
  btvRate: 5,
  defaultUnitPrice: 2.50,
  items: distributionResults.map(item => ({
    flatId: item.flatId,
    shareCount: item.shareCount,
    distributedConsumption: item.distributedConsumption,
    unitPrice: 2.50
  }))
};

const applyResponse = await fetch('/MeterReadings/apply-shared-consumption', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify(applyRequest)
});

const applyResult = await applyResponse.json();
console.log('Uygulama sonucu:', applyResult);
```

### Axios ile Kullanım
```javascript
import axios from 'axios';

// Tüm sayaç okumalarını getir
const getMeterReadings = async (params = {}) => {
  try {
    const response = await axios.get('/MeterReadings', { params });
    return response.data;
  } catch (error) {
    console.error('Sayaç okumaları alınırken hata:', error);
    throw error;
  }
};

// Toplu ekleme/güncelleme
const bulkUpsertMeterReadings = async (items) => {
  try {
    const response = await axios.post('/MeterReadings/bulk-upsert', { items });
    return response.data;
  } catch (error) {
    console.error('Toplu sayaç okuması işlenirken hata:', error);
    throw error;
  }
};

// Ortak tüketim dağıtımı hesaplama
const distributeSharedConsumption = async (request) => {
  try {
    const response = await axios.post('/MeterReadings/distribute-shared-consumption', request);
    return response.data;
  } catch (error) {
    console.error('Ortak tüketim dağıtımı hesaplanırken hata:', error);
    throw error;
  }
};

// Ortak tüketim uygulama
const applySharedConsumption = async (request) => {
  try {
    const response = await axios.post('/MeterReadings/apply-shared-consumption', request);
    return response.data;
  } catch (error) {
    console.error('Ortak tüketim uygulanırken hata:', error);
    throw error;
  }
};
```

---

## Önemli Notlar

1. **Bulk Upsert Formatı**: `command` wrapper kullanmayın, doğrudan `items` array'i gönderin
2. **Consumption Hesaplama**: Eğer `consumption` değeri gönderilmezse, backend otomatik olarak önceki okuma ile farkını hesaplar
3. **Duplicate Kontrolü**: Aynı daire, tip, dönem kombinasyonu için sadece bir okuma olabilir
4. **Tarih Formatı**: ISO 8601 formatında gönderin (`2025-09-12T00:00:00.000Z`)
5. **Maksimum Kayıt**: Bulk upsert'te maksimum 100 kayıt işlenebilir

Bu dökümantasyon ile frontend'ten backend'e doğru veri gönderebilir ve yanıtları doğru şekilde işleyebilirsiniz.