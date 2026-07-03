import { apiClient } from './client'
import type { Report, PaginatedResponse, ReportCategory, ReportSeverity } from '@/types'

export interface CreateReportRequest {
  driverName: string
  registrationNumber: string
  category: ReportCategory
  severity: ReportSeverity
  description: string
  incidentDate: string
  evidence?: File[]
}

export const reportsApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string }) =>
    apiClient.get<PaginatedResponse<Report>>('/api/reports', { params }).then((r) => r.data),
  getById: (id: string) =>
    apiClient.get<Report>(`/api/reports/${id}`).then((r) => r.data),
  create: ({ evidence, ...fields }: CreateReportRequest) => {
    const form = new FormData()
    Object.entries(fields).forEach(([k, v]) => form.append(k, v as string))
    evidence?.forEach((f) => form.append('evidence', f))
    return apiClient
      .post<Report>('/api/reports', form, { headers: { 'Content-Type': 'multipart/form-data' } })
      .then((r) => r.data)
  },
  approve: (id: string) =>
    apiClient.put<Report>(`/api/reports/${id}/approve`).then((r) => r.data),
  reject: (id: string, reason?: string) =>
    apiClient.put<Report>(`/api/reports/${id}/reject`, { reason }).then((r) => r.data),
  escalate: (id: string) =>
    apiClient.put<Report>(`/api/reports/${id}/escalate`).then((r) => r.data),
}
