<template>
  <div v-if="isVisible" class="modal-overlay" @click="closeModal">
    <div class="modal-content" @click.stop>
      <!-- Modal Header -->
      <div class="modal-header">
        <h2 class="modal-title">Ortak Tüketim Dağıtımı</h2>
        <button class="close-button" @click="closeModal">×</button>
      </div>

      <!-- Modal Body -->
      <div class="modal-body">
        <!-- Dönem Seçimi -->
        <div class="form-section">
          <label class="section-label">Dönem Seçimi</label>
          <div class="period-selectors">
            <select v-model="formData.periodYear" class="form-select">
              <option v-for="year in availableYears" :key="year" :value="year">{{ year }}</option>
            </select>
            <select v-model="formData.periodMonth" class="form-select">
              <option v-for="month in months" :key="month.value" :value="month.value">{{ month.name }}</option>
            </select>
          </div>
        </div>

        <!-- Tüketim Türü -->
        <div class="form-section">
          <label class="section-label">Ortak Tüketim Türü</label>
          <div class="radio-group">
            <label class="radio-option">
              <input type="radio" v-model="formData.consumptionType" value="electricity" />
              <span class="radio-icon">⚡</span>
              <span>Elektrik</span>
            </label>
            <label class="radio-option">
              <input type="radio" v-model="formData.consumptionType" value="water" />
              <span class="radio-icon">💧</span>
              <span>Su</span>
            </label>
            <label class="radio-option">
              <input type="radio" v-model="formData.consumptionType" value="both" />
              <span class="radio-icon">⚡💧</span>
              <span>Her İkisi</span>
            </label>
          </div>
        </div>

        <!-- Elektrik Tüketimi -->
        <div v-if="formData.consumptionType === 'electricity' || formData.consumptionType === 'both'" class="form-section">
          <div class="section-header">
            <span class="section-icon">⚡</span>
            <h3 class="section-title">Elektrik Tüketimi</h3>
          </div>
          <div class="input-group">
            <label class="input-label">Ortak Alan Tüketimi (kWh)</label>
            <input 
              v-model.number="formData.sharedAreaElectricity" 
              type="number" 
              step="0.1" 
              class="form-input"
              placeholder="0"
            />
          </div>
          <div class="input-group">
            <label class="input-label">Mescit Tüketimi (kWh)</label>
            <input 
              v-model.number="formData.mescitElectricity" 
              type="number" 
              step="0.1" 
              class="form-input"
              placeholder="0"
            />
          </div>
        </div>

        <!-- Su Tüketimi -->
        <div v-if="formData.consumptionType === 'water' || formData.consumptionType === 'both'" class="form-section">
          <div class="section-header">
            <span class="section-icon">💧</span>
            <h3 class="section-title">Su Tüketimi</h3>
          </div>
          <div class="input-group">
            <label class="input-label">Ortak Alan Tüketimi (m³)</label>
            <input 
              v-model.number="formData.sharedAreaWater" 
              type="number" 
              step="0.1" 
              class="form-input"
              placeholder="0"
            />
          </div>
          <div class="input-group">
            <label class="input-label">Mescit Tüketimi (m³)</label>
            <input 
              v-model.number="formData.mescitWater" 
              type="number" 
              step="0.1" 
              class="form-input"
              placeholder="0"
            />
          </div>
        </div>

        <!-- Fiyatlandırma Bilgileri -->
        <div v-if="pricingInfo" class="form-section">
          <div class="section-header">
            <span class="section-icon">💰</span>
            <h3 class="section-title">Fiyatlandırma Bilgileri</h3>
          </div>
          <div class="pricing-info">
            <div class="pricing-item">
              <span class="pricing-label">Birim Fiyat:</span>
              <span class="pricing-value">{{ pricingInfo.unitPrice }} TL</span>
            </div>
            <div class="pricing-item">
              <span class="pricing-label">KDV Oranı:</span>
              <span class="pricing-value">%{{ pricingInfo.vatRate }}</span>
            </div>
            <div class="pricing-item">
              <span class="pricing-label">BTV Oranı:</span>
              <span class="pricing-value">%{{ pricingInfo.btvRate }}</span>
            </div>
          </div>
        </div>

        <!-- Dağıtım Sonuçları -->
        <div v-if="distributionResults.length > 0" class="form-section">
          <div class="section-header">
            <span class="section-icon">📊</span>
            <h3 class="section-title">Dağıtım Sonuçları</h3>
          </div>
          <div class="results-table">
            <div class="table-header">
              <span>Daire</span>
              <span>Hisse</span>
              <span>Tüketim</span>
              <span>Tutar</span>
            </div>
            <div v-for="result in distributionResults" :key="result.flatId" class="table-row">
              <span>{{ result.flatNumber }}</span>
              <span>{{ result.shareCount }}</span>
              <span>{{ result.distributedConsumption.toFixed(2) }}</span>
              <span>{{ calculateAmount(result).toFixed(2) }} TL</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Footer -->
      <div class="modal-footer">
        <button class="btn btn-secondary" @click="closeModal">İptal</button>
        <button class="btn btn-primary" @click="calculateDistribution" :disabled="isLoading">
          {{ isLoading ? 'Hesaplanıyor...' : 'Hesapla' }}
        </button>
        <button 
          v-if="distributionResults.length > 0" 
          class="btn btn-success" 
          @click="applyDistribution" 
          :disabled="isApplying"
        >
          {{ isApplying ? 'Uygulanıyor...' : 'Uygula' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive, computed, watch } from 'vue'
import { meterReadingsService } from '@/services/meterReadingsService'

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
    // Form data
    const formData = reactive({
      periodYear: new Date().getFullYear(),
      periodMonth: new Date().getMonth() + 1,
      consumptionType: 'both',
      sharedAreaElectricity: 0,
      mescitElectricity: 0,
      sharedAreaWater: 0,
      mescitWater: 0
    })

    // State
    const isLoading = ref(false)
    const isApplying = ref(false)
    const pricingInfo = ref(null)
    const distributionResults = ref([])

    // Computed
    const availableYears = computed(() => {
      const currentYear = new Date().getFullYear()
      return Array.from({ length: 3 }, (_, i) => currentYear + i)
    })

    const months = computed(() => [
      { value: 1, name: 'Ocak' },
      { value: 2, name: 'Şubat' },
      { value: 3, name: 'Mart' },
      { value: 4, name: 'Nisan' },
      { value: 5, name: 'Mayıs' },
      { value: 6, name: 'Haziran' },
      { value: 7, name: 'Temmuz' },
      { value: 8, name: 'Ağustos' },
      { value: 9, name: 'Eylül' },
      { value: 10, name: 'Ekim' },
      { value: 11, name: 'Kasım' },
      { value: 12, name: 'Aralık' }
    ])

    // Methods
    const closeModal = () => {
      emit('close')
    }

    const loadPricingInfo = async () => {
      try {
        if (formData.consumptionType === 'electricity' || formData.consumptionType === 'both') {
          const electricityPricing = await meterReadingsService.getPricing(
            formData.periodYear, 
            formData.periodMonth, 
            0 // Electricity
          )
          pricingInfo.value = electricityPricing
        } else if (formData.consumptionType === 'water') {
          const waterPricing = await meterReadingsService.getPricing(
            formData.periodYear, 
            formData.periodMonth, 
            1 // Water
          )
          pricingInfo.value = waterPricing
        }
      } catch (error) {
        console.error('Fiyatlandırma bilgileri yüklenirken hata:', error)
      }
    }

    const calculateDistribution = async () => {
      isLoading.value = true
      try {
        // Elektrik ve su için ayrı ayrı hesaplama yap
        if (formData.consumptionType === 'electricity' || formData.consumptionType === 'both') {
          const electricityConsumption = (formData.sharedAreaElectricity || 0) + (formData.mescitElectricity || 0)
          if (electricityConsumption > 0) {
            const electricityResults = await meterReadingsService.distributeSharedConsumption({
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 0, // Electricity
              sharedAreaConsumption: formData.sharedAreaElectricity || 0,
              mescitConsumption: formData.mescitElectricity || 0
            })
            distributionResults.value = [...distributionResults.value, ...electricityResults]
          }
        }

        if (formData.consumptionType === 'water' || formData.consumptionType === 'both') {
          const waterConsumption = (formData.sharedAreaWater || 0) + (formData.mescitWater || 0)
          if (waterConsumption > 0) {
            const waterResults = await meterReadingsService.distributeSharedConsumption({
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 1, // Water
              sharedAreaConsumption: formData.sharedAreaWater || 0,
              mescitConsumption: formData.mescitWater || 0
            })
            distributionResults.value = [...distributionResults.value, ...waterResults]
          }
        }

        if (distributionResults.value.length === 0) {
          alert('Lütfen en az bir tüketim değeri girin.')
          return
        }
      } catch (error) {
        console.error('Dağıtım hesaplanırken hata:', error)
        alert('Dağıtım hesaplanırken hata oluştu.')
      } finally {
        isLoading.value = false
      }
    }

    const calculateAmount = (result) => {
      if (!pricingInfo.value) return 0
      
      const baseAmount = result.distributedConsumption * pricingInfo.value.unitPrice
      const vatAmount = baseAmount * (pricingInfo.value.vatRate / 100)
      const btvAmount = baseAmount * (pricingInfo.value.btvRate / 100)
      
      return baseAmount + vatAmount + btvAmount
    }

    const applyDistribution = async () => {
      isApplying.value = true
      try {
        // Elektrik ve su için ayrı ayrı uygulama yap
        if (formData.consumptionType === 'electricity' || formData.consumptionType === 'both') {
          const electricityConsumption = (formData.sharedAreaElectricity || 0) + (formData.mescitElectricity || 0)
          if (electricityConsumption > 0) {
            const electricityPricing = await meterReadingsService.getPricing(
              formData.periodYear, 
              formData.periodMonth, 
              0 // Electricity
            )
            
            const electricityResults = await meterReadingsService.distributeSharedConsumption({
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 0, // Electricity
              sharedAreaConsumption: formData.sharedAreaElectricity || 0,
              mescitConsumption: formData.mescitElectricity || 0
            })

            const electricityOperationId = `shared-electricity-${formData.periodYear}-${formData.periodMonth}-${Date.now()}`
            const electricityApplyData = {
              operationId: electricityOperationId,
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 0, // Electricity
              dueDate: new Date(formData.periodYear, formData.periodMonth, 15).toISOString(),
              vatRate: electricityPricing?.vatRate || 0,
              btvRate: electricityPricing?.btvRate || 0,
              defaultUnitPrice: electricityPricing?.unitPrice || 0,
              items: electricityResults.map(result => ({
                flatId: result.flatId,
                shareCount: result.shareCount,
                distributedConsumption: result.distributedConsumption,
                unitPrice: null
              }))
            }

            await meterReadingsService.applySharedConsumption(electricityApplyData)
          }
        }

        if (formData.consumptionType === 'water' || formData.consumptionType === 'both') {
          const waterConsumption = (formData.sharedAreaWater || 0) + (formData.mescitWater || 0)
          if (waterConsumption > 0) {
            const waterPricing = await meterReadingsService.getPricing(
              formData.periodYear, 
              formData.periodMonth, 
              1 // Water
            )
            
            const waterResults = await meterReadingsService.distributeSharedConsumption({
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 1, // Water
              sharedAreaConsumption: formData.sharedAreaWater || 0,
              mescitConsumption: formData.mescitWater || 0
            })

            const waterOperationId = `shared-water-${formData.periodYear}-${formData.periodMonth}-${Date.now()}`
            const waterApplyData = {
              operationId: waterOperationId,
              periodYear: formData.periodYear,
              periodMonth: formData.periodMonth,
              consumptionType: 1, // Water
              dueDate: new Date(formData.periodYear, formData.periodMonth, 15).toISOString(),
              vatRate: waterPricing?.vatRate || 0,
              btvRate: waterPricing?.btvRate || 0,
              defaultUnitPrice: waterPricing?.unitPrice || 0,
              items: waterResults.map(result => ({
                flatId: result.flatId,
                shareCount: result.shareCount,
                distributedConsumption: result.distributedConsumption,
                unitPrice: null
              }))
            }

            await meterReadingsService.applySharedConsumption(waterApplyData)
          }
        }
        
        alert('Başarıyla uygulandı!')
        emit('success')
        closeModal()
      } catch (error) {
        console.error('Dağıtım uygulanırken hata:', error)
        alert('Dağıtım uygulanırken hata oluştu.')
      } finally {
        isApplying.value = false
      }
    }

    // Watchers
    watch([() => formData.periodYear, () => formData.periodMonth, () => formData.consumptionType], () => {
      loadPricingInfo()
      distributionResults.value = []
    }, { immediate: true })

    return {
      formData,
      isLoading,
      isApplying,
      pricingInfo,
      distributionResults,
      availableYears,
      months,
      closeModal,
      calculateDistribution,
      calculateAmount,
      applyDistribution
    }
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: #1a1a1a;
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  color: white;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #333;
}

.modal-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
}

.close-button {
  background: none;
  border: none;
  color: white;
  font-size: 2rem;
  cursor: pointer;
  padding: 0;
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-body {
  padding: 20px;
}

.form-section {
  margin-bottom: 24px;
}

.section-label {
  display: block;
  font-weight: 600;
  margin-bottom: 8px;
  color: #e0e0e0;
}

.section-header {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.section-icon {
  font-size: 1.2rem;
  margin-right: 8px;
}

.section-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.period-selectors {
  display: flex;
  gap: 12px;
}

.form-select {
  flex: 1;
  padding: 8px 12px;
  background: #2a2a2a;
  border: 1px solid #444;
  border-radius: 6px;
  color: white;
  font-size: 14px;
}

.radio-group {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}

.radio-option {
  display: flex;
  align-items: center;
  cursor: pointer;
  padding: 8px 12px;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.radio-option:hover {
  background: #333;
}

.radio-option input[type="radio"] {
  margin-right: 8px;
}

.radio-icon {
  margin-right: 6px;
  font-size: 1.1rem;
}

.input-group {
  margin-bottom: 16px;
}

.input-label {
  display: block;
  font-weight: 500;
  margin-bottom: 6px;
  color: #e0e0e0;
}

.form-input {
  width: 100%;
  padding: 10px 12px;
  background: #2a2a2a;
  border: 1px solid #444;
  border-radius: 6px;
  color: white;
  font-size: 14px;
}

.form-input:focus {
  outline: none;
  border-color: #6366f1;
}

.pricing-info {
  background: #2a2a2a;
  padding: 16px;
  border-radius: 8px;
  border: 1px solid #444;
}

.pricing-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.pricing-item:last-child {
  margin-bottom: 0;
}

.pricing-label {
  color: #b0b0b0;
}

.pricing-value {
  font-weight: 600;
  color: #6366f1;
}

.results-table {
  background: #2a2a2a;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #444;
}

.table-header {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 1fr;
  background: #333;
  padding: 12px;
  font-weight: 600;
}

.table-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 1fr;
  padding: 12px;
  border-bottom: 1px solid #444;
}

.table-row:last-child {
  border-bottom: none;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #333;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background: #444;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #555;
}

.btn-primary {
  background: #6366f1;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #5856eb;
}

.btn-success {
  background: #10b981;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #059669;
}
</style>
