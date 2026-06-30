import { apiClient } from './client'
import type { Driver } from '@/types'

export const followApi = {
  getFollowedDrivers: () =>
    apiClient.get<Driver[]>('/api/follow/drivers').then((r) => r.data),
  follow: (driverId: string) =>
    apiClient.post(`/api/drivers/${driverId}/follow`).then((r) => r.data),
  unfollow: (driverId: string) =>
    apiClient.delete(`/api/drivers/${driverId}/follow`).then((r) => r.data),
  getStatus: (driverId: string) =>
    apiClient.get<{ isFollowing: boolean }>(`/api/drivers/${driverId}/follow`).then((r) => r.data.isFollowing),
}
