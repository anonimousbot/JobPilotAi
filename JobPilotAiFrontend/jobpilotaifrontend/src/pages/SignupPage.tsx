import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError, setStoredAuth } from '../api/client'
import '../app.css'

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
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to create your account. Please try again.')
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

      <main style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '32px 16px' }}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 40, maxWidth: 1100, width: '100%', alignItems: 'center' }}>
          {/* Left editorial */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
            <div style={{ display: 'inline-flex', alignItems: 'center', gap: 6, padding: '4px 12px', borderRadius: 999, background: 'var(--app-secondary-cont)', color: 'var(--app-on-sec-cont)', fontSize: 11, fontWeight: 700, letterSpacing: '0.08em', textTransform: 'uppercase', width: 'fit-content' }}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>verified</span>
              Professional Velocity
            </div>
            <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 48, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--app-on-surface)', margin: 0, lineHeight: 1.15 }}>
              Elevate your career with <span style={{ color: 'var(--app-secondary)' }}>Precision AI.</span>
            </h1>
            <p style={{ fontSize: 17, color: 'var(--app-on-surface-var)', lineHeight: 1.65, maxWidth: 440 }}>
              Join 10,000+ professionals accelerating their careers with AI-driven ATS optimization and professional momentum tracking.
            </p>
            <div style={{ display: 'flex', alignItems: 'center', gap: -12, marginTop: 4 }}>
              {[
                'https://lh3.googleusercontent.com/aida-public/AB6AXuDjR_qkZxICGEu9JQJC4LK6-u22GZdUz-YGMBMeBaVxNk3JA2DLjItTsD8VHkO5qhGb-OBBjth4TcEsuElEKrQ6KYid-Jf_kn2WecLV3k2lXZzzW4JpkfVqpOKt6cv5e-2vcRhPwTGYOEDXRc08I3FaVFLM0mykSJjSsjovVOaMXagvnu4hnD6GTHZ5_yFj0h-Gm_VwqSvoAVYbbhzd1nRKmeFjXfNylBT4klPVwt3qodcVf_79e5hTkU--PF4qz-Y6Kdth2RmlxU2_',
                'https://lh3.googleusercontent.com/aida-public/AB6AXuBeMerSqOOLxHdhRb-C7rMgy6hb7YM8yRbbTn8P4hCnEDt_-JWDEuA5j4r-wlosKHHD6iHMJAOXaKH1HW9Gh9jD9h6ovdbgaqZYsOLvGe0sh1sBezwm7-M0Kyio3wBi-zlbDvfZAbBIQb8Sp4DnIZQiJCROY7A2_hi5TPriKyrCP-gBe2o7VlA0LrfiVjTDiXGzsdDyCWh6i67BhtRubFY5l6KYzLlRiKMrtRX6--_Her3rbEOHX2SQjzAWPQ4X8eQQez9v8PhXvX9m',
              ].map((src, i) => (
                <div key={i} style={{ width: 40, height: 40, borderRadius: '50%', border: '2px solid var(--app-surface)', overflow: 'hidden', marginLeft: i > 0 ? -10 : 0, background: '#cbd5e1' }}>
                  <img src={src} alt="member" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                </div>
              ))}
              <span style={{ marginLeft: 16, fontSize: 13, fontWeight: 600, color: 'var(--app-on-surface-var)' }}>+10k active users</span>
            </div>
            <div style={{ borderRadius: 14, overflow: 'hidden', border: '1px solid var(--app-outline-var)', boxShadow: '0 8px 24px rgba(15,23,42,0.08)', marginTop: 8, position: 'relative', aspectRatio: '16/9' }}>
              <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuCC2sY-OCMu-ya3jG5HWufC4U5l1oeq-hqd9G1Xfzl1TZi9u4WfxlWURegGSUovA5SIjCRzv0jAJEqGtPBoSuFwXKQxaEnpI0ADjvI9JQ9zm55sXFhBRHWcJegZIQCXTCVTTLruzPwj-9v2MH_0A0jsMsOSMIs3IxZ2bZImtY-Z40tGPN7_je-KqHmFA-zHFQc42KADwpW5HftVF8I73R7-jmTqOl7uf5UyFnp-wN9kZw0zWUcJos3ShMb4qp1JeWRp-MFdLzAir8UT" alt="Dashboard Preview" style={{ width: '100%', height: '100%', objectFit: 'cover', opacity: 0.85 }} />
              <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(to top, var(--app-surface-lowest), transparent)' }} />
            </div>
          </div>

          {/* Right: Card */}
          <div>
            <div className="auth-card">
              <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 24, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 6 }}>Create Account</h2>
              <p style={{ fontSize: 13, color: 'var(--app-on-surface-var)', marginBottom: 24 }}>Start your journey to professional velocity today.</p>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10, marginBottom: 20 }}>
                <button className="social-btn" style={{ fontSize: 13, padding: '10px' }}>
                  <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuAk-YzVG2dilX9FRFzU5KSvyspgj5fc0TWkeXo4qgK07YYEdLbFpYb65mGdmL2CrnY3RAoNk2_-HFxgiC6VGhO47DF44GtUuIxuYej2JHecpkHBvi82l8UjUTfNCUzBqc6R0nzGaTGAkcGFklhVWpLorj-YMCiuNdIpnE1K4mSMYsBAoa6dyNYMdPuoeng6IXBFLYAbDHG0UcZ5kaM9m9iwaka-a18fu8R16df-_vf9G5e9BlIpuFj_xvnmj_z0XhbySlsVXDdEXGZn" alt="Google" style={{ width: 18, height: 18 }} /> Google
                </button>
                <button className="social-btn" style={{ fontSize: 13, padding: '10px' }}>
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
                    <input className="app-input" type={showPass ? 'text' : 'password'} placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required minLength={8} style={{ paddingRight: 42 }} />
                    <button type="button" onClick={() => setShowPass(s => !s)} style={{ position: 'absolute', right: 12, top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--app-outline)' }}>
                      <span className="material-symbols-outlined" style={{ fontSize: 20 }}>{showPass ? 'visibility_off' : 'visibility'}</span>
                    </button>
                  </div>
                  <p style={{ fontSize: 11, color: 'var(--app-on-surface-var)', marginTop: 4 }}>Must be at least 8 characters long.</p>
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
        </div>
      </main>
    </div>
  )
}
