import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { api, ApiError, getStoredAuth } from '../api/client'
import AppLayout from '../components/AppLayout'
import '../app.css'

export default function ProfilePage() {
  const auth = getStoredAuth()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [title, setTitle] = useState('')
  const [location, setLocation] = useState('')
  const [careerGoal, setCareerGoal] = useState('')
  const [careerLevel, setCareerLevel] = useState('')
  const [error, setError] = useState('')
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    void loadProfile()
  }, [])

  const loadProfile = async () => {
    setError('')
    try {
      const profile = await api.getProfile()
      setFirstName(profile.firstName)
      setLastName(profile.lastName)
      setTitle(profile.targetRole ?? '')
      setLocation(profile.location ?? '')
      setCareerGoal(profile.careerGoal ?? '')
      setCareerLevel(profile.careerLevel ?? '')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load profile.')
    }
  }

  const handleSave = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await api.updateProfile({
        firstName,
        lastName,
        phoneNumber: null,
        location,
        linkedInUrl: null,
        portfolioUrl: null,
        careerGoal,
        targetRole: title,
        careerLevel,
      })
      setSaved(true)
      setTimeout(() => setSaved(false), 2500)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to save profile.')
    }
  }

  const displayName = `${firstName} ${lastName}`.trim() || 'JobPilotAi User'
  const initials = `${firstName[0] ?? 'J'}${lastName[0] ?? 'P'}`.toUpperCase()

  return (
    <AppLayout title="Profile & Settings" subtitle="Manage your account and preferences.">
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}

      <div style={{ display: 'flex', alignItems: 'center', gap: 24, padding: 28, background: 'var(--app-surface-lowest)', borderRadius: 16, border: '1px solid var(--app-outline-var)', marginBottom: 28 }}>
        <div style={{ width: 80, height: 80, borderRadius: '50%', background: 'var(--app-primary-cont)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 32, fontWeight: 700, color: 'var(--app-on-primary-cont)', fontFamily: 'Hanken Grotesk', flexShrink: 0 }}>
          {initials}
        </div>
        <div style={{ flex: 1 }}>
          <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 22, fontWeight: 700, color: 'var(--app-on-surface)', margin: '0 0 4px' }}>{displayName}</h2>
          <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)', margin: '0 0 12px' }}>{title || 'Target role not set'} - {auth?.email ?? 'No email'}</p>
          <div className="badge badge-green" style={{ padding: '4px 14px' }}>{careerLevel || 'Profile active'}</div>
        </div>
      </div>

      <form onSubmit={handleSave}>
        <div className="settings-section">
          <div className="settings-section-title">Personal Information</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <div className="app-input-group">
              <label className="app-label">First Name</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">person</span>
                <input className="app-input" type="text" value={firstName} onChange={e => setFirstName(e.target.value)} required />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Last Name</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">badge</span>
                <input className="app-input" type="text" value={lastName} onChange={e => setLastName(e.target.value)} required />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Target Role</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">work</span>
                <input className="app-input" type="text" value={title} onChange={e => setTitle(e.target.value)} />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Location</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">location_on</span>
                <input className="app-input" type="text" value={location} onChange={e => setLocation(e.target.value)} />
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Career Level</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">leaderboard</span>
                <select className="app-input" value={careerLevel} onChange={e => setCareerLevel(e.target.value)}>
                  <option value="">Select level</option>
                  <option>Entry</option>
                  <option>Mid</option>
                  <option>Senior</option>
                  <option>Executive</option>
                </select>
              </div>
            </div>
            <div className="app-input-group">
              <label className="app-label">Career Goal</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">track_changes</span>
                <input className="app-input" type="text" value={careerGoal} onChange={e => setCareerGoal(e.target.value)} />
              </div>
            </div>
          </div>
        </div>

        <div className="settings-section">
          <div className="settings-section-title">Account</div>
          <div className="settings-row">
            <div>
              <div className="settings-row-label">Email Address</div>
              <div className="settings-row-desc">{auth?.email ?? 'Not available'}</div>
            </div>
            {auth?.emailConfirmed ? (
              <div className="badge badge-green">Verified</div>
            ) : (
              <div className="badge" style={{ background: '#fef3c7', color: '#92400e' }}>Not Verified</div>
            )}
          </div>
          <div className="settings-row">
            <div>
              <div className="settings-row-label">Security</div>
              <div className="settings-row-desc">JWT access tokens and refresh token rotation are active.</div>
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', gap: 12 }}>
          <button type="submit" className="btn-app-primary" style={{ padding: '12px 32px', fontSize: 15 }}>
            {saved ? 'Saved' : 'Save Changes'}
          </button>
          <button type="button" className="btn-app-secondary" style={{ fontSize: 14 }} onClick={() => void loadProfile()}>Cancel</button>
        </div>
      </form>
    </AppLayout>
  )
}
