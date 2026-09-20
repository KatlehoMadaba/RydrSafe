import { apiClient } from './client'
import type { Recommendation, PaginatedResponse, RecommendationCategory } from '@/types'

export interface CreateRecommendationRequest {
  category: RecommendationCategory
  subject: string
  message: string
}

export const recommendationsApi = {
  getMine: (params?: { page?: number; pageSize?: number }) =>
    apiClient
      .get<PaginatedResponse<Recommendation>>('/api/recommendations/mine', { params })
      .then((r) => r.data),
  create: (data: CreateRecommendationRequest) =>
    apiClient.post<Recommendation>('/api/recommendations', data).then((r) => r.data),
}
