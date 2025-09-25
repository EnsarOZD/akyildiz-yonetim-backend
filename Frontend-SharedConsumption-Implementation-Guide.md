# Frontend Ortak Tüketim Dağıtımı Uygulama Rehberi

## Genel Bakış

Bu rehber, ortak alan ve mescit elektrik tüketimini aktif kiracılara hisse oranında dağıtmak için frontend'te yapılması gerekenleri açıklar.

## İş Akışı

```
1. Ortak Tüketim Girişi → 2. Hesaplama → 3. Sonuçları Kontrol Et → 4. Uygulama Onayı → 5. Kaydet
```

---

## 1. Service Fonksiyonları (meterReadingsService.js)

### Eklenmesi Gereken Fonksiyonlar:

```javascript
// Ortak tüketim dağıtımı hesaplama
async distributeSharedConsumption(request) {
  try {
    const response = await api.post(`${BASE_URL}/distribute-shared-consumption`, request);
    return response.data;
  } catch (error) {
    console.error('Ortak tüketim dağıtımı hesaplanırken hata:', error);
    throw error;
  }
},

// Ortak tüketim uygulama
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

## 2. Ortak Tüketim Modal Komponenti

### SharedConsumptionModal.vue

```vue
<template>
  <div class="modal-overlay" v-if="isVisible" @click="closeModal">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h3>Ortak Tüketim Dağıtımı</h3>
        <button @click="closeModal" class="close-btn">&times;</button>
      </div>
      
      <div class="modal-body">
        <!-- 1. Giriş Aşaması -->
        <div v-if="step === 1" class="input-step">
          <div class="form-group">
            <label>Dönem Seçimi</label>
            <div class="period-selector">
              <select v-model="formData.periodYear">
                <option v-for="year in availableYears" :key="year" :value="year">
                  {{ year }}
                </option>
              </select>
              <select v-model="formData.periodMonth">
                <option v-for="month in months" :key="month.value" :value="month.value">
                  {{ month.label }}
                </option>
              </select>
            </div>
          </div>
          
          <div class="form-group">
            <label>Ortak Alan Tüketimi (kWh)</label>
            <input 
              type="number" 
              step="0.01" 
              v-model="formData.sharedAreaConsumption"
              placeholder="0.00"
            />
          </div>
          
          <div class="form-group">
            <label>Mescit Tüketimi (kWh)</label>
            <input 
              type="number" 
              step="0.01" 
              v-model="formData.mescitConsumption"
              placeholder="0.00"
            />
          </div>
          
          <div class="form-group">
            <label>Ortak Sayaçlar (Opsiyonel)</label>
            <select multiple v-model="formData.sharedElectricityFlatIds">
              <option v-for="flat in availableFlats" :key="flat.id" :value="flat.id">
                {{ flat.code }} - {{ flat.tenantCompanyName || 'Boş' }}
              </option>
            </select>
            <small>Birden fazla seçebilirsiniz. Seçilmezse manuel toplam kullanılır.</small>
          </div>
          
          <div class="form-actions">
            <button @click="calculateDistribution" class="btn btn-primary" :disabled="!canCalculate">
              Hesapla
            </button>
          </div>
        </div>
        
        <!-- 2. Sonuçları Kontrol Etme Aşaması -->
        <div v-if="step === 2" class="results-step">
          <div class="summary">
            <h4>Dağıtım Özeti</h4>
            <p><strong>Toplam Ortak Tüketim:</strong> {{ totalSharedConsumption }} kWh</p>
            <p><strong>Toplam Hisse:</strong> {{ totalShares }}</p>
            <p><strong>Birim Hisse Tüketimi:</strong> {{ perShareConsumption }} kWh</p>
          </div>
          
          <div class="results-table">
            <table>
              <thead>
                <tr>
                  <th>Daire</th>
                  <th>Kiracı</th>
                  <th>Hisse</th>
                  <th>Dağıtılan Tüketim (kWh)</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in distributionResults" :key="item.flatId">
                  <td>{{ item.flatNumber }}</td>
                  <td>{{ getTenantName(item.flatId) }}</td>
                  <td>{{ item.shareCount }}</td>
                  <td>{{ item.distributedConsumption.toFixed(2) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
          
          <div class="form-actions">
            <button @click="step = 1" class="btn btn-secondary">Geri</button>
            <button @click="step = 3" class="btn btn-primary">Devam Et</button>
          </div>
        </div>
        
        <!-- 3. Uygulama Onayı Aşaması -->
        <div v-if="step === 3" class="apply-step">
          <div class="form-group">
            <label>Vade Tarihi</label>
            <input type="date" v-model="applyData.dueDate" />
          </div>
          
          <div class="form-group">
            <label>KDV Oranı (%)</label>
            <input type="number" step="0.01" v-model="applyData.vatRate" />
          </div>
          
          <div class="form-group">
            <label>BTV Oranı (%)</label>
            <input type="number" step="0.01" v-model="applyData.btvRate" />
          </div>
          
          <div class="form-group">
            <label>Birim Fiyat (TL/kWh)</label>
            <input type="number" step="0.01" v-model="applyData.defaultUnitPrice" />
          </div>
          
          <div class="cost-summary">
            <h4>Maliyet Özeti</h4>
            <div v-for="item in distributionResults" :key="item.flatId" class="cost-item">
              <span>{{ item.flatNumber }}:</span>
              <span>{{ calculateItemCost(item) }} TL</span>
            </div>
            <div class="total-cost">
              <strong>Toplam: {{ calculateTotalCost() }} TL</strong>
            </div>
          </div>
          
          <div class="form-actions">
            <button @click="step = 2" class="btn btn-secondary">Geri</button>
            <button @click="applyDistribution" class="btn btn-success" :disabled="!canApply">
              Uygula ve Kaydet
            </button>
          </div>
        </div>
        
        <!-- 4. Başarı Aşaması -->
        <div v-if="step === 4" class="success-step">
          <div class="success-icon">✅</div>
          <h4>Ortak Tüketim Başarıyla Uygulandı!</h4>
          <div class="success-details">
            <p><strong>İşlem ID:</strong> {{ applyResult.operationId }}</p>
            <p><strong>Oluşturulan Sayaç Okumaları:</strong> {{ applyResult.createdMeterReadings }}</p>
            <p><strong>Oluşturulan Borç Kayıtları:</strong> {{ applyResult.createdUtilityDebts }}</p>
            <p><strong>Toplam Tutar:</strong> {{ applyResult.totalAmount }} TL</p>
          </div>
          <div class="form-actions">
            <button @click="closeModal" class="btn btn-primary">Tamam</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, watch } from 'vue'
import meterReadingsService from '@/services/meterReadingsService'
import flatsService from '@/services/flatsService'

export default {
  name: 'SharedConsumptionModal',
  props: {
    isVisible: {
      type: Boolean,
      default: false
    }
  },
  emits: ['close', 'success'],
  setup(props, { emit }) {
    const step = ref(1)
    const loading = ref(false)
    const distributionResults = ref([])
    const applyResult = ref(null)
    
    // Form verileri
    const formData = ref({
      periodYear: new Date().getFullYear(),
      periodMonth: new Date().getMonth() + 1,
      sharedAreaConsumption: 0,
      mescitConsumption: 0,
      sharedElectricityFlatIds: []
    })
    
    // Uygulama verileri
    const applyData = ref({
      dueDate: '',
      vatRate: 20,
      btvRate: 5,
      defaultUnitPrice: 2.50
    })
    
    // Yardımcı veriler
    const availableFlats = ref([])
    const months = [
      { value: 1, label: 'Ocak' },
      { value: 2, label: 'Şubat' },
      { value: 3, label: 'Mart' },
      { value: 4, label: 'Nisan' },
      { value: 5, label: 'Mayıs' },
      { value: 6, label: 'Haziran' },
      { value: 7, label: 'Temmuz' },
      { value: 8, label: 'Ağustos' },
      { value: 9, label: 'Eylül' },
      { value: 10, label: 'Ekim' },
      { value: 11, label: 'Kasım' },
      { value: 12, label: 'Aralık' }
    ]
    
    const availableYears = computed(() => {
      const currentYear = new Date().getFullYear()
      return Array.from({ length: 5 }, (_, i) => currentYear - 2 + i)
    })
    
    // Hesaplamalar
    const totalSharedConsumption = computed(() => {
      return formData.value.sharedAreaConsumption + formData.value.mescitConsumption
    })
    
    const totalShares = computed(() => {
      return distributionResults.value.reduce((sum, item) => sum + item.shareCount, 0)
    })
    
    const perShareConsumption = computed(() => {
      return totalShares.value > 0 ? totalSharedConsumption.value / totalShares.value : 0
    })
    
    // Validasyonlar
    const canCalculate = computed(() => {
      return formData.value.periodYear && 
             formData.value.periodMonth && 
             (formData.value.sharedAreaConsumption > 0 || formData.value.mescitConsumption > 0)
    })
    
    const canApply = computed(() => {
      return applyData.value.dueDate && 
             applyData.value.vatRate >= 0 && 
             applyData.value.btvRate >= 0 && 
             applyData.value.defaultUnitPrice > 0
    })
    
    // Fonksiyonlar
    const loadFlats = async () => {
      try {
        const flats = await flatsService.getFlats()
        availableFlats.value = flats.filter(flat => flat.isActive)
      } catch (error) {
        console.error('Daireler yüklenirken hata:', error)
      }
    }
    
    const calculateDistribution = async () => {
      loading.value = true
      try {
        const request = {
          sharedAreaConsumption: formData.value.sharedAreaConsumption,
          mescitConsumption: formData.value.mescitConsumption,
          periodYear: formData.value.periodYear,
          periodMonth: formData.value.periodMonth,
          sharedElectricityFlatIds: formData.value.sharedElectricityFlatIds.length > 0 
            ? formData.value.sharedElectricityFlatIds 
            : null
        }
        
        const results = await meterReadingsService.distributeSharedConsumption(request)
        distributionResults.value = results
        step.value = 2
      } catch (error) {
        console.error('Dağıtım hesaplanırken hata:', error)
        alert('Dağıtım hesaplanırken hata oluştu: ' + error.message)
      } finally {
        loading.value = false
      }
    }
    
    const calculateItemCost = (item) => {
      const baseAmount = item.distributedConsumption * applyData.value.defaultUnitPrice
      const vatAmount = baseAmount * (applyData.value.vatRate / 100)
      const btvAmount = baseAmount * (applyData.value.btvRate / 100)
      return (baseAmount + vatAmount + btvAmount).toFixed(2)
    }
    
    const calculateTotalCost = () => {
      return distributionResults.value.reduce((sum, item) => {
        return sum + parseFloat(calculateItemCost(item))
      }, 0).toFixed(2)
    }
    
    const applyDistribution = async () => {
      loading.value = true
      try {
        const request = {
          operationId: `shared-consumption-${formData.value.periodYear}-${formData.value.periodMonth}-${Date.now()}`,
          periodYear: formData.value.periodYear,
          periodMonth: formData.value.periodMonth,
          dueDate: applyData.value.dueDate,
          vatRate: applyData.value.vatRate,
          btvRate: applyData.value.btvRate,
          defaultUnitPrice: applyData.value.defaultUnitPrice,
          items: distributionResults.value.map(item => ({
            flatId: item.flatId,
            shareCount: item.shareCount,
            distributedConsumption: item.distributedConsumption,
            unitPrice: applyData.value.defaultUnitPrice
          }))
        }
        
        const result = await meterReadingsService.applySharedConsumption(request)
        applyResult.value = result
        step.value = 4
        
        // Parent component'e başarı bildirimi gönder
        emit('success', result)
      } catch (error) {
        console.error('Dağıtım uygulanırken hata:', error)
        alert('Dağıtım uygulanırken hata oluştu: ' + error.message)
      } finally {
        loading.value = false
      }
    }
    
    const getTenantName = (flatId) => {
      const flat = availableFlats.value.find(f => f.id === flatId)
      return flat?.tenantCompanyName || 'Boş'
    }
    
    const closeModal = () => {
      // Form verilerini sıfırla
      step.value = 1
      formData.value = {
        periodYear: new Date().getFullYear(),
        periodMonth: new Date().getMonth() + 1,
        sharedAreaConsumption: 0,
        mescitConsumption: 0,
        sharedElectricityFlatIds: []
      }
      applyData.value = {
        dueDate: '',
        vatRate: 20,
        btvRate: 5,
        defaultUnitPrice: 2.50
      }
      distributionResults.value = []
      applyResult.value = null
      
      emit('close')
    }
    
    // Component mount edildiğinde daireleri yükle
    watch(() => props.isVisible, (newVal) => {
      if (newVal) {
        loadFlats()
      }
    })
    
    return {
      step,
      loading,
      formData,
      applyData,
      distributionResults,
      applyResult,
      availableFlats,
      months,
      availableYears,
      totalSharedConsumption,
      totalShares,
      perShareConsumption,
      canCalculate,
      canApply,
      calculateDistribution,
      calculateItemCost,
      calculateTotalCost,
      applyDistribution,
      getTenantName,
      closeModal
    }
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #eee;
}

.modal-header h3 {
  margin: 0;
  color: #333;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #666;
}

.modal-body {
  padding: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #333;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.period-selector {
  display: flex;
  gap: 10px;
}

.period-selector select {
  flex: 1;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 30px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-success {
  background-color: #28a745;
  color: white;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.summary {
  background-color: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 20px;
}

.summary h4 {
  margin-top: 0;
  color: #333;
}

.results-table {
  overflow-x: auto;
}

.results-table table {
  width: 100%;
  border-collapse: collapse;
}

.results-table th,
.results-table td {
  padding: 10px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.results-table th {
  background-color: #f8f9fa;
  font-weight: 500;
}

.cost-summary {
  background-color: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
  margin-top: 20px;
}

.cost-summary h4 {
  margin-top: 0;
  color: #333;
}

.cost-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 5px;
}

.total-cost {
  border-top: 1px solid #ddd;
  padding-top: 10px;
  margin-top: 10px;
  font-size: 16px;
}

.success-step {
  text-align: center;
}

.success-icon {
  font-size: 48px;
  margin-bottom: 20px;
}

.success-details {
  background-color: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
  margin: 20px 0;
  text-align: left;
}

.success-details p {
  margin: 5px 0;
}
</style>
```

---

## 3. Ana Sayfaya Entegrasyon

### Dashboard.vue veya MeterReadings.vue'ye Ekleme:

```vue
<template>
  <!-- Mevcut içerik -->
  
  <!-- Ortak Tüketim Butonu -->
  <button @click="showSharedConsumptionModal = true" class="btn btn-primary">
    Ortak Tüketim Dağıtımı
  </button>
  
  <!-- Modal -->
  <SharedConsumptionModal 
    :isVisible="showSharedConsumptionModal"
    @close="showSharedConsumptionModal = false"
    @success="onSharedConsumptionSuccess"
  />
</template>

<script>
import SharedConsumptionModal from '@/components/SharedConsumptionModal.vue'

export default {
  components: {
    SharedConsumptionModal
  },
  setup() {
    const showSharedConsumptionModal = ref(false)
    
    const onSharedConsumptionSuccess = (result) => {
      console.log('Ortak tüketim başarıyla uygulandı:', result)
      // Gerekirse sayfa verilerini yenile
      // loadMeterReadings()
    }
    
    return {
      showSharedConsumptionModal,
      onSharedConsumptionSuccess
    }
  }
}
</script>
```

---

## 4. Test Senaryoları

### Test Edilmesi Gerekenler:

1. **Giriş Validasyonu:**
   - Boş alanlar kontrolü
   - Negatif değer kontrolü
   - Geçerli tarih kontrolü

2. **Hesaplama:**
   - Ortak alan + mescit tüketimi toplamı
   - Hisse oranında dağıtım
   - Yuvarlama hatalarının kontrolü

3. **Uygulama:**
   - Sayaç okumalarının oluşturulması
   - Borç kayıtlarının oluşturulması
   - Maliyet hesaplamaları (KDV/BTV dahil)

4. **Hata Durumları:**
   - Network hataları
   - Backend validasyon hataları
   - Duplicate işlem kontrolü

---

## 5. Kullanıcı Deneyimi İyileştirmeleri

### Öneriler:

1. **Loading States:** Her aşamada loading göstergesi
2. **Error Handling:** Kullanıcı dostu hata mesajları
3. **Confirmation:** Kritik işlemler için onay dialogları
4. **Auto-save:** Form verilerinin otomatik kaydedilmesi
5. **Keyboard Navigation:** Klavye ile modal navigasyonu

---

## 6. Responsive Tasarım

### Mobil Uyumluluk:

- Modal'ın mobil ekranlarda tam ekran olması
- Tablo scroll'u
- Touch-friendly buton boyutları
- Responsive form elemanları

Bu rehberi takip ederek ortak tüketim dağıtımı özelliğini frontend'te başarıyla uygulayabilirsiniz.
