import { apiClient } from './client'
import type { Driver, DriverListItem, PaginatedResponse } from '@/types'

export const driversApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string }) =>
    apiClient.get<PaginatedResponse<DriverListItem>>('/api/drivers', { params }).then((r) => r.data),
  getById: (id: string) =>
    apiClient.get<Driver>(`/api/drivers/${id}`).then((r) => r.data),
  search: (query: string) =>
    apiClient.get<DriverListItem[]>('/api/drivers/search', { params: { q: query } }).then((r) => r.data),
  getFlaggedCount: () =>
    apiClient.get<{ count: number }>('/api/drivers/flagged-count').then((r) => r.data.count),
}
