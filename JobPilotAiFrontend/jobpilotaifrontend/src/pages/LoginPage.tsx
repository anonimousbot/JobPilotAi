import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, setStoredAuth } from '../api/client'
import '../App.css'

export default function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      const auth = await api.login(email, password)
      setStoredAuth(auth)
      
      // Check if user has completed onboarding
      try {
        const profile = await api.getProfile()
        const isOnboardingComplete = profile.careerGoal && profile.targetRole && profile.careerLevel
        
        if (isOnboardingComplete) {
          navigate('/dashboard')
        } else {
          navigate('/onboarding/goals')
        }
      } catch {
        // If profile fetch fails, default to dashboard
        navigate('/dashboard')
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to sign in. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-split-layout">
      {/* Left editorial panel */}
      <div className="auth-editorial-panel">
        <div style={{ display: 'inline-block', padding: '4px 12px', borderRadius: 999, background: 'var(--app-primary-fixed)', color: 'var(--app-on-primary-fixed)', fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', width: 'fit-content', marginBottom: 8 }}>
          PROFESSIONAL VELOCITY
        </div>
        <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 44, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--app-on-surface)', lineHeight: 1.15, margin: 0 }}>
          Accelerate your career with AI precision.
        </h1>
        <p style={{ fontSize: 16, color: 'var(--app-on-surface-var)', lineHeight: 1.6, maxWidth: 400 }}>
          JobPilotAi empowers ambitious professionals to navigate the modern job market using high-performance data analytics and automated workflows.
        </p>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {[
            { icon: 'trending_up', title: 'Precision Matching', sub: '98% match accuracy for senior roles.' },
            { icon: 'bolt', title: 'Momentum Engine', sub: 'Automate your application funnel.' },
          ].map(f => (
            <div key={f.icon} style={{ display: 'flex', alignItems: 'center', gap: 16, padding: 16, background: 'rgba(255,255,255,0.8)', backdropFilter: 'blur(12px)', border: '1px solid rgba(226,232,240,0.8)', borderRadius: 12 }}>
              <div style={{ width: 44, height: 44, borderRadius: '50%', background: 'var(--app-secondary-cont)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--app-on-sec-cont)', flexShrink: 0 }}>
                <span className="material-symbols-outlined">{f.icon}</span>
              </div>
              <div>
                <div style={{ fontWeight: 700, color: 'var(--app-on-surface)', fontSize: 15 }}>{f.title}</div>
                <div style={{ fontSize: 13, color: 'var(--app-on-surface-var)' }}>{f.sub}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Right: Login form */}
      <div className="auth-form-container">
        <div className="auth-card">
          <div style={{ textAlign: 'center', marginBottom: 24 }}>
            <div style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 22, color: 'var(--app-on-surface)', marginBottom: 4 }}>JobPilotAi</div>
            <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 26, fontWeight: 700, color: 'var(--app-on-surface)', margin: '0 0 6px' }}>Welcome Back</h2>
            <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)' }}>Continue your journey to professional velocity.</p>
          </div>

          {/* Social */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 20 }}>
            <button className="social-btn" type="button">
              <svg width="18" height="18" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"/></svg>
              Continue with Google
            </button>
            <button className="social-btn" type="button">
              <svg width="18" height="18" fill="#0077b5" viewBox="0 0 24 24"><path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z" /></svg>
              Continue with LinkedIn
            </button>
          </div>

          <div className="auth-divider"><div className="auth-divider-line" /><span className="auth-divider-text">Or continue with email</span><div className="auth-divider-line" /></div>

          {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', justifyContent: 'center', padding: 10 }}>{error}</div>}

          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div className="app-input-group">
              <label className="app-label">Email Address</label>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">mail</span>
                <input className="app-input" type="email" placeholder="name@company.com" value={email} onChange={e => setEmail(e.target.value)} required />
              </div>
            </div>
            <div className="app-input-group">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <label className="app-label">Password</label>
                <a href="#" onClick={(e) => { e.preventDefault(); alert('Password reset functionality would be implemented here. For now, please contact support.') }} style={{ fontSize: 12, color: 'var(--app-primary)', fontWeight: 600, cursor: 'pointer' }}>Forgot password?</a>
              </div>
              <div className="app-input-wrap">
                <span className="material-symbols-outlined app-input-icon">lock</span>
                <input className="app-input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required />
              </div>
            </div>
            <label style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, color: 'var(--app-on-surface-var)', cursor: 'pointer' }}>
              <input type="checkbox" style={{ width: 16, height: 16 }} />
              Remember me for 30 days
            </label>
            <button className="auth-submit-btn" type="submit" disabled={loading}>{loading ? 'Signing in...' : 'Sign In'}</button>
          </form>

          <p style={{ textAlign: 'center', marginTop: 20, fontSize: 14, color: 'var(--app-on-surface-var)' }}>
            Don't have an account?{' '}
            <span style={{ color: 'var(--app-primary)', fontWeight: 700, cursor: 'pointer' }} onClick={() => navigate('/signup')}>Sign Up</span>
          </p>
        </div>
      </div>
    </div>
  )
}
