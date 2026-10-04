import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, setStoredAuth } from '../api/client'
import '../App.css'

export default function SignupPage() {
  const navigate = useNavigate()
  const [showPass, setShowPass] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      await api.register(email, password)
      await api.verifyEmail(email)
      const auth = await api.login(email, password)
      setStoredAuth(auth)

      const [firstName, ...lastNameParts] = fullName.trim().split(/\s+/)
      await api.updateProfile({
        firstName: firstName || 'New',
        lastName: lastNameParts.join(' ') || 'User',
        phoneNumber: null,
        location: null,
        linkedInUrl: null,
        portfolioUrl: null,
        careerGoal: null,
        targetRole: null,
        careerLevel: null,
      })

      navigate('/onboarding/goals')
    } catch (err: any) {
      const message = err?.message || (typeof err === 'string' ? err : 'Unable to create your account. Please try again.')
      setError(message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <header className="auth-header">
        <span className="auth-logo" style={{ cursor: 'pointer' }} onClick={() => navigate('/')}>JobPilotAi</span>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ fontSize: 14, color: 'var(--app-on-surface-var)' }}>Already have an account?</span>
          <button style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-primary)', background: 'none', border: 'none', cursor: 'pointer', letterSpacing: '0.04em' }} onClick={() => navigate('/login')}>Sign In</button>
        </div>
      </header>

      <main className="auth-split-layout" style={{ flex: 1 }}>
        {/* Left editorial */}
        <div className="auth-editorial-panel" style={{ borderRight: 'none', borderBottom: 'none' }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '4px 12px', borderRadius: 999, background: 'var(--app-secondary-cont)', color: 'var(--app-on-sec-cont)', fontSize: 11, fontWeight: 700, letterSpacing: '0.08em', textTransform: 'uppercase', width: 'fit-content' }}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>verified</span>
              Professional Velocity
            </div>
            <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 44, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--app-on-surface)', margin: 0, lineHeight: 1.15 }}>
              Elevate your career with <span style={{ color: 'var(--app-secondary)' }}>Precision AI.</span>
            </h1>
            <p style={{ fontSize: 16, color: 'var(--app-on-surface-var)', lineHeight: 1.65, maxWidth: 440 }}>
              Join 10,000+ professionals accelerating their careers with AI-driven ATS optimization and professional momentum tracking.
            </p>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: 4 }}>
              <div style={{ display: 'flex', alignItems: 'center' }}>
                {['DC', 'SJ'].map((initials, i) => (
                  <div key={i} style={{ width: 36, height: 36, borderRadius: '50%', border: '2px solid var(--app-surface)', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--app-primary-cont)', color: 'var(--app-on-primary-cont)', fontSize: 12, fontWeight: 700, marginLeft: i > 0 ? -10 : 0 }}>
                    {initials}
                  </div>
                ))}
              </div>
              <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--app-on-surface-var)' }}>+10k active users</span>
            </div>
          </div>
        </div>

        {/* Right: Card */}
        <div className="auth-form-container">
          <div className="auth-card">
            <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 24, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 6 }}>Create Account</h2>
            <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginBottom: 24 }}>Start your journey to professional velocity today.</p>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: 10, marginBottom: 20 }}>
              <button className="social-btn" type="button" style={{ fontSize: 13, padding: '10px' }}>
                <svg width="18" height="18" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"/></svg> Google
              </button>
              <button className="social-btn" type="button" style={{ fontSize: 13, padding: '10px' }}>
                <svg width="18" height="18" fill="#0077B5" viewBox="0 0 24 24"><path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.761 0 5-2.239 5-5v-14c0-2.761-2.239-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z" /></svg> LinkedIn
              </button>
            </div>

              <div className="auth-divider"><div className="auth-divider-line" /><span className="auth-divider-text">Or continue with</span><div className="auth-divider-line" /></div>

              {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', justifyContent: 'center', padding: 10, marginBottom: 14 }}>{error}</div>}

              <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div className="app-input-group">
                  <label className="app-label">Full Name</label>
                  <div className="app-input-wrap">
                    <span className="material-symbols-outlined app-input-icon">person</span>
                    <input className="app-input" type="text" placeholder="John Doe" value={fullName} onChange={e => setFullName(e.target.value)} required />
                  </div>
                </div>
                <div className="app-input-group">
                  <label className="app-label">Email Address</label>
                  <div className="app-input-wrap">
                    <span className="material-symbols-outlined app-input-icon">mail</span>
                    <input className="app-input" type="email" placeholder="john@company.com" value={email} onChange={e => setEmail(e.target.value)} required />
                  </div>
                </div>
                <div className="app-input-group">
                  <label className="app-label">Password</label>
                  <div className="app-input-wrap">
                    <span className="material-symbols-outlined app-input-icon">lock</span>
                    <input className="app-input" type={showPass ? 'text' : 'password'} placeholder="••••••••••••" value={password} onChange={e => setPassword(e.target.value)} required minLength={12} style={{ paddingRight: 42 }} />
                    <button type="button" onClick={() => setShowPass(s => !s)} style={{ position: 'absolute', right: 12, top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--app-outline)' }}>
                      <span className="material-symbols-outlined" style={{ fontSize: 20 }}>{showPass ? 'visibility_off' : 'visibility'}</span>
                    </button>
                  </div>
                  <p style={{ fontSize: 11, color: 'var(--app-on-surface-var)', marginTop: 4 }}>Must be at least 12 characters long, with uppercase, lowercase, and a number.</p>
                </div>
                <label style={{ display: 'flex', alignItems: 'flex-start', gap: 8, fontSize: 13, color: 'var(--app-on-surface-var)', cursor: 'pointer', lineHeight: 1.5 }}>
                  <input type="checkbox" style={{ marginTop: 2, width: 16, height: 16, flexShrink: 0 }} required />
                  I agree to the <span style={{ color: 'var(--app-primary)', fontWeight: 700 }}>Terms of Service</span> and <span style={{ color: 'var(--app-primary)', fontWeight: 700 }}>Privacy Policy</span>.
                </label>
                <button className="auth-submit-btn" type="submit" disabled={loading}>{loading ? 'Creating account...' : 'Create Account'}</button>
              </form>

              <p style={{ textAlign: 'center', marginTop: 16, fontSize: 13, color: 'var(--app-on-surface-var)' }}>
                Already have an account? <span style={{ color: 'var(--app-secondary)', fontWeight: 700, cursor: 'pointer' }} onClick={() => navigate('/login')}>Sign In</span>
              </p>
            </div>
          </div>
      </main>
    </div>
  )
}
