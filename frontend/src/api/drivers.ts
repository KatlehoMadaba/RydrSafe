import { apiClient } from './client'
import type { Driver, PaginatedResponse } from '@/types'

export const driversApi = {
  getAll: (params?: { page?: number; pageSize?: number; search?: string }) =>
    apiClient.get<PaginatedResponse<Driver>>('/api/drivers', { params }).then((r) => r.data),
  getById: (id: string) =>
    apiClient.get<Driver>(`/api/drivers/${id}`).then((r) => r.data),
  search: (query: string) =>
    apiClient.get<Driver[]>('/api/drivers/search', { params: { q: query } }).then((r) => r.data),
}
