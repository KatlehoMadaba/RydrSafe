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

/** Clause 4 / POPIA s24 — the account holder correcting their own details. */
export interface UpdateProfileRequest {
  fullName: string
  email: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface DeleteAccountRequest {
  password: string
  reason?: string
}

/** What the server actually did, so the confirmation can state it rather than promise it. */
export interface DeleteAccountResponse {
  email: string
  /** Clause 29.2 — reports kept but unlinked from the account. */
  reportsDeIdentified: number
  deletedAt: string
  message: string
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

  updateProfile: (data: UpdateProfileRequest) =>
    apiClient.put<User>('/api/auth/me', data).then((r) => r.data),

  /** Returns a fresh token pair — the change signs every other session out, including this one. */
  changePassword: (data: ChangePasswordRequest) =>
    apiClient.put<AuthResponse>('/api/auth/me/password', data).then((r) => r.data),

  /** Immediate and irreversible. Clause 29. */
  deleteAccount: (data: DeleteAccountRequest) =>
    apiClient.delete<DeleteAccountResponse>('/api/auth/me', { data }).then((r) => r.data),

  /**
   * Revokes the stored refresh token. Clearing local storage alone leaves it usable.
   * Takes the token explicitly because the caller clears storage straight afterwards, and the
   * request interceptor would otherwise find nothing to attach.
   */
  logout: (token?: string) =>
    apiClient
      .post('/api/auth/logout', null, token ? { headers: { Authorization: `Bearer ${token}` } } : undefined)
      .then((r) => r.data),
}
