import { apiClient } from './client'
import type { Appeal, DriverSelfCheckResult, PaginatedResponse, DriverStatus } from '@/types'

/** Part C. These routes are unauthenticated by design — drivers are not RydrSafe users. */
export const driverRightsApi = {
  /** Clause 34 (POPIA s23). */
  selfCheck: (body: { registrationNumber: string; driverName?: string; contactEmail: string }) =>
    apiClient.post<DriverSelfCheckResult>('/api/driver-rights/self-check', body).then((r) => r.data),

  /** Clause 35. Suspends the driver's public status while it is open. */
  createAppeal: (body: {
    registrationNumber: string
    grounds: string
    detail: string
    contactEmail: string
    contactPhone?: string
    identityEvidenceNote: string
  }) =>
    apiClient
      .post<{ id: string; message: string }>('/api/driver-rights/appeals', body)
      .then((r) => r.data),

  /** Clause 6.5. */
  submitRightOfReply: (body: {
    registrationNumber: string
    response: string
    contactEmail: string
  }) => apiClient.post('/api/driver-rights/right-of-reply', body).then((r) => r.data),

  // ---- Moderator ----

  getAppeals: (params?: { page?: number; pageSize?: number }) =>
    apiClient
      .get<PaginatedResponse<Appeal>>('/api/driver-rights/appeals', { params })
      .then((r) => r.data),

  resolveAppeal: (
    id: string,
    body: {
      status: 'UnderReview' | 'Upheld' | 'Dismissed'
      outcome: string
      reviewedRiskScore?: boolean
      reviewedDriverResponse?: boolean
      reviewedScoringLogic?: boolean
    },
  ) => apiClient.put(`/api/driver-rights/appeals/${id}`, body).then((r) => r.data),

  offerRightOfReply: (driverId: string, proposedStatus: DriverStatus) =>
    apiClient
      .post(`/api/driver-rights/drivers/${driverId}/offer-right-of-reply`, null, {
        params: { proposedStatus },
      })
      .then((r) => r.data),

  /** Clause 7.3 — the only route that writes a driver's public status. */
  setDriverStatus: (
    driverId: string,
    body: {
      status: DriverStatus
      reason: string
      reviewedRiskScore: boolean
      reviewedDriverResponse?: boolean
      reviewedScoringLogic: boolean
    },
  ) => apiClient.put(`/api/driver-rights/drivers/${driverId}/status`, body).then((r) => r.data),

  getSuggestedStatus: (driverId: string) =>
    apiClient
      .get<{ riskScore: number; suggestedStatus: DriverStatus }>(
        `/api/driver-rights/drivers/${driverId}/suggested-status`,
      )
      .then((r) => r.data),
}
