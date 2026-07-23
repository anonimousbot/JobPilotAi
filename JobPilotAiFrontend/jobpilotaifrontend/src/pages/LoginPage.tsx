import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, setStoredAuth } from '../api/client'
import '../app.css'

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
    <div className="auth-page" style={{ flexDirection: 'row', minHeight: '100svh' }}>
      {/* Left editorial panel */}
      <div className="hidden md:flex" style={{
        width: '50%', flexDirection: 'column', justifyContent: 'center',
        padding: '64px', gap: 32, background: 'var(--app-surface)',
        borderRight: '1px solid var(--app-outline-var)'
      }}>
        <div style={{ display: 'inline-block', padding: '4px 12px', borderRadius: 999, background: 'var(--app-primary-fixed)', color: 'var(--app-on-primary-fixed)', fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', width: 'fit-content', marginBottom: 8 }}>
          PROFESSIONAL VELOCITY
        </div>
        <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 48, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--app-on-surface)', lineHeight: 1.15, margin: 0 }}>
          Accelerate your career with AI precision.
        </h1>
        <p style={{ fontSize: 18, color: 'var(--app-on-surface-var)', lineHeight: 1.6, maxWidth: 400 }}>
          JobPilotAi empowers ambitious professionals to navigate the modern job market using high-performance data analytics and automated workflows.
        </p>
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {[
            { icon: 'trending_up', title: 'Precision Matching', sub: '98% match accuracy for senior roles.' },
            { icon: 'bolt', title: 'Momentum Engine', sub: 'Automate your application funnel.' },
          ].map(f => (
            <div key={f.icon} style={{ display: 'flex', alignItems: 'center', gap: 16, padding: 16, background: 'rgba(255,255,255,0.8)', backdropFilter: 'blur(12px)', border: '1px solid rgba(226,232,240,0.8)', borderRadius: 12 }}>
              <div style={{ width: 48, height: 48, borderRadius: '50%', background: 'var(--app-secondary-cont)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--app-on-sec-cont)', flexShrink: 0 }}>
                <span className="material-symbols-outlined">{f.icon}</span>
              </div>
              <div>
                <div style={{ fontWeight: 700, color: 'var(--app-on-surface)', fontSize: 16 }}>{f.title}</div>
                <div style={{ fontSize: 13, color: 'var(--app-on-surface-var)' }}>{f.sub}</div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Right: Login form */}
      <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 24 }}>
        <div className="auth-card">
          <div style={{ textAlign: 'center', marginBottom: 28 }}>
            <div style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 22, color: 'var(--app-on-surface)', marginBottom: 4 }}>JobPilotAi</div>
            <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 28, fontWeight: 700, color: 'var(--app-on-surface)', margin: '0 0 6px' }}>Welcome Back</h2>
            <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)' }}>Continue your journey to professional velocity.</p>
          </div>

          {/* Social */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 20 }}>
            <button className="social-btn">
              <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuBL0pmMmpBzMugKpgRKN5c7dLl8SPbj3f8pnx_KN_ElZ2nh1M66UtyR6y3I42t-9mL1a0GEBWTfREOTTX7hE8YlOaZ6C0kHGhHIzTKH9ehcPSIRT-qbDPpK4BjHsGgnmv4lqMziyxszfPsnmL5KKWfXnXgjkfsajuQdCrsukhW8lhAzHVY4IemNzvBuKY6DT70NhRapPBvqd_eXv-WQ_ZvYR2na-IQQ9pczu4m5H5vQtYrlzGW6gibOX8LamxIoZ-8cSXFC5YqjFuKG" alt="Google" style={{ width: 20, height: 20 }} />
              Continue with Google
            </button>
            <button className="social-btn">
              <svg width="20" height="20" fill="#0077b5" viewBox="0 0 24 24"><path d="M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z" /></svg>
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
