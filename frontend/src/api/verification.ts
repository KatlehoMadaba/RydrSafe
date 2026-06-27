import { apiClient } from './client'
import type { VerificationResult, PaginatedResponse } from '@/types'

export interface VerificationHistory {
  id: string
  result: VerificationResult
  createdAt: string
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
  getHistory: (params?: { page?: number; pageSize?: number }) =>
    apiClient
      .get<PaginatedResponse<VerificationHistory>>('/api/verification/history', { params })
      .then((r) => r.data),
}
