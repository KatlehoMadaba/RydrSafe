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

/**
 * Clause 6.2 publication stages. Only `Corroborated` is ever visible to users other than the
 * reporter and the moderation team. `Escalated` is gone — it had no defined meaning and no
 * effect on publication.
 */
export type ReportStatus =
  | 'Pending'
  | 'Approved'
  | 'Corroborated'
  | 'Rejected'
  | 'Withdrawn'

/** Clause 6.1. Category A alleges criminal conduct and is subject to the corroboration threshold. */
export type ReportClassification = 'CategoryA' | 'CategoryB'

/** Which limb of clause 6.3 carried a report to `Corroborated`. */
export type CorroborationPath =
  | 'None'
  | 'IndependentReports'
  | 'OfficialReference'
  | 'PublicRecord'

export type AppealStatus = 'Received' | 'UnderReview' | 'Upheld' | 'Dismissed' | 'Withdrawn'

export interface Report {
  id: string
  driverId: string
  driver?: Driver
  userId: string
  user?: User
  category: ReportCategory
  classification: ReportClassification
  severity: ReportSeverity
  /** Never returned to anyone but the reporter and moderators (clause 6.4). */
  description: string
  status: ReportStatus
  corroborationPath: CorroborationPath
  officialReference?: string | null
  officialReferenceVerified: boolean
  corroboratedAt?: string | null
  incidentDate: string
  reportedToPolice: boolean
  createdAt: string
}

/** Clause 6.4 — the only shape in which report data is shown to other users. */
export interface PublicReportSummary {
  category: ReportCategory
  severityBand: ReportSeverity
  corroboratedCount: number
  earliestIncident?: string | null
  latestIncident?: string | null
}

export interface ReportStatusAudit {
  id: string
  actorUserId: string
  fromStatus: ReportStatus
  toStatus: ReportStatus
  reason: string
  reviewedReportContent: boolean
  reviewedDriverResponse: boolean
  reviewedRiskScore: boolean
  createdAt: string
}

export interface DriverSelfCheckResult {
  recordExists: boolean
  driverName?: string | null
  publicStatus?: DriverStatus | null
  corroboratedReportCount: number
  summaries: PublicReportSummary[]
  firstSeen?: string | null
  message: string
}

export interface Appeal {
  id: string
  driverId: string
  driverName: string
  grounds: string
  detail: string
  contactEmail: string
  status: AppealStatus
  identityVerified: boolean
  publicStatusSuspended: boolean
  outcome?: string | null
  createdAt: string
  dueAt: string
  resolvedAt?: string | null
}

/** Served by GET /api/platform/config — tells the client which categories are currently accepted. */
export interface PlatformConfig {
  agreementVersion: string
  categoryAProcessingEnabled: boolean
  categoryAPublicationEnabled: boolean
  categoryADisabledReason: string | null
  reportCategories: {
    value: ReportCategory
    classification: ReportClassification
    available: boolean
  }[]
  requiredConsents: string[]
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
  totalCount: number
  page: number
  pageSize: number
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}
