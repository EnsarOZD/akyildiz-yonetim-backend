<div v-if="filteredTenants.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  <div v-for="tenant in filteredTenants" :key="tenant.id" 
       class="card bg-base-100 shadow-lg hover:shadow-2xl transition-shadow duration-300 transform hover:-translate-y-1">
    <div class="card-body">
      <div class="flex items-start justify-between">
        <div class="flex items-center gap-4">
          <div class="avatar placeholder">
            <div :class="getAvatarColor(tenant.companyName)" class="bg-neutral-focus text-neutral-content rounded-full w-14">
              <span class="text-xl font-bold">{{ getAvatarInitial(tenant.companyName) }}</span>
            </div>
          </div>
          <div>
            <h2 class="card-title text-base-content">{{ tenant.companyName }}</h2>
            <p class="text-sm text-base-content/70">
              İletişim: {{ tenant.contactPersonName }} ({{ tenant.contactPersonPhone }})
            </p>
            <p class="text-xs text-base-content/50">
              {{ formatDate(tenant.contractStartDate) }} - {{ tenant.contractEndDate ? formatDate(tenant.contractEndDate) : 'Süresiz' }}
            </p>
          </div>
        </div>
        <div class="dropdown dropdown-end">
          <label tabindex="0" class="btn btn-ghost btn-sm btn-circle">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" /></svg>
          </label>
          <ul tabindex="0" class="dropdown-content menu p-2 shadow bg-base-200 rounded-box w-40 z-10">
            <li><a @click="viewTenantDetail(tenant.id)">Detayları Gör</a></li>
            <li><a @click="startEdit(tenant)">Düzenle</a></li>
            <li v-if="!tenant.isActive"><a @click="activateTenant(tenant.id)">Aktif Et</a></li>
            <li v-else><a @click="deactivateTenant(tenant.id)">Pasif Et</a></li>
            <li><a @click="askDelete(tenant)" class="text-error">Sil</a></li>
          </ul>
        </div>
      </div>

      <div class="divider my-3"></div>
      
      <div class="flex justify-between items-center text-sm">
        <span :class="['badge font-semibold', tenant.isActive ? 'badge-success' : 'badge-ghost']">
          {{ tenant.isActive ? 'Aktif' : 'Pasif' }}
        </span>
        <div class="text-right">
          <div class="font-semibold text-base-content/80">Aylık Aidat</div>
          <div class="text-lg font-bold text-success">
            {{ formatCurrency(tenant.monthlyAidat) }}
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
<div v-else class="text-center py-16">
  <p class="text-xl text-base-content/60">Aramanızla eşleşen kiracı bulunamadı.</p>
</div>

const filteredTenants = computed(() => {
  let filtered = tenants.value

  if (statusFilter.value === "active") {
    filtered = filtered.filter(t => t.isActive)
  } else if (statusFilter.value === "passive") {
    filtered = filtered.filter(t => !t.isActive)
  }

  if (search.value) {
    const s = search.value.toLowerCase()
    filtered = filtered.filter(t =>
      (t.companyName || '').toLowerCase().includes(s) ||
      (t.contactPersonName || '').toLowerCase().includes(s) ||
      (t.contactPersonEmail || '').toLowerCase().includes(s)
    )
  }
  return filtered.sort((a,b) => a.companyName.localeCompare(b.companyName))
})

const getAvatarInitial = (name) => (name ? name.charAt(0).toUpperCase() : '?')
const getAvatarColor = (name) => {
  if (!name) return 'bg-gray-500'
  const colors = ['bg-primary', 'bg-secondary', 'bg-accent', 'bg-info', 'bg-success', 'bg-warning', 'bg-error']
  const charCodeSum = name.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)
  return colors[charCodeSum % colors.length]
} 