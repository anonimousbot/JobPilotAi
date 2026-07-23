import type {
  AnalysisResponse,
  AuthResponse,
  CoverLetterResponse,
  JobMatchComparisonResponse,
  JobMatchResponse,
  ProfileResponse,
  ResumeResponse,
  SubscriptionResponse,
  UploadResumeResponse,
} from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7176'
const AUTH_KEY = 'jobpilot.auth'

type ApiEnvelope<T> = {
  success?: boolean
  data?: T
  errors?: unknown
}

export class ApiError extends Error {
  status: number
  details?: unknown

  constructor(
    message: string,
    status: number,
    details?: unknown,
  ) {
    super(message)
    this.status = status
    this.details = details
  }
}

export function getStoredAuth(): AuthResponse | null {
  const raw = localStorage.getItem(AUTH_KEY)
  if (!raw) return null

  try {
    return JSON.parse(raw) as AuthResponse
  } catch {
    localStorage.removeItem(AUTH_KEY)
    return null
  }
}

export function setStoredAuth(auth: AuthResponse) {
  localStorage.setItem(AUTH_KEY, JSON.stringify(auth))
}

export function clearStoredAuth() {
  localStorage.removeItem(AUTH_KEY)
}

export function isAuthenticated() {
  const auth = getStoredAuth()
  return Boolean(auth?.accessToken)
}

let isRefreshing = false
let refreshSubscribers: Array<(token: string) => void> = []

function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach(callback => callback(token))
  refreshSubscribers = []
}

async function refreshToken(): Promise<string> {
  const auth = getStoredAuth()
  if (!auth?.refreshToken) {
    throw new Error('No refresh token available')
  }

  const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: auth.refreshToken }),
  })

  if (!response.ok) {
    clearStoredAuth()
    throw new Error('Token refresh failed')
  }

  const text = await response.text()
  const payload = text ? JSON.parse(text) as ApiEnvelope<AuthResponse> | AuthResponse : undefined
  const newAuth = (payload && typeof payload === 'object' && 'data' in payload) ? (payload as ApiEnvelope<AuthResponse>).data : payload as AuthResponse
  
  if (newAuth) {
    setStoredAuth(newAuth)
    return newAuth.accessToken
  }
  
  throw new Error('Invalid refresh response')
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const auth = getStoredAuth()
  const headers = new Headers(init.headers)

  if (!(init.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (auth?.accessToken) {
    headers.set('Authorization', `Bearer ${auth.accessToken}`)
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  })

  if (response.status === 401 && auth?.refreshToken) {
    if (!isRefreshing) {
      isRefreshing = true
      try {
        const newToken = await refreshToken()
        onTokenRefreshed(newToken)
        headers.set('Authorization', `Bearer ${newToken}`)
        const retryResponse = await fetch(`${API_BASE_URL}${path}`, {
          ...init,
          headers,
        })
        if (!retryResponse.ok) {
          throw new ApiError(readErrorMessage(undefined, retryResponse.statusText), retryResponse.status)
        }
        const text = await retryResponse.text()
        const payload = text ? JSON.parse(text) as ApiEnvelope<T> | T : undefined
        const envelope = payload as ApiEnvelope<T> | undefined
        if (envelope && typeof envelope === 'object' && 'success' in envelope && 'data' in envelope) {
          return envelope.data as T
        }
        return payload as T
      } finally {
        isRefreshing = false
      }
    } else {
      return new Promise((resolve, reject) => {
        subscribeTokenRefresh(async (token: string) => {
          headers.set('Authorization', `Bearer ${token}`)
          try {
            const retryResponse = await fetch(`${API_BASE_URL}${path}`, {
              ...init,
              headers,
            })
            if (!retryResponse.ok) {
              reject(new ApiError(readErrorMessage(undefined, retryResponse.statusText), retryResponse.status))
              return
            }
            const text = await retryResponse.text()
            const payload = text ? JSON.parse(text) as ApiEnvelope<T> | T : undefined
            const envelope = payload as ApiEnvelope<T> | undefined
            if (envelope && typeof envelope === 'object' && 'success' in envelope && 'data' in envelope) {
              resolve(envelope.data as T)
            } else {
              resolve(payload as T)
            }
          } catch (error) {
            reject(error)
          }
        })
      })
    }
  }

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  const payload = text ? JSON.parse(text) as ApiEnvelope<T> | T : undefined

  if (!response.ok) {
    const envelope = payload as ApiEnvelope<T> | undefined
    throw new ApiError(readErrorMessage(envelope, response.statusText), response.status, envelope?.errors ?? payload)
  }

  const envelope = payload as ApiEnvelope<T> | undefined
  if (envelope && typeof envelope === 'object' && 'success' in envelope && 'data' in envelope) {
    return envelope.data as T
  }

  return payload as T
}

function readErrorMessage(payload: ApiEnvelope<unknown> | undefined, fallback: string) {
  if (!payload?.errors) return fallback || 'Request failed'
  if (Array.isArray(payload.errors)) return payload.errors.join(', ')
  if (typeof payload.errors === 'string') return payload.errors
  if (typeof payload.errors === 'object') {
    return Object.values(payload.errors)
      .flat()
      .filter(Boolean)
      .join(', ') || fallback
  }
  return fallback
}

export const api = {
  baseUrl: API_BASE_URL,
  refreshToken: () => refreshToken(),
  login: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  register: (email: string, password: string) =>
    request<{ id: string; email: string }>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  verifyEmail: (email: string) =>
    request<{ success: boolean }>('/api/auth/verify-email', {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),
  logout: (refreshToken: string) =>
    request<void>('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    }),
  forgotPassword: (email: string) =>
    request<{ success: boolean }>('/api/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),
  resetPassword: (email: string, token: string, newPassword: string) =>
    request<{ success: boolean }>('/api/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify({ email, token, newPassword }),
    }),
  getProfile: () => request<ProfileResponse>('/api/profile'),
  updateProfile: (profile: Omit<ProfileResponse, 'id' | 'userId' | 'updatedAt'>) =>
    request<ProfileResponse>('/api/profile', {
      method: 'PUT',
      body: JSON.stringify(profile),
    }),
  updateOnboarding: (careerGoal: string, targetRole: string, careerLevel: string, currentJobTitle: string | undefined, yearsOfExperience: number | null | undefined) =>
    request<ProfileResponse>('/api/profile/onboarding', {
      method: 'PUT',
      body: JSON.stringify({ careerGoal, targetRole, careerLevel, currentJobTitle, yearsOfExperience }),
    }),
  listResumes: () => request<ResumeResponse[]>('/api/resumes'),
  uploadResume: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return request<UploadResumeResponse>('/api/resumes', {
      method: 'POST',
      body: formData,
    })
  },
  deleteResume: (id: string) => request<void>(`/api/resumes/${id}`, { method: 'DELETE' }),
  listAnalyses: () => request<AnalysisResponse[]>('/api/analyses'),
  getAnalysis: (id: string) => request<AnalysisResponse>(`/api/analyses/${id}`),
  createAnalysis: (resumeId: string, jobDescription?: string) =>
    request<AnalysisResponse>('/api/analyses', {
      method: 'POST',
      body: JSON.stringify({ resumeId, jobDescription }),
    }),
  listCoverLetters: () => request<CoverLetterResponse[]>('/api/coverletters'),
  getCoverLetter: (id: string) => request<CoverLetterResponse>(`/api/coverletters/${id}`),
  createCoverLetter: (requestBody: {
    resumeId: string | null
    jobTitle: string
    companyName: string
    jobDescription: string
    tone: string
  }) =>
    request<CoverLetterResponse>('/api/coverletters', {
      method: 'POST',
      body: JSON.stringify(requestBody),
    }),
  updateCoverLetter: (id: string, editedContent: string) =>
    request<CoverLetterResponse>(`/api/coverletters/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ editedContent }),
    }),
  deleteCoverLetter: (id: string) => request<void>(`/api/coverletters/${id}`, { method: 'DELETE' }),
  regenerateCoverLetter: (id: string) =>
    request<CoverLetterResponse>(`/api/coverletters/${id}/regenerate`, { method: 'POST' }),
  searchJobMatches: (query?: string, location?: string, resumeId?: string) =>
    request<JobMatchResponse[]>('/api/jobmatches/search', {
      method: 'POST',
      body: JSON.stringify({ query, location, resumeId }),
    }),
  getJobMatches: () => request<JobMatchResponse[]>('/api/jobmatches'),
  compareJobMatch: (id: string) =>
    request<JobMatchComparisonResponse>(`/api/jobmatches/${id}/compare`, { method: 'POST' }),
  saveJobMatch: (id: string) => request(`/api/jobmatches/${id}/save`, { method: 'POST' }),
  unsaveJobMatch: (id: string) => request<void>(`/api/jobmatches/${id}/save`, { method: 'DELETE' }),
  getCurrentSubscription: () => request<SubscriptionResponse>('/api/subscriptions/current'),
  getSubscriptionHistory: () => request<SubscriptionResponse[]>('/api/subscriptions/history'),
  upgradeSubscription: () => request<SubscriptionResponse>('/api/subscriptions/upgrade', { method: 'POST' }),
  cancelSubscription: () => request<SubscriptionResponse>('/api/subscriptions/cancel', { method: 'POST' }),
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(value))
}

export function formatFileSize(bytes: number) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}
