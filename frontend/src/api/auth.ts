import { apiClient } from './client'
import type { User } from '@/types'

export interface LoginRequest { email: string; password: string }

/** Part D. One entry per checkbox; the server stores each one against the account. */
export interface ConsentAcceptance {
  consentKey: string
  accepted: boolean
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
  /** Clause 3.1 — the 18+ rule is applied to this, not to a tick alone. */
  dateOfBirth: string
  consents: ConsentAcceptance[]
  agreementVersion: string
  locale?: string
}

export interface ConsentRecord {
  consentKey: string
  agreementVersion: string
  locale: string
  collectionSurface: string
  accepted: boolean
  acceptedAt: string
  withdrawnAt: string | null
}
export interface AuthResponse {
  accessToken: string
  refreshToken: string
  fullName: string
  email: string
  role: string
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>('/api/auth/login', data).then((r) => r.data),
  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>('/api/auth/register', data).then((r) => r.data),
  refreshToken: (refreshToken: string) =>
    apiClient.post<AuthResponse>('/api/auth/refresh-token', { refreshToken }).then((r) => r.data),
  me: () => apiClient.get<User>('/api/auth/me').then((r) => r.data),
  myConsents: () =>
    apiClient.get<ConsentRecord[]>('/api/auth/me/consents').then((r) => r.data),
}
