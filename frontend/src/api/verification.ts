import { apiClient } from './client'
import type { VerificationResult, PaginatedResponse, DriverStatus } from '@/types'

export interface VerificationHistory {
  id: string
  driverName: string | null
  registrationNumber: string | null
  status: DriverStatus
  riskScore: number
  verifiedAt: string
}

export const verificationApi = {
  upload: (files: File[]) => {
    const form = new FormData()
    files.forEach((f, i) => form.append(`image${i + 1}`, f))
    return apiClient
      .post<VerificationResult>('/api/verification/upload', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
  verifyManual: (data: { registrationNumber?: string; driverName?: string; phoneNumber?: string }) =>
    apiClient
      .post<VerificationResult>('/api/verification/manual', data)
      .then((r) => r.data),
  getHistory: (params?: { page?: number; pageSize?: number }) =>
    apiClient
      .get<PaginatedResponse<VerificationHistory>>('/api/verification/history', { params })
      .then((r) => r.data),
  getFollowStatus: (driverId: string) =>
    apiClient
      .get<{ isFollowing: boolean }>(`/api/drivers/${driverId}/follow`)
      .then((r) => r.data.isFollowing),
  followDriver: (driverId: string) =>
    apiClient.post(`/api/drivers/${driverId}/follow`),
  unfollowDriver: (driverId: string) =>
    apiClient.delete(`/api/drivers/${driverId}/follow`),
}
