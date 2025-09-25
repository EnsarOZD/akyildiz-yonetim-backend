# Ortak Tüketim Dağıtımı - Frontend API Dökümantasyonu

## Genel Bakış

Bu dökümantasyon, ortak alan ve mescit elektrik tüketimini aktif kiracılara hisse oranında dağıtmak için frontend'te kullanılacak API endpoint'lerini açıklar.

## İş Akışı

```
1. Sistem Fiyatlarını Getir → 2. Ortak Tüketim Hesapla → 3. Sonuçları Kontrol Et → 4. Uygula ve Kaydet
```

---

## API Endpoint'leri

### 1. Sistem Fiyatlandırma Bilgilerini Getir

#### Request
```http
GET /MeterReadings/pricing/{year}/{month}/{type}
```

#### Parameters
- `year` (int): Dönem yılı (örn: 2025)
- `month` (int): Dönem ayı (1-12)
- `type` (int): Sayaç tipi (0=Elektrik, 1=Su)

#### Response
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

#### JavaScript Örneği
```javascript
// Sistem fiyatlarını getir
const getPricing = async (year, month, type = 0) => {
  try {
    const response = await fetch(`/MeterReadings/pricing/${year}/${month}/${type}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Fiyatlandırma bilgileri alınırken hata:', error);
    throw error;
  }
};

// Kullanım
const pricing = await getPricing(2025, 9, 0);
console.log('Birim fiyat:', pricing.unitPrice);
console.log('KDV oranı:', pricing.vatRate);
```

---

### 2. Ortak Tüketim Dağıtımını Hesapla

#### Request
```http
POST /MeterReadings/distribute-shared-consumption
```

#### Request Body
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

#### Response
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

#### JavaScript Örneği
```javascript
// Ortak tüketim dağıtımını hesapla
const distributeSharedConsumption = async (request) => {
  try {
    const response = await fetch('/MeterReadings/distribute-shared-consumption', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Dağıtım hesaplanırken hata:', error);
    throw error;
  }
};

// Kullanım
const distributionRequest = {
  sharedAreaConsumption: 150.50,
  mescitConsumption: 75.25,
  periodYear: 2025,
  periodMonth: 9,
  sharedElectricityFlatIds: ["flat-id-1", "flat-id-2"]
};

const results = await distributeSharedConsumption(distributionRequest);
console.log('Dağıtım sonuçları:', results);
```

---

### 3. Ortak Tüketimi Uygula ve Kaydet

#### Request
```http
POST /MeterReadings/apply-shared-consumption
```

#### Request Body
```json
{
  "operationId": "shared-consumption-2025-09-001",
  "periodYear": 2025,
  "periodMonth": 9,
  "dueDate": "2025-10-15T00:00:00.000Z",
  "vatRate": 0,
  "btvRate": 0,
  "defaultUnitPrice": 0,
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

#### Response
```json
{
  "operationId": "shared-consumption-2025-09-001",
  "createdMeterReadings": 2,
  "createdUtilityDebts": 2,
  "totalAmount": 160.65,
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
      "consumption": 25.50,
      "unitPrice": 2.50,
      "amount": 80.325
    }
  ]
}
```

#### JavaScript Örneği
```javascript
// Ortak tüketimi uygula
const applySharedConsumption = async (request) => {
  try {
    const response = await fetch('/MeterReadings/apply-shared-consumption', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Dağıtım uygulanırken hata:', error);
    throw error;
  }
};

// Kullanım
const applyRequest = {
  operationId: `shared-consumption-${Date.now()}`,
  periodYear: 2025,
  periodMonth: 9,
  dueDate: "2025-10-15T00:00:00.000Z",
  vatRate: 0, // 0 gönderilirse sistem varsayılanını kullanır
  btvRate: 0, // 0 gönderilirse sistem varsayılanını kullanır
  defaultUnitPrice: 0, // 0 gönderilirse sistem varsayılanını kullanır
  items: distributionResults.map(item => ({
    flatId: item.flatId,
    shareCount: item.shareCount,
    distributedConsumption: item.distributedConsumption,
    unitPrice: 2.50 // Özel birim fiyat (opsiyonel)
  }))
};

const result = await applySharedConsumption(applyRequest);
console.log('Uygulama sonucu:', result);
```

---

## Frontend Service Sınıfı

### meterReadingsService.js'e Eklenecek Fonksiyonlar

```javascript
// Sistem fiyatlandırma bilgilerini getir
async getPricing(year, month, type = 0) {
  try {
    const response = await api.get(`${BASE_URL}/pricing/${year}/${month}/${type}`);
    return response.data;
  } catch (error) {
    console.error('Fiyatlandırma bilgileri alınırken hata:', error);
    throw error;
  }
},

// Ortak tüketim dağıtımını hesapla
async distributeSharedConsumption(request) {
  try {
    const response = await api.post(`${BASE_URL}/distribute-shared-consumption`, request);
    return response.data;
  } catch (error) {
    console.error('Ortak tüketim dağıtımı hesaplanırken hata:', error);
    throw error;
  }
},

// Ortak tüketimi uygula
async applySharedConsumption(request) {
  try {
    const response = await api.post(`${BASE_URL}/apply-shared-consumption`, request);
    return response.data;
  } catch (error) {
    console.error('Ortak tüketim uygulanırken hata:', error);
    throw error;
  }
}
```

---

## Tam İş Akışı Örneği

```javascript
// Ortak tüketim dağıtımı tam iş akışı
const processSharedConsumption = async (formData) => {
  try {
    // 1. Sistem fiyatlarını getir
    const pricing = await meterReadingsService.getPricing(
      formData.periodYear, 
      formData.periodMonth, 
      0 // Elektrik
    );
    
    console.log('Sistem fiyatları:', pricing);
    
    // 2. Ortak tüketim dağıtımını hesapla
    const distributionRequest = {
      sharedAreaConsumption: formData.sharedAreaConsumption,
      mescitConsumption: formData.mescitConsumption,
      periodYear: formData.periodYear,
      periodMonth: formData.periodMonth,
      sharedElectricityFlatIds: formData.sharedElectricityFlatIds || null
    };
    
    const distributionResults = await meterReadingsService.distributeSharedConsumption(distributionRequest);
    console.log('Dağıtım sonuçları:', distributionResults);
    
    // 3. Uygulama isteği hazırla
    const applyRequest = {
      operationId: `shared-consumption-${formData.periodYear}-${formData.periodMonth}-${Date.now()}`,
      periodYear: formData.periodYear,
      periodMonth: formData.periodMonth,
      dueDate: formData.dueDate,
      vatRate: formData.customVatRate || 0, // 0 = sistem varsayılanını kullan
      btvRate: formData.customBtvRate || 0, // 0 = sistem varsayılanını kullan
      defaultUnitPrice: formData.customUnitPrice || 0, // 0 = sistem varsayılanını kullan
      items: distributionResults.map(item => ({
        flatId: item.flatId,
        shareCount: item.shareCount,
        distributedConsumption: item.distributedConsumption,
        unitPrice: formData.customUnitPrice || null // Özel birim fiyat (opsiyonel)
      }))
    };
    
    // 4. Uygula ve kaydet
    const result = await meterReadingsService.applySharedConsumption(applyRequest);
    console.log('Uygulama sonucu:', result);
    
    return result;
    
  } catch (error) {
    console.error('Ortak tüketim işlemi sırasında hata:', error);
    throw error;
  }
};
```

---

## Önemli Notlar

### 1. Fiyatlandırma Mantığı
- **0 değeri gönderilirse:** Sistem varsayılan fiyatını kullanır
- **Pozitif değer gönderilirse:** Özel fiyat olarak kullanılır
- **Sistem fiyatları:** Veritabanından dinamik olarak alınır

### 2. Hata Yönetimi
```javascript
try {
  const result = await processSharedConsumption(formData);
  // Başarılı işlem
} catch (error) {
  if (error.response?.status === 400) {
    // Validasyon hatası
    console.error('Geçersiz veri:', error.response.data);
  } else if (error.response?.status === 409) {
    // Duplicate işlem hatası
    console.error('Bu işlem daha önce uygulanmış');
  } else {
    // Genel hata
    console.error('Beklenmeyen hata:', error.message);
  }
}
```

### 3. Loading States
```javascript
const [loading, setLoading] = useState({
  pricing: false,
  distribution: false,
  application: false
});

// Kullanım
setLoading(prev => ({ ...prev, pricing: true }));
const pricing = await getPricing(year, month, type);
setLoading(prev => ({ ...prev, pricing: false }));
```

### 4. Form Validasyonu
```javascript
const validateForm = (formData) => {
  const errors = {};
  
  if (!formData.periodYear || formData.periodYear < 2000) {
    errors.periodYear = 'Geçerli bir yıl seçiniz';
  }
  
  if (!formData.periodMonth || formData.periodMonth < 1 || formData.periodMonth > 12) {
    errors.periodMonth = 'Geçerli bir ay seçiniz';
  }
  
  if (formData.sharedAreaConsumption < 0) {
    errors.sharedAreaConsumption = 'Ortak alan tüketimi 0\'dan küçük olamaz';
  }
  
  if (formData.mescitConsumption < 0) {
    errors.mescitConsumption = 'Mescit tüketimi 0\'dan küçük olamaz';
  }
  
  return errors;
};
```

Bu dökümantasyon ile frontend'te ortak tüketim dağıtımı özelliğini tam olarak uygulayabilirsiniz!
