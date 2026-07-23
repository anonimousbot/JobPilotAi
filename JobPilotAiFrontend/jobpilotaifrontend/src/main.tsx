import { StrictMode } from 'react'
import type { ReactNode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import './index.css'
import { isAuthenticated } from './api/client'

// Landing
import App, { ThemeProvider } from './App.tsx'

// Auth / Onboarding
import LoginPage from './pages/LoginPage'
import SignupPage from './pages/SignupPage'
import OnboardingGoalsPage from './pages/OnboardingGoalsPage'
import OnboardingExperiencePage from './pages/OnboardingExperiencePage'
import OnboardingResumePage from './pages/OnboardingResumePage'

// App pages
import DashboardPage from './pages/DashboardPage'
import MyResumesPage from './pages/MyResumesPage'
import ATSAnalysisPage from './pages/ATSAnalysisPage'
import CoverLetterPage from './pages/CoverLetterPage'
import JobMatchPage from './pages/JobMatchPage'
import SubscriptionPage from './pages/SubscriptionPage'
import ProfilePage from './pages/ProfilePage'
import GenerateCoverLetterPage from './pages/GenerateCoverLetterPage'
import ATSDeepDivePage from './pages/ATSDeepDivePage'

function PrivateRoute({ children }: { children: ReactNode }) {
  return isAuthenticated() ? children : <Navigate to="/login" replace />
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <BrowserRouter>
        <Routes>
          {/* Landing page */}
          <Route path="/" element={<App />} />

          {/* Auth */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/signup" element={<SignupPage />} />

          {/* Onboarding flow */}
          <Route path="/onboarding/goals" element={<PrivateRoute><OnboardingGoalsPage /></PrivateRoute>} />
          <Route path="/onboarding/experience" element={<PrivateRoute><OnboardingExperiencePage /></PrivateRoute>} />
          <Route path="/onboarding/resume" element={<PrivateRoute><OnboardingResumePage /></PrivateRoute>} />

          {/* App / Dashboard */}
          <Route path="/dashboard" element={<PrivateRoute><DashboardPage /></PrivateRoute>} />
          <Route path="/resumes" element={<PrivateRoute><MyResumesPage /></PrivateRoute>} />
          <Route path="/ats-analysis" element={<PrivateRoute><ATSAnalysisPage /></PrivateRoute>} />
          <Route path="/ats-deep-dive" element={<PrivateRoute><ATSDeepDivePage /></PrivateRoute>} />
          <Route path="/cover-letter" element={<PrivateRoute><CoverLetterPage /></PrivateRoute>} />
          <Route path="/generate-cover-letter" element={<PrivateRoute><GenerateCoverLetterPage /></PrivateRoute>} />
          <Route path="/job-match" element={<PrivateRoute><JobMatchPage /></PrivateRoute>} />
          <Route path="/subscription" element={<PrivateRoute><SubscriptionPage /></PrivateRoute>} />
          <Route path="/profile" element={<PrivateRoute><ProfilePage /></PrivateRoute>} />

          {/* Catch-all */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  </StrictMode>,
)
