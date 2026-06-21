import { apiClient } from './client'
import type { Notification } from '@/types'

export const notificationsApi = {
  getAll: () => apiClient.get<Notification[]>('/api/notifications').then((r) => r.data),
  markRead: (id: string) =>
    apiClient.put<Notification>(`/api/notifications/${id}/read`).then((r) => r.data),
  markAllRead: () => apiClient.put('/api/notifications/read-all').then((r) => r.data),
}
