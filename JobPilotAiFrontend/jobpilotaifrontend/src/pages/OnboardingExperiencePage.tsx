import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import '../app.css'

export default function OnboardingExperiencePage() {
  const navigate = useNavigate()
  const [currentJobTitle, setCurrentJobTitle] = useState('')
  const [targetRole, setTargetRole] = useState('')
  const [yearsOfExperience, setYearsOfExperience] = useState('')
  const [careerLevel, setCareerLevel] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const careerGoal = localStorage.getItem('jobpilot.onboardingGoal') ?? 'land-new-job'
      await api.updateOnboarding(
        careerGoal,
        targetRole,
        careerLevel,
        currentJobTitle,
        yearsOfExperience ? parseInt(yearsOfExperience, 10) : null,
      )
      navigate('/onboarding/resume')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to save your onboarding details.')
    } finally {
      setLoading(false)
    }
  }
  return (
    <div className="auth-page" style={{ minHeight: '100svh' }}>
      <header className="onboarding-header">
        <div style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 20, color: 'var(--app-on-surface)' }}>JobPilotAi</div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-on-surface-var)', letterSpacing: '0.06em' }}>STEP 2 OF 3</span>
          <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-secondary)' }}>66% COMPLETE</span>
          <div className="onboarding-progress-track">
            <div className="onboarding-progress-fill" style={{ width: '66%' }} />
          </div>
        </div>
      </header>

      <main style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '40px 16px' }}>
        <div style={{ width: '100%', maxWidth: 540, background: 'var(--app-surface-lowest)', borderRadius: 16, border: '1px solid var(--app-outline-var)', boxShadow: '0 4px 20px rgba(15,23,42,0.08)', padding: '36px' }}>
          <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 24, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 6 }}>Experience & Level</h1>
          <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)', marginBottom: 28, lineHeight: 1.6 }}>
            Help JobPilotAi calibrate its matching engine by defining your professional trajectory.
          </p>

          {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', justifyContent: 'center', padding: 10, marginBottom: 16 }}>{error}</div>}

          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
            <div className="app-input-group">
              <label className="app-label">Current Job Title</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">work</span>
                <input className="app-input" type="text" placeholder="e.g. Senior Software Engineer" value={currentJobTitle} onChange={e => setCurrentJobTitle(e.target.value)} />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Target Job Title</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">track_changes</span>
                <input className="app-input" type="text" placeholder="e.g. Engineering Manager" value={targetRole} onChange={e => setTargetRole(e.target.value)} required />
              </div>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
              <div className="app-input-group">
                <label className="app-label">Years of Experience</label>
                <div className="app-input-wrap">
                  <span className="material-symbols-outlined app-input-icon">calendar_today</span>
                  <input className="app-input" type="number" placeholder="0" min="0" value={yearsOfExperience} onChange={e => setYearsOfExperience(e.target.value)} />
                </div>
              </div>
              <div className="app-input-group">
                <label className="app-label">Career Level</label>
                <div className="app-input-wrap">
                  <span className="material-symbols-outlined app-input-icon">leaderboard</span>
                  <select className="app-input" style={{ appearance: 'none', cursor: 'pointer' }} value={careerLevel} onChange={e => setCareerLevel(e.target.value)} required>
                    <option value="">Select Level</option>
                    <option>Entry</option>
                    <option>Mid</option>
                    <option>Senior</option>
                    <option>Executive</option>
                  </select>
                </div>
              </div>
            </div>

            <div style={{ display: 'flex', gap: 12, paddingTop: 16, borderTop: '1px solid var(--app-outline-var)' }}>
              <button type="button" onClick={() => navigate('/onboarding/goals')} className="btn-app-secondary" style={{ flex: 1, justifyContent: 'center' }}>Back</button>
              <button type="submit" className="btn-app-primary" style={{ flex: 2, justifyContent: 'center', padding: '12px 20px', fontSize: 15 }} disabled={loading}>{loading ? 'Saving...' : 'Continue to Optimization'}</button>
            </div>
          </form>
        </div>

        <p style={{ marginTop: 24, fontSize: 13, color: 'var(--app-on-surface-var)', fontStyle: 'italic', textAlign: 'center', maxWidth: 360, opacity: 0.6 }}>
          "Professional Velocity is achieving the next step in your career 30% faster than traditional methods."
        </p>
      </main>
    </div>
  )
}
