import { apiClient } from './client'
import type { User, PaginatedResponse } from '@/types'

export const usersApi = {
  getAll: (params?: { page?: number; pageSize?: number; role?: string }) =>
    apiClient.get<PaginatedResponse<User>>('/api/users', { params }).then((r) => r.data),
  getById: (id: string) => apiClient.get<User>(`/api/users/${id}`).then((r) => r.data),
  updateRole: (id: string, role: string) =>
    apiClient.put<User>(`/api/users/${id}/role`, { role }).then((r) => r.data),
  deactivate: (id: string) =>
    apiClient.put<User>(`/api/users/${id}/deactivate`).then((r) => r.data),
}
