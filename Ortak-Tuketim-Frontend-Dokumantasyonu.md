# Ortak Tüketim Bölüştürmesi - Frontend API Dökümantasyonu

## Genel Bakış
Bu dökümantasyon, ortak alan ve mescit elektrik/su tüketimini aktif kiracılara hisse oranında dağıtmak için backend API endpoint'lerini açıklar.

## İş Akışı
```
1. Sistem Fiyatlarını Getir → 2. Ortak Tüketim Hesapla → 3. Sonuçları Kontrol Et → 4. Uygula ve Kaydet
```

---

## 1. Sistem Fiyatlandırma Bilgilerini Getir

### Endpoint
```http
GET /MeterReadings/pricing/{year}/{month}/{meterType}
```

### Parameters
- `year` (int): Dönem yılı (örn: 2025)
- `month` (int): Dönem ayı (1-12)
- `meterType` (int): Sayaç tipi (0=Elektrik, 1=Su)

### Response Format
```json
{
  "unitPrice": 2.50,
  "vatRate": 20.00,
  "btvRate": 5.00,
  "effectiveDate": "2025-01-01T00:00:00Z",
  "expiryDate": null,
  "description": "2025 yılı elektrik fiyatlandırması",
  "meterType": 0,
  "year": 2025,
  "month": 9
}
```

### JavaScript Örneği
```javascript
// Elektrik fiyatları için
const electricityPricing = await meterReadingsService.getPricing(2025, 9, 0);

// Su fiyatları için
const waterPricing = await meterReadingsService.getPricing(2025, 9, 1);
```

---

## 2. Ortak Tüketim Dağıtımını Hesapla

### Endpoint
```http
POST /MeterReadings/distribute-shared-consumption
```

### Request Body Format
```json
{
  "periodYear": 2025,
  "periodMonth": 9,
  "meterType": 0,
  "sharedAreaConsumption": 50.0,
  "mescitConsumption": 30.0,
  "sharedMeterFlatIds": null
}
```

### Parameters Açıklaması
- `periodYear` (int): Dönem yılı
- `periodMonth` (int): Dönem ayı
- `meterType` (int): Sayaç tipi (0=Elektrik, 1=Su)
- `sharedAreaConsumption` (decimal): Ortak alan tüketimi
- `mescitConsumption` (decimal): Mescit tüketimi
- `sharedMeterFlatIds` (array, optional): Ortak sayaçların FlatId'leri

### Response Format
```json
[
  {
    "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
    "flatNumber": "5.KAT",
    "shareCount": 1,
    "distributedConsumption": 25.0
  },
  {
    "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
    "flatNumber": "2.KAT",
    "shareCount": 1,
    "distributedConsumption": 25.0
  }
]
```

### JavaScript Örneği
```javascript
// Elektrik için dağıtım hesapla
const electricityDistribution = await meterReadingsService.distributeSharedConsumption({
  periodYear: 2025,
  periodMonth: 9,
  meterType: 0, // Elektrik
  sharedAreaConsumption: 50.0,
  mescitConsumption: 30.0
});

// Su için dağıtım hesapla
const waterDistribution = await meterReadingsService.distributeSharedConsumption({
  periodYear: 2025,
  periodMonth: 9,
  meterType: 1, // Su
  sharedAreaConsumption: 20.0,
  mescitConsumption: 10.0
});
```

---

## 3. Ortak Tüketimi Uygula ve Kaydet

### Endpoint
```http
POST /MeterReadings/apply-shared-consumption
```

### Request Body Format
```json
{
  "operationId": "shared-electricity-2025-9-1735045905000",
  "periodYear": 2025,
  "periodMonth": 9,
  "dueDate": "2025-10-15T00:00:00.000Z",
  "meterType": 0,
  "vatRate": 0,
  "btvRate": 0,
  "defaultUnitPrice": 0,
  "items": [
    {
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "shareCount": 1,
      "distributedConsumption": 25.0,
      "unitPrice": null
    },
    {
      "flatId": "cdb379d6-d9ea-40b6-a783-3eda443d43bd",
      "shareCount": 1,
      "distributedConsumption": 25.0,
      "unitPrice": null
    }
  ]
}
```

### Parameters Açıklaması
- `operationId` (string): Benzersiz işlem ID'si
- `periodYear` (int): Dönem yılı
- `periodMonth` (int): Dönem ayı
- `dueDate` (datetime): Vade tarihi
- `meterType` (int): Sayaç tipi (0=Elektrik, 1=Su)
- `vatRate` (decimal): KDV oranı (%)
- `btvRate` (decimal): BTV oranı (%)
- `defaultUnitPrice` (decimal): Birim fiyat
- `items` (array): Dağıtım sonuçları

### Response Format
```json
{
  "operationId": "shared-electricity-2025-9-1735045905000",
  "createdMeterReadings": 4,
  "createdUtilityDebts": 4,
  "totalAmount": 312.50,
  "pricingUsed": {
    "unitPrice": 2.50,
    "vatRate": 20.00,
    "btvRate": 5.00,
    "effectiveDate": "2025-01-01T00:00:00Z",
    "description": "2025 yılı elektrik fiyatlandırması"
  },
  "createdItems": [
    {
      "flatId": "2ac684f9-12bf-4fb0-83a5-d276e9ffd10c",
      "flatNumber": "5.KAT",
      "meterReadingId": "meter-reading-id-1",
      "utilityDebtId": "utility-debt-id-1",
      "consumption": 25.0,
      "unitPrice": 2.50,
      "amount": 78.125
    }
  ]
}
```

### JavaScript Örneği
```javascript
// Elektrik için uygula
const electricityResult = await meterReadingsService.applySharedConsumption({
  operationId: `shared-electricity-${year}-${month}-${Date.now()}`,
  periodYear: 2025,
  periodMonth: 9,
  dueDate: new Date(2025, 9, 15).toISOString(),
  meterType: 0, // Elektrik
  vatRate: electricityPricing.vatRate,
  btvRate: electricityPricing.btvRate,
  defaultUnitPrice: electricityPricing.unitPrice,
  items: electricityDistribution.map(result => ({
    flatId: result.flatId,
    shareCount: result.shareCount,
    distributedConsumption: result.distributedConsumption,
    unitPrice: null
  }))
});

// Su için uygula
const waterResult = await meterReadingsService.applySharedConsumption({
  operationId: `shared-water-${year}-${month}-${Date.now()}`,
  periodYear: 2025,
  periodMonth: 9,
  dueDate: new Date(2025, 9, 15).toISOString(),
  meterType: 1, // Su
  vatRate: waterPricing.vatRate,
  btvRate: waterPricing.btvRate,
  defaultUnitPrice: waterPricing.unitPrice,
  items: waterDistribution.map(result => ({
    flatId: result.flatId,
    shareCount: result.shareCount,
    distributedConsumption: result.distributedConsumption,
    unitPrice: null
  }))
});
```

---

## Service Fonksiyonları

### meterReadingsService.js
```javascript
export const meterReadingsService = {
  // Fiyatlandırma bilgilerini getir
  async getPricing(year, month, meterType) {
    try {
      const { data } = await api.get(`${BASE_URL}/pricing/${year}/${month}/${meterType}`)
      return data
    } catch (error) {
      console.error('Fiyatlandırma bilgileri alınırken hata:', error)
      throw error
    }
  },

  // Ortak tüketim dağıtımını hesapla
  async distributeSharedConsumption(requestData) {
    try {
      console.log('Distribute Service Request:', JSON.stringify(requestData, null, 2))
      const { data } = await api.post(`${BASE_URL}/distribute-shared-consumption`, requestData)
      return data
    } catch (error) {
      console.error('Ortak tüketim dağıtımı hesaplanırken hata:', error)
      throw error
    }
  },

  // Ortak tüketimi uygula
  async applySharedConsumption(requestData) {
    try {
      console.log('Apply Service Request:', JSON.stringify(requestData, null, 2))
      const { data } = await api.post(`${BASE_URL}/apply-shared-consumption`, requestData)
      return data
    } catch (error) {
      console.error('Ortak tüketim uygulanırken hata:', error)
      throw error
    }
  }
}
```

---

## Tam Örnek Kullanım

### Vue.js Component Örneği
```javascript
// Ortak tüketim modal'ında
const handleSharedConsumption = async () => {
  try {
    // 1. Fiyatlandırma bilgilerini al
    const electricityPricing = await meterReadingsService.getPricing(
      formData.periodYear, 
      formData.periodMonth, 
      0 // Elektrik
    )
    
    const waterPricing = await meterReadingsService.getPricing(
      formData.periodYear, 
      formData.periodMonth, 
      1 // Su
    )

    // 2. Elektrik dağıtımını hesapla
    if (formData.electricityConsumption > 0) {
      const electricityDistribution = await meterReadingsService.distributeSharedConsumption({
        periodYear: formData.periodYear,
        periodMonth: formData.periodMonth,
        meterType: 0, // Elektrik
        sharedAreaConsumption: formData.sharedAreaElectricity,
        mescitConsumption: formData.mescitElectricity
      })

      // 3. Elektrik dağıtımını uygula
      await meterReadingsService.applySharedConsumption({
        operationId: `shared-electricity-${formData.periodYear}-${formData.periodMonth}-${Date.now()}`,
        periodYear: formData.periodYear,
        periodMonth: formData.periodMonth,
        dueDate: new Date(formData.periodYear, formData.periodMonth, 15).toISOString(),
        meterType: 0, // Elektrik
        vatRate: electricityPricing.vatRate,
        btvRate: electricityPricing.btvRate,
        defaultUnitPrice: electricityPricing.unitPrice,
        items: electricityDistribution.map(result => ({
          flatId: result.flatId,
          shareCount: result.shareCount,
          distributedConsumption: result.distributedConsumption,
          unitPrice: null
        }))
      })
    }

    // 4. Su dağıtımını hesapla
    if (formData.waterConsumption > 0) {
      const waterDistribution = await meterReadingsService.distributeSharedConsumption({
        periodYear: formData.periodYear,
        periodMonth: formData.periodMonth,
        meterType: 1, // Su
        sharedAreaConsumption: formData.sharedAreaWater,
        mescitConsumption: formData.mescitWater
      })

      // 5. Su dağıtımını uygula
      await meterReadingsService.applySharedConsumption({
        operationId: `shared-water-${formData.periodYear}-${formData.periodMonth}-${Date.now()}`,
        periodYear: formData.periodYear,
        periodMonth: formData.periodMonth,
        dueDate: new Date(formData.periodYear, formData.periodMonth, 15).toISOString(),
        meterType: 1, // Su
        vatRate: waterPricing.vatRate,
        btvRate: waterPricing.btvRate,
        defaultUnitPrice: waterPricing.unitPrice,
        items: waterDistribution.map(result => ({
          flatId: result.flatId,
          shareCount: result.shareCount,
          distributedConsumption: result.distributedConsumption,
          unitPrice: null
        }))
      })
    }

    alert('Ortak tüketim başarıyla uygulandı!')
  } catch (error) {
    console.error('Ortak tüketim uygulanırken hata:', error)
    alert('Hata oluştu: ' + error.message)
  }
}
```

---

## Önemli Notlar

### 1. MeterType Değerleri
- `0` = Elektrik (Electricity)
- `1` = Su (Water)

### 2. OperationId Formatı
- Benzersiz olmalı
- Önerilen format: `shared-{meterType}-{year}-{month}-{timestamp}`
- Örnek: `shared-electricity-2025-9-1735045905000`

### 3. Hata Yönetimi
- Tüm endpoint'lerde try-catch kullanın
- Hata mesajlarını kullanıcıya gösterin
- Console'da detaylı log tutun

### 4. Validation
- Tüketim değerleri 0'dan büyük olmalı
- Dönem bilgileri geçerli olmalı
- En az bir daire seçilmiş olmalı

### 5. Performance
- Fiyatlandırma bilgilerini cache'leyin
- Büyük dağıtımlarda loading gösterin
- İşlem sonrası sayfayı yenileyin

---

## Test Senaryoları

### 1. Elektrik Dağıtımı
```javascript
// Test verisi
const testData = {
  periodYear: 2025,
  periodMonth: 9,
  meterType: 0,
  sharedAreaConsumption: 100.0,
  mescitConsumption: 50.0
}

// Test
const result = await meterReadingsService.distributeSharedConsumption(testData)
console.log('Elektrik dağıtım sonucu:', result)
```

### 2. Su Dağıtımı
```javascript
// Test verisi
const testData = {
  periodYear: 2025,
  periodMonth: 9,
  meterType: 1,
  sharedAreaConsumption: 30.0,
  mescitConsumption: 20.0
}

// Test
const result = await meterReadingsService.distributeSharedConsumption(testData)
console.log('Su dağıtım sonucu:', result)
```

### 3. Fiyatlandırma Testi
```javascript
// Elektrik fiyatları
const electricityPricing = await meterReadingsService.getPricing(2025, 9, 0)
console.log('Elektrik fiyatları:', electricityPricing)

// Su fiyatları
const waterPricing = await meterReadingsService.getPricing(2025, 9, 1)
console.log('Su fiyatları:', waterPricing)
```

---

Bu dökümantasyon ile frontend'de ortak tüketim bölüştürmesi özelliğini kolayca implement edebilirsiniz! 🚀
