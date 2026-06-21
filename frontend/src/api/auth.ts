import { apiClient } from './client'
import type { User } from '@/types'

export interface LoginRequest { email: string; password: string }
export interface RegisterRequest { fullName: string; email: string; password: string }
export interface AuthResponse { token: string; refreshToken: string; user: User }

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResponse>('/api/auth/login', data).then((r) => r.data),
  register: (data: RegisterRequest) =>
    apiClient.post<AuthResponse>('/api/auth/register', data).then((r) => r.data),
  refreshToken: (refreshToken: string) =>
    apiClient.post<AuthResponse>('/api/auth/refresh-token', { refreshToken }).then((r) => r.data),
  me: () => apiClient.get<User>('/api/auth/me').then((r) => r.data),
}
