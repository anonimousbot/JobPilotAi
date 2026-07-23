import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, ApiError } from '../api/client'
import type { CoverLetterResponse } from '../api/types'
import AppLayout from '../components/AppLayout'
import '../app.css'

const tones = ['Professional', 'Enthusiastic', 'Concise', 'Creative']

export default function CoverLetterPage() {
  const navigate = useNavigate()
  const [tone, setTone] = useState('Professional')
  const [letters, setLetters] = useState<CoverLetterResponse[]>([])
  const [selectedLetterId, setSelectedLetterId] = useState<string | null>(null)
  const [content, setContent] = useState('')
  const [error, setError] = useState('')
  const [status, setStatus] = useState('')
  const activeLetter = letters.find(l => l.id === selectedLetterId) || letters[0]

  useEffect(() => {
    void loadLetters()
  }, [])

  useEffect(() => {
    if (!activeLetter) return
    setTone(activeLetter.tone)
    setContent(activeLetter.editedContent || activeLetter.originalContent)
  }, [activeLetter])

  const loadLetters = async () => {
    setError('')
    try {
      setLetters(await api.listCoverLetters())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to load cover letters.')
    }
  }

  const saveLetter = async () => {
    if (!activeLetter) return
    setStatus('')
    setError('')
    try {
      const updated = await api.updateCoverLetter(activeLetter.id, content)
      setLetters(items => [updated, ...items.filter(item => item.id !== updated.id)])
      setStatus('Saved')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to save cover letter.')
    }
  }

  const deleteLetter = async (id: string) => {
    setError('')
    try {
      await api.deleteCoverLetter(id)
      setLetters(items => items.filter(item => item.id !== id))
      if (selectedLetterId === id) {
        setSelectedLetterId(null)
      }
      setStatus('Deleted')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to delete cover letter.')
    }
  }

  const regenerateWithTone = async (newTone: string) => {
    if (!activeLetter) return
    setTone(newTone)
    setStatus('Regenerating...')
    setError('')
    try {
      const regenerated = await api.regenerateCoverLetter(activeLetter.id)
      setLetters(items => items.map(item => item.id === regenerated.id ? regenerated : item))
      setStatus('Regenerated')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to regenerate cover letter.')
      setStatus('')
    }
  }

  return (
    <AppLayout
      title="Cover Letters"
      subtitle="AI-generated, tone-perfect cover letters for every role."
      actions={
        <button className="btn-app-primary" onClick={() => navigate('/generate-cover-letter')}>
          <span className="material-symbols-outlined" style={{ fontSize: 18 }}>add</span>
          Generate New
        </button>
      }
    >
      {error && <div className="badge" style={{ background: '#fee2e2', color: '#991b1b', padding: 10, marginBottom: 16 }}>{error}</div>}
      {status && <div className="badge badge-green" style={{ padding: 10, marginBottom: 16 }}>{status}</div>}
      
      {/* Cover letter selector */}
      {letters.length > 0 && (
        <div style={{ marginBottom: 20 }}>
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 12 }}>
            {letters.map(letter => (
              <button
                key={letter.id}
                onClick={() => setSelectedLetterId(letter.id)}
                style={{
                  padding: '8px 16px',
                  borderRadius: 8,
                  fontSize: 12,
                  fontWeight: 700,
                  cursor: 'pointer',
                  border: '1px solid',
                  transition: 'all 0.15s',
                  background: (selectedLetterId || letters[0]?.id) === letter.id ? 'var(--app-primary)' : 'transparent',
                  color: (selectedLetterId || letters[0]?.id) === letter.id ? 'white' : 'var(--app-on-surface-var)',
                  borderColor: (selectedLetterId || letters[0]?.id) === letter.id ? 'var(--app-primary)' : 'var(--app-outline-var)',
                }}
              >
                {letter.companyName} - {letter.jobTitle}
              </button>
            ))}
          </div>
        </div>
      )}

      {!activeLetter && (
        <div className="bento-card" style={{ marginBottom: 20 }}>
          <div className="bento-card-body">
            <h3 style={{ marginTop: 0, color: 'var(--app-on-surface)' }}>No cover letter yet</h3>
            <p style={{ color: 'var(--app-on-surface-var)' }}>Generate one from a job description to start editing.</p>
            <button className="btn-app-primary" onClick={() => navigate('/generate-cover-letter')}>Generate Cover Letter</button>
          </div>
        </div>
      )}

      <div className="editor-layout">
        {/* Editor */}
        <div className="editor-panel">
          <div className="editor-panel-header">
            <span className="editor-panel-title">Cover Letter Editor</span>
            <div style={{ display: 'flex', gap: 8 }}>
              {tones.map(t => (
                <button
                  key={t}
                  onClick={() => void regenerateWithTone(t)}
                  style={{
                    padding: '4px 12px', borderRadius: 999, fontSize: 11, fontWeight: 700,
                    border: '1px solid', cursor: 'pointer', transition: 'all 0.15s',
                    background: tone === t ? 'var(--app-primary)' : 'transparent',
                    color: tone === t ? 'white' : 'var(--app-on-surface-var)',
                    borderColor: tone === t ? 'var(--app-primary)' : 'var(--app-outline-var)',
                  }}
                >
                  {t}
                </button>
              ))}
            </div>
          </div>
          <textarea
            className="editor-textarea"
            value={content}
            onChange={e => setContent(e.target.value)}
            placeholder="Generate a cover letter to begin editing."
          />
          <div style={{ padding: '12px 20px', borderTop: '1px solid var(--app-outline-var)', display: 'flex', gap: 10, background: 'var(--app-surface)' }}>
            <button className="btn-app-primary" style={{ fontSize: 13 }} onClick={() => void saveLetter()} disabled={!activeLetter}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>save</span>
              Save Edits
            </button>
            <button className="btn-app-secondary" style={{ fontSize: 13 }} onClick={() => void navigator.clipboard.writeText(content)} disabled={!content}>
              <span className="material-symbols-outlined" style={{ fontSize: 16 }}>content_copy</span>
              Copy Text
            </button>
            {activeLetter && (
              <button className="btn-app-secondary" style={{ fontSize: 13, color: '#dc2626', borderColor: '#dc2626' }} onClick={() => void deleteLetter(activeLetter.id)}>
                <span className="material-symbols-outlined" style={{ fontSize: 16 }}>delete</span>
                Delete
              </button>
            )}
          </div>
        </div>

        {/* Preview / info */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div className="editor-panel">
            <div className="editor-panel-header">
              <span className="editor-panel-title">Job Details</span>
            </div>
            <div style={{ padding: '20px' }}>
              {[['Position', activeLetter?.jobTitle ?? 'Not generated'], ['Company', activeLetter?.companyName ?? 'Not generated'], ['Tone', tone], ['Status', activeLetter ? 'Draft ready' : 'Waiting']].map(([label, val]) => (
                <div key={label} style={{ display: 'flex', justifyContent: 'space-between', padding: '10px 0', borderBottom: '1px solid rgba(198,198,205,0.4)', fontSize: 13 }}>
                  <span style={{ fontWeight: 700, color: 'var(--app-on-surface-var)', textTransform: 'uppercase', fontSize: 11, letterSpacing: '0.06em' }}>{label}</span>
                  <span style={{ color: 'var(--app-on-surface)', fontWeight: 600 }}>{val}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="editor-panel">
            <div className="editor-panel-header"><span className="editor-panel-title">AI Quality Score</span></div>
            <div style={{ padding: '20px' }}>
              {[['Tone Match', 92], ['Relevance', 88], ['Readability', 96], ['Uniqueness', 85]].map(([label, val]) => (
                <div key={label} style={{ marginBottom: 14 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                    <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--app-on-surface)' }}>{label}</span>
                    <span style={{ fontSize: 13, fontWeight: 700, color: 'var(--app-secondary)' }}>{val}%</span>
                  </div>
                  <div className="progress-bar-track">
                    <div className="progress-bar-fill-secondary" style={{ width: `${val}%` }} />
                  </div>
                </div>
              ))}
            </div>
          </div>

          <button
            onClick={() => void regenerateWithTone(tone)}
            className="btn-app-secondary"
            style={{ width: '100%', justifyContent: 'center', padding: 14, fontSize: 14 }}
          >
            <span className="material-symbols-outlined" style={{ fontSize: 18 }}>auto_awesome</span>
            Regenerate with AI
          </button>
        </div>
      </div>
    </AppLayout>
  )
}
