import { useState, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import '../app.css'

export default function OnboardingResumePage() {
  const navigate = useNavigate()
  const [dragOver, setDragOver] = useState(false)
  const [uploadState, setUploadState] = useState<'idle' | 'uploading' | 'done'>('idle')
  const [fileName, setFileName] = useState('')
  const [error, setError] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  const handleFile = async (file: File) => {
    setFileName(file.name)
    setError('')
    setUploadState('uploading')

    try {
      await api.uploadResume(file)
      setUploadState('done')
    } catch (err) {
      setUploadState('idle')
      setError(err instanceof ApiError ? err.message : 'Unable to upload resume. Please try again.')
    }
  }

  return (
    <div className="auth-page" style={{ minHeight: '100svh' }}>
      <header className="onboarding-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontFamily: 'Hanken Grotesk', fontWeight: 700, fontSize: 20, color: 'var(--app-on-surface)' }}>JobPilotAi</span>
          <div style={{ width: 1, height: 24, background: 'var(--app-outline-var)' }} />
          <span style={{ fontSize: 11, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--app-on-surface-var)' }}>Onboarding</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontSize: 12, fontWeight: 700, color: 'var(--app-on-surface-var)', letterSpacing: '0.06em' }}>STEP 3 OF 3</span>
          <div className="onboarding-progress-track">
            <div className="onboarding-progress-fill" style={{ width: '100%' }} />
          </div>
        </div>
      </header>

      <main style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '40px 16px' }}>
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <h1 style={{ fontFamily: 'Hanken Grotesk', fontSize: 32, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 8 }}>Power Up Your Profile</h1>
          <p style={{ fontSize: 17, color: 'var(--app-on-surface-var)', maxWidth: 560, margin: '0 auto' }}>
            Upload your resume to let JobPilotAi analyze your career momentum and unlock professional-grade recommendations.
          </p>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '7fr 5fr', gap: 20, width: '100%', maxWidth: 900 }}>
          {/* Dropzone */}
          <div>
            <div
              className={`dropzone${dragOver ? ' drag-over' : ''}`}
              onDragOver={e => { e.preventDefault(); setDragOver(true) }}
              onDragLeave={() => setDragOver(false)}
              onDrop={e => { e.preventDefault(); setDragOver(false); const f = e.dataTransfer.files[0]; if (f) void handleFile(f) }}
              onClick={() => inputRef.current?.click()}
              style={{ minHeight: 360 }}
            >
              <input ref={inputRef} type="file" accept=".pdf,.docx" style={{ display: 'none' }} onChange={e => { const f = e.target.files?.[0]; if (f) void handleFile(f) }} />

              {uploadState === 'idle' && <>
                <div className="dropzone-icon"><span className="material-symbols-outlined" style={{ fontSize: 36 }}>cloud_upload</span></div>
                <h2 style={{ fontFamily: 'Hanken Grotesk', fontSize: 20, fontWeight: 700, color: 'var(--app-on-surface)', marginBottom: 8 }}>Resume Upload</h2>
                <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)', marginBottom: 20 }}>Drag and drop your PDF or DOCX here</p>
                <button className="btn-app-primary" style={{ padding: '10px 28px', fontSize: 15 }}>Select File</button>
                <div style={{ display: 'flex', gap: 16, marginTop: 20 }}>
                  {['picture_as_pdf|PDF', 'description|DOCX'].map(s => { const [icon, label] = s.split('|'); return (
                    <div key={label} style={{ display: 'flex', alignItems: 'center', gap: 4, color: 'var(--app-on-surface-var)' }}>
                      <span className="material-symbols-outlined" style={{ fontSize: 16 }}>{icon}</span>
                      <span style={{ fontSize: 12, fontWeight: 700 }}>{label}</span>
                    </div>
                  )})}
                </div>
                {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginTop: 16 }}>{error}</div>}
              </>}

              {uploadState === 'uploading' && <>
                <div style={{ width: 64, height: 64, border: '4px solid var(--app-secondary)', borderTopColor: 'transparent', borderRadius: '50%', animation: 'spin 0.8s linear infinite', marginBottom: 20 }} />
                <h3 style={{ fontFamily: 'Hanken Grotesk', fontSize: 18, fontWeight: 700, color: 'var(--app-on-surface)' }}>Analyzing {fileName}...</h3>
                <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)' }}>Extracting career momentum data</p>
              </>}

              {uploadState === 'done' && <>
                <div style={{ width: 64, height: 64, background: 'var(--app-secondary)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', marginBottom: 20 }}>
                  <span className="material-symbols-outlined" style={{ fontSize: 32, color: 'white' }}>check</span>
                </div>
                <h3 style={{ fontFamily: 'Hanken Grotesk', fontSize: 20, fontWeight: 700, color: 'var(--app-secondary)', marginBottom: 8 }}>Resume Optimized</h3>
                <p style={{ fontSize: 14, color: 'var(--app-on-surface-var)', marginBottom: 12 }}>{fileName} uploaded successfully.</p>
                <div style={{ background: 'var(--app-surface-cont)', padding: '8px 16px', borderRadius: 8, fontSize: 12, fontWeight: 700, color: 'var(--app-on-sec-cont)' }}>Ready for ATS analysis</div>
              </>}
            </div>
          </div>

          {/* Benefits */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {[
              { icon: 'analytics', title: 'Instant ATS Analysis', desc: 'See exactly how your resume scores against industry-standard tracking systems.', bg: 'var(--app-surface-highest)', col: 'var(--app-primary-cont)' },
              { icon: 'auto_awesome', title: 'Personalized AI Recommendations', desc: 'Get tailor-made advice on how to improve your bullet points and impact metrics.', bg: 'var(--app-secondary-cont)', col: 'var(--app-on-sec-cont)' },
            ].map(b => (
              <div key={b.icon} style={{ background: 'var(--app-surface-lowest)', border: '1px solid var(--app-outline-var)', borderRadius: 14, padding: 20, display: 'flex', alignItems: 'flex-start', gap: 16, transition: 'box-shadow 0.2s' }}>
                <div style={{ width: 44, height: 44, borderRadius: 10, background: b.bg, display: 'flex', alignItems: 'center', justifyContent: 'center', color: b.col, flexShrink: 0 }}>
                  <span className="material-symbols-outlined">{b.icon}</span>
                </div>
                <div>
                  <div style={{ fontWeight: 700, fontSize: 15, color: 'var(--app-on-surface)', marginBottom: 4 }}>{b.title}</div>
                  <div style={{ fontSize: 13, color: 'var(--app-on-surface-var)', lineHeight: 1.5 }}>{b.desc}</div>
                </div>
              </div>
            ))}
            <div style={{ background: 'var(--app-primary-cont)', borderRadius: 14, padding: 24, position: 'relative', overflow: 'hidden', minHeight: 140 }}>
              <div style={{ position: 'relative', zIndex: 1 }}>
                <p style={{ fontSize: 10, fontWeight: 700, letterSpacing: '0.1em', textTransform: 'uppercase', color: 'rgba(255,255,255,0.5)', marginBottom: 6 }}>Metric Preview</p>
                <h4 style={{ fontFamily: 'Hanken Grotesk', fontSize: 22, fontWeight: 700, color: 'white', marginBottom: 4 }}>Professional Velocity</h4>
                <p style={{ fontSize: 12, color: 'rgba(255,255,255,0.5)' }}>Score will generate after upload</p>
              </div>
              <div style={{ position: 'absolute', top: 16, right: 16, width: 44, height: 44, borderRadius: '50%', border: '2px dashed rgba(255,255,255,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <span className="material-symbols-outlined" style={{ color: 'rgba(255,255,255,0.4)', fontSize: 20 }}>lock</span>
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', maxWidth: 900, marginTop: 28, paddingTop: 20, borderTop: '1px solid var(--app-outline-var)' }}>
          <button onClick={() => navigate('/onboarding/experience')} style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 14, fontWeight: 600, color: 'var(--app-on-surface-var)', background: 'none', border: 'none', cursor: 'pointer' }}>
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>arrow_back</span> Skip for now
          </button>
          <button
            onClick={() => navigate('/dashboard')}
            className="btn-app-primary"
            style={{ padding: '14px 40px', fontSize: 18, fontFamily: 'Hanken Grotesk', background: 'var(--app-secondary)', boxShadow: '0 4px 16px rgba(0,108,73,0.25)', borderRadius: 14 }}
          >
            {uploadState === 'done' ? 'View Dashboard' : 'Finish Setup'}
          </button>
        </div>
      </main>

      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </div>
  )
}
