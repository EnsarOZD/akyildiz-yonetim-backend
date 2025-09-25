// meterReadingsService.js - Ortak tüketim fonksiyonları ekle

// ... mevcut kod ...

// Ortak tüketim dağıtımı için yeni fonksiyonlar
export const meterReadingsService = {
  // ... mevcut fonksiyonlar ...

  // Fiyatlandırma bilgilerini getir
  async getPricing(year, month, type) {
    try {
      const { data } = await api.get(`${BASE_URL}/pricing/${year}/${month}/${type}`)
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
