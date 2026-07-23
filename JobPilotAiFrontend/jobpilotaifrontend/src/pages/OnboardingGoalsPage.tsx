import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import '../app.css'

const goals = [
  { icon: 'rocket_launch', title: 'Landing a new job', desc: 'Finding the perfect role in your current industry.', value: 'land-new-job' },
  { icon: 'alt_route', title: 'Switching careers', desc: 'Pivoting into a new field or professional domain.', value: 'switch-careers' },
  { icon: 'trending_up', title: 'Internal promotion', desc: 'Climbing the ladder within your existing company.', value: 'earn-promotion' },
  { icon: 'explore', title: 'Just exploring', desc: 'Browsing opportunities to see what\'s out there.', value: 'just-exploring' },
]

export default function OnboardingGoalsPage() {
  const navigate = useNavigate()
  const [selected, setSelected] = useState<number | null>(null)
  const [showToast, setShowToast] = useState(false)

  const handleContinue = () => {
    if (selected === null) return
    localStorage.setItem('jobpilot.onboardingGoal', goals[selected].value)
    setShowToast(true)
    setTimeout(() => { setShowToast(false); navigate('/onboarding/experience') }, 1200)
  }

  return (
    <div className="auth-page" style={{ minHeight: '100svh' }}>
      {/* Background blobs */}
      <div style={{ position: 'fixed', inset: 0, zIndex: 0, pointerEvents: 'none', overflow: 'hidden' }}>
        <div style={{ position: 'absolute', top: '10%', left: '5%', width: 400, height: 400, background: 'rgba(108,248,187,0.12)', borderRadius: '50%', filter: 'blur(100px)' }} />
        <div style={{ position: 'absolute', bottom: '10%', right: '5%', width: 400, height: 400, background: 'rgba(220,233,255,0.4)', borderRadius: '50%', filter: 'blur(100px)' }} />
      </div>

      {/* Header */}
      <header className="onboarding-header" style={{ position: 'sticky', top: 0, zIndex: 50 }}>
        <div style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 20, color: 'var(--app-on-surface)', cursor: 'pointer' }} onClick={() => navigate('/')}>JobPilotAi</div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-on-surface-var)', letterSpacing: '0.06em' }}>STEP 1 OF 3</span>
          <div className="onboarding-progress-track">
            <div className="onboarding-progress-fill" style={{ width: '33%' }} />
          </div>
        </div>
      </header>

      <main style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '40px 16px', position: 'relative', zIndex: 1 }}>
        <div style={{ width: '100%', maxWidth: 780, display: 'flex', flexDirection: 'column', gap: 32 }}>
          {/* Heading */}
          <div style={{ textAlign: 'center' }}>
            <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 48, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--app-on-surface)', margin: '0 0 10px' }}>
              What is your primary career goal?
            </h1>
            <p style={{ fontSize: 18, color: 'var(--app-on-surface-var)', lineHeight: 1.6 }}>
              We'll tailor your experience to accelerate your Professional Velocity.
            </p>
          </div>

          {/* Options grid */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            {goals.map((g, i) => (
              <button key={i} className={`goal-card${selected === i ? ' selected' : ''}`} onClick={() => setSelected(i)}>
                <div className="goal-card-icon">
                  <span className="material-symbols-outlined" style={{ fontSize: 32, color: 'var(--app-secondary)' }}>{g.icon}</span>
                </div>
                <div>
                  <div className="goal-card-title">{g.title}</div>
                  <div className="goal-card-desc">{g.desc}</div>
                </div>
              </button>
            ))}
          </div>

          {/* Footer controls */}
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderTop: '1px solid var(--app-outline-var)', paddingTop: 20 }}>
            <button onClick={() => navigate('/')} style={{ fontSize: 13, fontWeight: 700, color: 'var(--app-on-surface-var)', background: 'none', border: 'none', cursor: 'pointer', letterSpacing: '0.04em' }}>
              Back
            </button>
            <button
              onClick={handleContinue}
              disabled={selected === null}
              style={{
                padding: '12px 32px', background: selected !== null ? 'var(--app-primary)' : 'var(--app-outline-var)',
                color: 'white', border: 'none', borderRadius: 10, fontSize: 15, fontWeight: 700,
                cursor: selected !== null ? 'pointer' : 'not-allowed', transition: 'background 0.2s',
                boxShadow: selected !== null ? '0 2px 8px rgba(0,0,0,0.15)' : 'none'
              }}
            >
              Continue
            </button>
          </div>
        </div>
      </main>

      {/* Toast */}
      <div style={{
        position: 'fixed', bottom: 24, left: '50%', transform: `translateX(-50%) translateY(${showToast ? 0 : 80}px)`,
        opacity: showToast ? 1 : 0, transition: 'all 0.3s ease',
        background: 'var(--app-on-sec-cont)', color: 'white', padding: '10px 20px',
        borderRadius: 999, fontSize: 13, fontWeight: 700,
        display: 'flex', alignItems: 'center', gap: 8, boxShadow: '0 4px 16px rgba(0,0,0,0.2)',
        zIndex: 999, pointerEvents: 'none'
      }}>
        <span className="material-symbols-outlined" style={{ fontSize: 18, fontVariationSettings: "'FILL' 1" }}>check_circle</span>
        Goal preference captured
      </div>
    </div>
  )
}
