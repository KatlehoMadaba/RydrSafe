export type UserRole = 'passenger' | 'moderator' | 'admin'

export interface User {
  id: string
  fullName: string
  email: string
  role: UserRole
  createdAt: string
}

export interface Driver {
  id: string
  driverName: string
  phoneNumber: string
  riskScore: number
  status: DriverStatus
  vehicles: Vehicle[]
  reportCount: number
  createdAt: string
  updatedAt: string
}

export type DriverStatus = 'Safe' | 'UnderReview' | 'Flagged' | 'HighRisk'

export interface Vehicle {
  id: string
  driverId: string
  registrationNumber: string
  make: string
  model: string
  color: string
  createdAt: string
}

export type ReportCategory =
  | 'RecklessDriving'
  | 'Harassment'
  | 'Assault'
  | 'Theft'
  | 'Fraud'
  | 'UnsafeVehicle'
  | 'IntoxicatedDriving'
  | 'Other'

export type ReportSeverity = 'Low' | 'Medium' | 'High' | 'Critical'
export type ReportStatus = 'Pending' | 'Approved' | 'Rejected' | 'Escalated'

export interface Report {
  id: string
  driverId: string
  driver?: Driver
  userId: string
  user?: User
  category: ReportCategory
  severity: ReportSeverity
  description: string
  status: ReportStatus
  incidentDate: string
  createdAt: string
}

export interface Notification {
  id: string
  userId: string
  title: string
  message: string
  isRead: boolean
  createdAt: string
}

export interface VerificationResult {
  driverName: string
  registrationNumber: string
  phoneNumber?: string
  vehicleMake?: string
  vehicleModel?: string
  status: DriverStatus
  riskScore: number
  reportCount: number
  driverId?: string
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}
