import { apiClient } from './client'
import type { PlatformConfig } from '@/types'

export const platformApi = {
  getConfig: () => apiClient.get<PlatformConfig>('/api/platform/config').then((r) => r.data),
}
