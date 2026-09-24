import React, { createContext, useContext, useEffect, useState } from 'react'
import type { User, UserRole } from '@/types'
import { authApi } from '@/api/auth'
import type { AuthResponse, RegisterRequest } from '@/api/auth'

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => void
  /** Merge a saved profile change into the session without a round trip. */
  applyProfile: (patch: Partial<Pick<User, 'fullName' | 'email'>>) => void
  /** Adopt a token pair the server handed back, e.g. after a password change. */
  applySession: (data: AuthResponse) => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function decodeJwt(token: string): { sub?: string; exp?: number } {
  try {
    return JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
  } catch {
    return {}
  }
}

function buildUser(data: { accessToken: string; fullName: string; email: string; role: string }): User {
  const claims = decodeJwt(data.accessToken)
  return {
    id: claims.sub ?? '',
    fullName: data.fullName,
    email: data.email,
    role: data.role.toLowerCase() as UserRole,
    createdAt: new Date().toISOString(),
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    const stored = localStorage.getItem('user')
    if (!token || !stored) { setIsLoading(false); return }

    const claims = decodeJwt(token)
    if (claims.exp && claims.exp * 1000 < Date.now()) {
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
      setIsLoading(false)
      return
    }

    try {
      setUser(JSON.parse(stored))
    } catch {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    } finally {
      setIsLoading(false)
    }
  }, [])

  const login = async (email: string, password: string) => {
    const data = await authApi.login({ email, password })
    const me = buildUser(data)
    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify(me))
    setUser(me)
  }

  const register = async (request: RegisterRequest) => {
    const data = await authApi.register(request)
    const me = buildUser(data)
    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify(me))
    setUser(me)
  }

  const logout = () => {
    // Revoke server-side first, while the token is still there to authenticate with. The local
    // session is cleared either way — a user who wants out should not be held in by a failed
    // network call — but without this the refresh token stays valid after "sign out".
    const token = localStorage.getItem('token')
    if (token) void authApi.logout(token).catch(() => {})

    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
    setUser(null)
  }

  const applyProfile = (patch: Partial<Pick<User, 'fullName' | 'email'>>) => {
    setUser((current) => {
      if (!current) return current
      const next = { ...current, ...patch }
      localStorage.setItem('user', JSON.stringify(next))
      return next
    })
  }

  const applySession = (data: AuthResponse) => {
    const me = buildUser(data)
    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    // createdAt is not in the auth payload, so keep the one already on the session rather than
    // stamping "now" over a real join date.
    setUser((current) => {
      const next = current ? { ...me, createdAt: current.createdAt } : me
      localStorage.setItem('user', JSON.stringify(next))
      return next
    })
  }

  return (
    <AuthContext.Provider
      value={{ user, isLoading, login, register, logout, applyProfile, applySession }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
