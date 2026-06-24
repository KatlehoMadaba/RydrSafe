import React, { createContext, useContext, useEffect, useState } from 'react'
import type { User, UserRole } from '@/types'
import { authApi } from '@/api/auth'

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (fullName: string, email: string, password: string) => Promise<void>
  logout: () => void
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

  const register = async (fullName: string, email: string, password: string) => {
    const data = await authApi.register({ fullName, email, password })
    const me = buildUser(data)
    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify(me))
    setUser(me)
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
