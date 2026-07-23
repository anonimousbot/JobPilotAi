export type AuthResponse = {
  userId: string
  email: string
  role: string
  emailConfirmed: boolean
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export type ProfileResponse = {
  id: string
  userId: string
  firstName: string
  lastName: string
  phoneNumber: string | null
  location: string | null
  linkedInUrl: string | null
  portfolioUrl: string | null
  careerGoal: string | null
  targetRole: string | null
  careerLevel: string | null
  updatedAt: string
}

export type ResumeResponse = {
  id: string
  fileName: string
  fileType: string
  fileSize: number
  contentHash: string
  uploadedAt: string
}

export type UploadResumeResponse = {
  id: string
  fileName: string
  fileType: string
  fileSize: number
  duplicateFound: boolean
  uploadedAt: string
}

export type AnalysisResponse = {
  id: string
  resumeId: string
  atsScore: number
  keywordMatchScore: number
  skillsCoverageScore: number
  formattingScore: number
  experienceScore: number
  educationScore: number
  weaknesses: string[]
  recommendations: string[]
  missingKeywords: string[]
  strengths: string[]
  createdAt: string
}

export type CoverLetterResponse = {
  id: string
  resumeId: string | null
  jobTitle: string
  companyName: string
  jobDescription: string
  tone: string
  originalContent: string
  editedContent: string
  createdAt: string
  updatedAt: string
}

export type JobMatchResponse = {
  id: string
  jobTitle: string
  companyName: string
  location: string
  matchScore: number
  tags: string[]
  summary: string
  saved: boolean
}

export type JobMatchComparisonResponse = {
  jobMatchId: string
  jobTitle: string
  companyName: string
  matchScore: number
  requiredSkills: string[]
  matchedSkills: string[]
  missingSkills: string[]
  recommendations: string[]
  insight: string
}

export type SubscriptionResponse = {
  id: string
  planType: string
  status: string
  startDate: string
  endDate: string | null
  billingCycle: string
}
