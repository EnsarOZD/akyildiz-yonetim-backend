# MeterReadings Bulk-Upsert API Hata Çözüm Dokümantasyonu

## 🚨 Sorun Tanımı

Frontend'den `POST /MeterReadings/bulk-upsert` endpoint'ine gönderilen veri formatında hatalar var. API şu hataları döndürüyor:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "command": ["The command field is required."],
    "$.items[0].flatId": ["The JSON value could not be converted to System.Guid. Path: $.items[0].flatId | LineNumber: 0 | BytePositionInLine: 34."]
  }
}
```

## 🔍 Hata Analizi

### 1. **Command Field Hatası**
- Backend `command` field'ı bekliyor ama frontend göndermiyor
- **Çözüm**: Request body'yi doğru formatta gönder

### 2. **FlatId GUID Hatası**
- `flatId` değeri GUID formatında değil
- **Çözüm**: GUID string formatında gönder

## ✅ Doğru Veri Formatı

### **Backend Beklediği Format:**
```json
{
  "items": [
    {
      "id": null,
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "type": 0,
      "periodYear": 2024,
      "periodMonth": 9,
      "readingValue": 1250.50,
      "consumption": null,
      "readingDate": "2024-09-23T14:36:45.000Z",
      "note": "Elektrik okuması"
    }
  ]
}
```

### **Field Açıklamaları:**
- `id`: `null` (yeni kayıt) veya `string` (güncelleme)
- `flatId`: GUID string formatında
- `type`: `0` (Electricity) veya `1` (Water)
- `periodYear`: Sayısal yıl
- `periodMonth`: Sayısal ay (1-12)
- `readingValue`: Sayısal sayaç değeri
- `consumption`: `null` (otomatik hesapla) veya sayısal değer
- `readingDate`: ISO 8601 tarih formatı
- `note`: String veya `null`

## 🔧 Frontend Düzeltmeleri

### **1. Service Layer Düzeltmesi**

```javascript
// meterReadingsService.js
export const bulkUpsertMeterReadings = async (items) => {
  try {
    // ✅ Doğru format - wrapper olmadan direkt items gönder
    const requestBody = {
      items: items.map(item => ({
        id: item.id || null,
        flatId: item.flatId,  // GUID string olarak
        type: parseInt(item.type),
        periodYear: parseInt(item.periodYear),
        periodMonth: parseInt(item.periodMonth),
        readingValue: parseFloat(item.readingValue),
        consumption: item.consumption ? parseFloat(item.consumption) : null,
        readingDate: item.readingDate,
        note: item.note || null
      }))
    };

    // 🔍 Debug için console log
    console.log('BulkUpsert Request Body:', JSON.stringify(requestBody, null, 2));

    const response = await apiService.post('/MeterReadings/bulk-upsert', requestBody);
    return response;
  } catch (error) {
    console.error('Toplu sayaç okuması işlenirken hata:', error);
    throw error;
  }
};
```

### **2. Vue Component Düzeltmesi**

```javascript
// ElectricityModal.vue
const saveAll = async () => {
  try {
    // Form verilerini hazırla
    const items = formData.value.map(item => ({
      id: null,  // Yeni kayıt için null
      flatId: item.flatId,  // GUID string olarak
      type: 0,  // Electricity
      periodYear: parseInt(formData.value.periodYear),
      periodMonth: parseInt(formData.value.periodMonth),
      readingValue: parseFloat(item.readingValue),
      consumption: null,  // Otomatik hesapla
      readingDate: new Date().toISOString(),
      note: item.note || null
    }));

    // ✅ Doğru format ile gönder
    await meterReadingsService.bulkUpsertMeterReadings(items);
    
    // Başarı mesajı
    showSuccess('Sayaç okumaları başarıyla kaydedildi!');
    
  } catch (error) {
    console.error('Kaydetme hatası:', error);
    showError('Sayaç okumaları kaydedilemedi: ' + error.message);
  }
};
```

### **3. TypeScript Interface Tanımları**

```typescript
// types/meterReadings.ts
interface BulkUpsertMeterReadingItem {
  id?: string | null;           // null ise create, string ise update
  flatId: string;               // GUID string formatında
  type: number;                 // 0=Electricity, 1=Water
  periodYear: number;
  periodMonth: number;
  readingValue: number;
  consumption?: number | null;  // null ise otomatik hesaplanır
  readingDate: string;          // ISO 8601 format
  note?: string | null;
}

interface BulkUpsertRequest {
  items: BulkUpsertMeterReadingItem[];
}

// API Response
interface BulkUpsertResponse {
  isSuccess: boolean;
  data: number;  // Etkilenen satır sayısı
  errorMessage?: string;
  errors?: string[];
}
```

### **4. React Hook Örneği**

```javascript
// hooks/useMeterReadings.js
import { useState } from 'react';
import { meterReadingsService } from '../services/meterReadingsService';

export const useMeterReadings = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const bulkUpsertMeterReadings = async (items) => {
    setLoading(true);
    setError(null);
    
    try {
      // Veri formatını kontrol et
      const formattedItems = items.map(item => ({
        id: item.id || null,
        flatId: item.flatId,
        type: parseInt(item.type),
        periodYear: parseInt(item.periodYear),
        periodMonth: parseInt(item.periodMonth),
        readingValue: parseFloat(item.readingValue),
        consumption: item.consumption ? parseFloat(item.consumption) : null,
        readingDate: item.readingDate || new Date().toISOString(),
        note: item.note || null
      }));

      const response = await meterReadingsService.bulkUpsertMeterReadings(formattedItems);
      return response;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    bulkUpsertMeterReadings,
    loading,
    error
  };
};
```

## 🧪 Test Verisi Örneği

### **Başarılı Request Örneği:**
```json
{
  "items": [
    {
      "id": null,
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "type": 0,
      "periodYear": 2024,
      "periodMonth": 9,
      "readingValue": 1250.50,
      "consumption": null,
      "readingDate": "2024-09-23T14:36:45.000Z",
      "note": "Elektrik okuması"
    },
    {
      "id": null,
      "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
      "type": 0,
      "periodYear": 2024,
      "periodMonth": 9,
      "readingValue": 2100.75,
      "consumption": null,
      "readingDate": "2024-09-23T14:36:45.000Z",
      "note": "Elektrik okuması"
    }
  ]
}
```

### **Başarılı Response Örneği:**
```json
{
  "isSuccess": true,
  "data": 2
}
```

## 🔍 Debug Adımları

### **1. Request Body Kontrolü**
```javascript
// Console'da kontrol et
console.log('Request Body:', JSON.stringify(requestBody, null, 2));
```

### **2. Network Tab Kontrolü**
- Browser DevTools > Network tab
- `bulk-upsert` request'ini bul
- Request payload'ını kontrol et
- Response'u kontrol et

### **3. Backend Log Kontrolü**
Backend'de şu logları kontrol et:
- Request body formatı
- Validation hataları
- Exception detayları

## ⚠️ Yaygın Hatalar ve Çözümleri

### **1. GUID Format Hatası**
```javascript
// ❌ Yanlış
flatId: 123

// ✅ Doğru
flatId: "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c"
```

### **2. Veri Tipi Hatası**
```javascript
// ❌ Yanlış
readingValue: "1250.50"

// ✅ Doğru
readingValue: 1250.50
```

### **3. Tarih Format Hatası**
```javascript
// ❌ Yanlış
readingDate: "2024-09-23"

// ✅ Doğru
readingDate: "2024-09-23T14:36:45.000Z"
```

### **4. Wrapper Hatası**
```javascript
// ❌ Yanlış
{
  "command": {
    "items": [...]
  }
}

// ✅ Doğru
{
  "items": [...]
}
```

## 📋 Kontrol Listesi

- [ ] Request body'de `command` wrapper'ı yok
- [ ] `flatId` GUID string formatında
- [ ] Sayısal değerler doğru parse edilmiş
- [ ] Tarih ISO 8601 formatında
- [ ] `consumption` null veya sayısal
- [ ] `type` 0 veya 1
- [ ] `periodMonth` 1-12 arası
- [ ] `periodYear` geçerli yıl

## 🚀 Test Senaryoları

### **1. Başarılı Kayıt**
- Geçerli veri ile test et
- Response'da `isSuccess: true` kontrol et
- Etkilenen satır sayısını kontrol et

### **2. Validation Hatası**
- Geçersiz `flatId` ile test et
- Geçersiz `type` ile test et
- Eksik alanlar ile test et

### **3. Boş Liste**
- Boş `items` array ile test et
- Response'u kontrol et

Bu dokümantasyonu takip ederek bulk-upsert endpoint'i sorunsuz çalışacaktır.

