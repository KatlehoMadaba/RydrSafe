import { apiClient } from './client'
import type {
  Report,
  PaginatedResponse,
  ReportCategory,
  ReportSeverity,
  ReportClassification,
  PublicReportSummary,
  ReportStatusAudit,
} from '@/types'

export interface CreateReportRequest {
  driverName: string
  registrationNumber: string
  category: ReportCategory
  severity: ReportSeverity
  description: string
  incidentDate: string
  reportedToPolice: boolean
  /** Clause 6.3(b) — optional SAPS CAS/AR number. */
  officialReference?: string
  /** Hashed server-side; used only for the clause 6.3(a) independence check. */
  deviceFingerprint?: string
}

/**
 * Clause 7.3(c): every moderation decision carries a reason and a record of what the moderator
 * actually reviewed. The API rejects a decision without them, so these are not optional.
 */
export interface ModerationDecision {
  reason: string
  reviewedReportContent: boolean
  reviewedDriverResponse?: boolean
  reviewedRiskScore?: boolean
}

export const reportsApi = {
  getAll: (params?: { page?: number; pageSize?: number; status?: string }) =>
    apiClient.get<PaginatedResponse<Report>>('/api/reports', { params }).then((r) => r.data),

  getMine: () => apiClient.get<Report[]>('/api/reports/mine').then((r) => r.data),

  getById: (id: string) =>
    apiClient.get<Report>(`/api/reports/${id}`).then((r) => r.data),

  /** Clause 6.4 — the reduced view any signed-in user may see. */
  getPublicSummary: (driverId: string) =>
    apiClient
      .get<PublicReportSummary[]>(`/api/reports/driver/${driverId}/public-summary`)
      .then((r) => r.data),

  getAudits: (id: string) =>
    apiClient.get<ReportStatusAudit[]>(`/api/reports/${id}/audits`).then((r) => r.data),

  create: (data: CreateReportRequest) =>
    apiClient.post<{ id: string }>('/api/reports', data).then((r) => r.data),

  approve: (id: string, decision: ModerationDecision) =>
    apiClient.put(`/api/reports/${id}/approve`, decision).then((r) => r.data),

  reject: (id: string, decision: ModerationDecision) =>
    apiClient.put(`/api/reports/${id}/reject`, decision).then((r) => r.data),

  reclassify: (id: string, classification: ReportClassification, reason: string) =>
    apiClient.put(`/api/reports/${id}/classification`, { classification, reason }).then((r) => r.data),

  verifyOfficialReference: (id: string, verified: boolean, reason: string) =>
    apiClient.put(`/api/reports/${id}/official-reference`, { verified, reason }).then((r) => r.data),

  recordPublicRecord: (
    id: string,
    body: { sourceType: string; sourceUrl?: string; sourceReference?: string; reason: string },
  ) => apiClient.put(`/api/reports/${id}/public-record`, body).then((r) => r.data),

  revokeCorroborationSource: (id: string, reason: string) =>
    apiClient
      .delete(`/api/reports/${id}/corroboration-source`, {
        data: { reason, reviewedReportContent: true },
      })
      .then((r) => r.data),

  /** Clause 6.2 — a reporter withdrawing their own report. */
  withdraw: (id: string, reason: string) =>
    apiClient
      .post(`/api/reports/${id}/withdraw`, { reason, reviewedReportContent: true })
      .then((r) => r.data),
}
